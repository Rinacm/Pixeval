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
    public class RankingAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly RankOption rankOption;
        private readonly DateTime date;

        public RankingAsyncEnumerable(RankOption rankOption, DateTime date, MakoClient makoClient)
            : base(makoClient) => (this.rankOption, this.date) = (rankOption, date);

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            return item.DistinctTagCorrespondenceValidation(collection, MakoClient.ContextualBoundedSession);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new RankingAsyncEnumerator(this, rankOption, date, MakoClient);
        }

        private class RankingAsyncEnumerator : RecursivelyIterablePixivAsyncEnumerator<Illustration, RankingResponse>
        {
            private readonly string rankOptionParameter;
            private readonly string dateParameter;

            public RankingAsyncEnumerator(IPixivAsyncEnumerable<Illustration> pixivEnumerable, RankOption rankOption, DateTime date, MakoClient makoClient)
                : base(pixivEnumerable, makoClient)
            {
                rankOptionParameter = (string) rankOption.GetEnumMetadataContent();
                dateParameter = date.ToString("yyyy-MM-dd");
            }

            public override Illustration Current => CurrentEntityEnumerator.Current;

            protected override string NextUrl() => Entity.NextUrl;

            protected override string InitialUrl() => $"/v1/illust/ranking?filter=for_android&mode={rankOptionParameter}&date={dateParameter}";

            protected override IEnumerator<Illustration> GetNewEnumerator()
            {
                return Entity.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            protected override async Task<Result<(Type, RankingResponse)>> GetResponse(string url)
            {
                var result = await MakoClient.GetMakoTaggedHttpClient(MakoHttpClientKind.AppApi).GetJsonAsync<RankingResponse>(url);
                return result.Illusts.IsNotNullOrEmpty()
                    ? Result<(Type, RankingResponse)>.Success((GetType(), result))
                    : Result<(Type, RankingResponse)>.Failure;
            }
        }
    }
}