// Copyright (c) Microsoft. All rights reserved.

using Google.Apis.Auth.OAuth2;
using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Concepts.ChatCompletion;

/// <summary>
/// These examples demonstrate different ways of using chat completion with Google VertexAI and GoogleAI APIs.
/// </summary>
public sealed class Google_GeminiLegacyChatCompletionService(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task UsingGoogleGenAI()
    {
        Console.WriteLine("============= Google GenAI - Gemini Chat Completion =============");

        string geminiApiKey = TestConfiguration.GoogleAI.ApiKey;
        string geminiModelId = TestConfiguration.GoogleAI.Gemini.ModelId;

        if (geminiApiKey is null || geminiModelId is null)
        {
            Console.WriteLine("Gemini credentials not found. Skipping example.");
            return;
        }

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(new Client(vertexAI: false, apiKey: geminiApiKey).AsIChatClient(geminiModelId).AsChatCompletionService());
        Kernel kernel = builder.Build();

        await ProcessChatCompletionServiceAsync(kernel);
    }

    [Fact]
    public async Task UsingGoogleVertexAI()
    {
        Console.WriteLine("============= Vertex AI - Gemini Chat Completion =============");

        Assert.NotNull(TestConfiguration.VertexAI.Location);
        Assert.NotNull(TestConfiguration.VertexAI.ProjectId);
        Assert.NotNull(TestConfiguration.VertexAI.Gemini.ModelId);

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddChatClient(new Client(
            vertexAI: true,
            credential: await GetCredentialAsync(),
            location: TestConfiguration.VertexAI.Location,
            project: TestConfiguration.VertexAI.ProjectId)
                .AsIChatClient(TestConfiguration.VertexAI.Gemini.ModelId));

        Kernel kernel = builder.Build();

        async ValueTask<UserCredential?> GetCredentialAsync()
        {
            if (TestConfiguration.VertexAI.ClientId is null || TestConfiguration.VertexAI.ClientSecret is null)
            {
                return null;
            }

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = TestConfiguration.VertexAI.ClientId,
                    ClientSecret = TestConfiguration.VertexAI.ClientSecret
                },
                ["https://www.googleapis.com/auth/cloud-platform"],
                "user",
                CancellationToken.None);

            var userCredential = await credential.WaitAsync(CancellationToken.None);

            return userCredential;
        }

        await ProcessChatCompletionServiceAsync(kernel);
    }

    private async Task ProcessChatCompletionServiceAsync(Kernel kernel)
    {
        var chatHistory = new ChatHistory("You are an expert in the tool shop.");
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        // First user message
        chatHistory.AddUserMessage("Hi, I'm looking for new power tools, any suggestion?");
        await MessageOutputAsync(chatHistory);

        // First assistant message
        var reply = await chat.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);

        // Second user message
        chatHistory.AddUserMessage("I'm looking for a drill, a screwdriver and a hammer.");
        await MessageOutputAsync(chatHistory);

        // Second assistant message
        reply = await chat.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);
    }

    /// <summary>
    /// Outputs the last message of the chat history
    /// </summary>
    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
