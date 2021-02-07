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
    internal class KeywordSearchAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly DateTime? startDate;
        private readonly DateTime? endDate;
        private readonly SearchMatchOption searchMatchOption;
        private readonly IllustrationSortOption illustrationSortOption;
        private readonly SearchDuration? searchDuration;
        private readonly int searchCount;
        private readonly string keyword;
        private readonly uint start ;

        public KeywordSearchAsyncEnumerable(
            MakoClient makoClient,
            string keyword,
            uint start,
            int searchCount,
            SearchMatchOption searchMatchOption,
            IllustrationSortOption illustrationSortOption,
            SearchDuration? searchDuration,
            DateTime? startDate = null,
            DateTime? endDate = null
        ) : base(makoClient)
        {
            this.keyword = keyword;
            this.start = start;
            this.searchCount = searchCount;
            this.searchMatchOption = searchMatchOption;
            this.illustrationSortOption = illustrationSortOption;
            this.searchDuration = searchDuration;
            this.startDate = startDate;
            this.endDate = endDate;
        }

        public override void InsertTo(IList<Illustration> list, Illustration illustration)
        {
            illustration.Let(_ =>
            {
                if (MakoClient.ContextualBoundedSession.IsPremium && illustrationSortOption == IllustrationSortOption.Popularity)
                    list.Add(illustration);
                else
                    list.AddSorted(illustration, MakoClient.GetService<IComparer<Illustration>>(illustrationSortOption.GetEnumMetadataContent() as Type));
            });
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new KeywordSearchAsyncEnumerator(this, start, searchCount, keyword, searchMatchOption, searchDuration, illustrationSortOption, startDate, endDate, MakoClient);
        }

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            return item.DistinctTagCorrespondenceValidation(collection, MakoClient.ContextualBoundedSession);
        }

        private class KeywordSearchAsyncEnumerator : RecursivelyIterablePixivAsyncEnumerator<Illustration, QueryWorksResponse>
        {
            private readonly IllustrationSortOption illustrationSortOption;
            private readonly SearchMatchOption searchMatchOption;
            private readonly SearchDuration? searchDuration;
            private readonly DateTime? startDate;
            private readonly DateTime? endDate;
            private readonly int searchCount;
            private readonly uint current;
            private readonly string keyword;
            private int currentIllustIndex;

            public KeywordSearchAsyncEnumerator(
                IPixivAsyncEnumerable<Illustration> pixivEnumerable,
                uint current,
                int searchCount,
                string keyword,
                SearchMatchOption searchMatchOption,
                SearchDuration? searchDuration,
                IllustrationSortOption illustrationSortOption,
                DateTime? startDate,
                DateTime? endDate,
                MakoClient makoClient
            ) : base(pixivEnumerable, MakoAPIKind.AppApi, makoClient)
            {
                this.current = current;
                this.searchCount = searchCount;
                this.keyword = keyword;
                this.searchMatchOption = searchMatchOption;
                this.searchDuration = searchDuration;
                this.startDate = startDate;
                this.endDate = endDate;
                this.illustrationSortOption = illustrationSortOption;
            }

            protected override string NextUrl()
            {
                return Entity.NextUrl;
            }

            protected override bool HasNext()
            {
                return searchCount == -1 || currentIllustIndex++ < searchCount;
            }

            protected override string InitialUrl()
            {
                var searchTarget = (string) searchMatchOption.GetEnumMetadataContent();
                var sort = illustrationSortOption switch
                {
                    IllustrationSortOption.Popularity when MakoClient.ContextualBoundedSession.IsPremium => "&sort=popular_desc",
                    IllustrationSortOption.PublishDate                                                   => "&sort=date_desc",
                    _                                                                                    => null
                };
                var start = startDate.ApplyIfNonnull(dn => $"&start_date={dn:yyyy-MM-dd}");
                var end = endDate.ApplyIfNonnull(dn => $"&end_date={dn:yyyy-MM-dd}");
                var duration = searchDuration.ApplyIfNonnull(du => $"&duration={du.GetEnumMetadataContent()}");
                return $"/v1/search/illust?search_target={searchTarget}&word={keyword}&filter=for_ios&offset={current}{sort}{start}{end}{duration}";
            }

            protected override bool HasNextPage()
            {
                var next = NextUrl();
                return int.Parse(next[(next.LastIndexOf('=') + 1)..]) < 5000;
            }

            protected override IEnumerator<Illustration> GetNewEnumerator()
            {
                return Entity.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            protected override bool ValidateResponse(QueryWorksResponse entity)
            {
                return Entity.Illusts.IsNotNullOrEmpty();
            }
        }
    }
}