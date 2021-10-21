//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Pages.SharedModels
{
    public class PageSelectorModel
    {
        public PageSelectorModel(
            int currentPage,
            int maxPage,
            string pageParamName = "p",
            Dictionary<string, string>? routeData = null)
        {
            if (currentPage < 0)
                throw new ArgumentOutOfRangeException(nameof(currentPage), "Value can't be negative");
            if (maxPage < 0)
                throw new ArgumentOutOfRangeException(nameof(maxPage), "Value can't be negative");

            CurrentPage = currentPage;
            MaxPage = maxPage;
            PageParamName = pageParamName ?? throw new ArgumentNullException(nameof(pageParamName));
            RouteData = routeData ?? new Dictionary<string, string>();
        }

        public int CurrentPage { get; }
        public int MaxPage { get; }
        public string PageParamName { get; }
        public IDictionary<string, string> RouteData { get; }
    }
}
