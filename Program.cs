using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernel.Plugins;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace SemanticKernel
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            /* Simple demo, AI answering your question */
            //await Demo_BasicQuestion.ExecuteDemo(); //uncomment me to try me

            /* Chatbot experience (ungrounded) */
            //await Demo_ChatBot.ExecuteDemo(); //uncomment me to try me

            /* 
             * Chatbot experience grounded with RAG 
             * Enables a chat experience where users can ask questions about information that AI wasn't train on. 
             * E.g. internal documentation, domain knowledge, and other information unavailable to the public.
             */
            //await Demo_AISearch_RAG_Only.ExecuteDemo(); //uncomment me to try me

            /* 
             * Copilot experience  
             * Enables a chat experience where users can ask AI to accomplish complex tasks building on chat, RAG, and now Plugins.
             * Plugins enables the AI to invoke functionality inside and outside this application to complete tasks.
             * Example:
             *  5 months ago Donald went on vacation. What is the weather like right now for that location?
             *  This question will trigger AI to make a plan of how to solve this query using the plugins at its disposal:
             *      1) AI will invoke my AI Search plugin to find the location of Donald's vacation
             *      2) Then invoke the GPS plugin to translate the gathered location to GPS coordinates (using an external API service) needed by the Weather plugin
             *      3) Then it will invoke the Weather plugin call external service to get the current weather for the GPS coordinates
             */
            await Demo_Plugins.ExecuteDemo(); //uncomment me to try me

            /* 
             * Multi-agent chatbot experience
             * Enables a chat experience where multiple AI agents can collaborate to solve a complex task.
             * In this demo several AI agents discusses and collaborates to find the next big game product.
             */
            //await Demo_MultiAgent_Chatbot.ExecuteDemo(); //uncomment me to try me


            /* 
             * Same as above, but each AI agent has access to different plugins.
             * This enables the AI agents to have different capabilities and specialized knowledge.
             * Can enable automation of complex tasks that require multiple steps and different types of knowledge.
             */
            //await Demo_MultiAgent_Plugins.ExecuteDemo();


            /* 
             * Below are methods to test some of the plugins in the Plugins folder.
             * They were made to for testing and debugging purposes.
             */
            //await CityToGpsPlugin.test_LatLong_REST_API();
            //await WeatherServicePlugin.test_Weather_REST_API();
        }
    }
}
