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
using System.Threading.Tasks;
using Mako.Model;
using NUnit.Framework;

namespace Mako.Test
{
    [Order(4)]
    public class KeywordSearchAsyncEnumerableTest
    {
        private static MakoClient MakoClient => Global.MakoClient;

        [Test, Parallelizable]
        public async Task KeywordSearchTest()
        {
            var list = new List<Illustration>();
            await foreach (var illustration in MakoClient.Search("東方project", searchCount: 500))
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                list.Add(illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
        }

        [Test, Parallelizable]
        public async Task ResultListBookmarksOrderTest()
        {
            var list = new List<Illustration>();
            var enumerable = MakoClient.Search("東方project", searchCount: 500, illustrationSortOption: IllustrationSortOption.Popularity);
            await foreach (var illustration in enumerable)
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                enumerable.InsertTo(list, illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.Bookmarks, true));
        }

        [Test, Parallelizable]
        public async Task ResultListPublishDateOrderTest()
        {
            var list = new List<Illustration>();
            var enumerable = MakoClient.Search("東方project", searchCount: 500, illustrationSortOption: IllustrationSortOption.PublishDate);
            await foreach (var illustration in enumerable)
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                enumerable.InsertTo(list, illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsSorted(i => i.PublishDate, true));
        }
    }
}