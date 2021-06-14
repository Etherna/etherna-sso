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

using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Etherna.SSOServer.Pages
{
    public class RedirectModel : PageModel
    {
        // Methods.
        public string RedirectUrl { get; private set; } = default!;

        // Properties.
        public void OnGet(string redirectUrl)
        {
            if (redirectUrl is null)
                throw new ArgumentNullException(nameof(redirectUrl));

            RedirectUrl = redirectUrl;
        }
    }
}
