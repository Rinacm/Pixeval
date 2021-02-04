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
using System.Threading.Tasks;
using Mako.Model;
using Mako.Net;
using Mako.Net.ResponseModel;
using Mako.Util;

namespace Mako.Internal
{
    internal class KeywordSearchAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly SearchMatchOption searchMatchOption;
        private readonly IllustrationSortOption illustrationSortOption;
        private readonly int searchCount;
        private readonly string keyword;
        private readonly uint start;

        public KeywordSearchAsyncEnumerable(
            MakoClient makoClient,
            string keyword,
            uint start,
            int searchCount,
            SearchMatchOption searchMatchOption,
            IllustrationSortOption illustrationSortOption
        ) : base(makoClient) => (this.start, this.keyword, this.searchCount, this.searchMatchOption, this.illustrationSortOption) = (start, keyword, searchCount, searchMatchOption, illustrationSortOption);

        public override void InsertTo(IList<Illustration> list, Illustration illustration)
        {
            illustration.Let(_ =>
            {
                if (MakoClient.ContextualBoundedSession.IsPremium)
                    list.Add(illustration);
                else
                    list.AddSorted(illustration, MakoClient.GetService<IComparer<Illustration>>(illustrationSortOption.GetEnumMetadataContent() as Type));
            });
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new KeywordSearchAsyncEnumerator(this, start, searchCount, keyword, searchMatchOption, MakoClient);
        }

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            return item.DistinctTagCorrespondenceValidation(collection, MakoClient.ContextualBoundedSession);
        }

        private class KeywordSearchAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration, QueryWorksResponse>
        {
            private readonly SearchMatchOption searchMatchOption;
            private readonly int searchCount;
            private readonly MakoClient makoClient;
            private readonly uint current;
            private readonly string keyword;
            private QueryWorksResponse response;
            private int currentIllustIndex;

            public KeywordSearchAsyncEnumerator(
                IPixivAsyncEnumerable<Illustration> pixivEnumerable,
                uint current,
                int searchCount,
                string keyword,
                SearchMatchOption searchMatchOption,
                MakoClient makoClient
            ) : base(pixivEnumerable) => (this.current, this.keyword, this.makoClient, this.searchCount, this.searchMatchOption) = (current, keyword, makoClient, searchCount, searchMatchOption);

            public override Illustration Current => CurrentEntityEnumerator.Current;

            protected override IEnumerator<Illustration> CurrentEntityEnumerator { get; set; }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (IsCancellationRequested)
                    return false;
                if (searchCount != -1 && currentIllustIndex++ >= searchCount)
                    return false;

                if (response == null)
                {
                    var searchTarget = (string) searchMatchOption.GetEnumMetadataContent();
                    var sort = makoClient.ContextualBoundedSession.IsPremium ? "date_desc" : "popular_desc";
                    UpdateEnumerator(await GetResponseOrThrow($"/v1/search/illust?search_target={searchTarget}&sort={sort}&word={keyword}&filter=for_android&offset={current}"));
                    PixivEnumerable.RequestedPages++;
                }

                if (CurrentEntityEnumerator.MoveNext())
                    return true;
                // has next page or not, since the Pixiv API limited request illusts up to 5000
                if (int.Parse(response.NextUrl[(response.NextUrl.LastIndexOf('=') + 1)..]) >= 5000)
                    return false;

                UpdateEnumerator(response);
                PixivEnumerable.RequestedPages++;
                return true;
            }

            protected override void UpdateEnumerator(QueryWorksResponse entity)
            {
                response = entity;
                CurrentEntityEnumerator = response.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            protected override async Task<QueryWorksResponse> GetResponseOrThrow(string url)
            {
                var result = await makoClient.GetMakoTaggedHttpClient(MakoHttpClientKind.AppApi).GetJsonAsync<QueryWorksResponse>(url);
                return result.NullIfFalse(() => result.Illusts.IsNotNullOrEmpty()) ??
                    throw Errors.EnumeratingNetworkException(
                        nameof(KeywordSearchAsyncEnumerable),
                        nameof(KeywordSearchAsyncEnumerator),
                        url, PixivEnumerable.RequestedPages,
                        $"The result collection is empty, this mostly indicates that the keyword {keyword} corresponds no results",
                        makoClient.ContextualBoundedSession.Bypass
                    );
            }
        }
    }
}