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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mako.Test
{
    public class LoginTest
    {
        [Test, Order(1)]
        public async Task Login()
        {
            await Global.MakoClient.Login();
            var session = Global.MakoClient.ContextualBoundedSession;
            Assert.NotNull(session);
            Console.WriteLine(Global.MakoClient.ContextualBoundedSession);
        }

        [Test, Order(2)]
        public async Task Refresh()
        {
            await Global.MakoClient.Refresh();
            var session = Global.MakoClient.ContextualBoundedSession;
            Assert.NotNull(session);
            Console.WriteLine(Global.MakoClient.ContextualBoundedSession);
        }

#if DEBUG
        [Test, Order(3)]
        public async Task AutoRefresh()
        {
            Global.MakoClient.ContextualBoundedSession.Invalidate();
            await Global.MakoClient.Gallery(Global.MakoClient.ContextualBoundedSession.Id, RestrictionPolicy.Public).GetAsyncEnumerator().MoveNextAsync();
        }
#endif
    }
}