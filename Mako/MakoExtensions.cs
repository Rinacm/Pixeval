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
using System.Linq;
using Mako.Model;
using Mako.Util;

// ReSharper disable ParameterTypeCanBeEnumerable.Global
namespace Mako
{
    public static class MakoExtensions
    {
        public static bool Validate(this Illustration illustration, ISet<string> excludeTags, ISet<string> includeTags, int minBookmarks)
        {
            return illustration.Check(() =>
            {
                if (excludeTags.IsNotNullOrEmpty() && excludeTags.Any(x => x.IsNotNullOrEmpty() && illustration.Tags.Any(t => t.Name.EqualsIgnoreCase(x))))
                {
                    return false;
                }

                if (includeTags.IsNotNullOrEmpty() && includeTags.Any(x => x.IsNotNullOrEmpty() && illustration.Tags.All(i => !i.Name.EqualsIgnoreCase(x))))
                {
                    return false;
                }

                return illustration.Bookmarks >= minBookmarks;
            });
        }

        public static Illustration ToIllustration(this IllustrationEssential.Illust illust)
        {
            var illustration = new Illustration
            {
                Bookmarks = (int) illust.TotalBookmarks,
                Id = illust.Id.ToString(),
                IsLiked = illust.IsBookmarked,
                IsManga = illust.PageCount != 1,
                IsUgoira = illust.Type == "ugoira",
                OriginalUrl = illust.MetaSinglePage.OriginalImageUrl,
                LargeUrl = illust.ImageUrls.Large,
                Tags = illust.Tags.Select(t => new Tag { Name = t.Name, TranslatedName = t.TranslatedName }),
                ThumbnailUrl = illust.ImageUrls.Medium.IsNullOrEmpty() ? illust.ImageUrls.SquareMedium : illust.ImageUrls.Medium,
                Title = illust.Title,
                ArtistId = illust.User.Id.ToString(),
                ArtistName = illust.User.Name,
                Resolution = $"{illust.Width}x{illust.Height}",
                TotalViews = (int) illust.TotalView,
                PublishDate = illust.CreateDate
            };
            if (illustration.IsManga)
            {
                illustration.MangaMetadata = illust.MetaPages.Select(mp =>
                {
                    var p = (Illustration) illustration.Clone();
                    p.ThumbnailUrl = mp.ImageUrls.Medium;
                    p.OriginalUrl = mp.ImageUrls.Original;
                    p.LargeUrl = mp.ImageUrls.Large;
                    return p;
                }).ToArray();
                foreach (var i in illustration.MangaMetadata)
                {
                    i.MangaMetadata = illustration.MangaMetadata;
                }
            }

            return illustration;
        }
    }
}