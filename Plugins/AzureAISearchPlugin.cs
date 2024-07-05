
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class AzureAISearchPlugin
{
    private const string EmbeddingModelName = "text-embedding-ada-002";

    [KernelFunction("Search")]
    [Description("Search and find internal HR information related to the employees. Information covers holiday plans, vacation periodes, sick days, family details, etc.")]
    public async Task<string> AISearch([Description("Search query against AI Search")] string inputQuery)
    {
        /*
         * Integrated vectorization feature in Azure AI Search
         * This demo is connecting to an AI Search Index that has been created with integrated vectorization.
         https://learn.microsoft.com/en-us/azure/search/search-get-started-portal-import-vectors?tabs=sample-data-storage%2Cmodel-aoai
         */
        Console.WriteLine($"-----------------------------------------------");
        Console.WriteLine("Initiating Function: AISearch");

        var appSettings = ConfigurationManager.AppSettings;
        
        // Load environment variables  
        string AISearchEndpoint = appSettings["aisearch_endpoint"] ?? string.Empty;
        string AISearchKey = appSettings["aisearch_apiKey"] ?? string.Empty;
        string AISearchIndexName = "integratedvectorization" ?? string.Empty;
        string openaiApiKey = appSettings["aoai_apiKey"] ?? string.Empty;
        string openaiEndpoint = appSettings["aoai_endpoint"] ?? string.Empty;
        string openaiModelId = appSettings["aoai_modelId"] ?? string.Empty;

        // Initialize OpenAI client  
        AzureKeyCredential credential = new AzureKeyCredential(openaiApiKey);
        OpenAIClient openAIClient = new OpenAIClient(new Uri(openaiEndpoint), credential);

        // Initialize Azure AI Search clients  
        AzureKeyCredential searchCredential = new AzureKeyCredential(AISearchKey);
        SearchIndexClient indexClient = new SearchIndexClient(new Uri(AISearchEndpoint), searchCredential);
        SearchClient searchClient = indexClient.GetSearchClient(AISearchIndexName);

        //return top 1 result from the search
        return await SimpleHybridSearch(searchClient, openAIClient, inputQuery);
    }
    // Function to generate embeddings  
    private static async Task<IReadOnlyList<float>> GenerateEmbeddings(string text, OpenAIClient openAIClient)
    {
        var data = new List<string> { text };
        var response = await openAIClient.GetEmbeddingsAsync(new EmbeddingsOptions(EmbeddingModelName,data));

        return response.Value.Data[0].Embedding.ToArray();
    }
    internal static async Task<string> SimpleHybridSearch(SearchClient searchClient, OpenAIClient openAIClient, string query, int k = 3)
    {
        // Generate the embedding for the query  
        var queryEmbeddings = await GenerateEmbeddings(query, openAIClient);
        
        // Perform the vector similarity search  
        var searchOptions = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = k, Fields = { "text_vector" } } } //text_vector is my field containing the vectorized text content
            },
            Size = k,
            Select = { "title", "chunk", "metadata_storage_path" }, //chunk is my text content field. Index created using integrated vectorization in ai search.
        };

        // pulling the text content from the search results into custom class IndexSchema
        Response<SearchResults<IndexSchema>> responseI = await searchClient.SearchAsync<IndexSchema>(query, searchOptions);

        //SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);

        //int count = 0;
        //await foreach (SearchResult<SearchDocument> result in response.GetResultsAsync())
        //{
        //    count++;
        //    Console.WriteLine($"Title: {result.Document["title"]}");
        //    Console.WriteLine($"Score: {result.Score}\n");
        //    Console.WriteLine($"Content: {result.Document["content"]}");
        //    Console.WriteLine($"Category: {result.Document["category"]}\n");
        //}
        //Console.WriteLine($"Total Results: {count}");

        await foreach (SearchResult<IndexSchema> result in responseI.Value.GetResultsAsync())
        {
            Console.WriteLine($"Top 1 Document Retrieved: {result.Document.Title}:\n{result.Document.Chunk}\n-----------------------------------------------");
            return result.Document.Chunk; // Return text from first result
        }
        return string.Empty; // Return empty string if no results
    }

    private sealed class IndexSchema
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("chunk")]
        public string Chunk { get; set; }

        [JsonPropertyName("vector")]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}
