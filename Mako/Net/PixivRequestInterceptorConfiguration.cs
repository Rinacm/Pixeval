#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mako.Util;

namespace Mako.Net
{
    public class PixivRequestInterceptorConfiguration : IInterceptConfigurations
    {
        public IReadOnlyDictionary<string, string> Configurations { get; }

        public PixivRequestInterceptorConfiguration()
        {
            Configurations = new Dictionary<string, string>
            {
                ["ApiHost"] = "^app-api\\.pixiv\\.net$",
                ["WebApiHost"] = "^(pixiv\\.net)|(www\\.pixiv\\.net)$",
                ["OAuthHost"] = "^oauth\\.secure\\.pixiv\\.net$",
                ["BypassHost"] = "^app-api\\.pixiv\\.net$|^(pixiv\\.net)|(www\\.pixiv\\.net)$",
                ["ImageHost"] = "i.pximg.net"
            };
        }

        public string Token { get; set; }

        public string Cookie { get; set; }

        public bool Bypass { get; set; } = true;

        public string MirrorHost { get; set; }

        public string ImageHost => Configurations[nameof(ImageHost)];

        public Regex ApiHost => Configurations[nameof(ApiHost)].ToRegex();

        public Regex WebApiHost => Configurations[nameof(WebApiHost)].ToRegex();

        public Regex OAuthHost => Configurations[nameof(OAuthHost)].ToRegex();

        public Regex BypassHost => Configurations[nameof(BypassHost)].ToRegex();
    }
}