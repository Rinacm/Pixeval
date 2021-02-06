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
using Mako.Model;
using Mako.Net;
using Mako.Net.ResponseModel;
using Mako.Util;

namespace Mako.Internal
{
    internal class BookmarkAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly string uid;
        private readonly RestrictionPolicy restrictionPolicy;

        public BookmarkAsyncEnumerable(MakoClient makoClient, string uid, RestrictionPolicy restrictionPolicy)
            : base(makoClient)
        {
            (this.uid, this.restrictionPolicy) = (uid, restrictionPolicy);
        }

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            return item.DistinctTagCorrespondenceValidation(collection, MakoClient.ContextualBoundedSession);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new BookmarkAsyncEnumerator(this, restrictionPolicy, uid, MakoClient);
        }

        private class BookmarkAsyncEnumerator : RecursivelyIterablePixivAsyncEnumerator<Illustration, BookmarkResponse>
        {
            private readonly RestrictionPolicy restrictionPolicy;
            private readonly string uid;

            public BookmarkAsyncEnumerator(IPixivAsyncEnumerable<Illustration> pixivEnumerable, RestrictionPolicy restrictionPolicy, string uid, MakoClient makoClient)
                : base(pixivEnumerable, MakoAPIKind.AppApi, makoClient)
            {
                (this.restrictionPolicy, this.uid) = (restrictionPolicy, uid);
            }

            protected override IEnumerator<Illustration> GetNewEnumerator()
            {
                return Entity.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            protected override string NextUrl()
            {
                return Entity.NextUrl;
            }

            protected override string InitialUrl()
            {
                return restrictionPolicy switch
                {
                    RestrictionPolicy.Public  => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios",
                    RestrictionPolicy.Private => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=private&filter=for_ios",
                    _                         => throw new ArgumentOutOfRangeException()
                };
            }

            protected override bool ValidateResponse(BookmarkResponse entity)
            {
                return entity.Illusts.IsNotNullOrEmpty();
            }
        }
    }
}