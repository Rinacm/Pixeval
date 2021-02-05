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
using JetBrains.Annotations;

namespace Mako
{
    [PublicAPI]
    public interface IPixivAsyncEnumerable<E> : IAsyncEnumerable<E>, ICancellable
    {
        /// <summary>
        /// How many pages has been requested
        /// </summary>
        int RequestedPages { get; set; }

        /// <summary>
        /// Insert an <paramref name="element"/> into <paramref name="list"/>.
        /// <para>
        /// when enumerating the contents, there may be such a requirement that
        /// needs to insert an element into a list in a proper way(such as sort
        /// the element in some order), this function will be helpful for those
        /// kinds of scenarios
        /// </para>
        /// </summary>
        void InsertTo(IList<E> list, E element);

        /// <summary>
        /// Check if an <paramref cref="item"/> is valid to be inserted into <paramref cref="collection"/>
        /// <para>
        /// Use this function cooperatively with <see cref="InsertTo"/>
        /// </para>
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        /// <param name="collection">The list</param>
        /// <returns>The validity of the <paramref name="item"/></returns>
        bool Validate(E item, IList<E> collection);
    }
}