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
  /// Job state.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum JobState {
    /// <summary>
    /// The job state is unspecified.
    /// </summary>
    [JsonPropertyName("JOB_STATE_UNSPECIFIED")] JOB_STATE_UNSPECIFIED,

    /// <summary>
    /// The job has been just created or resumed and processing has not yet begun.
    /// </summary>
    [JsonPropertyName("JOB_STATE_QUEUED")] JOB_STATE_QUEUED,

    /// <summary>
    /// The service is preparing to run the job.
    /// </summary>
    [JsonPropertyName("JOB_STATE_PENDING")] JOB_STATE_PENDING,

    /// <summary>
    /// The job is in progress.
    /// </summary>
    [JsonPropertyName("JOB_STATE_RUNNING")] JOB_STATE_RUNNING,

    /// <summary>
    /// The job completed successfully.
    /// </summary>
    [JsonPropertyName("JOB_STATE_SUCCEEDED")] JOB_STATE_SUCCEEDED,

    /// <summary>
    /// The job failed.
    /// </summary>
    [JsonPropertyName("JOB_STATE_FAILED")] JOB_STATE_FAILED,

    /// <summary>
    /// The job is being cancelled. From this state the job may only go to either
    /// `JOB_STATE_SUCCEEDED`, `JOB_STATE_FAILED` or `JOB_STATE_CANCELLED`.
    /// </summary>
    [JsonPropertyName("JOB_STATE_CANCELLING")] JOB_STATE_CANCELLING,

    /// <summary>
    /// The job has been cancelled.
    /// </summary>
    [JsonPropertyName("JOB_STATE_CANCELLED")] JOB_STATE_CANCELLED,

    /// <summary>
    /// The job has been stopped, and can be resumed.
    /// </summary>
    [JsonPropertyName("JOB_STATE_PAUSED")] JOB_STATE_PAUSED,

    /// <summary>
    /// The job has expired.
    /// </summary>
    [JsonPropertyName("JOB_STATE_EXPIRED")] JOB_STATE_EXPIRED,

    /// <summary>
    /// The job is being updated. Only jobs in the `JOB_STATE_RUNNING` state can be updated. After
    /// updating, the job goes back to the `JOB_STATE_RUNNING` state.
    /// </summary>
    [JsonPropertyName("JOB_STATE_UPDATING")] JOB_STATE_UPDATING,

    /// <summary>
    /// The job is partially succeeded, some results may be missing due to errors.
    /// </summary>
    [JsonPropertyName("JOB_STATE_PARTIALLY_SUCCEEDED")] JOB_STATE_PARTIALLY_SUCCEEDED
  }
}
