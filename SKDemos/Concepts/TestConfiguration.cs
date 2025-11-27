// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.CompilerServices;

namespace Concepts;

public sealed class TestConfiguration
{
    private readonly IConfigurationRoot _configRoot;
    private static TestConfiguration? s_instance;

    private TestConfiguration(IConfigurationRoot configRoot)
    {
        this._configRoot = configRoot;
    }

    public static void Initialize(IConfigurationRoot configRoot)
    {
        s_instance = new TestConfiguration(configRoot);
    }

    public static IConfigurationRoot? ConfigurationRoot => s_instance?._configRoot;

    public static GoogleAIConfig GoogleAI => LoadSection<GoogleAIConfig>();

    public static VertexAIConfig VertexAI => LoadSection<VertexAIConfig>();

    public static IConfigurationSection GetSection(string caller)
    {
        return s_instance?._configRoot.GetSection(caller) ??
               throw new InvalidOperationException($"Configuration section '{caller}' not found.");
    }

    private static T LoadSection<T>([CallerMemberName] string? caller = null)
    {
        if (s_instance is null)
        {
            throw new InvalidOperationException(
                "TestConfiguration must be initialized with a call to Initialize(IConfigurationRoot) before accessing configuration values.");
        }

        if (string.IsNullOrEmpty(caller))
        {
            throw new ArgumentNullException(nameof(caller));
        }

        return s_instance._configRoot.GetSection(caller).Get<T>() ??
               throw new InvalidOperationException($"Configuration section '{caller}' not found.");
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    public class GoogleAIConfig
    {
        public string ApiKey { get; set; }
        public string EmbeddingModelId { get; set; }
        public GeminiConfig Gemini { get; set; }

        public class GeminiConfig
        {
            public string ModelId { get; set; }
        }
    }

    public class VertexAIConfig
    {
        public string? BearerKey { get; set; }
        public string EmbeddingModelId { get; set; }
        public string Location { get; set; }
        public string ProjectId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public GeminiConfig Gemini { get; set; }

        public class GeminiConfig
        {
            public string ModelId { get; set; }
        }
    }
}
