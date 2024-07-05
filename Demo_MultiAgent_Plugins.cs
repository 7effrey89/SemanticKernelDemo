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
using SemanticKernel.Plugins;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernel
{
    internal class Demo_MultiAgent_Plugins
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
            Kernel kernelAgent1 = builder.Build();
            Kernel kernelAgent2 = builder.Build();

            kernelAgent1.Plugins.AddFromType<AzureAISearchPlugin>("Search");
            kernelAgent2.Plugins.AddFromType<CityToGpsPlugin>("CityGps");
            kernelAgent2.Plugins.AddFromType<WeatherServicePlugin>("Weather");

            // Create an instance of OpenAIPromptExecutionSettings
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            string HR_Manager = """
                You are a helpful assistant. You have to search in the internal hr database for the employee details, travel and vacation plans.
            """;

            string Master_Of_Tools = """
               You are a helpful assistant. You have a collection of tools at your disposal to look up generic information.
            """;

            string Moderator = """
               You are a helpful assistant. You evaluate if the information provided from other AI agents answers the question of the user. 
               If it satisfies the user, you announce 'approve'. If not, provide actions and feedback on how to reach the desired outcome.
            """;

#pragma warning disable SKEXP0110, SKEXP0001 // Rethrow to preserve stack details
            //https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.chatcompletionagent?view=semantic-kernel-dotnet
            ChatCompletionAgent HR_ManagerAgent =
                       new()
                       {
                           Instructions = HR_Manager,
                           Name = "HR_Manager",
                           Kernel = kernelAgent1,
                           ExecutionSettings = openAIPromptExecutionSettings
                       };

            ChatCompletionAgent ToolMasterAgent =
                       new()
                       {
                           Instructions = Master_Of_Tools,
                           Name = "Tool_Master",
                           Kernel = kernelAgent2,
                           ExecutionSettings = openAIPromptExecutionSettings
                       };

            ChatCompletionAgent ModeratorAgent =
                       new()
                       {
                           Instructions = Moderator,
                           Name = "Moderator",
                           Kernel = kernel
                       };

            AgentGroupChat chat =
            new(HR_ManagerAgent, ToolMasterAgent, ModeratorAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                Agents = [ModeratorAgent],
                                MaximumIterations = 6,
                            }
                    }
            };
            string input = $"{getMonthsDifferenceIwasAtVacation} months ago Donald went on vacation. What is the weather like right now for that location";

            //Add something to the chat history
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, "What is the date today?"));
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.Assistant, DateTime.Now.ToString("MM/dd/yyyy h:mm tt")));

            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
            Console.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var content in chat.InvokeAsync())
            {
                Console.WriteLine($"-----------------------------------------------------------");
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            }
        }
        private static int getMonthsDifferenceIwasAtVacation()
        {
            DateTime startDate = new DateTime(2024, 2, 1);
            DateTime currentDate = DateTime.Now;

            int monthsDifference = ((currentDate.Year - startDate.Year) * 12) + currentDate.Month - startDate.Month;
            return monthsDifference;
        }
        private sealed class ApprovalTerminationStrategy : TerminationStrategy
        {
            // Terminate when the final message contains the term "approve"
            protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
                => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
        }

    }
}
