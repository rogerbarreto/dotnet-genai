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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Google.GenAI;

namespace Google.GenAI.Tests
{
    [TestClass]
    public class CommonTest
    {
        [TestMethod]
        public void MoveValueByPath_Wildcard()
        {
            var data = new JsonObject
            {
                ["requests"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "1" }
                                }
                            }
                        },
                        ["outputDimensionality"] = 64
                    },
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "2" }
                                }
                            }
                        },
                        ["outputDimensionality"] = 64
                    },
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "3" }
                                }
                            }
                        },
                        ["outputDimensionality"] = 64
                    }
                }
            };

            var paths = new Dictionary<string, string>
            {
                { "requests[].*", "requests[].request.*" }
            };

            Common.MoveValueByPath(data, paths);

            var expected = new JsonObject
            {
                ["requests"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "1" }
                                }
                            },
                            ["outputDimensionality"] = 64
                        }
                    },
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "2" }
                                }
                            },
                            ["outputDimensionality"] = 64
                        }
                    },
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = new JsonObject
                            {
                                ["parts"] = new JsonArray
                                {
                                    new JsonObject { ["text"] = "3" }
                                }
                            },
                            ["outputDimensionality"] = 64
                        }
                    }
                }
            };

            Assert.AreEqual(expected.ToJsonString(), data.ToJsonString());
        }

        [TestMethod]
        public void MoveValueByPath_DocstringExample()
        {
            var data = new JsonObject
            {
                ["requests"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["content"] = "v1"
                    },
                    new JsonObject
                    {
                        ["content"] = "v2"
                    }
                }
            };

            var paths = new Dictionary<string, string>
            {
                { "requests[].*", "requests[].request.*" }
            };

            Common.MoveValueByPath(data, paths);

            var expected = new JsonObject
            {
                ["requests"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = "v1"
                        }
                    },
                    new JsonObject
                    {
                        ["request"] = new JsonObject
                        {
                            ["content"] = "v2"
                        }
                    }
                }
            };
            Assert.AreEqual(expected.ToJsonString(), data.ToJsonString());
        }
    }
}
