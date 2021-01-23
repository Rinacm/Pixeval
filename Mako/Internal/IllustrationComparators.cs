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
using Mako.Model;

namespace Mako.Internal
{
    public class IllustrationPopularityComparator : IComparer<Illustration>
    {
        public int Compare(Illustration x, Illustration y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.Bookmarks < y.Bookmarks ? 1 : x.Bookmarks == y.Bookmarks ? 0 : -1;
        }
    }

    public class IllustrationPublishDateComparator : IComparer<Illustration>
    {
        public int Compare(Illustration x, Illustration y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.PublishDate < y.PublishDate ? 1 : x.PublishDate == y.PublishDate ? 0 : -1;
        }
    }
}