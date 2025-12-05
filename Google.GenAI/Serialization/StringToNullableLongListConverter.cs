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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Google.GenAI.Serialization
{
  internal class StringToNullableLongListConverter : JsonConverter<List<long>?>
  {
    public override List<long>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType == JsonTokenType.Null)
      {
        return null;
      }

      if (reader.TokenType != JsonTokenType.StartArray)
      {
        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing List<long>. Expected StartArray.");
      }

      var list = new List<long>();

      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndArray)
        {
          return list;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
          string? stringValue = reader.GetString();
          if (long.TryParse(stringValue, out long longValue))
          {
            list.Add(longValue);
          }
          else
          {
            throw new JsonException($"Could not parse string '{stringValue}' to long.");
          }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
          list.Add(reader.GetInt64());
        }
        else
        {
          throw new JsonException($"Unexpected token type {reader.TokenType} in array. Expected String or Number.");
        }
      }

      throw new JsonException("Unexpected end of JSON while reading array.");
    }

    public override void Write(Utf8JsonWriter writer, List<long>? value, JsonSerializerOptions options)
    {
      if (value == null)
      {
        writer.WriteNullValue();
        return;
      }

      writer.WriteStartArray();
      foreach (var item in value)
      {
        writer.WriteNumberValue(item);
      }
      writer.WriteEndArray();
    }
  }
}
