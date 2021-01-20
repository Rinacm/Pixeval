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
using System.Threading;
using Mako.Util;

namespace Mako.Internal
{
    /// <summary>
    /// Specified enumerable for Pixiv resources, <strong>IT IS TRANSIENT, CREATE A NEW INSTANCE ON EACH TIME YOU WANT TO USE IT</strong>
    /// </summary>
    /// <typeparam name="E">Model type</typeparam>
    internal abstract class AbstractPixivAsyncEnumerable<E> : IPixivAsyncEnumerable<E>
    {
        protected AbstractPixivAsyncEnumerable(MakoClient makoClient) => MakoClient = makoClient;

        protected MakoClient MakoClient { get; }

        public int RequestedPages { get; set; }

        public bool Cancelled { get; set; }

        public virtual Action<IList<E>, E> InsertPolicy() => (es, e) => e.Let(es.Add);

        public abstract IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default);

        public virtual bool Validate(E item, IList<E> collection) => item.Check(() => !collection.Contains(item));
    }
}