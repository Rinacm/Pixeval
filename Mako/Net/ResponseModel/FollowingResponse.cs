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
using Mako.Model;
using Newtonsoft.Json;

namespace Mako.Net.ResponseModel
{
    public class FollowingResponse
    {
        [JsonProperty("user_previews")]
        public List<UserPreview> UserPreviews { get; set; }

        [JsonProperty("next_url")]
        public string NextUrl { get; set; }

        public class UserPreview
        {
            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("illusts")]
            public List<IllustrationEssential.Illust> Illusts { get; set; }

            [JsonProperty("novels")]
            public List<Novel> Novels { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }

        public class Series
        {
            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }
        }

        public class IllustTag
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }

        public class Novel
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("restrict")]
            public long Restrict { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("is_original")]
            public bool IsOriginal { get; set; }

            [JsonProperty("image_urls")]
            public IllustrationEssential.ImageUrls ImageUrls { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("tags")]
            public List<NovelTag> Tags { get; set; }

            [JsonProperty("page_count")]
            public long PageCount { get; set; }

            [JsonProperty("text_length")]
            public long TextLength { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("series")]
            public Series Series { get; set; }

            [JsonProperty("is_bookmarked")]
            public bool IsBookmarked { get; set; }

            [JsonProperty("total_bookmarks")]
            public long TotalBookmarks { get; set; }

            [JsonProperty("total_view")]
            public long TotalView { get; set; }

            [JsonProperty("visible")]
            public bool Visible { get; set; }

            [JsonProperty("total_comments")]
            public long TotalComments { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }

            [JsonProperty("is_mypixiv_only")]
            public bool IsMyPixivOnly { get; set; }

            [JsonProperty("is_x_restricted")]
            public bool IsXRestricted { get; set; }
        }

        public class NovelTag
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("added_by_uploaded_user")]
            public bool AddedByUploadedUser { get; set; }
        }
    }
}