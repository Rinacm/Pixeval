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

using JetBrains.Annotations;
using Mako.Util;

namespace Mako
{
    /// <summary>
    /// Indicates how to match the keyword with correspond illustrations
    /// </summary>
    [PublicAPI]
    public enum SearchMatchOption
    {

        /// <summary>
        /// Partial match (tags)
        /// </summary>
        [EnumMetadata("partial_match_for_tags")]
        PartialMatchForTags,

        /// <summary>
        /// Exact match (tags)
        /// </summary>
        [EnumMetadata("exact_match_for_tags")]
        ExactMatchForTags,

        /// <summary>
        /// Match the title and caption
        /// </summary>
        [EnumMetadata("title_and_caption")]
        TitleAndCaption
    }
}