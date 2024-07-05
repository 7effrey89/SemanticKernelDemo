using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

using System.Configuration;

namespace SemanticKernel
{
    internal class Demo_AISearch_RAG_Only
    {
        //https://devblogs.microsoft.com/semantic-kernel/azure-openai-on-your-data-with-semantic-kernel/
        /*
            This demo uses the AzureChatExtensionsOptions to call AI Search to do RAG.
            My experience is that function calling will take precedence over the use of AzureChatExtensionsOptions resulting in the Ai Agent doesn't get the chance to do RAG. 
            Thus, this demo only focuses on using AzureChatExtensionsOptions to retrieve information from AI Search.
         */
        public static async Task ExecuteDemo()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string modelId = appSettings["aoai_modelId"] ?? string.Empty;
            string endpoint = appSettings["aoai_endpoint"] ?? string.Empty;
            string apiKey = appSettings["aoai_apiKey"] ?? string.Empty;
            string AISearchEndpoint = appSettings["aisearch_endpoint"] ?? string.Empty;
            string AISearchKey = appSettings["aisearch_apiKey"] ?? string.Empty;
            string AISearchIndexName = "integratedvectorization" ?? string.Empty;

            // Create kernel
            IKernelBuilder builder = Kernel.CreateBuilder();

            // Create a kernel with Azure OpenAI chat completion
            builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            // Build the kernel
            Kernel kernel = builder.Build();

            // Retrieve the chat completion service
            var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();

            // Add a plugin (the LightsPlugin class is defined below)
            //kernel.Plugins.AddFromType<LightsPlugin>("Lights");

            //optional
            var azureSearchExtensionConfiguration = new AzureSearchChatExtensionConfiguration
            {
                SearchEndpoint = new Uri(AISearchEndpoint),
                Authentication = new OnYourDataApiKeyAuthenticationOptions(AISearchKey),
                IndexName = AISearchIndexName
            };


            // Enable planning
            // Create an instance of OpenAIPromptExecutionSettings
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                // Set AzureChatExtensionsOptions
                AzureChatExtensionsOptions = new AzureChatExtensionsOptions
                {
                    Extensions = { azureSearchExtensionConfiguration }
                }
                //apparently RAG and function calling is not working well together. seems like tool will go first, and not utilize the azure search extention.
                //,
                //// Set ToolCallBehavior
                //ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };


            // Create chat history
            var history = new ChatHistory();

            // Initiate a back-and-forth chat
            string userInput = string.Empty;

            while (userInput is not null)
            {
                // Collect user input
                Console.Write("User > ");
                userInput = Console.ReadLine();

                if (userInput != string.Empty)
                {
                    // Add user input
                    history.AddUserMessage(userInput);

                    // Get the response from the AI
                    var result = await chatCompletionService.GetChatMessageContentAsync(
                        history,
                        executionSettings: openAIPromptExecutionSettings,
                        kernel: kernel
                    );

                    // Print the results
                    Console.WriteLine("Assistant > " + result);

                    // Add the message from the agent to the chat history
                    history.AddMessage(result.Role, result.Content ?? string.Empty); //if result.Content is null => string.Empty

                }
            }
        }
        
    }
}
