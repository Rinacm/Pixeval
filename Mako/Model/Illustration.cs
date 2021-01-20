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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Mako.Model
{
    [PublicAPI]
    public class Illustration : ICloneable
    {
        public string Id { get; set; }

        public bool IsUgoira { get; set; }

        public string OriginalUrl { get; set; }

        public string LargeUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public int Bookmarks { get; set; }

        public bool IsLiked { get; set; }

        public bool IsManga { get; set; }

        public string Title { get; set; }

        public string ArtistName { get; set; }

        public string ArtistId { get; set; }

        public IEnumerable<Tag> Tags { get; set; }

        public Illustration[] MangaMetadata { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        public int TotalViews { get; set; }

        public int TotalComments { get; set; }

        public IEnumerable<string> Comments { get; set; }

        public string Resolution { get; set; }

        public bool IsR18 => Tags?.Any(x => Regex.IsMatch(x?.Name ?? string.Empty, "[Rr][-]?18[Gg]?") || Regex.IsMatch(x?.TranslatedName ?? string.Empty, "[Rr][-]?18[Gg]?")) ?? false;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}