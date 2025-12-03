/*
 * Copyright 2025 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Manually-generated code. Do not remove.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Google.GenAI.Serialization;

namespace Google.GenAI.Types {
  /// <summary>
  /// An abstract operation.
  /// </summary>

  public abstract record Operation<TOperation> {
    /// <summary>
    /// The server-assigned name, which is only unique within the same service that originally
    /// returns it. If you use the default HTTP mapping, the `name` should be a resource name ending
    /// with `operations/{unique_id}`.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ? Name { get; set; }

    /// <summary>
    /// Service-specific metadata associated with the operation. It typically contains progress
    /// information and common metadata such as create time. Some services might not provide such
    /// metadata. Any method that returns a long-running operation should document the metadata
    /// type, if any.
    /// </summary>
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>
        ? Metadata {
            get; set;
          }

    /// <summary>
    /// If the value is `false`, it means the operation is still in progress. If `true`, the
    /// operation is completed, and either `error` or `response` is available.
    /// </summary>
    [JsonPropertyName("done")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool
        ? Done {
            get; set;
          }

    /// <summary>
    /// The error result of the operation in case of failure or cancellation.
    /// </summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>
        ? Error {
            get; set;
          }

    /// <summary>
    /// Creates a new Operation object from an API response.
    /// </summary>
    public abstract TOperation FromApiResponse(JsonNode response, bool isVertexAI);
  }
}
