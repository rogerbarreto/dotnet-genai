// Copyright (c) Microsoft. All rights reserved.

using Google.Apis.Auth.OAuth2;
using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Concepts.ChatCompletion;

/// <summary>
/// These examples demonstrate different ways of using chat completion with Google VertexAI and GoogleAI APIs.
/// </summary>
public sealed class Google_GeminiChatClient(ITestOutputHelper output) : BaseTest(output)
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
        builder.Services.AddChatClient(new Client(vertexAI: false, apiKey: geminiApiKey).AsIChatClient(geminiModelId));
        Kernel kernel = builder.Build();

        await ProcessChatClientAsync(kernel);
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
            // If no client id/secret provided, will attempt to use the default credentials setup in the environment
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

        await ProcessChatClientAsync(kernel);
    }

    private async Task ProcessChatClientAsync(Kernel kernel)
    {
        List<ChatMessage> chatHistory = [new ChatMessage(ChatRole.System, "You are an expert in the tool shop.")];
        var chat = kernel.GetRequiredService<IChatClient>();

        // First user message
        chatHistory.Add(new ChatMessage(ChatRole.User, "Hi, I'm looking for new power tools, any suggestion?"));
        await MessageOutputAsync(chatHistory);

        // First assistant message
        var reply = await chat.GetResponseAsync(chatHistory);
        chatHistory.AddRange(reply.Messages);
        await MessageOutputAsync(chatHistory);

        // Second user message
        chatHistory.Add(new ChatMessage(ChatRole.User, "I'm looking for a drill, a screwdriver and a hammer."));
        await MessageOutputAsync(chatHistory);

        // Second assistant message
        reply = await chat.GetResponseAsync(chatHistory);
        chatHistory.AddRange(reply.Messages);
        await MessageOutputAsync(chatHistory);
    }

    /// <summary>
    /// Outputs the last message of the chat history
    /// </summary>
    private Task MessageOutputAsync(IList<ChatMessage> chatHistory)
    {
        var message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Text}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
