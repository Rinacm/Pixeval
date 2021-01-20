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
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Mako.Internal
{
    /// <summary>
    /// Specified enumerator for Pixiv resources, <strong>IT IS TRANSIENT, CREATE A NEW INSTANCE ON EACH TIME YOU WANT TO USE IT</strong>
    /// </summary>
    /// <typeparam name="E">Model type</typeparam>
    /// <typeparam name="C">Entity type</typeparam>
    internal abstract class AbstractPixivAsyncEnumerator<E, C> : IAsyncEnumerator<E>
    {
        protected IPixivAsyncEnumerable<E> PixivEnumerable;

        protected abstract IEnumerator<E> CurrentEntityEnumerator { get; set; }

        protected AbstractPixivAsyncEnumerator(IPixivAsyncEnumerable<E> pixivEnumerable)
        {
            PixivEnumerable = pixivEnumerable;
        }

        public virtual ValueTask DisposeAsync() => DisposeInternal();

        private ValueTask DisposeInternal()
        {
            CurrentEntityEnumerator = null;
            PixivEnumerable = null;
            return default;
        }

        public abstract ValueTask<bool> MoveNextAsync();

        public abstract E Current { get; }

        protected abstract void UpdateEnumerator();

        [ItemCanBeNull]
        protected abstract Task<C> GetResponse(string url);
    }
}