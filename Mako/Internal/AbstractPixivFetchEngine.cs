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
using System.Threading;
using Mako.Util;

namespace Mako.Internal
{
    /// <summary>
    /// As a wrapper or so-called "higher infrastructure" of <see cref="AbstractPixivAsyncEnumerator{E,C}"/>,
    /// this class also manages the life cycle and statistics data while emitting the elements produced from
    /// <see cref="AbstractPixivAsyncEnumerator{E,C}"/>
    /// </summary>
    /// <typeparam name="E">Model type</typeparam>
    public abstract class AbstractPixivFetchEngine<E> : IPixivFetchEngine<E>
    {
        protected AbstractPixivFetchEngine(MakoClient makoClient)
        {
            MakoClient = makoClient;
        }

        protected MakoClient MakoClient { get; }

        /// <inheritdoc cref="IPixivFetchEngine{E}.RequestedPages"/>
        public int RequestedPages { get; set; }

        /// <summary>
        /// Get or set the cancellation state of current <see cref="AbstractPixivFetchEngine{E}"/>, the
        /// cancellation is cooperative, in each enumeration the <see cref="AbstractPixivAsyncEnumerator{E,C}.MoveNextAsync"/>
        /// will check if the operation has been cancelled, and returns false if so
        /// </summary>
        public bool Cancelled { get; set; }

        public abstract IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IPixivFetchEngine{E}.InsertTo"/>
        public virtual void InsertTo(IList<E> list, E element)
        {
            element.Let(list.Add);
        }

        /// <inheritdoc cref="IPixivFetchEngine{E}.Validate"/>
        public virtual bool Validate(E item, IList<E> collection)
        {
            return item.Check(() => !collection.Contains(item));
        }
    }
}