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
using Mako.Net.ResponseModel;
using Mako.Util;

namespace Mako.Internal
{
    internal class KeywordSearchAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly IllustrationSortOption illustrationSortOption;
        private readonly string keyword;
        private readonly bool isPremium;
        private readonly uint start;
        private readonly SearchMatchOption searchMatchOption;

        public KeywordSearchAsyncEnumerable(MakoClient makoClient, string keyword, IllustrationSortOption illustrationSortOption, SearchMatchOption searchMatchOption, bool isPremium, uint start = 1)
            : base(makoClient) => (this.searchMatchOption, this.isPremium, this.start, this.keyword, this.illustrationSortOption) = (searchMatchOption, isPremium, start.CoerceAt(1), keyword, illustrationSortOption);

        public override Action<IList<Illustration>, Illustration> InsertPolicy()
        {
            return (list, i) =>
            {
                i.Let(_ =>
                {
                    if (isPremium)
                    {
                        list.Add(i);
                    }
                    else
                    {
                        list.AddSorted(i, MakoClient.GetService<IComparer<Illustration>>(illustrationSortOption.GetEnumMetadataContent() as Type));
                    }
                });
            };
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        private class KeywordSearchAsyncEnumerator : AbstractPixivAsyncEnumerator<QueryWorksResponse, Illustration>
        {
            private readonly int current;
            private readonly bool isPremium;
            private readonly string keyword;
            private readonly SearchMatchOption matchOption;

            public KeywordSearchAsyncEnumerator(IPixivAsyncEnumerable<QueryWorksResponse> pixivEnumerable, int current, bool isPremium, string keyword, SearchMatchOption matchOption)
                : base(pixivEnumerable) => (this.current, this.isPremium, this.keyword, this.matchOption) = (current, isPremium, keyword, matchOption);

            public override QueryWorksResponse Current => CurrentEntityEnumerator.Current;

            protected override IEnumerator<QueryWorksResponse> CurrentEntityEnumerator { get; set; }
            public override ValueTask<bool> MoveNextAsync()
            {
                throw new NotImplementedException();
            }

            protected override void UpdateEnumerator(Illustration entity)
            {
                throw new NotImplementedException();
            }

            protected override Task<Illustration> GetResponseOrThrow(string url)
            {
                throw new NotImplementedException();
            }
        }
    }
}