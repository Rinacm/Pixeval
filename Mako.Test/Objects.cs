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
using Mako.Model;

namespace Mako.Test
{
    public static class Objects
    {
        public static bool IsSorted<T, K>(this IEnumerable<T> enumerable, Func<T, K> keyExtractor, bool descend = false) where K : IComparable<K>
        {
            var elements = enumerable as T[] ?? enumerable.ToArray();
            K prev = keyExtractor(elements.FirstOrDefault());
            foreach (var ele in elements)
            {
                var key = keyExtractor(ele);
                var comparedResult = key.CompareTo(prev);
                if (descend ? comparedResult > 0 : comparedResult < 0) return false;
                prev = key;
            }

            return true;
        }

        public static bool IsDistinct<T>(this IEnumerable<T> enumerable, Func<T, object> keySelector)
        {
            var delegatedComparer = new DelegatedEqualityComparer<T>(keySelector);
            var ts = enumerable as T[] ?? enumerable.ToArray();
            return ts.All(e1 => ts.All(e2 => ReferenceEquals(e1, e2) || !delegatedComparer.Equals(e2, e1)));
        }

        public static void Print(this Illustration illustration)
        {
            Console.WriteLine($"ID: {illustration.Id} Title: {illustration.Title} UserName: {illustration.ArtistName} Bookmarks: {illustration.Bookmarks}");
        }
    }
}