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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Mako.Util
{
    /// <summary>
    /// Helper functions on <see cref="IEnumerable{T}"/>
    /// </summary>
    [PublicAPI]
    public static class Enumerates
    {
        /// <summary>
        /// Check if a <see cref="IEnumerable{T}"/> is not null or has no element
        /// </summary>
        /// <param name="enumerable"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Any();
        }

        /// <summary>
        /// Check if a <see cref="IEnumerable{T}"/> is null or has no element
        /// </summary>
        /// <param name="enumerable"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        /// <summary>
        /// Select all the non-null elements in a <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerable"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> SelectNotNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(e => e != null);
        }

        /// <summary>
        /// Select all the non-null elements in a <see cref="IEnumerable{T}"/>,
        /// and apply a <see cref="Func{T,TResult}"/> on each of them
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="selector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public static IEnumerable<R> SelectNotNull<T, R>(this IEnumerable<T> enumerable, Func<T, R> selector)
        {
            return enumerable.Where(e => e != null).Select(selector);
        }
    }
}