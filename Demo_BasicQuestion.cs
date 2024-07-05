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
    internal class Demo_BasicQuestion
    {
        /*
            This demo uses shows the basics of how to use the Azure OpenAI chat completion feature in the Semantic Kernel.
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

            var result = await kernel.InvokePromptAsync("who let the dogs out?");
            Console.WriteLine(result);

        }
        
    }
}
