# Semantic Kernel
Semantic Kernel is a lightweight, open-source development kit designed to simplify the integration of advanced AI models into your applications. Whether you’re working with C#, Python, or Java, Semantic Kernel provides a robust framework to build AI agents that can interact with various large language models (LLMs) and perform complex tasks efficiently.

## Key Features:
**Enterprise Ready**: Semantic Kernel is built to support enterprise-grade solutions, ensuring reliability and scalability. It includes features like telemetry support and security enhancements, making it suitable for large-scale deployments.  
**Automating Business Processes**: By combining AI models with existing APIs, Semantic Kernel can automate a wide range of business processes. It acts as middleware, translating AI model requests into function calls and returning the results seamlessly.  
**Modular and Extensible**: The framework is designed to be highly modular, allowing developers to add their existing code as plugins. This flexibility maximizes your investment in existing technologies and enables easy integration of new AI services.  
**Future-Proof**: Semantic Kernel is designed to evolve with technological advancements. When new AI models are released, you can integrate them without needing to rewrite your entire codebase, ensuring your solutions remain cutting-edge.  

## Getting Started:
To start using Semantic Kernel, you can follow the quick start guide available on the Microsoft Learn website:
https://learn.microsoft.com/en-us/semantic-kernel/overview/

You can also have a view of some of the code samples I've made to accelerate your AI journey.

## Prerequisite for RAG solutions
Some of the samples utilizes a search index in AI Search to accomplish RAG. RAG is a common technique to bring specialized information from an knowledge database.
In this demo I have followed the guide below to create such index that consist of information from the documents in the /InternalDocuments folder. 
https://learn.microsoft.com/en-us/azure/search/search-get-started-portal-import-vectors?tabs=sample-data-storage%2Cmodel-aoai#start-the-wizard

## AI answer:
![image](https://github.com/7effrey89/SemanticKernelDemo/assets/30802073/94c1b4c8-e3af-49c9-bc59-4744c7b5a991)  
[Demo_BasicQuestion.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_BasicQuestion.cs)  
Answers a question based on general knowledge it was trained on. Nothing more.

## Chatbot experience:
![image](https://github.com/7effrey89/SemanticKernelDemo/assets/30802073/6a3f27ee-6a20-4cc6-81f2-2f51be3912aa)  
[Demo_ChatBot.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_ChatBot.cs)  
Chat experience with general knowledge. Chat conversation is maintained that allows users to follow up on a response from the Ai agent.

## Ask your document - RAG:
![image](https://github.com/7effrey89/SemanticKernelDemo/assets/30802073/797233a5-7220-4d97-8945-11c0d842d1d5)  
[Demo_AISearch_RAG_Only.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_AISearch_RAG_Only.cs)  
Chat experience with general knowledge + specialized knowledge. 
Chat conversation is maintained that allows users to follow up on a response from the Ai agent.
Ai agent can retrieve specialized information from knowledge database e.g. internal documents in Azure AI search to serve user queries. This is also known as RAG.

## Copilot experience: 
![image](https://github.com/7effrey89/SemanticKernelDemo/assets/30802073/920b9eca-6546-4045-990c-648afe9f3fa7)
[Demo_Plugins.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_Plugins.cs)  
Enables a chat experience where users can ask AI to accomplish complex tasks building on chat, RAG, and now Plugins.
Plugins enables the AI to invoke functionality inside and outside this application to complete tasks.

Example: Sending below prompt to semantic kernel trigger it to make a plan of how to solve this query using the plugins at its disposal:

>5 months ago Donald went on vacation. What is the weather like right now for that location?

Typical plan made during the demo:
1) AI will invoke my AI Search plugin to find the location of Donald's vacation
2) Then invoke the GPS plugin to translate the gathered location to GPS coordinates (using an external API service) needed by the Weather plugin
3) Then it will invoke the Weather plugin call external service to get the current weather for the GPS coordinates

## Multi-Agent chatbot with plugins experience: 
![image](https://github.com/7effrey89/SemanticKernelDemo/assets/30802073/6781a1d0-6ec9-44eb-b871-3803b93a25b3)  
[Demo_MultiAgent_Plugins.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_MultiAgent_Plugins.cs)  
Same as above, but each AI agent has access to different plugins.
This enables the AI agents to have different capabilities and specialized knowledge.
Can enable automation of complex tasks that require multiple steps and different types of knowledge.

## Multi-Agent chatbot without plugins experience: 
[Demo_MultiAgent_Chatbot.cs](https://github.com/7effrey89/SemanticKernelDemo/blob/master/Demo_MultiAgent_Chatbot.cs)  
Enables a chat experience where multiple AI agents can collaborate to solve a complex task.
In this demo several AI agents discusses and collaborates to find the next big game product
