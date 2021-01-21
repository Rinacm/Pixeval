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
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Mako.Net;
using Mako.Net.ResponseModel;
using Mako.Util;

namespace Mako.Internal
{
    internal class GalleryAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly string uid;
        private readonly RestrictionPolicy restrictionPolicy;

        public GalleryAsyncEnumerable(MakoClient makoClient, string uid, RestrictionPolicy restrictionPolicy)
            : base(makoClient) => (this.uid, this.restrictionPolicy) = (uid, restrictionPolicy);

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            var session = MakoClient.ContextualBoundedSession;
            return item.Check(() => collection.All(i => i.Id != item.Id) && item.Validate(session.ExcludeTags, session.IncludeTags, session.MinBookmark));
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new GalleryAsyncEnumerator(this, restrictionPolicy, uid, MakoClient);
        }

        private class GalleryAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration, GalleryResponse>
        {
            private readonly MakoClient makoClient;
            private readonly RestrictionPolicy restrictionPolicy;
            private readonly string uid;
            private GalleryResponse current;

            protected override IEnumerator<Illustration> CurrentEntityEnumerator { get; set; }

            public override Illustration Current => CurrentEntityEnumerator.Current;

            public GalleryAsyncEnumerator(IPixivAsyncEnumerable<Illustration> pixivEnumerable, RestrictionPolicy restrictionPolicy, string uid, MakoClient makoClient)
                : base(pixivEnumerable) => (this.restrictionPolicy, this.uid, this.makoClient) = (restrictionPolicy, uid, makoClient);

            protected override void UpdateEnumerator(GalleryResponse entity)
            {
                current = entity;
                CurrentEntityEnumerator = current.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (IsCancellationRequested)
                    return false; // fast-path
                if (current == null)
                {
                    UpdateEnumerator(await GetResponseOrThrow(ConstructUrl()));
                    PixivEnumerable.RequestedPages++;
                }

                if (CurrentEntityEnumerator.MoveNext())
                    return true;
                if (current.NextUrl.IsNullOrEmpty())
                    return false;

                UpdateEnumerator(await GetResponseOrThrow(current.NextUrl));
                PixivEnumerable.RequestedPages++;
                return true;
            }

            private string ConstructUrl()
            {
                return restrictionPolicy switch
                {
                    RestrictionPolicy.Public  => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios",
                    RestrictionPolicy.Private => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=private&filter=for_ios",
                    _                         => throw new ArgumentOutOfRangeException()
                };
            }

            protected override async Task<GalleryResponse> GetResponseOrThrow(string url)
            {
                var result = await makoClient.GetMakoTaggedHttpClient(MakoHttpClientKind.AppApi).GetJsonAsync<GalleryResponse>(url);
                return result.NullIfFalse(() => result.Illusts.IsNotNullOrEmpty()) ??
                    throw Errors.EnumeratingNetworkException(
                        nameof(GalleryAsyncEnumerable),
                        nameof(GalleryAsyncEnumerator),
                        MakoUrls.AppApiBaseUrl + ConstructUrl(), PixivEnumerable.RequestedPages,
                        $"The result collection is empty, this mostly indicates that the user with specified Uid: {uid} does not exists.",
                        makoClient.ContextualBoundedSession.Bypass
                    );
            }
        }
    }
}