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
    internal class Demo_ChatBot
    {
        /*
            This demo uses shows the basics of how to use the Azure OpenAI chat completion feature in the Semantic Kernel.
            //https://learn.microsoft.com/en-us/semantic-kernel/concepts/agents?pivots=programming-language-csharp
         */
        public static async Task ExecuteDemo()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string modelId = appSettings["aoai_modelId"] ?? string.Empty;
            string endpoint = appSettings["aoai_endpoint"] ?? string.Empty;
            string apiKey = appSettings["aoai_apiKey"] ?? string.Empty;

            // Create kernel
            IKernelBuilder builder = Kernel.CreateBuilder();

            // Create a kernel with Azure OpenAI chat completion
            builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            // Build the kernel
            Kernel kernel = builder.Build();

            // Retrieve the chat completion service
            var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();

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
