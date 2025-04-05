﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ClientModel.Primitives;
using System.Linq;
using System.Net;

const string ApiKeyName = "GEMINI_API_KEY";
Console.WriteLine("Hello, World!");

var envVar = Environment.GetEnvironmentVariable(ApiKeyName);
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName);
var apiEndpoint = "";
var modelId = "gemini-2.5-pro-exp-03-25";

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var builder = Kernel.CreateBuilder().AddGoogleAIGeminiChatCompletion(modelId, apiKey);
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
};

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
GeminiPromptExecutionSettings geminiPromptExecutionSettings = new()
{
    //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
};
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

PromptExecutionSettings promptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()    
};

var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: geminiPromptExecutionSettings, //openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);

