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
using System.Threading.Tasks;
using Mako.Model;
using NUnit.Framework;

namespace Mako.Test
{
    [Order(4)]
    public class RecommendsEngineTest
    {
        private static MakoClient MakoClient => Global.MakoClient;

        private static async Task<List<Illustration>> TryGetRecommends(IllustrationSortOption illustrationSortOption, RecommendationType recommendationType)
        {
            var list = new List<Illustration>();
            var enumerable = MakoClient.Recommends(illustrationSortOption, recommendationType);
            await foreach (var illustration in enumerable)
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                enumerable.InsertTo(list, illustration);
            }

            return list;
        }

        [Test]
        public async Task GetRecommendsUnspecifiedWithUnspecifiedOrder()
        {
            var list = await TryGetRecommends(IllustrationSortOption.Unspecified, RecommendationType.Unspecified);
            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
        }

        [Test]
        public async Task GetRecommendIllustrationsWithPublishDateOrder()
        {
            var list = await TryGetRecommends(IllustrationSortOption.PublishDate, RecommendationType.Illustration);
            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.PublishDate, true));
            Assert.IsTrue(list.All(i => i.IsManga is not true));
        }

        [Test]
        public async Task GetRecommendIllustrationsWithPopularityOrder()
        {
            var list = await TryGetRecommends(IllustrationSortOption.Popularity, RecommendationType.Illustration);
            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.Bookmarks, true));
            Assert.IsTrue(list.All(i => i.IsManga is not true));
        }

        [Test]
        public async Task GetRecommendMangaWithPublishDateOrder()
        {
            var list = await TryGetRecommends(IllustrationSortOption.PublishDate, RecommendationType.Manga);
            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.PublishDate, true));
            Assert.IsTrue(list.All(i => i.IsManga is true));
        }

        [Test]
        public async Task GetRecommendMangaWithPopularityOrder()
        {
            var list = await TryGetRecommends(IllustrationSortOption.Popularity, RecommendationType.Manga);
            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.Bookmarks, true));
            Assert.IsTrue(list.All(i => i.IsManga is true));
        }
    }
}