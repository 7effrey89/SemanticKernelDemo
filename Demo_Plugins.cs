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
using Microsoft.SemanticKernel.Embeddings;
using Azure.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Reflection.Metadata;
using SemanticKernel.Plugins;

namespace SemanticKernel
{
    internal class Demo_Plugins
    {
        /*
            This demo shows how to use plugins in Semantic Kernel to call functionality in the C# application, external services through REST APIs, AI search for document retrieval.
            The plugins are defined in the Plugins folder.
            When interacting with the AI agent, it will plan the steps required to complete the task and possibly invoke the listed plugins to complete the task.
         */
        public static async Task ExecuteDemo()
        {
            bool doChatStreaming = true; //Control whether to stream the chat or not

            var appSettings = ConfigurationManager.AppSettings;
            string modelId = appSettings["aoai_modelId"] ?? string.Empty;
            string endpoint = appSettings["aoai_endpoint"] ?? string.Empty;
            string apiKey = appSettings["aoai_apiKey"] ?? string.Empty;

            // Create kernel
            IKernelBuilder builder = Kernel.CreateBuilder();

            //builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

            // Create a kernel with Azure OpenAI chat completion
            builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            // Build the kernel
            Kernel kernel = builder.Build();

            // Retrieve the chat completion service
            IChatCompletionService chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();

            // Add a plugin (the LightsPlugin class is defined below)
            kernel.Plugins.AddFromType<LightsPlugin>("Lights");
            kernel.Plugins.AddFromType<AzureAISearchPlugin>("Search");
            kernel.Plugins.AddFromType<CityToGpsPlugin>("CityGps");
            kernel.Plugins.AddFromType<WeatherServicePlugin>("Weather");

            // Enable planning
            // Create an instance of OpenAIPromptExecutionSettings
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };


            // Create chat history
            var history = new ChatHistory("""
            You are a friendly assistant. You will complete required steps
            and request approval before taking any consequential actions.If the user doesn't provide
            enough information for you to complete a task, you will keep asking questions until you have
            enough information to complete the task.
            """
            );

            //Add something to the chat history
            history.AddUserMessage("What is the date today?");
            history.AddSystemMessage(DateTime.Now.ToString("MM/dd/yyyy h:mm tt"));

            // Initiate a back-and-forth chat
            string userInput;

            while (true)
            {
                // Collect user input
                Console.Write("User > ");
                userInput = Console.ReadLine() ?? string.Empty;

                if (userInput != string.Empty)
                {
                    //Diagnostic command to show chat history. Type "chathistory" to see the chat history.
                    if (userInput.ToLower() == "chathistory")
                    {
                        Console.WriteLine("=========================");
                        int i = 0;
                        foreach (var item in history)
                        {
                            Console.WriteLine("Chat History Item:" + i);
                            Console.WriteLine(item.Content);
                            i++;
                        }
                        Console.WriteLine("=========================");
                    }
                    else
                    {
                        // Add user input
                        history.AddUserMessage(userInput);

                        // Get the response from the AI
                        Task<ChatResponse> result = MsgDelivery(doChatStreaming, history, chatCompletionService, openAIPromptExecutionSettings, kernel);

                        // Add the message from the agent to the chat history
                        history.AddMessage(result.Result.role, result.Result.response);
                    }

                }
            }
        }
        ///<summary>
        ///<para>Method to choose either streaming or Chatcompletion</para>
        ///</summary>
        private static async Task<ChatResponse> MsgDelivery(bool doStreaming, ChatHistory history, IChatCompletionService chatCompletionService, OpenAIPromptExecutionSettings openAIPromptExecutionSettings, Kernel kernel)
        {
            if (doStreaming)
            {
                return await ChatStreaming(history, chatCompletionService, openAIPromptExecutionSettings, kernel);
            }
            else
            {
                return await ChatCompletion(history, chatCompletionService, openAIPromptExecutionSettings, kernel);
            }
        }
        ///<summary>
        ///<para>Method for sending the response once completed instead of streaming it</para>
        ///</summary>
        private static async Task<ChatResponse> ChatCompletion(ChatHistory history, IChatCompletionService chatCompletionService, OpenAIPromptExecutionSettings openAIPromptExecutionSettings, Kernel kernel)
        {
            // Get the response from the AI
            ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );

            // Print the results
            Console.WriteLine("Assistant > " + result);

            ChatResponse response = new ChatResponse(result.Role, result.Content ?? string.Empty);

            return response;
        }
        ///<summary>
        ///<para>Method for streaming the response</para>
        ///</summary>
        private static async Task<ChatResponse> ChatStreaming(ChatHistory history, IChatCompletionService chatCompletionService, OpenAIPromptExecutionSettings openAIPromptExecutionSettings, Kernel kernel)
        {
            // Get the response from the AI
            IAsyncEnumerable<StreamingChatMessageContent> result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );

            Console.Write("Assistant > ");

            // Stream the results
            string fullMessage = "";
            await foreach (var content in result)
            {
                Console.Write(content.Content);
                fullMessage += content.Content;
            }
            Console.WriteLine();

            ChatResponse response = new ChatResponse(AuthorRole.System, fullMessage ?? string.Empty);

            return response;
        }
        private class ChatResponse
        {
            public string response { get; set; }
            public AuthorRole role { get; set; }

            public ChatResponse(AuthorRole role, string response)
            {
                this.response = response;
                this.role = role;
            }
        }
    }
}
