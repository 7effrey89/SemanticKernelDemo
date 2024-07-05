using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernel
{
    internal class Demo_MultiAgent_Chatbot
    {
        /*
         * Required nuget:
            dotnet add package Microsoft.SemanticKernel.Agents.Abstractions --version 1.14.1-alpha
            dotnet add package Microsoft.SemanticKernel.Agents.Core --version 1.14.1-alpha
         https://medium.com/@akshaykokane09/step-by-step-guide-to-develop-ai-multi-agent-system-using-microsoft-semantic-kernel-and-gpt-4o-f5991af40ea6
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

            kernel.Plugins.AddFromType<LightsPlugin>("Lights");

            string ProductManager = """
                You are a retailer, and you have the responsibility of finding the next big blockbuster game for children for your store. 
                You will incorporate your feedback into the information received (you wil update the pitch with your feedback).
                You will only approve the final product if it meets the requirements of all stakeholders.
            """;

            string toys_manufactor_consultant = """
               You are a toys manufactorer consultant and provides useful considerations for product development and logistics considerations.
               You will provide tangible feedback on several perspectives of how to improve the product e.g. from a safty, playability, longevity, educational perspective, packaging design, materials, pricing, and more.
               When suggestions are made also include an example.
               You will provide 2 sections: 
               1) Your feedback, considerations, concerns
               2)You will incorporate your feedback into the information received (you wil update the pitch with your feedback).
               You will only approve once all important aspects of your concerns are specifically covered.
            """;
            // let's refine and expand on a few critical elements:
            //Enhancements

            string sustainability_manager = """
                You are the sustainability manager of the toy store and you need to minimize the amount of plastics used in your products to be aligned with the company's green policy
                You will provide tangible feedback on how to make the product and packing more sustainable and environmentally friendly.
                When suggestions are made also include an example.
                You will provide 2 sections: 
                1) Your feedback, considerations, concerns
                2)You will incorporate your feedback into the information received (you wil update the pitch with your feedback).
                You will only approve once all important aspects of your concerns are specifically covered.
            """;

            string parents = """
                You are a parent and you want to buy a toy for your child. However, as a parent you might also have some safty and educational concerns, and with a tight budget you want to make sure that the toy is worth the money.
                You are concerned about children's addiction to screens
                When suggestions are made also include an example.
                You will provide 2 sections: 
                1) Your feedback, considerations, concerns
                2)You will incorporate your feedback into the information received (you wil update the pitch with your feedback).
                You will only approve once all important aspects of your concerns are specifically covered.
            """;


#pragma warning disable SKEXP0110, SKEXP0001 // Rethrow to preserve stack details

            ChatCompletionAgent ProductManagerAgent =
                       new()
                       {
                           Instructions = ProductManager,
                           Name = "ProductManager",
                           Kernel = kernel
                       };

            ChatCompletionAgent ToyManufactorConsultantAgent =
                       new()
                       {
                           Instructions = toys_manufactor_consultant,
                           Name = "toys_manufactor_consultant",
                           Kernel = kernel
                       };

            ChatCompletionAgent SustainabilityManagerAgent =
                       new()
                       {
                           Instructions = sustainability_manager,
                           Name = "sustainability_manager",
                           Kernel = kernel
                       };

            ChatCompletionAgent ParentsAgent =
                       new()
                       {
                           Instructions = parents,
                           Name = "parents",
                           Kernel = kernel
                       };
            AgentGroupChat chat =
            new(ProductManagerAgent, ToyManufactorConsultantAgent, SustainabilityManagerAgent, ParentsAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                Agents = [ProductManagerAgent],
                                MaximumIterations = 6,
                            }
                    }
            };

            // Invoke chat and display messages.
            string input = """
                Provide a pitch for a physical game for children. I need to find a good idea for a game that will be a hit in the market. 
                Target group is children between 5-7 years old.
                """;

            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
            Console.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var content in chat.InvokeAsync())
            {
                Console.WriteLine($"-----------------------------------------------------------");
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            }
        }
        private sealed class ApprovalTerminationStrategy : TerminationStrategy
        {
            // Terminate when the final message contains the term "approve"
            protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
                => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
        }

    }
}
