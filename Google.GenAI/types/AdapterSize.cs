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

// Auto-generated code. Do not edit.

using System.Text.Json.Serialization;

namespace Google.GenAI.Types {
  /// <summary>
  /// Adapter size for tuning. This enum is not supported in Gemini API.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum AdapterSize {
    /// <summary>
    /// Adapter size is unspecified.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_UNSPECIFIED")] ADAPTER_SIZE_UNSPECIFIED,

    /// <summary>
    /// Adapter size 1.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_ONE")] ADAPTER_SIZE_ONE,

    /// <summary>
    /// Adapter size 2.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_TWO")] ADAPTER_SIZE_TWO,

    /// <summary>
    /// Adapter size 4.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_FOUR")] ADAPTER_SIZE_FOUR,

    /// <summary>
    /// Adapter size 8.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_EIGHT")] ADAPTER_SIZE_EIGHT,

    /// <summary>
    /// Adapter size 16.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_SIXTEEN")] ADAPTER_SIZE_SIXTEEN,

    /// <summary>
    /// Adapter size 32.
    /// </summary>
    [JsonPropertyName("ADAPTER_SIZE_THIRTY_TWO")] ADAPTER_SIZE_THIRTY_TWO
  }
}
