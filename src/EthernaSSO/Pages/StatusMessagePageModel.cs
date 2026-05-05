// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
//
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SSOServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Pages
{
    /// <summary>
    /// Base class for Razor Pages that need to display a status message across redirects via TempData.
    /// Stores message text and type as primitive TempData values, since
    /// <see cref="Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure.DefaultTempDataSerializer"/>
    /// only supports primitive types.
    /// </summary>
    public abstract class StatusMessagePageModel : PageModel
    {
        [TempData]
        public string? StatusMessageText { get; set; }

        [TempData]
        public int StatusMessageTypeValue { get; set; }

        public StatusMessage? StatusMessage
        {
            get => StatusMessageText is not null
                ? new StatusMessage(StatusMessageText, (StatusMessageType)StatusMessageTypeValue)
                : null;
            set
            {
                StatusMessageText = value?.Text;
                StatusMessageTypeValue = (int)(value?.Type ?? StatusMessageType.Success);
            }
        }
    }
}
