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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI.Types;

namespace Google.GenAI
{
    /// <summary>
    /// Pager class for iterating through paginated results.
    /// Supports asynchronous enumeration via <c>await foreach</c> and explicit page fetching via <c>NextPageAsync()</c>.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the pager.</typeparam>
    /// <typeparam name="TConfig">The type of configuration for list requests.</typeparam>
    /// <typeparam name="TResponse">The type of response from list requests.</typeparam>
    public class Pager<TItem, TConfig, TResponse> : IAsyncEnumerable<TItem>
        where TItem : class
    {
        private readonly Func<TConfig, Task<TResponse>> _requestFunc;
        private readonly Func<TResponse, IReadOnlyList<TItem>?> _extractItems;
        private readonly Func<TResponse, string?> _extractNextPageToken;
        private readonly Func<TResponse, HttpResponse?> _extractHttpResponse;
        private readonly Func<TConfig, string, TConfig> _updateConfigPageToken;
        private readonly int _requestedPageSize;

        private TConfig _currentConfig;
        private IReadOnlyList<TItem> _currentPage;
        private string? _nextPageToken;
        private HttpResponse? _sdkHttpResponse;

        /// <summary>
        /// Constructs a Pager.
        /// </summary>
        /// <param name="requestFunc">Function to fetch a page given a config.</param>
        /// <param name="extractItems">Function to extract items from a response.</param>
        /// <param name="extractNextPageToken">Function to extract next page token from a response.</param>
        /// <param name="extractHttpResponse">Function to extract HTTP response from a response.</param>
        /// <param name="updateConfigPageToken">Function to update config with a new page token.</param>
        /// <param name="initialConfig">Initial configuration for the first request.</param>
        /// <param name="initialResponse">Initial response from the first request.</param>
        /// <param name="requestedPageSize">The requested page size.</param>
        public Pager(
            Func<TConfig, Task<TResponse>> requestFunc,
            Func<TResponse, IReadOnlyList<TItem>?> extractItems,
            Func<TResponse, string?> extractNextPageToken,
            Func<TResponse, HttpResponse?> extractHttpResponse,
            Func<TConfig, string, TConfig> updateConfigPageToken,
            TConfig initialConfig,
            TResponse initialResponse,
            int requestedPageSize)
        {
            _requestFunc = requestFunc ?? throw new ArgumentNullException(nameof(requestFunc));
            _extractItems = extractItems ?? throw new ArgumentNullException(nameof(extractItems));
            _extractNextPageToken = extractNextPageToken ?? throw new ArgumentNullException(nameof(extractNextPageToken));
            _extractHttpResponse = extractHttpResponse ?? throw new ArgumentNullException(nameof(extractHttpResponse));
            _updateConfigPageToken = updateConfigPageToken ?? throw new ArgumentNullException(nameof(updateConfigPageToken));
            _currentConfig = initialConfig ?? throw new ArgumentNullException(nameof(initialConfig));
            _requestedPageSize = requestedPageSize;
            _currentPage = new List<TItem>();

            InitializeFromResponse(initialResponse);
        }

        /// <summary>
        /// Gets the current page of items.
        /// </summary>
        public IReadOnlyList<TItem> CurrentPage => _currentPage;

        /// <summary>
        /// Gets the requested page size.
        /// </summary>
        public int PageSize => _requestedPageSize > 0 ? _requestedPageSize : _currentPage.Count;

        /// <summary>
        /// Gets the number of items in the current page.
        /// </summary>
        public int Count => _currentPage.Count;

        /// <summary>
        /// Gets a value indicating whether there are more pages available.
        /// </summary>
        public bool HasMorePages => !string.IsNullOrEmpty(_nextPageToken);

        /// <summary>
        /// Gets the HTTP response for the current page.
        /// </summary>
        public HttpResponse? SdkHttpResponse => _sdkHttpResponse;

        /// <summary>
        /// Fetches the next page of items. This makes a new API request.
        /// </summary>
        /// <returns>A <see cref="Task{Boolean}"/> that represents the asynchronous operation. The task result is <c>true</c> if the next page was successfully fetched, or <c>false</c> if there are no more pages available.</returns>
        public async Task<bool> NextPageAsync()
        {
            if (!HasMorePages)
            {
                return false;
            }

            _currentConfig = _updateConfigPageToken(_currentConfig, _nextPageToken!);
            var response = await _requestFunc(_currentConfig);
            InitializeFromResponse(response);
            return true;
        }

        /// <summary>
        /// Returns an asynchronous enumerator that iterates through all items across all pages.
        /// </summary>
        public async IAsyncEnumerator<TItem> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            var currentIndex = 0;
            while (true)
            {
                if (currentIndex < _currentPage.Count)
                {
                    yield return _currentPage[currentIndex];
                    currentIndex++;
                }
                else if (HasMorePages)
                {
                    await NextPageAsync();
                    currentIndex = 0;
                }
                else
                {
                    break;
                }
            }
        }


        private void InitializeFromResponse(TResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            _currentPage = _extractItems(response) ?? new List<TItem>();
            _nextPageToken = _extractNextPageToken(response);
            _sdkHttpResponse = _extractHttpResponse(response);
        }
    }
}
