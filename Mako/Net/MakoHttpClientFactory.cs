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
using System.Net.Http;
using Mako.Util;

namespace Mako.Net
{
    /// <summary>
    /// Factory to store and dispatch different kinds of <see cref="MakoTaggedHttpClient"/>
    /// </summary>
    public class MakoHttpClientFactory
    {
        private readonly IReadOnlyDictionary<MakoHttpClientKind, MakoTaggedHttpClient> clients;

        public MakoHttpClientFactory(IEnumerable<MakoTaggedHttpClient> clients)
        {
            this.clients = clients.ToDictionary(c => c.ClientKind, Scopes.Identity<MakoTaggedHttpClient>());
        }

        public MakoTaggedHttpClient this[MakoHttpClientKind kind] => clients[kind];

        public static MakoTaggedHttpClient Create(MakoHttpClientKind kind, HttpMessageHandler handler, Action<MakoTaggedHttpClient> action = null)
        {
            var mako = new MakoTaggedHttpClient(handler) { ClientKind = kind };
            action.Let(ac => ac(mako));
            return mako;
        }
    }
}