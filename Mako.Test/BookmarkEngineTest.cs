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
    public class BookmarkEngineTest
    {
        private static MakoClient MakoClient => Global.MakoClient;

        [Test, Parallelizable]
        public async Task PublicBookmarksTest()
        {
            var list = new List<Illustration>();
            await foreach (var illustration in MakoClient.Bookmarks(MakoClient.ContextualBoundedSession.Id, RestrictionPolicy.Public))
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                list.Add(illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.All(i => i.IsLiked));
        }

        [Test, Parallelizable]
        public async Task PrivateBookmarksTest()
        {
            var list = new List<Illustration>();
            await foreach (var illustration in MakoClient.Bookmarks(MakoClient.ContextualBoundedSession.Id, RestrictionPolicy.Private))
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                list.Add(illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsTrue(list.All(i => i.IsLiked));
        }
    }
}