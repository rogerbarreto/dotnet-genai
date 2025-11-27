using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Google.GenAI.Tests;

[TestClass]
public class GoogleGenAIExtensionsTest
{
  [TestMethod]
  public void AsIChatClient_NullModels_ThrowsArgumentNullException()
  {
    Models? models = null;
    Assert.ThrowsException<ArgumentNullException>(() => models!.AsIChatClient("gemini-2.5-pro"));
  }

  [TestMethod]
  public void AsIChatClient_NullClient_ThrowsArgumentNullException()
  {
    Client? client = null;
    Assert.ThrowsException<ArgumentNullException>(() => client!.AsIChatClient("gemini-2.5-pro"));
  }

  [TestMethod]
  public void AsIChatClient_ValidModels_ReturnsNonNull()
  {
    var models = new Models(new MockApiClient("", ""));
    
    Assert.IsNotNull(models.AsIChatClient("gemini-2.5-pro"));
  }

  [TestMethod]
  public void IChatClient_GetService_Models_ReturnsUnderlyingModels()
  {
    var models = new Models(new MockApiClient("", ""));
    var client = models.AsIChatClient("gemini-2.5-pro");
    
    Assert.AreSame(models, client.GetService<Models>());
    Assert.IsNull(client.GetService<Client>());
  }

  [TestMethod]
  public void IChatClient_GetService_Client_ReturnsUnderlyingClient()
  {
    var client = new Client(apiKey: "fake-api-key");
    var chatClient = client.AsIChatClient("gemini-2.5-pro");
    Assert.AreSame(client, chatClient.GetService<Client>());
    Assert.AreSame(client.Models, chatClient.GetService<Models>());
  }

  [TestMethod]
  public void IChatClient_GetService_NullServiceType_ThrowsArgumentNullException()
  {
    var client = CreateChatClient("", "");
    
    Assert.ThrowsException<ArgumentNullException>(() => client.GetService(null!));
  }

  [TestMethod]
  public void AsIImageGenerator_WithModels_ReturnsImageGenerator()
  {
    var models = new Models(new MockApiClient("", ""));
    
    Assert.IsNotNull(models.AsIImageGenerator("imagen-3.0-generate-001"));
  }

  [TestMethod]
  public void AsIImageGenerator_WithClient_ReturnsImageGenerator()
  {
    var client = new Client(apiKey: "fake-api-key");
    
    Assert.IsNotNull(client.AsIImageGenerator("imagen-3.0-generate-001"));
  }

  [TestMethod]
  public void IImageGenerator_GetService_ImageGeneratorMetadata_ReturnsValidMetadata()
  {
    var models = new Models(new MockApiClient("", ""));
    var imageGenerator = models.AsIImageGenerator("imagen-3.0-generate-001");
    
    var metadata = imageGenerator.GetService<ImageGeneratorMetadata>();
    
    Assert.IsNotNull(metadata);
    Assert.IsNotNull(metadata.ProviderName);
    Assert.AreEqual("gcp.gen_ai", metadata.ProviderName);
    Assert.IsNotNull(metadata.ProviderUri);
    Assert.IsInstanceOfType(metadata.ProviderUri, typeof(Uri));
  }

  [TestMethod]
  public void IImageGenerator_GetService_Models_ReturnsUnderlyingModels()
  {
    var models = new Models(new MockApiClient("", ""));
    var imageGenerator = models.AsIImageGenerator("imagen-3.0-generate-001");
    
    var retrievedModels = imageGenerator.GetService<Models>();
    Assert.IsNotNull(retrievedModels);
    Assert.AreSame(models, retrievedModels);

    Assert.IsNull(imageGenerator.GetService<Client>());
  }

  [TestMethod]
  public void IImageGenerator_GetService_Client_ReturnsUnderlyingClient()
  {
    var client = new Client(apiKey: "fake-api-key");
    var imageGenerator = client.AsIImageGenerator("imagen-3.0-generate-001");
    
    Assert.AreSame(client, imageGenerator.GetService<Client>());
    Assert.AreSame(client.Models, imageGenerator.GetService<Models>());
  }

  [TestMethod]
  public void IImageGenerator_GetService_NullServiceType_ThrowsArgumentNullException()
  {
    var models = new Models(new MockApiClient("", ""));
    var imageGenerator = models.AsIImageGenerator("imagen-3.0-generate-001");
    
    Assert.ThrowsException<ArgumentNullException>(() => imageGenerator.GetService(null!));
  }

  [TestMethod]
  public void AsIImageGenerator_NullModels_ThrowsArgumentNullException()
  {
    Models? models = null;
    Assert.ThrowsException<ArgumentNullException>(() => models!.AsIImageGenerator("imagen-3.0-generate-001"));
  }

  [TestMethod]
  public void AsIImageGenerator_NullClient_ThrowsArgumentNullException()
  {
    Client? client = null;
    Assert.ThrowsException<ArgumentNullException>(() => client!.AsIImageGenerator("imagen-3.0-generate-001"));
  }

  [TestMethod]
  public async Task IChatClient_BasicRequestResponse()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hello, world!"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hello there! The classic first words of any new program.\n\nHow can I help you today?"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 20,
          "totalTokenCount": 1024,
          "promptTokensDetails": [
            {
              "modality": "TEXT",
              "tokenCount": 5
            }
          ],
          "thoughtsTokenCount": 999
        },
        "modelVersion": "gemini-2.5-pro",
        "responseId": "UTQBaY2BNLXg_uMP9Y_W8Q8"
      }
      """);

    var response = await client.GetResponseAsync("Hello, world!");
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.AreEqual("Hello there! The classic first words of any new program.\n\nHow can I help you today?", response.Messages[0].Text);
    Assert.AreEqual(1, response.Messages[0].Contents.Count);
    Assert.IsInstanceOfType(response.Messages[0].Contents[0], typeof(TextContent));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(5, response.Usage.InputTokenCount);
    Assert.AreEqual(20, response.Usage.OutputTokenCount);
    Assert.AreEqual(1024, response.Usage.TotalTokenCount);
    Assert.AreEqual("gemini-2.5-pro", response.ModelId);
    Assert.IsNotNull(response.RawRepresentation);
    Assert.AreEqual("UTQBaY2BNLXg_uMP9Y_W8Q8", ((GenerateContentResponse)response.RawRepresentation).ResponseId);
  }

  [TestMethod]
  public async Task IChatClient_MultipleMessagesConversation()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What is 2+2?"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "4"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "And what is 3+3?"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "6"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 18,
          "candidatesTokenCount": 1,
          "totalTokenCount": 19
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "What is 2+2?"),
      new(ChatRole.Assistant, "4"),
      new(ChatRole.User, "And what is 3+3?")
    ];

    var response = await client.GetResponseAsync(messages);
    
    Assert.IsNotNull(response);
    Assert.AreEqual("6", response.Messages[0].Text);
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
  }

  [TestMethod]
  public async Task IChatClient_WithSystemInstructions()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What should I do?"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {},
        "systemInstruction": {
          "parts": [
            {
              "text": "You are a helpful pirate. Always respond like a pirate."
            }
          ]
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Ahoy there, matey!  What be troublin' ye?  Tell ol' Captain Helpful what needs doin', and I'll steer ye in the right direction, or me name ain't... well, it ain't really Captain Helpful, but ye get the idea!  Spit it out, scallywag! \n"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 24,
          "candidatesTokenCount": 62,
          "totalTokenCount": 86
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.System, "You are a helpful pirate. Always respond like a pirate."),
      new(ChatRole.User, "What should I do?")
    ];

    var response = await client.GetResponseAsync(messages);
    
    Assert.IsNotNull(response);
    Assert.IsTrue(response.Messages[0].Text.Contains("matey") || response.Messages[0].Text.Contains("Ahoy"));
  }

  [TestMethod]
  public async Task IChatClient_WithTemperatureAndTopP()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Say hello"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "temperature": 0.1,
          "topP": 0.95
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hello! 👋 \n"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 4,
          "candidatesTokenCount": 5,
          "totalTokenCount": 9
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      Temperature = 0.1f,
      TopP = 0.95f
    };

    var response = await client.GetResponseAsync("Say hello", options);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_WithMaxTokens()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Write a long story"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "maxOutputTokens": 10
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Once upon a time, in a land far, far away"
                }
              ],
              "role": "model"
            },
            "finishReason": "MAX_TOKENS",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 6,
          "candidatesTokenCount": 10,
          "totalTokenCount": 16
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      MaxOutputTokens = 10
    };

    var response = await client.GetResponseAsync("Write a long story", options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(ChatFinishReason.Length, response.FinishReason);
    Assert.AreEqual(10, response.Usage?.OutputTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_WithStopSequences()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Count: 1, 2, 3, 4, 5"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "stopSequences": ["4"]
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Okay, you've started counting: 1, 2, 3,"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 15,
          "candidatesTokenCount": 13,
          "totalTokenCount": 28
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      StopSequences = ["4"]
    };

    var response = await client.GetResponseAsync("Count: 1, 2, 3, 4, 5", options);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(response.Messages[0].Text.Contains('4'));
  }

  [TestMethod]
  public async Task IChatClient_WithTopK()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Pick a number"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "topK": 40
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "7 \n"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 3,
          "totalTokenCount": 8
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      TopK = 40
    };

    var response = await client.GetResponseAsync("Pick a number", options);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_MultipleContentParts()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "First part. "
              },
              {
                "text": "Second part."
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Okay, I received your message in two parts: \"First part. \" and \"Second part.\"\n\nWhat would you like me to do with this information? \n"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 10,
          "candidatesTokenCount": 33,
          "totalTokenCount": 43
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage message = new(ChatRole.User, [
      new TextContent("First part. "),
      new TextContent("Second part.")
    ]);

    var response = await client.GetResponseAsync([message]);
    
    Assert.IsNotNull(response);
    Assert.IsTrue(response.Messages[0].Text.Contains("two parts") || response.Messages[0].Text.Contains("First") && response.Messages[0].Text.Contains("Second"));
  }

  [TestMethod]
  public async Task IChatClient_EmptyMessage()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": ""
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hello! How can I help you today? \n"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 2,
          "candidatesTokenCount": 10,
          "totalTokenCount": 12
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    var response = await client.GetResponseAsync("");
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_ResponseWithMultipleTextParts()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Tell me about AI"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "AI, or Artificial Intelligence, refers to the simulation of human intelligence in machines"
                },
                {
                  "text": " that are programmed to think and learn."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 6,
          "candidatesTokenCount": 25,
          "totalTokenCount": 31
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    var response = await client.GetResponseAsync("Tell me about AI");
    
    Assert.IsNotNull(response);
    var message = response.Messages[0];
    Assert.IsTrue(message.Contents.Count >= 1);
    var combinedText = string.Join("", message.Contents.OfType<TextContent>().Select(c => c.Text));
    Assert.IsTrue(combinedText.Contains("Artificial Intelligence") || combinedText.Contains("AI"));
  }

  [TestMethod]
  public async Task IChatClient_UsageMetadataValidation()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Test"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Test received."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 2,
          "candidatesTokenCount": 4,
          "totalTokenCount": 6
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    var response = await client.GetResponseAsync("Test");
    
    Assert.IsNotNull(response);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(2, response.Usage.InputTokenCount);
    Assert.AreEqual(4, response.Usage.OutputTokenCount);
    Assert.AreEqual(6, response.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_ModelVersionInResponse()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hi"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hi there!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 2,
          "candidatesTokenCount": 4,
          "totalTokenCount": 6
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    var response = await client.GetResponseAsync("Hi");
    
    Assert.IsNotNull(response);
    Assert.AreEqual("gemini-2.5-pro", response.ModelId);
  }

  [TestMethod]
  public async Task IChatClient_WithFunctionTool()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What's the weather?"
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets the current weather",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "location": {
                      "type": "string"
                    }
                  },
                  "required": ["location"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "functionCall": {
                    "name": "get_weather",
                    "args": {
                      "location": "San Francisco"
                    }
                  }
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 10,
          "candidatesTokenCount": 8,
          "totalTokenCount": 18
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      Tools = [AIFunctionFactory.Create((string location) => "sunny", "get_weather", "Gets the current weather")]
    };

    var response = await client.GetResponseAsync("What's the weather?", options);
    
    Assert.IsNotNull(response);
    var functionCall = response.Messages[0].Contents.OfType<FunctionCallContent>().FirstOrDefault();
    Assert.IsNotNull(functionCall);
    Assert.AreEqual("get_weather", functionCall.Name);
  }

  [TestMethod]
  public async Task IChatClient_WithFunctionResult()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "get_weather",
                  "response": {
                    "result": "sunny"
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Based on the weather data, it's sunny!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 12,
          "candidatesTokenCount": 10,
          "totalTokenCount": 22
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage message = new(ChatRole.User, [
      new FunctionResultContent("get_weather", "sunny")
    ]);

    var response = await client.GetResponseAsync([message]);
    
    Assert.IsNotNull(response);
    Assert.IsTrue(response.Messages[0].Text.Contains("sunny"));
  }

  [TestMethod]
  public async Task IChatClient_ResponseWithImageData()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What's in this image?"
              },
              {
                "inlineData": {
                  "mimeType": "image/png",
                  "data": "iVBORw0K"
                }
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "I see a simple image."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 15,
          "candidatesTokenCount": 8,
          "totalTokenCount": 23
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    byte[] imageBytes = Convert.FromBase64String("iVBORw0K");
    ChatMessage message = new(ChatRole.User, [
      new TextContent("What's in this image?"),
      new DataContent(imageBytes, "image/png")
    ]);

    var response = await client.GetResponseAsync([message]);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_ResponseWithImageUri()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Describe this"
              },
              {
                "fileData": {
                  "mimeType": "image/jpeg",
                  "fileUri": "https://example.com/image.jpg"
                }
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "The image shows an example."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 12,
          "candidatesTokenCount": 7,
          "totalTokenCount": 19
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage message = new(ChatRole.User, [
      new TextContent("Describe this"),
      new UriContent(new Uri("https://example.com/image.jpg"), "image/jpeg")
    ]);

    var response = await client.GetResponseAsync([message]);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_WithJsonResponseFormat()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Return JSON"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "responseMimeType": "application/json"
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "{\"result\": \"success\"}"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 8,
          "candidatesTokenCount": 6,
          "totalTokenCount": 14
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      ResponseFormat = ChatResponseFormat.Json
    };

    var response = await client.GetResponseAsync("Return JSON", options);
    
    Assert.IsNotNull(response);
    Assert.IsTrue(response.Messages[0].Text.Contains("result"));
  }

  [TestMethod]
  public async Task IChatClient_LongConversationHistory()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hi"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "Hello"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "How are you?"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "I'm good"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Great"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Glad to hear it!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 20,
          "candidatesTokenCount": 6,
          "totalTokenCount": 26
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "Hi"),
      new(ChatRole.Assistant, "Hello"),
      new(ChatRole.User, "How are you?"),
      new(ChatRole.Assistant, "I'm good"),
      new(ChatRole.User, "Great")
    ];

    var response = await client.GetResponseAsync(messages);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_FinishReasonMaxTokens()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Write a story"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "maxOutputTokens": 5
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Once upon a time"
                }
              ],
              "role": "model"
            },
            "finishReason": "MAX_TOKENS",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 5,
          "totalTokenCount": 10
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      MaxOutputTokens = 5
    };

    var response = await client.GetResponseAsync("Write a story", options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(ChatFinishReason.Length, response.FinishReason);
  }

  [TestMethod]
  public async Task IChatClient_MultipleSystemMessages()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Tell me something"
              }
            ],
            "role": "user"
          }
        ],
        "systemInstruction": {
          "parts": [
            {
              "text": "You are helpful. You are concise."
            }
          ]
        },
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Sure!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 15,
          "candidatesTokenCount": 3,
          "totalTokenCount": 18
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.System, [new TextContent("You are helpful. "), new TextContent("You are concise.")]),
      new(ChatRole.User, "Tell me something")
    ];

    var response = await client.GetResponseAsync(messages);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_WithAllGenerationOptions()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Test"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "temperature": 0.5,
          "topP": 0.9,
          "topK": 20,
          "maxOutputTokens": 100,
          "stopSequences": ["END"]
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Response text"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 3,
          "totalTokenCount": 8
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      Temperature = 0.5f,
      TopP = 0.9f,
      TopK = 20,
      MaxOutputTokens = 100,
      StopSequences = ["END"]
    };

    var response = await client.GetResponseAsync("Test", options);
    
    Assert.IsNotNull(response);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
  }

  [TestMethod]
  public async Task IChatClient_MultiTurnConversationWithRepeatedFunctionCalls()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Check the stock price for GOOGL"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "get_stock_price",
                  "name": "get_stock_price",
                  "args": {
                    "symbol": "GOOGL"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "get_stock_price",
                  "name": "get_stock_price",
                  "response": {
                    "result": {
                      "symbol": "GOOGL",
                      "price": 142.50
                    }
                  }
                }
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "GOOGL is currently trading at $142.50."
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Now check MSFT"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "get_stock_price",
                  "name": "get_stock_price",
                  "args": {
                    "symbol": "MSFT"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "get_stock_price",
                  "name": "get_stock_price",
                  "response": {
                    "result": {
                      "symbol": "MSFT",
                      "price": 378.91
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets current stock price",
                "name": "get_stock_price",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "symbol": {
                      "type": "string"
                    }
                  },
                  "required": ["symbol"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Microsoft (MSFT) is trading at $378.91, which is significantly higher than Google's $142.50."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 85,
          "candidatesTokenCount": 22,
          "totalTokenCount": 107
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    AIFunction stockFunc = AIFunctionFactory.Create(
      (string symbol) => symbol == "GOOGL" 
        ? new { symbol = "GOOGL", price = 142.50 }
        : new { symbol = "MSFT", price = 378.91 },
      "get_stock_price",
      "Gets current stock price");

    ChatMessage[] messages =
    [
      new(ChatRole.User, "Check the stock price for GOOGL"),
      new(ChatRole.Assistant, [new FunctionCallContent("get_stock_price", "get_stock_price", new Dictionary<string, object?> { ["symbol"] = "GOOGL" })]),
      new(ChatRole.User, [new FunctionResultContent("get_stock_price", new { symbol = "GOOGL", price = 142.50 })]),
      new(ChatRole.Assistant, "GOOGL is currently trading at $142.50."),
      new(ChatRole.User, "Now check MSFT"),
      new(ChatRole.Assistant, [new FunctionCallContent("get_stock_price", "get_stock_price", new Dictionary<string, object?> { ["symbol"] = "MSFT" })]),
      new(ChatRole.User, [new FunctionResultContent("get_stock_price", new { symbol = "MSFT", price = 378.91 })])
    ];

    ChatOptions options = new()
    {
      Tools = [stockFunc]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.IsTrue(response.Messages[0].Text.Contains("MSFT") || response.Messages[0].Text.Contains("Microsoft"));
    Assert.IsTrue(response.Messages[0].Text.Contains("378.91"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(85, response.Usage.InputTokenCount);
    Assert.AreEqual(22, response.Usage.OutputTokenCount);
    Assert.AreEqual(107, response.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_ComplexMultiToolConversationWithErrorHandling()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "I need to schedule a meeting and send an email about it"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "schedule_meeting",
                  "name": "schedule_meeting",
                  "args": {
                    "title": "Team Sync",
                    "date": "2025-11-01",
                    "duration": 60
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "schedule_meeting",
                  "name": "schedule_meeting",
                  "response": {
                    "result": {
                      "meetingId": "MTG123",
                      "status": "scheduled"
                    }
                  }
                }
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "send_email",
                  "name": "send_email",
                  "args": {
                    "to": "team@example.com",
                    "subject": "Team Sync Scheduled",
                    "body": "Meeting scheduled for Nov 1, 2025"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "send_email",
                  "name": "send_email",
                  "response": {
                    "result": {
                      "messageId": "MSG456",
                      "sent": true
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Schedules a meeting",
                "name": "schedule_meeting",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "title": {"type": "string"},
                    "date": {"type": "string"},
                    "duration": {"type": "integer"}
                  },
                  "required": ["title", "date", "duration"]
                }
              },
              {
                "description": "Sends an email",
                "name": "send_email",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "to": {"type": "string"},
                    "subject": {"type": "string"},
                    "body": {"type": "string"}
                  },
                  "required": ["to", "subject", "body"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Done! I've scheduled your Team Sync meeting for November 1st, 2025 (meeting ID: MTG123) and sent an email notification to the team (message ID: MSG456)."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 120,
          "candidatesTokenCount": 35,
          "totalTokenCount": 155
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "I need to schedule a meeting and send an email about it"),
      new(ChatRole.Assistant, [new FunctionCallContent("schedule_meeting", "schedule_meeting", new Dictionary<string, object?> { ["title"] = "Team Sync", ["date"] = "2025-11-01", ["duration"] = 60 })]),
      new(ChatRole.User, [new FunctionResultContent("schedule_meeting", new { meetingId = "MTG123", status = "scheduled" })]),
      new(ChatRole.Assistant, [new FunctionCallContent("send_email", "send_email", new Dictionary<string, object?> { ["to"] = "team@example.com", ["subject"] = "Team Sync Scheduled", ["body"] = "Meeting scheduled for Nov 1, 2025" })]),
      new(ChatRole.User, [new FunctionResultContent("send_email", new { messageId = "MSG456", sent = true })])
    ];

    ChatOptions options = new()
    {
      Tools = [
        AIFunctionFactory.Create((string title, string date, int duration) => new { meetingId = "MTG123", status = "scheduled" }, "schedule_meeting", "Schedules a meeting"),
        AIFunctionFactory.Create((string to, string subject, string body) => new { messageId = "MSG456", sent = true }, "send_email", "Sends an email")
      ]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.AreEqual(1, response.Messages[0].Contents.Count);
    Assert.IsInstanceOfType(response.Messages[0].Contents[0], typeof(TextContent));
    Assert.IsTrue(response.Messages[0].Text.Contains("MTG123"));
    Assert.IsTrue(response.Messages[0].Text.Contains("MSG456"));
    Assert.IsTrue(response.Messages[0].Text.Contains("scheduled") || response.Messages[0].Text.Contains("sent"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(120, response.Usage.InputTokenCount);
    Assert.AreEqual(35, response.Usage.OutputTokenCount);
    Assert.IsNotNull(response.RawRepresentation);
  }

  [TestMethod]
  public async Task IChatClient_VeryLongConversationWithMixedContent()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hello, I need help planning a trip"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "I'd be happy to help you plan your trip! Where would you like to go?"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Paris, France"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "Excellent choice! When are you planning to travel?"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Next month"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "search_flights",
                  "name": "search_flights",
                  "args": {
                    "destination": "Paris",
                    "month": "December"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "search_flights",
                  "name": "search_flights",
                  "response": {
                    "result": {
                      "flights": [
                        {"airline": "Air France", "price": 750},
                        {"airline": "Delta", "price": 820}
                      ]
                    }
                  }
                }
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "I found flights for you. Air France has flights for $750 and Delta for $820. Which would you prefer?"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Air France please"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "book_flight",
                  "name": "book_flight",
                  "args": {
                    "airline": "Air France",
                    "price": 750
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "book_flight",
                  "name": "book_flight",
                  "response": {
                    "result": {
                      "confirmationNumber": "AF789XYZ",
                      "status": "confirmed"
                    }
                  }
                }
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "Great! Your flight is booked. Confirmation: AF789XYZ. Would you like me to help find a hotel?"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Yes, something near the Eiffel Tower"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "search_hotels",
                  "name": "search_hotels",
                  "args": {
                    "location": "Eiffel Tower",
                    "city": "Paris"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "search_hotels",
                  "name": "search_hotels",
                  "response": {
                    "result": {
                      "hotels": [
                        {"name": "Hotel Eiffel Seine", "price": 200},
                        {"name": "Pullman Paris Tour Eiffel", "price": 350}
                      ]
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Searches for flights",
                "name": "search_flights",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "destination": {"type": "string"},
                    "month": {"type": "string"}
                  },
                  "required": ["destination", "month"]
                }
              },
              {
                "description": "Books a flight",
                "name": "book_flight",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "airline": {"type": "string"},
                    "price": {"type": "number"}
                  },
                  "required": ["airline", "price"]
                }
              },
              {
                "description": "Searches for hotels",
                "name": "search_hotels",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "location": {"type": "string"},
                    "city": {"type": "string"}
                  },
                  "required": ["location", "city"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "I found two great options near the Eiffel Tower: Hotel Eiffel Seine at $200/night and Pullman Paris Tour Eiffel at $350/night. Which one interests you?"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 250,
          "candidatesTokenCount": 38,
          "totalTokenCount": 288
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "Hello, I need help planning a trip"),
      new(ChatRole.Assistant, "I'd be happy to help you plan your trip! Where would you like to go?"),
      new(ChatRole.User, "Paris, France"),
      new(ChatRole.Assistant, "Excellent choice! When are you planning to travel?"),
      new(ChatRole.User, "Next month"),
      new(ChatRole.Assistant, [new FunctionCallContent("search_flights", "search_flights", new Dictionary<string, object?> { ["destination"] = "Paris", ["month"] = "December" })]),
      new(ChatRole.User, [new FunctionResultContent("search_flights", new { flights = new[] { new { airline = "Air France", price = 750 }, new { airline = "Delta", price = 820 } } })]),
      new(ChatRole.Assistant, "I found flights for you. Air France has flights for $750 and Delta for $820. Which would you prefer?"),
      new(ChatRole.User, "Air France please"),
      new(ChatRole.Assistant, [new FunctionCallContent("book_flight", "book_flight", new Dictionary<string, object?> { ["airline"] = "Air France", ["price"] = 750 })]),
      new(ChatRole.User, [new FunctionResultContent("book_flight", new { confirmationNumber = "AF789XYZ", status = "confirmed" })]),
      new(ChatRole.Assistant, "Great! Your flight is booked. Confirmation: AF789XYZ. Would you like me to help find a hotel?"),
      new(ChatRole.User, "Yes, something near the Eiffel Tower"),
      new(ChatRole.Assistant, [new FunctionCallContent("search_hotels", "search_hotels", new Dictionary<string, object?> { ["location"] = "Eiffel Tower", ["city"] = "Paris" })]),
      new(ChatRole.User, [new FunctionResultContent("search_hotels", new { hotels = new[] { new { name = "Hotel Eiffel Seine", price = 200 }, new { name = "Pullman Paris Tour Eiffel", price = 350 } } })])
    ];

    ChatOptions options = new()
    {
      Tools = [
        AIFunctionFactory.Create((string destination, string month) => new { flights = new[] { new { airline = "Air France", price = 750 } } }, "search_flights", "Searches for flights"),
        AIFunctionFactory.Create((string airline, double price) => new { confirmationNumber = "AF789XYZ", status = "confirmed" }, "book_flight", "Books a flight"),
        AIFunctionFactory.Create((string location, string city) => new { hotels = new[] { new { name = "Hotel Eiffel Seine", price = 200 } } }, "search_hotels", "Searches for hotels")
      ]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.IsTrue(response.Messages[0].Text.Contains("Hotel Eiffel Seine") || response.Messages[0].Text.Contains("Pullman"));
    Assert.IsTrue(response.Messages[0].Text.Contains("200") || response.Messages[0].Text.Contains("350"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(250, response.Usage.InputTokenCount);
    Assert.AreEqual(38, response.Usage.OutputTokenCount);
    Assert.AreEqual(288, response.Usage.TotalTokenCount);
    Assert.AreEqual("gemini-2.5-pro", response.ModelId);
  }

  [TestMethod]
  public async Task IChatClient_ParallelFunctionCallsInConversation()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Compare the weather in New York, London, and Tokyo"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "args": {
                    "city": "New York"
                  }
                }
              },
              {
                "functionCall": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "args": {
                    "city": "London"
                  }
                }
              },
              {
                "functionCall": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "args": {
                    "city": "Tokyo"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "response": {
                    "result": {
                      "city": "New York",
                      "temp": 45,
                      "condition": "cloudy"
                    }
                  }
                }
              },
              {
                "functionResponse": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "response": {
                    "result": {
                      "city": "London",
                      "temp": 50,
                      "condition": "rainy"
                    }
                  }
                }
              },
              {
                "functionResponse": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "response": {
                    "result": {
                      "city": "Tokyo",
                      "temp": 65,
                      "condition": "sunny"
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets weather for a city",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "city": {
                      "type": "string"
                    }
                  },
                  "required": ["city"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Here's the weather comparison:\n- New York: 45°F and cloudy\n- London: 50°F and rainy\n- Tokyo: 65°F and sunny\n\nTokyo has the warmest and best weather of the three cities!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 75,
          "candidatesTokenCount": 48,
          "totalTokenCount": 123
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "Compare the weather in New York, London, and Tokyo"),
      new(ChatRole.Assistant, [
        new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["city"] = "New York" }),
        new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["city"] = "London" }),
        new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["city"] = "Tokyo" })
      ]),
      new(ChatRole.User, [
        new FunctionResultContent("get_weather", new { city = "New York", temp = 45, condition = "cloudy" }),
        new FunctionResultContent("get_weather", new { city = "London", temp = 50, condition = "rainy" }),
        new FunctionResultContent("get_weather", new { city = "Tokyo", temp = 65, condition = "sunny" })
      ])
    ];

    ChatOptions options = new()
    {
      Tools = [AIFunctionFactory.Create((string city) => 
        city == "New York" ? new { city = "New York", temp = 45, condition = "cloudy" } :
        city == "London" ? new { city = "London", temp = 50, condition = "rainy" } :
        new { city = "Tokyo", temp = 65, condition = "sunny" },
        "get_weather", "Gets weather for a city")]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.IsTrue(response.Messages[0].Text.Contains("New York"));
    Assert.IsTrue(response.Messages[0].Text.Contains("London"));
    Assert.IsTrue(response.Messages[0].Text.Contains("Tokyo"));
    Assert.IsTrue(response.Messages[0].Text.Contains("45") || response.Messages[0].Text.Contains("50") || response.Messages[0].Text.Contains("65"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(75, response.Usage.InputTokenCount);
    Assert.AreEqual(48, response.Usage.OutputTokenCount);
    Assert.AreEqual(123, response.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_WithTextReasoningContent()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Think step by step about this math problem: What is 15 * 23?"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "responseModalities": ["TEXT"],
          "thinkingConfig": {
            "includeThoughts": true,
            "thinkingBudget": -1
          }
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "thought": true,
                  "text": "Let me break this down:\n15 * 23 = 15 * (20 + 3)\n= (15 * 20) + (15 * 3)\n= 300 + 45\n= 345"
                },
                {
                  "text": "The answer is 345."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 18,
          "candidatesTokenCount": 45,
          "totalTokenCount": 63,
          "thoughtsTokenCount": 35
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      RawRepresentationFactory = (_) => new GenerateContentConfig
      {
        ResponseModalities = ["TEXT"],
        ThinkingConfig = new ThinkingConfig { IncludeThoughts = true, ThinkingBudget = -1 }
      }
    };

    var response = await client.GetResponseAsync("Think step by step about this math problem: What is 15 * 23?", options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.AreEqual(2, response.Messages[0].Contents.Count);
    
    // First part should be reasoning
    var reasoningContent = response.Messages[0].Contents[0] as TextReasoningContent;
    Assert.IsNotNull(reasoningContent, "First content should be TextReasoningContent");
    Assert.IsTrue(reasoningContent.Text.Contains("break this down") || reasoningContent.Text.Contains("15 * 20"));
    
    // Second part should be final answer
    var textContent = response.Messages[0].Contents[1] as TextContent;
    Assert.IsNotNull(textContent, "Second content should be TextContent");
    Assert.IsTrue(textContent.Text.Contains("345"));
    
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(18, response.Usage.InputTokenCount);
    Assert.AreEqual(45, response.Usage.OutputTokenCount);
    Assert.AreEqual(63, response.Usage.TotalTokenCount);
    Assert.IsNotNull(response.Usage.AdditionalCounts);
    Assert.AreEqual(35, response.Usage.AdditionalCounts["ThoughtsTokenCount"]);
  }

  [TestMethod]
  public async Task IChatClient_WithThinkingConfigProtected()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Solve this: 7^3"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "thinkingConfig": {
            "includeThoughts": true
          }
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "thought": true,
                  "thoughtSignature": "dGhpbmtpbmcgc2lnbmF0dXJlIGRhdGE="
                },
                {
                  "text": "7^3 = 343"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 8,
          "candidatesTokenCount": 15,
          "totalTokenCount": 23,
          "thoughtsTokenCount": 10
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      RawRepresentationFactory = (_) => new GenerateContentConfig
      {
        ThinkingConfig = new ThinkingConfig 
        { 
          IncludeThoughts = true
        }
      }
    };

    var response = await client.GetResponseAsync("Solve this: 7^3", options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    // When thought is protected (only thoughtSignature, no text), it may be merged with the answer
    Assert.IsTrue(response.Messages[0].Contents.Count >= 1);
    
    var textContent = response.Messages[0].Contents.OfType<TextContent>().FirstOrDefault();
    Assert.IsNotNull(textContent, "Should have text content with answer");
    Assert.IsTrue(textContent.Text.Contains("343"));
    
    // Check if we got reasoning content with protected data
    var reasoningContent = response.Messages[0].Contents.OfType<TextReasoningContent>().FirstOrDefault();
    if (reasoningContent != null)
    {
      Assert.IsNotNull(reasoningContent.ProtectedData);
      Assert.AreEqual("dGhpbmtpbmcgc2lnbmF0dXJlIGRhdGE=", reasoningContent.ProtectedData);
    }
    
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(8, response.Usage.InputTokenCount);
    Assert.AreEqual(15, response.Usage.OutputTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_ComplexConversationWithFunctionCalls()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What's the weather in Seattle and how does it compare to San Francisco?"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "args": {
                    "location": "Seattle"
                  }
                }
              },
              {
                "functionCall": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "args": {
                    "location": "San Francisco"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "response": {
                    "result": {
                      "temperature": 55,
                      "condition": "rainy"
                    }
                  }
                }
              },
              {
                "functionResponse": {
                  "id": "get_weather",
                  "name": "get_weather",
                  "response": {
                    "result": {
                      "temperature": 72,
                      "condition": "sunny"
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets weather for a location",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "location": {
                      "type": "string"
                    }
                  },
                  "required": ["location"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Seattle is currently 55°F and rainy, while San Francisco is warmer at 72°F and sunny. San Francisco has much better weather today - it's 17 degrees warmer and not raining!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 45,
          "candidatesTokenCount": 38,
          "totalTokenCount": 83
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    AIFunction weatherFunc = AIFunctionFactory.Create(
      (string location) => location == "Seattle" 
        ? new { temperature = 55, condition = "rainy" }
        : new { temperature = 72, condition = "sunny" },
      "get_weather",
      "Gets weather for a location");

    ChatMessage[] messages =
    [
      new(ChatRole.User, "What's the weather in Seattle and how does it compare to San Francisco?"),
      new(ChatRole.Assistant, [
        new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["location"] = "Seattle" }),
        new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["location"] = "San Francisco" })
      ]),
      new(ChatRole.User, [
        new FunctionResultContent("get_weather", new { temperature = 55, condition = "rainy" }),
        new FunctionResultContent("get_weather", new { temperature = 72, condition = "sunny" })
      ])
    ];

    ChatOptions options = new()
    {
      Tools = [weatherFunc]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.IsTrue(response.Messages[0].Text.Contains("Seattle"));
    Assert.IsTrue(response.Messages[0].Text.Contains("San Francisco"));
    Assert.IsTrue(response.Messages[0].Text.Contains("55") || response.Messages[0].Text.Contains("72"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(45, response.Usage.InputTokenCount);
    Assert.AreEqual(38, response.Usage.OutputTokenCount);
    Assert.AreEqual(83, response.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_ExtendedConversationWithMultipleTools()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Book me a flight to Paris"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "search_flights",
                  "name": "search_flights",
                  "args": {
                    "destination": "Paris",
                    "date": "2025-11-01"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "search_flights",
                  "name": "search_flights",
                  "response": {
                    "result": {
                      "flights": [
                        {"airline": "Air France", "price": 850, "duration": "11h"},
                        {"airline": "United", "price": 920, "duration": "12h"}
                      ]
                    }
                  }
                }
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "I found 2 flights for you. Air France for $850 (11h) and United for $920 (12h). Which would you prefer?"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "Book the Air France flight"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "functionCall": {
                  "id": "book_flight",
                  "name": "book_flight",
                  "args": {
                    "airline": "Air France",
                    "price": 850
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "book_flight",
                  "name": "book_flight",
                  "response": {
                    "result": {
                      "confirmationNumber": "AF12345",
                      "status": "confirmed"
                    }
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Searches for flights",
                "name": "search_flights",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "destination": {"type": "string"},
                    "date": {"type": "string"}
                  },
                  "required": ["destination", "date"]
                }
              },
              {
                "description": "Books a flight",
                "name": "book_flight",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "airline": {"type": "string"},
                    "price": {"type": "number"}
                  },
                  "required": ["airline", "price"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Perfect! I've booked your Air France flight to Paris. Your confirmation number is AF12345. Have a great trip!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 120,
          "candidatesTokenCount": 25,
          "totalTokenCount": 145
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatMessage[] messages =
    [
      new(ChatRole.User, "Book me a flight to Paris"),
      new(ChatRole.Assistant, [new FunctionCallContent("search_flights", "search_flights", new Dictionary<string, object?> { ["destination"] = "Paris", ["date"] = "2025-11-01" })]),
      new(ChatRole.User, [new FunctionResultContent("search_flights", new { flights = new[] { new { airline = "Air France", price = 850, duration = "11h" }, new { airline = "United", price = 920, duration = "12h" } } })]),
      new(ChatRole.Assistant, "I found 2 flights for you. Air France for $850 (11h) and United for $920 (12h). Which would you prefer?"),
      new(ChatRole.User, "Book the Air France flight"),
      new(ChatRole.Assistant, [new FunctionCallContent("book_flight", "book_flight", new Dictionary<string, object?> { ["airline"] = "Air France", ["price"] = 850 })]),
      new(ChatRole.User, [new FunctionResultContent("book_flight", new { confirmationNumber = "AF12345", status = "confirmed" })])
    ];

    ChatOptions options = new()
    {
      Tools = [
        AIFunctionFactory.Create((string destination, string? date) => new { flights = new[] { new { airline = "Air France", price = 850 } } }, "search_flights", "Searches for flights"),
        AIFunctionFactory.Create((string airline, double price) => new { confirmationNumber = "AF12345", status = "confirmed" }, "book_flight", "Books a flight")
      ]
    };

    var response = await client.GetResponseAsync(messages, options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.AreEqual(1, response.Messages[0].Contents.Count);
    Assert.IsInstanceOfType(response.Messages[0].Contents[0], typeof(TextContent));
    Assert.IsTrue(response.Messages[0].Text.Contains("AF12345"));
    Assert.IsTrue(response.Messages[0].Text.Contains("Air France") || response.Messages[0].Text.Contains("booked"));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(120, response.Usage.InputTokenCount);
    Assert.AreEqual(25, response.Usage.OutputTokenCount);
    Assert.AreEqual(145, response.Usage.TotalTokenCount);
    Assert.IsNotNull(response.RawRepresentation);
    Assert.IsInstanceOfType(response.RawRepresentation, typeof(GenerateContentResponse));
    Assert.AreEqual("gemini-2.5-pro", response.ModelId);
  }

  [TestMethod]
  public async Task IChatClient_RawRepresentationFactoryWithCustomConfig()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Test custom config"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "temperature": 0.3,
          "candidateCount": 1,
          "responseModalities": ["TEXT"]
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Response with custom configuration."
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 7,
          "totalTokenCount": 12
        },
        "modelVersion": "gemini-2.5-pro"
      }
      """);

    ChatOptions options = new()
    {
      Temperature = 0.3f,
      RawRepresentationFactory = (_) => new GenerateContentConfig
      {
        CandidateCount = 1,
        ResponseModalities = ["TEXT"]
      }
    };

    var response = await client.GetResponseAsync("Test custom config", options);
    
    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, response.Messages[0].Role);
    Assert.IsFalse(string.IsNullOrEmpty(response.Messages[0].Text));
    Assert.AreEqual(ChatFinishReason.Stop, response.FinishReason);
    Assert.IsNotNull(response.Usage);
    Assert.AreEqual(5, response.Usage.InputTokenCount);
    Assert.AreEqual(7, response.Usage.OutputTokenCount);
    Assert.IsNotNull(response.RawRepresentation);
    
    var rawResponse = response.RawRepresentation as GenerateContentResponse;
    Assert.IsNotNull(rawResponse);
    Assert.AreEqual(1, rawResponse.Candidates?.Count);
  }

  [TestMethod]
  public async Task IChatClient_StreamingWithThinkingAndFunctionCall()
  {
    var weatherFunc = AIFunctionFactory.Create(
      (string location) => $"72°F and sunny in {location}",
      "get_weather",
      "Gets the weather for a location");

    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Think carefully about why someone in Paris might want to know the weather, then tell me the weather there."
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets the weather for a location",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "location": {
                      "type": "string"
                    }
                  },
                  "required": [
                    "location"
                  ]
                }
              }
            ]
          }
        ],
        "generationConfig": {
          "thinkingConfig": {
            "includeThoughts": true,
            "thinkingBudget": -1
          }
        }
      }
      """, """
      data: {"candidates": [{"content": {"parts": [{"text": "Understanding the context","thought": true}],"role": "model"},"index": 0}],"usageMetadata": {"promptTokenCount": 60,"totalTokenCount": 65,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 60}],"thoughtsTokenCount": 5},"modelVersion": "gemini-2.5-pro"}
      data: {"candidates": [{"content": {"parts": [{"text": "Of course! Someone in Paris might want to know the weather to decide whether to","thoughtSignature": "CiIB0e2Kb3a4gkvo"}],"role": "model"},"index": 0}],"usageMetadata": {"promptTokenCount": 60,"candidatesTokenCount": 15,"totalTokenCount": 80,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 60}],"thoughtsTokenCount": 5},"modelVersion": "gemini-2.5-pro"}
      data: {"candidates": [{"content": {"parts": [{"text": " stroll along the Seine, visit an outdoor market, or perhaps opt for a cozy day in a museum."}],"role": "model"},"index": 0}],"usageMetadata": {"promptTokenCount": 60,"candidatesTokenCount": 35,"totalTokenCount": 100,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 60}],"thoughtsTokenCount": 5},"modelVersion": "gemini-2.5-pro"}
      data: {"candidates": [{"content": {"parts": [{"text": " Let me check the weather for you."}],"role": "model"},"index": 0}],"usageMetadata": {"promptTokenCount": 60,"candidatesTokenCount": 41,"totalTokenCount": 106,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 60}],"thoughtsTokenCount": 5},"modelVersion": "gemini-2.5-pro"}
      data: {"candidates": [{"content": {"parts": [{"functionCall": {"name": "get_weather","args": {"location": "Paris"}}}],"role": "model"},"finishReason": "STOP","index": 0}],"usageMetadata": {"promptTokenCount": 60,"candidatesTokenCount": 47,"totalTokenCount": 112,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 60}],"thoughtsTokenCount": 5},"modelVersion": "gemini-2.5-pro"}
      """);

    ChatOptions options = new()
    {
      Tools = [weatherFunc],
      RawRepresentationFactory = (_) => new GenerateContentConfig
      {
        ThinkingConfig = new ThinkingConfig { IncludeThoughts = true, ThinkingBudget = -1 }
      }
    };

    // Use a message list instead of a string
    List<ChatMessage> messageHistory =
    [
      new(ChatRole.User, "Think carefully about why someone in Paris might want to know the weather, then tell me the weather there.")
    ];

    List<ChatResponseUpdate> updates = [];
    await foreach (var update in client.GetStreamingResponseAsync(messageHistory, options))
    {
      updates.Add(update);
    }

    // Validate the updates
    Assert.AreEqual(5, updates.Count);
    
    // First update: Thinking content with "thought": true
    Assert.IsTrue(updates[0].Contents.Any(c => c is TextReasoningContent));
    var thinkingContent1 = updates[0].Contents.OfType<TextReasoningContent>().First();
    Assert.IsTrue(thinkingContent1.Text.Contains("context"));
    
    // Second update: Text with thoughtSignature
    Assert.IsTrue(updates[1].Contents.Any(c => c is TextContent));
    var textContent1 = updates[1].Contents.OfType<TextContent>().First();
    Assert.IsTrue(textContent1.Text.Contains("Paris"));
    
    // Third update: More text
    Assert.IsTrue(updates[2].Contents.Any(c => c is TextContent));
    var textContent2 = updates[2].Contents.OfType<TextContent>().First();
    Assert.IsTrue(textContent2.Text.Contains("Seine") || textContent2.Text.Contains("museum"));
    
    // Fourth update: More text
    Assert.IsTrue(updates[3].Contents.Any(c => c is TextContent));
    var textContent3 = updates[3].Contents.OfType<TextContent>().First();
    Assert.IsTrue(textContent3.Text.Contains("weather"));
    
    // Fifth update: Function call
    Assert.IsTrue(updates[4].Contents.Any(c => c is FunctionCallContent));
    var functionCall = updates[4].Contents.OfType<FunctionCallContent>().First();
    Assert.AreEqual("get_weather", functionCall.Name);
    Assert.IsTrue(functionCall.Arguments?.ContainsKey("location"));
    Assert.AreEqual("Paris", functionCall.Arguments?["location"]?.ToString());
    Assert.AreEqual(ChatFinishReason.Stop, updates[4].FinishReason);
    
    // Validate final response
    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual(1, finalResponse.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    
    // Should have thinking, text, and function call
    Assert.IsTrue(finalResponse.Messages[0].Contents.Count >= 3);
    Assert.IsTrue(finalResponse.Messages[0].Contents.Any(c => c is TextReasoningContent));
    Assert.IsTrue(finalResponse.Messages[0].Contents.Any(c => c is TextContent));
    Assert.IsTrue(finalResponse.Messages[0].Contents.Any(c => c is FunctionCallContent));
    
    // Validate usage metadata
    Assert.IsNotNull(finalResponse.Usage);
    Assert.IsTrue(finalResponse.Usage.InputTokenCount > 0);
    Assert.IsTrue(finalResponse.Usage.OutputTokenCount > 0);
    Assert.IsTrue(finalResponse.Usage.TotalTokenCount > 0);
    
    // Add the updates to the message history
    messageHistory.AddRange(updates.ToChatResponse().Messages);
    
    // Verify message history now has user message + assistant message with function call
    Assert.AreEqual(2, messageHistory.Count);
    Assert.AreEqual(ChatRole.User, messageHistory[0].Role);
    Assert.AreEqual(ChatRole.Assistant, messageHistory[1].Role);
    Assert.IsTrue(messageHistory[1].Contents.Any(c => c is FunctionCallContent));
    
    // Add function result
    var functionCallFromHistory = messageHistory[1].Contents.OfType<FunctionCallContent>().First();
    var result = await weatherFunc.InvokeAsync(new(functionCallFromHistory.Arguments));
    messageHistory.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(functionCallFromHistory.CallId, result)]));
    
    // Verify we have 3 messages now
    Assert.AreEqual(3, messageHistory.Count);
    Assert.AreEqual(ChatRole.Tool, messageHistory[2].Role);
    
    // Now make a second streaming call with the full conversation history
    // This client has the expected request/response for the second turn
    // Note: The expected request needs to match the actual serialization from the message history,
    // which includes thought markers and uses "user" role for function results
    var client2 = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Think carefully about why someone in Paris might want to know the weather, then tell me the weather there."
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "thought": true,
                "text": "Understanding the context"
              },
              {
                "thoughtSignature": "CiIB0e2Kb3a4gkvo",
                "text": "Of course! Someone in Paris might want to know the weather to decide whether to"
              },
              {
                "text": " stroll along the Seine, visit an outdoor market, or perhaps opt for a cozy day in a museum. Let me check the weather for you."
              },
              {
                "functionCall": {
                  "id": "",
                  "name": "get_weather",
                  "args": {
                    "location": "Paris"
                  }
                }
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "functionResponse": {
                  "id": "",
                  "name": "get_weather",
                  "response": {
                    "result": "72\u00B0F and sunny in Paris"
                  }
                }
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets the weather for a location",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "location": {
                      "type": "string"
                    }
                  },
                  "required": [
                    "location"
                  ]
                }
              }
            ]
          }
        ],
        "generationConfig": {
          "thinkingConfig": {
            "includeThoughts": true,
            "thinkingBudget": -1
          }
        }
      }
      """, """
      data: {"candidates": [{"content": {"parts": [{"text": "The weather in Paris"}],"role": "model"},"index": 0}],"usageMetadata": {"promptTokenCount": 145,"candidatesTokenCount": 4,"totalTokenCount": 149,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 145}]},"modelVersion": "gemini-2.5-pro"}
      data: {"candidates": [{"content": {"parts": [{"text": " is 72°F and sunny."}],"role": "model"},"finishReason": "STOP","index": 0}],"usageMetadata": {"promptTokenCount": 145,"candidatesTokenCount": 13,"totalTokenCount": 158,"promptTokensDetails": [{"modality": "TEXT","tokenCount": 145}]},"modelVersion": "gemini-2.5-pro"}
      """);
    
    List<ChatResponseUpdate> updates2 = [];
    await foreach (var update in client2.GetStreamingResponseAsync(messageHistory, options))
    {
      updates2.Add(update);
    }
    
    // Validate the second response
    Assert.AreEqual(2, updates2.Count);
    
    // First update: Text content
    Assert.IsTrue(updates2[0].Contents.Any(c => c is TextContent));
    var text1 = updates2[0].Contents.OfType<TextContent>().First();
    Assert.IsTrue(text1.Text.Contains("weather") || text1.Text.Contains("Paris"));
    
    // Second update: More text with finish reason
    Assert.IsTrue(updates2[1].Contents.Any(c => c is TextContent));
    var text2 = updates2[1].Contents.OfType<TextContent>().First();
    Assert.IsTrue(text2.Text.Contains("72") || text2.Text.Contains("sunny"));
    Assert.AreEqual(ChatFinishReason.Stop, updates2[1].FinishReason);
    
    // Validate the final aggregated response
    var finalResponse2 = updates2.ToChatResponse();
    Assert.IsNotNull(finalResponse2);
    Assert.AreEqual(1, finalResponse2.Messages.Count);
    Assert.AreEqual(ChatRole.Assistant, finalResponse2.Messages[0].Role);
    
    // Should have text content
    Assert.IsTrue(finalResponse2.Messages[0].Contents.Any(c => c is TextContent));
    
    // Validate usage
    Assert.IsNotNull(finalResponse2.Usage);
    Assert.IsTrue(finalResponse2.Usage.InputTokenCount > 0);
    Assert.IsTrue(finalResponse2.Usage.OutputTokenCount > 0);
    Assert.IsTrue(finalResponse2.Usage.TotalTokenCount > 0);
    
    // Add the second response to history and verify final state
    messageHistory.AddRange(finalResponse2.Messages);
    Assert.AreEqual(4, messageHistory.Count);
    Assert.AreEqual(ChatRole.User, messageHistory[0].Role);
    Assert.AreEqual(ChatRole.Assistant, messageHistory[1].Role);
    Assert.AreEqual(ChatRole.Tool, messageHistory[2].Role);
    Assert.AreEqual(ChatRole.Assistant, messageHistory[3].Role);
  }

  [TestMethod]
  public async Task IChatClient_StreamingBasicResponse()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Say hello"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hello"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": " there"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 4,
          "candidatesTokenCount": 3,
          "totalTokenCount": 7
        }
      }
      """);

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("Say hello"))
    {
      updates.Add(update);
      Assert.IsNotNull(update);
      Assert.AreEqual(ChatRole.Assistant, update.Role);
    }

    Assert.AreEqual(3, updates.Count);
    Assert.AreEqual("Hello", updates[0].Text);
    Assert.AreEqual(" there", updates[1].Text);
    Assert.AreEqual("!", updates[2].Text);
    Assert.AreEqual(ChatFinishReason.Stop, updates[2].FinishReason);
    Assert.IsNotNull(updates[2].Contents);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("Hello there!", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(4, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(3, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(7, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingWithFunctionCall()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "What is the weather in Paris?"
              }
            ],
            "role": "user"
          }
        ],
        "tools": [
          {
            "functionDeclarations": [
              {
                "description": "Gets weather",
                "name": "get_weather",
                "parametersJsonSchema": {
                  "type": "object",
                  "properties": {
                    "city": {
                      "type": "string"
                    }
                  },
                  "required": ["city"]
                }
              }
            ]
          }
        ],
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "functionCall": {
                    "name": "get_weather",
                    "args": {
                      "city": "Paris"
                    }
                  }
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 15,
          "candidatesTokenCount": 8,
          "totalTokenCount": 23
        }
      }
      """);

    AIFunction weatherFunc = AIFunctionFactory.Create(
      (string city) => new { city, temp = 72, condition = "sunny" },
      "get_weather",
      "Gets weather");

    ChatOptions options = new() { Tools = [weatherFunc] };

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("What is the weather in Paris?", options))
    {
      updates.Add(update);
    }

    Assert.AreEqual(1, updates.Count);
    Assert.IsNotNull(updates[0].Contents);
    Assert.IsInstanceOfType(updates[0].Contents[0], typeof(FunctionCallContent));
    
    var functionCall = (FunctionCallContent)updates[0].Contents[0];
    Assert.AreEqual("get_weather", functionCall.Name);
    Assert.IsNotNull(functionCall.Arguments);
    Assert.IsTrue(functionCall.Arguments.ContainsKey("city"));

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.AreEqual(1, finalResponse.Messages[0].Contents.Count);
    Assert.IsInstanceOfType(finalResponse.Messages[0].Contents[0], typeof(FunctionCallContent));
    var finalFunctionCall = (FunctionCallContent)finalResponse.Messages[0].Contents[0];
    Assert.AreEqual("get_weather", finalFunctionCall.Name);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(15, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(8, finalResponse.Usage.OutputTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingMultipleChunks()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Count to five"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "1"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": ", 2"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": ", 3"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": ", 4"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": ", 5"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 9,
          "totalTokenCount": 14
        }
      }
      """);

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("Count to five"))
    {
      updates.Add(update);
      Assert.IsNotNull(update);
    }

    Assert.AreEqual(5, updates.Count);
    Assert.AreEqual("1", updates[0].Text);
    Assert.AreEqual(", 2", updates[1].Text);
    Assert.AreEqual(", 3", updates[2].Text);
    Assert.AreEqual(", 4", updates[3].Text);
    Assert.AreEqual(", 5", updates[4].Text);
    Assert.AreEqual(ChatFinishReason.Stop, updates[4].FinishReason);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("1, 2, 3, 4, 5", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(5, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(9, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(14, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingWithSystemMessage()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hi"
              }
            ],
            "role": "user"
          }
        ],
        "systemInstruction": {
          "parts": [
            {
              "text": "Be brief"
            }
          ]
        },
        "generationConfig": {}
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hello"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 6,
          "candidatesTokenCount": 2,
          "totalTokenCount": 8
        }
      }
      """);

    ChatMessage[] messages = [
      new(ChatRole.System, "Be brief"),
      new(ChatRole.User, "Hi")
    ];

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync(messages))
    {
      updates.Add(update);
    }

    Assert.AreEqual(1, updates.Count);
    Assert.AreEqual("Hello", updates[0].Text);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("Hello", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(6, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(2, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(8, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingWithConversationHistory()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hi"
              }
            ],
            "role": "user"
          },
          {
            "parts": [
              {
                "text": "Hello"
              }
            ],
            "role": "model"
          },
          {
            "parts": [
              {
                "text": "How are you?"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "I'm doing"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": " well, thanks!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 12,
          "candidatesTokenCount": 6,
          "totalTokenCount": 18
        }
      }
      """);

    ChatMessage[] messages = [
      new(ChatRole.User, "Hi"),
      new(ChatRole.Assistant, "Hello"),
      new(ChatRole.User, "How are you?")
    ];

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync(messages))
    {
      updates.Add(update);
    }

    Assert.AreEqual(2, updates.Count);
    Assert.AreEqual("I'm doing", updates[0].Text);
    Assert.AreEqual(" well, thanks!", updates[1].Text);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("I'm doing well, thanks!", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(12, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(6, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(18, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingFinishReasonMaxTokens()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Write a story"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "maxOutputTokens": 3
        }
      }
      """, """
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Once upon"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": " a"
                }
              ],
              "role": "model"
            },
            "finishReason": "MAX_TOKENS",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 5,
          "candidatesTokenCount": 3,
          "totalTokenCount": 8
        }
      }
      """);

    ChatOptions options = new() { MaxOutputTokens = 3 };

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("Write a story", options))
    {
      updates.Add(update);
    }

    Assert.AreEqual(2, updates.Count);
    Assert.AreEqual(ChatFinishReason.Length, updates[1].FinishReason);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("Once upon a", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Length, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(5, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(3, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(8, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingWithTemperature()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Be creative"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {
          "temperature": 0.5
        }
      }
      """, """
      {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Let me think creatively!"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 4,
          "candidatesTokenCount": 6,
          "totalTokenCount": 10
        }
      }
      """);

    ChatOptions options = new() { Temperature = 0.5f };

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("Be creative", options))
    {
      updates.Add(update);
    }

    Assert.AreEqual(1, updates.Count);
    
    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("Let me think creatively!", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(4, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(6, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(10, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IChatClient_StreamingEmptyInitialChunks()
  {
    IChatClient client = CreateChatClient("""
      {
        "contents": [
          {
            "parts": [
              {
                "text": "Hello"
              }
            ],
            "role": "user"
          }
        ],
        "generationConfig": {}
      }
      """, """
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": ""
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": "Hi"
                }
              ],
              "role": "model"
            },
            "index": 0
          }
        ]
      }
      data: {
        "candidates": [
          {
            "content": {
              "parts": [
                {
                  "text": " there"
                }
              ],
              "role": "model"
            },
            "finishReason": "STOP",
            "index": 0
          }
        ],
        "usageMetadata": {
          "promptTokenCount": 2,
          "candidatesTokenCount": 3,
          "totalTokenCount": 5
        }
      }
      """);

    List<ChatResponseUpdate> updates = [];;
    await foreach (var update in client.GetStreamingResponseAsync("Hello"))
    {
      updates.Add(update);
    }

    Assert.AreEqual(3, updates.Count);
    Assert.AreEqual("", updates[0].Text);
    Assert.AreEqual("Hi", updates[1].Text);
    Assert.AreEqual(" there", updates[2].Text);

    var finalResponse = updates.ToChatResponse();
    Assert.IsNotNull(finalResponse);
    Assert.AreEqual("Hi there", finalResponse.Messages[0].Text);
    Assert.AreEqual(ChatRole.Assistant, finalResponse.Messages[0].Role);
    Assert.AreEqual(ChatFinishReason.Stop, finalResponse.FinishReason);
    Assert.IsNotNull(finalResponse.Usage);
    Assert.AreEqual(2, finalResponse.Usage.InputTokenCount);
    Assert.AreEqual(3, finalResponse.Usage.OutputTokenCount);
    Assert.AreEqual(5, finalResponse.Usage.TotalTokenCount);
  }

  [TestMethod]
  public async Task IImageGeneration_BasicRequest()
  {
    IImageGenerator imageGenerator = CreateImageGenerator("""
      {
        "instances": [
          {
            "prompt": "A red apple"
          }
        ],
        "parameters": {
          "sampleCount": 1
        }
      }
      """, """
      {
        "predictions": [
          {
            "bytesBase64Encoded": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            "mimeType": "image/png"
          }
        ]
      }
      """);

    var response = await imageGenerator.GenerateImagesAsync("A red apple");

    Assert.IsNotNull(response);
    Assert.AreEqual(1, response.Contents.Count);
    Assert.IsInstanceOfType(response.Contents[0], typeof(DataContent));
    
    var dataContent = (DataContent)response.Contents[0];
    Assert.IsNotNull(dataContent.Data);
    Assert.IsTrue(dataContent.Data.Length > 0);
    Assert.AreEqual("image/png", dataContent.MediaType);
  }

  [TestMethod]
  public async Task IImageGeneration_WithOptions()
  {
    IImageGenerator imageGenerator = CreateImageGenerator("""
      {
        "instances": [
          {
            "prompt": "A blue car"
          }
        ],
        "parameters": {
          "sampleCount": 2,
          "aspectRatio": "16:9"
        }
      }
      """, """
      {
        "predictions": [
          {
            "bytesBase64Encoded": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==",
            "mimeType": "image/png"
          },
          {
            "bytesBase64Encoded": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAGA8fLCKwAAAABJRU5ErkJggg==",
            "mimeType": "image/png"
          }
        ]
      }
      """);

    var options = new ImageGenerationOptions
    {
      Count = 2,
      ImageSize = new System.Drawing.Size(2048, 1024)
    };

    var response = await imageGenerator.GenerateImagesAsync("A blue car", options);

    Assert.IsNotNull(response);
    Assert.AreEqual(2, response.Contents.Count);
    Assert.IsInstanceOfType(response.Contents[0], typeof(DataContent));
    Assert.IsInstanceOfType(response.Contents[1], typeof(DataContent));
    
    var dataContent1 = (DataContent)response.Contents[0];
    var dataContent2 = (DataContent)response.Contents[1];
    Assert.IsNotNull(dataContent1.Data);
    Assert.IsNotNull(dataContent2.Data);
    Assert.IsTrue(dataContent1.Data.Length > 0);
    Assert.IsTrue(dataContent2.Data.Length > 0);
    Assert.AreEqual("image/png", dataContent1.MediaType);
    Assert.AreEqual("image/png", dataContent2.MediaType);
  }

  private static IChatClient CreateChatClient(string expectedRequest, string actualResponse, bool makeRealRequest = false)
  {
    Models models = new(new MockApiClient(expectedRequest, actualResponse, makeRealRequest));
    return models.AsIChatClient("gemini-2.5-pro");
  }

  private static IImageGenerator CreateImageGenerator(string expectedRequest, string actualResponse, bool makeRealRequest = false)
  {
    Models models = new(new MockApiClient(expectedRequest, actualResponse, makeRealRequest));
    return models.AsIImageGenerator("imagen-3.0-generate-001");
  }

  private sealed class MockApiClient : ApiClient
  {
    private readonly string _expectedRequest;
    private readonly string _actualResponse;
    private readonly bool _makeRealRequest;

    public MockApiClient(string expectedRequest, string actualResponse, bool makeRealRequest = false) : base("fake_api_key", new() { BaseUrl = "http://localhost/" })
    {
      _expectedRequest = expectedRequest;
      _actualResponse = actualResponse;
      _makeRealRequest = makeRealRequest;
    }

    private static bool JsonEquals(string json1, string json2)
    {
      using var doc1 = JsonDocument.Parse(json1);
      using var doc2 = JsonDocument.Parse(json2);
      return JsonElementEquals(doc1.RootElement, doc2.RootElement);
    }

    private static bool JsonElementEquals(JsonElement e1, JsonElement e2)
    {
      if (e1.ValueKind == e2.ValueKind) switch (e1.ValueKind)
      {
        case JsonValueKind.Object:
          var props1 = e1.EnumerateObject().OrderBy(p => p.Name).ToList();
          var props2 = e2.EnumerateObject().OrderBy(p => p.Name).ToList();
          return
            props1.Count == props2.Count && props1.Zip(props2).All(pair =>
              pair.First.Name == pair.Second.Name && JsonElementEquals(pair.First.Value, pair.Second.Value));

        case JsonValueKind.Array:
          var arr1 = e1.EnumerateArray().ToList();
          var arr2 = e2.EnumerateArray().ToList();
          return arr1.Count == arr2.Count && arr1.Zip(arr2).All(pair => JsonElementEquals(pair.First, pair.Second));

        case JsonValueKind.Number:
          return Math.Abs(e1.GetDouble() - e2.GetDouble()) < 0.0001;

        case JsonValueKind.String:
          return e1.GetString() == e2.GetString();

        case JsonValueKind.True:
        case JsonValueKind.False:
        case JsonValueKind.Null:
          return true;
      }

      return false;
    }

    public override async Task<ApiResponse> RequestAsync(
      HttpMethod httpMethod, string path, string requestJson, HttpOptions? requestHttpOptions, CancellationToken cancellationToken = default)
    {
      string responseJson = _actualResponse;

      if (_makeRealRequest)
      {
        using var client = new HttpApiClient(System.Environment.GetEnvironmentVariable("AI:Google:ApiKey"), null);
        using var response = await client.RequestAsync(HttpMethod.Post, @"models/gemini-2.5-pro:generateContent", @"{ ""contents"":[{""parts"":[{""text"":""Hello, world!""}],""role"":""user""}],""generationConfig"":{}}", null);
        responseJson = await response.GetEntity().ReadAsStringAsync();
      }

      // Use JSON-aware comparison to handle property ordering, Unicode escapes, and float precision
      Assert.IsTrue(JsonEquals(_expectedRequest, requestJson),
        $"Expected:<{_expectedRequest}>. Actual:<{requestJson}>.");
      return new HttpApiResponse(new HttpResponseMessage()
      {
        Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
      });
    }

    public override async IAsyncEnumerable<ApiResponse> RequestStreamAsync(
      HttpMethod httpMethod, string path, string requestJson, HttpOptions? requestHttpOptions, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      string responseData = _actualResponse;
      
      if (_makeRealRequest)
      {
        // Make a real streaming request
        using var client = new HttpApiClient(System.Environment.GetEnvironmentVariable("AI:Google:ApiKey"), null);
        StringBuilder fullResponse = new();
        
        await foreach (var apiResponse in client.RequestStreamAsync(httpMethod, path, requestJson, requestHttpOptions, cancellationToken))
        {
          var responseJson = await apiResponse.GetEntity().ReadAsStringAsync();
          fullResponse.AppendLine("data: " + responseJson);
          Console.WriteLine($"CAPTURED STREAMING CHUNK: {responseJson}");
        }
        
        responseData = fullResponse.ToString();
        Console.WriteLine($"\n\nFULL CAPTURED REQUEST:\n{requestJson}");
        Console.WriteLine($"\n\nFULL CAPTURED RESPONSE:\n{responseData}");
      }
      else
      {
        // Use JSON-aware comparison to handle property ordering, Unicode escapes, and float precision
        Assert.IsTrue(JsonEquals(_expectedRequest, requestJson),
          $"Expected:<{_expectedRequest}>. Actual:<{requestJson}>.");
      }
      
      // Split streaming response by "data:" prefix (if present)
      foreach (var chunk in responseData.Split(["data:"], StringSplitOptions.RemoveEmptyEntries))
      {
        var trimmedChunk = chunk.Trim();
        if (!string.IsNullOrEmpty(trimmedChunk))
        {
          await Task.Yield();
          yield return new StreamingApiResponse(trimmedChunk, new HttpResponseMessage());
        }
      }
    }

    private static string RemoveWhitespace(string input) => Regex.Replace(input, @"\s+", "");
  }
}

