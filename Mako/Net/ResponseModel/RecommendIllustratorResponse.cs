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
using Mako.Model;
using Newtonsoft.Json;

namespace Mako.Net.ResponseModel
{
    public class RecommendIllustratorResponse
    {
        [JsonProperty("user_previews")]
        public UserPreview[] UserPreviews { get; set; }

        [JsonProperty("next_url")]
        public string NextUrl { get; set; }

        public class UserPreview
        {
            [JsonProperty("user")]
            public IllustrationEssential.User User { get; set; }

            [JsonProperty("illusts")]
            public List<IllustrationEssential.Illust> Illusts { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }
    }
}