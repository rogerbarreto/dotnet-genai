Google Gen AI .NET SDK provides an interface for developers to integrate
Google's generative models into their .NET applications. It supports the
[Gemini Developer API](https://ai.google.dev/gemini-api/docs) and
[Vertex AI](https://cloud.google.com/vertex-ai/generative-ai/docs/learn/overview)
APIs.

## Supported .NET version

This library is built for .NET 8.0 and netstandard2.1.

## Full API Reference

The full API reference is hosted in the [dedicated GitHub Page](https://googleapis.github.io/dotnet-genai/api/index.html)

## Install
In your dotnet project directory, type the the following command

```
dotnet add package Google.GenAI
```

## Imports

```csharp
using Google.GenAI;
using Google.GenAI.Types;
```

## Create a client

Please run one of the following code blocks to create a client for
different services ([Gemini Developer API](https://ai.google.dev/gemini-api/docs) or [Vertex AI](https://cloud.google.com/vertex-ai/generative-ai/docs/learn/overview)).

```csharp
using Google.GenAI;

//  Only run this block for Gemini Developer API
var client = new Client(apiKey: apiKey);
```

```csharp
using Google.GenAI;

// only run this block for Vertex AI API
client = new Client(
    project: project, location: location, vertexAI: true
)
```

**(Optional) Using environment variables:**

You can create a client by configuring the necessary environment variables.
Configuration setup instructions depends on whether you're using the Gemini
Developer API or the Gemini API in Vertex AI.

**Gemini Developer API:** Set the `GOOGLE_API_KEY`. It will automatically be
picked up by the client.

```bash
export GEMINI_API_KEY='your-api-key'
```

**Gemini API on Vertex AI:** Set `GOOGLE_GENAI_USE_VERTEXAI`,
`GOOGLE_CLOUD_PROJECT` and `GOOGLE_CLOUD_LOCATION`, as shown below:

```bash
export GOOGLE_GENAI_USE_VERTEXAI=true
export GOOGLE_CLOUD_PROJECT='your-project-id'
export GOOGLE_CLOUD_LOCATION='us-central1'
```

```csharp
using Google.GenAI;

client = new Client();
```

## Types
Parameter types are specified in the `Google.GenAI.Types` namespace.

## Models
The `client.Models` module exposes model inferencing. See `Create a client`
section above to initialize a client.

### Generate Content

#### With simple text content

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class GenerateContentSimpleText {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();
    var response = await client.Models.GenerateContentAsync(
      model: "gemini-2.0-flash", contents: "why is the sky blue?"
    );
    Console.WriteLine(response.Candidates[0].Content.Parts[0].Text);
  }
}
```
#### System Instructions and Other Configs

The output of the model can be influenced by several optional settings
available in GenerateContentAsync's config parameter. For example, to make a model more
deterministic, lowering the `Temperature` parameter reduces randomness, with
values near 0 minimizing variability. Capabilities and parameter defaults for
each model is shown in the
[Vertex AI docs](https://cloud.google.com/vertex-ai/generative-ai/docs/models/gemini/2-5-flash)
and [Gemini API docs](https://ai.google.dev/gemini-api/docs/models) respectively.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class GenerateContentWithConfig {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();
    var generateContentConfig =
    new GenerateContentConfig {
        SystemInstruction = new Content
        {
          Parts = new List<Part> {
              new Part {Text = "I say high you say low."}
          }
        },
        Temperature = 0.1,
        MaxOutputTokens = 3
    };
    var response = await client.Models.GenerateContentAsync(
        model: "gemini-2.0-flash",
        contents: "high",
        config: generateContentConfig
    );
    Console.WriteLine(response.Candidates[0].Content.Parts[0].Text);
  }
}

```

#### Safety Settings

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

class GenerateContentWithSafetySettings {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var safetySettings = new List<SafetySetting> {
      new SafetySetting {
        Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH,
        Threshold = HarmBlockThreshold.BLOCK_LOW_AND_ABOVE
      }
    };
    var generateContentConfig = new GenerateContentConfig
    {
      SafetySettings =  new List<SafetySetting>(safetySettings)
    };
    var response = await client.Models.GenerateContentAsync(
        model: "gemini-2.0-flash",
        contents: "say something hateful",
        config: generateContentConfig
    );
    Console.WriteLine(response.Candidates[0].SafetyRatings);
  }
}
```

#### Json response scehema

However you define your schema, don't duplicate it in your input prompt,
including by giving examples of expected JSON output. If you do, the generated
output might be lower in quality.

```csharp
using System.Threading.Tasks;
using Goolge.GenAI;
using Google.GenAI.Types;

public class GenerateContentWithJsonSchema {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    // define the response schema you desire
    Schema countryInfo = new Schema {
      Properties =
        new Dictionary<string, Schema> {
          {
            "title", new Schema { Type = Type.STRING, Title = "Title" }
          },
          {
            "population", new Schema { Type = Type.INTEGER, Title = "Population" }
          },
          {
            "capital", new Schema { Type = Type.STRING, Title = "Capital" }
          },
          {
            "continent", new Schema { Type = Type.STRING, Title = "Continent" }
          },
          {
            "language", new Schema { Type = Type.STRING, Title = "Language" }
          }
        },
      PropertyOrdering =
          new List<string> { "title", "population", "capital", "continent", "language" },
      Required = new List<string> { "title", "population", "capital", "continent", "language" },
      Title = "CountryInfo", Type = Type.OBJECT
    };

    var response = await client.Models.GenerateContentAsync(
        model: "gemini-2.0-flash",
        contents: "Give me information about Australia",
        config: new GenerateContentConfig {
            ResponseMimeType = "application/json",
            ResponseSchema = countryInfo
        }
    );

    string text = response.Candidates[0].Content.Parts[0].Text;
    var parsedText = JsonSerializer.Deserialize<Dictionary<string, object>>(text);
    Console.WriteLine(parsedText);
  }
}

```

### Generate Content Stream

The usage of GenerateContentStreamAsync is similar to GenerateContentAsync, this section shows one simple example to showcase the nuance in the usage.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

class GenerateContentStreamSimpleText {
  public static async Task main() {

    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();
    await foreach (var chunk in client.Models.GenerateContentStreamAsync(
        model: "gemini-2.0-flash",
        contents: "why is the sky blue?"
    )) {
        Console.WriteLine(chunk.Candidates[0].Content.Parts[0].Text);
    }
  }
}

```

### Generate Images

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class GenerateImagesSimple {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();
    var generateImagesConfig = new GenerateImagesConfig
    {
      NumberOfImages = 1,
      AspectRatio = "1:1",
      SafetyFilterLevel = SafetyFilterLevel.BLOCK_LOW_AND_ABOVE,
      PersonGeneration = PersonGeneration.DONT_ALLOW,
      IncludeSafetyAttributes = true,
      IncludeRaiReason = true,
      OutputMimeType = "image/jpeg",
    };
    var response = await client.Models.GenerateImagesAsync(
      model: "imagen-3.0-generate-002",
      prompt: "Red skateboard",
      config: generateImagesConfig
    );
    // Do something with the generated image
    var image = response.GeneratedImages.First().Image;
  }
}
```

### Upscale Image

Upscaling an image is only supported on the Vertex AI client.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class UpscaleImageSimple {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();
    var upscaleImageConfig = new UpscaleImageConfig {
      OutputMimeType = "image/jpeg", EnhanceInputImage = true
    };
    var image; // Image to upscale here
    var response = await client.Models.UpscaleImageAsync(
      model: modelName, image: image, upscaleFactor: "x2",
      config: upscaleImageConfig);

    // Do something with the generated image
    var image = response.GeneratedImages.First().Image;
  }
}
```

### Edit Image

Edit image uses a separate model from generate and upscale.

Edit image is only supported in the Vertex AI client.

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class EditImageSimple {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    List<IReferenceImage> referenceImages = new List<IReferenceImage>();

    // Raw reference image
    var rawReferenceImage = new RawReferenceImage {
      ReferenceImage = Image.FromFile("path/to/file", "image/png"),
      ReferenceId = 1,
    };
    referenceImages.Add(rawReferenceImage);

    // Mask reference image, generated by model
    var maskReferenceImage = new MaskReferenceImage {
      ReferenceId = 2,
      Config =
          new MaskReferenceConfig {
            MaskMode = MaskReferenceMode.MASK_MODE_BACKGROUND,
            MaskDilation = 0.06,
          }
    };
    referenceImages.Add(maskReferenceImage);

    var editImageConfig = new EditImageConfig {
      EditMode = EditMode.EDIT_MODE_INPAINT_INSERTION,
      NumberOfImages = 1,
      OutputMimeType = "image/jpeg",
    };

    var editImageResponse = await vertexClient.Models.EditImageAsync(
        model: "imagen-3.0-capability-001",
        prompt: "Change the colors of [1] using the mask [2]",
        referenceImages: referenceImages,
        config: editImageConfig);

    // Do something with the generated image
    var image = editImageResponse.GeneratedImages.First().Image;
  }
}
```

### Segment Image

Segment image is only supported in the Vertex AI client.

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class SegmentImageSimple {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var segmentImageConfig = new SegmentImageConfig {
      Mode = SegmentMode.BACKGROUND,
      MaxPredictions = 1,
    };

    var segmentImageResponse = await vertexClient.Models.SegmentImageAsync(
        model: modelName,
        source: new SegmentImageSource {
          Image = Image.FromFile("path/to/image.png", "image/png"),
        },
        config: segmentImageConfig);

    // Do something with the generated mask
    var mask = segmentImageResponse.GeneratedMasks.First().Mask;
  }
}
```

### Count Tokens

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class CountTokensExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var response = await client.Models.CountTokensAsync(
      model: "gemini-2.0-flash",
      contents: "What is the capital of France?"
    );

    Console.Writeline(response.TotalTokens);
  }
}

```

### Compute Tokens

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class ComputeTokensExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var response = await client.Models.ComputeTokensAsync(
      model: "gemini-2.0-flash",
      contents: "What is the capital of France?"
    );

    Console.Writeline(response.TokensInfo);
  }
}

```

### Embed Content

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class EmbedContentExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var response = await client.Models.EmbedContentAsync(
      model: "text-embedding-004",
      contents: "What is the capital of France?"
    );

    Console.WriteLine(response.Embeddings[0].Values);
  }
}
```

### Get Model

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class GetModelExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    // get a base model
    var baseModelResponse = await client.Models.GetAsync(
      model: "gemini-2.5-flash"
    );

    // get a tuned model
    var tunedModelResponse = await client.Models.GetAsync(
      model: "models/your-tuned-model"
    );
  }
}
```

### Update Model

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class UpdateModelExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    var response = await client.Models.UpdateAsync(
      model: "models/your-tuned-model",
      config: new UpdateModelConfig { Description = "updated model description" }
    );

    Console.WriteLine(response.Description);
  }
}
```

### Delete Model

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class DeleteModelExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    await client.Models.DeleteAsync(
      model: "models/your-tuned-model"
    );
  }
}
```

### List Models

The `ListAsync` method returns a `Pager` object that allows you to iterate through pages of models. If `QueryBase` is set to `true` (the default) in `ListModelsConfig`, it lists base models; otherwise, it lists tuned models.

```csharp
using Google.GenAI;
using Google.GenAI.Types;

public class ListModelsExample {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client();

    // List base models with default settings
    Console.WriteLine("Base Models:");
    var pager = await client.Models.ListAsync();
    await foreach(var model in pager)
    {
        Console.WriteLine(model.Name);
    }

    // List tuned models with a page size of 10
    Console.WriteLine("Tuned Models:");
    var config = new ListModelsConfig { QueryBase = false, PageSize = 10 };
    var tunedModelsPager = await client.Models.ListAsync(config);
    await foreach(var model in tunedModelsPager)
    {
        Console.WriteLine(model.Name);
    }
  }
}
```

## Batches
The `client.Batches` module can be used to manage batch jobs.
See `Create a client` section above to initialize a client.

### Create Batch Job
Batch jobs can be created from GCS URIs or BigQuery URIs when using Vertex AI, or from Files or Inlined Requests when using the Gemini API.

#### With GCS URI (Vertex AI only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateBatchWithGcs {
    public static async Task main()
    {
        // assuming credentials are set up for Vertex AI in environment variables as instructed above.
        var client = new Client();
        var src = new BatchJobSource
        {
            GcsUri = new List<string> { "gs://unified-genai-tests/batches/input/generate_content_requests.jsonl" },
            Format = "jsonl"
        };
        var config = new CreateBatchJobConfig
        {
            DisplayName = "test_batch_gcs",
            Dest = new BatchJobDestination
            {
                GcsUri = "gs://unified-genai-tests/batches/output",
                Format = "jsonl"
            }
        };
        var response = await client.Batches.CreateAsync("gemini-2.5-flash", src, config);
        Console.WriteLine($"Created Vertex AI batch job: {response.Name}");
    }
}
```

#### With BigQuery URI (Vertex AI only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateBatchWithBigQuery {
    public static async Task main()
    {
        // assuming credentials are set up for Vertex AI in environment variables as instructed above.
        var client = new Client();
        var src = new BatchJobSource
        {
            BigqueryUri = "bq://storage-samples.generative_ai.batch_requests_for_multimodal_input",
            Format = "bigquery"
        };
        var config = new CreateBatchJobConfig
        {
            DisplayName = "test_batch_bigquery",
            Dest = new BatchJobDestination
            {
                BigqueryUri = "bq://REDACTED.unified_genai_tests_batches.generate_content_output",
                Format = "bigquery"
            }
        };
        var response = await client.Batches.CreateAsync("gemini-2.5-flash", src, config);
        Console.WriteLine($"Created Vertex AI batch job: {response.Name}");
    }
}
```

#### With Inlined Requests (Gemini API only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateBatchWithInlinedRequests {
    public static async Task main()
    {
        // assuming credentials are set up for Gemini API in environment variables as instructed above.
        var client = new Client();
        var safetySettings = new List<SafetySetting>
        {
            new SafetySetting { Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH, Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH }
        };
        var inlineRequest = new InlinedRequest
        {
            Contents = new List<Content>
            {
                new Content
                {
                    Parts = new List<Part> { new Part { Text = "Hello!" } },
                    Role = "user"
                }
            },
            Metadata = new Dictionary<string, string> { { "key", "request-1" } },
            Config = new GenerateContentConfig
            {
                SafetySettings = safetySettings
            }
        };
        var src = new BatchJobSource
        {
            InlinedRequests = new List<InlinedRequest> { inlineRequest }
        };
        var config = new CreateBatchJobConfig
        {
            DisplayName = "test_batch_inlined"
        };
        var response = await client.Batches.CreateAsync("gemini-2.5-flash", src, config);
        Console.WriteLine($"Created Gemini API batch job: {response.Name}");
    }
}
```

#### With File Name (Gemini API only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateBatchWithFile {
    public static async Task main()
    {
        // assuming credentials are set up for Gemini API in environment variables as instructed above.
        var client = new Client();
        var src = new BatchJobSource
        {
            FileName = "files/your-file-id"
        };
        var config = new CreateBatchJobConfig
        {
            DisplayName = "test_batch_file"
        };
        var response = await client.Batches.CreateAsync("gemini-2.5-flash", src, config);
        Console.WriteLine($"Created Gemini API batch job: {response.Name}");
    }
}
```

### Create Embedding Batch Job
Embedding batch jobs are only supported in Gemini API, from Files or Inlined Requests.

#### With Inlined Requests (Gemini API only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateEmbeddingBatchWithInlinedRequests {
    public static async Task main()
    {
        // assuming credentials are set up for Gemini API in environment variables as instructed above.
        var client = new Client();
        var src = new EmbeddingsBatchJobSource
        {
            InlinedRequests = new EmbedContentBatch
            {
                Config = new EmbedContentConfig { OutputDimensionality = 64 },
                Contents = new List<Content>
                {
                    new Content { Parts = new List<Part> { new Part { Text = "1" } } },
                    new Content { Parts = new List<Part> { new Part { Text = "2" } } },
                    new Content { Parts = new List<Part> { new Part { Text = "3" } } },
                }
            }
        };
        var config = new CreateEmbeddingsBatchJobConfig
        {
            DisplayName = "test_batch_embedding_inlined"
        };
        var response = await client.Batches.CreateEmbeddingsAsync("gemini-embedding-001", src, config);
        Console.WriteLine($"Created Gemini API embedding batch job: {response.Name}");
    }
}
```

#### With File Name (Gemini API only)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateEmbeddingBatchWithFile {
    public static async Task main()
    {
        // assuming credentials are set up for Gemini API in environment variables as instructed above.
        var client = new Client();
        var src = new EmbeddingsBatchJobSource
        {
            FileName = "files/your-file-id",
        };
        var config = new CreateEmbeddingsBatchJobConfig
        {
            DisplayName = "test_batch_embedding_file"
        };
        var response = await client.Batches.CreateEmbeddingsAsync("gemini-embedding-001", src, config);
        Console.WriteLine($"Created Gemini API embedding batch job: {response.Name}");
    }
}
```

### Get Batch Job
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class GetBatchJob {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        // Use a batch job name from a previously created batch job.
        var batchJob = await client.Batches.GetAsync(name: "batches/your-batch-job-name");
        Console.WriteLine($"Batch job name: {batchJob.Name}, State: {batchJob.State}");
    }
}
```

### Cancel Batch Job
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CancelBatchJob {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        // Use a batch job name from a previously created batch job.
        await client.Batches.CancelAsync(name: "batches/your-batch-job-name");
        Console.WriteLine("Batch job cancelled.");
    }
}
```

### List Batch Jobs
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class ListBatchJobs {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        var pager = await client.Batches.ListAsync(new ListBatchJobsConfig { PageSize = 10 });

        await foreach(var job in pager)
        {
            Console.WriteLine($"Batch job name: {job.Name}, State: {job.State}");
        }
    }
}
```

### Delete Batch Job
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class DeleteBatchJob {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        // Use a batch job name from a previously created batch job.
        await client.Batches.DeleteAsync(name: "batches/your-batch-job-name");
        Console.WriteLine("Batch job deleted.");
    }
}
```

## Caches
The `client.Caches` module can be used to manage cached content.
See `Create a client` section above to initialize a client.

### Create Cache
Cacheable content can be created from Google Cloud Storage URIs when using Vertex AI, or from File URIs when using the Gemini API.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateCache {
    public static async Task main()
    {
        // Example for Vertex AI with GCS URIs:
        var vertexClient = new Client(project: project, location: location, vertexAI: true);
        var vertexConfig = new CreateCachedContentConfig {
            Contents = new List<Content> {
                new Content {
                    Role = "user",
                    Parts = new List<Part> {
                        new Part { FileData = new FileData { FileUri = "gs://cloud-samples-data/generative-ai/pdf/2312.11805v3.pdf", MimeType = "application/pdf" } },
                        new Part { FileData = new FileData { FileUri = "gs://cloud-samples-data/generative-ai/pdf/2403.05530.pdf", MimeType = "application/pdf" } }
                    }
                }
            },
            DisplayName = "my-vertex-cache",
            Ttl = "600s"
        };
        var vertexResponse = await vertexClient.Caches.CreateAsync(model: "gemini-2.5-flash", config: vertexConfig);
        Console.WriteLine($"Created Vertex AI cache: {vertexResponse.Name}");

        // Example for Gemini API with File URIs:
        var geminiClient = new Client(apiKey: apiKey);
        var geminiConfig = new CreateCachedContentConfig {
            Contents = new List<Content> {
                new Content {
                    Role = "user",
                    Parts = new List<Part> {
                        new Part { FileData = new FileData { FileUri = "https://generativelanguage.googleapis.com/v1beta/files/file-id", MimeType = "application/pdf" } }
                    }
                }
            },
            DisplayName = "my-gemini-cache",
            Ttl = "600s"
        };
        var geminiResponse = await geminiClient.Caches.CreateAsync(model: "gemini-2.5-flash", config: geminiConfig);
        Console.WriteLine($"Created Gemini API cache: {geminiResponse.Name}");
    }
}
```

### Get Cache
```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class GetCache {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        // Use a cache name from a previously created cache.
        var cachedContent = await client.Caches.GetAsync(name: "cachedContents/your-cache-name", config: null);
        Console.WriteLine($"Cache name: {cachedContent.Name}, DisplayName: {cachedContent.DisplayName}");
    }
}
```

### Update Cache
```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class UpdateCache {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        var updateConfig = new UpdateCachedContentConfig { Ttl = "1200s" };
        // Use a cache name from a previously created cache.
        var cachedContent = await client.Caches.UpdateAsync(name: "cachedContents/your-cache-name", config: updateConfig);
        Console.WriteLine($"Cache updated. New expiration time: {cachedContent.ExpireTime}");
    }
}
```

### Delete Cache
```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class DeleteCache {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        // Use a cache name from a previously created cache.
        await client.Caches.DeleteAsync(name: "cachedContents/your-cache-name", config: null);
        Console.WriteLine("Cache deleted.");
    }
}
```

### List Caches
```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class ListCaches {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        var pager = await client.Caches.ListAsync(new ListCachedContentConfig { PageSize = 10 });

        await foreach(var cache in pager)
        {
            Console.WriteLine($"Cache name: {cache.Name}, DisplayName: {cache.DisplayName}");
        }
    }
}
```

## Tunings
The `client.Tunings` module exposes model tuning. See `Create a client`
section above to initialize a client.

### Create Tuning Job (Vertex Only)

#### With simple training data

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateTuningJobSimple {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client(vertexAI: true);
    var trainingDataset = new TuningDataset {
      GcsUri = "gs://cloud-samples-data/ai-platform/generative_ai/gemini-2_0/text/sft_train_data.jsonl"
    };
    var tuningJob = await client.Tunings.TuneAsync(
      baseModel: "gemini-2.5-flash",
      trainingDataset: trainingDataset,
    );
    Console.WriteLine(tuningJob.State);
  }
}
```

#### Hyperparameters and Other Configs

The tuning job can be configured by several optional settings
available in Tune's config parameter. For example, we can configure
the number of epochs to train for, or specify a validation dataset.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class CreateTuningJobWithConfig {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client(vertexAI: true);
    var trainingDataset = new TuningDataset {
      GcsUri = "gs://cloud-samples-data/ai-platform/generative_ai/gemini-2_0/text/sft_train_data.jsonl"
    };
    var validationDataset = new TuningValidationDataset {
      GcsUri = "gs://cloud-samples-data/ai-platform/generative_ai/gemini-2_0/text/sft_validation_data.jsonl"
    };
    var config = new CreateTuningJobConfig {
      TunedModelDisplayName = "Tuned Model",
      EpochCount = 3,
      LearningRateMultiplier = 0.5,
      ValidationDataset = validationDataset,
    };
    var tuningJob = await client.Tunings.TuneAsync(
      baseModel: "gemini-2.5-flash",
      trainingDataset: trainingDataset,
      config: config,
    );
    Console.WriteLine(tuningJob.State);
  }
}
```

#### Preference Tuning

You can perform preference tuning by setting `Method` to `TuningMethod.PREFERENCE_TUNING` in `CreateTuningJobConfig`.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class PreferenceTuningJob {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client(vertexAI: true);
    var trainingDataset = new TuningDataset {
      GcsUri = "gs://cloud-samples-data/ai-platform/generative_ai/gemini-1_5/text/sft_train_data.jsonl"
    };
    var config = new CreateTuningJobConfig {
      TunedModelDisplayName = "Tuned Model",
      Method = TuningMethod.PREFERENCE_TUNING,
      EpochCount = 1,
    };
    var tuningJob = await client.Tunings.TuneAsync(
      baseModel: "gemini-2.5-flash",
      trainingDataset: trainingDataset,
      config: config,
    );
    Console.WriteLine(tuningJob.State);
  }
}
```

#### Continuous Tuning

You can perform continuous tuning on a previously tuned model by passing
the tuned model's resource name as the `baseModel` parameter.

```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class ContinuousTuningJob {
  public static async Task main() {
    // assuming credentials are set up in environment variables as instructed above.
    var client = new Client(vertexAI: true);
    var trainingDataset = new TuningDataset {
      GcsUri = "gs://cloud-samples-data/ai-platform/generative_ai/gemini-2_0/text/sft_train_data.jsonl"
    };
    // Continuously tune a previously tuned model by passing in its resource name.
    var tunedModelResourceName = "models/your-tuned-model";
    var tuningJob = await client.Tunings.TuneAsync(
      baseModel: tunedModelResourceName,
      trainingDataset: trainingDataset,
    );
    Console.WriteLine(tuningJob.State);
  }
}
```

### List Tuning Jobs
```csharp
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

public class ListTuningJobs {
    public static async Task main()
    {
        // assuming credentials are set up in environment variables as instructed above.
        var client = new Client();
        var pager = await client.Tunings.ListAsync(new ListTuningJobsConfig { PageSize = 2 });

        await foreach(var tuningJob in pager)
        {
            Console.WriteLine($"Tuning job name: {tuningJob.Name}, State: {tuningJob.State}");
        }
    }
}
```
