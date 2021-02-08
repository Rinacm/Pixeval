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
    public class RecommendsEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly IllustrationSortOption illustrationSortOption;
        private readonly RecommendationType recommendationType;

        public RecommendsEngine(MakoClient makoClient, IllustrationSortOption illustrationSortOption, RecommendationType recommendationType)
            : base(makoClient)
        {
            this.illustrationSortOption = illustrationSortOption;
            this.recommendationType = recommendationType;
        }

        public override void InsertTo(IList<Illustration> list, Illustration illustration)
        {
            illustration.Let(_ => list.AddSorted(illustration, MakoClient.GetService<IComparer<Illustration>>(illustrationSortOption.GetEnumMetadataContent() as Type)));
        }

        public override bool Validate(Illustration item, IList<Illustration> collection)
        {
            return item.DistinctTagCorrespondenceValidation(collection, MakoClient.ContextualBoundedSession);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new RecommendsAsyncEnumerator(this, recommendationType, MakoClient);
        }

        private class RecommendsAsyncEnumerator : RecursivelyIterablePixivAsyncEnumerator<Illustration, RecommendResponse>
        {
            private readonly RecommendationType recommendationType;

            public RecommendsAsyncEnumerator(IPixivFetchEngine<Illustration> pixivEnumerable, RecommendationType recommendationType, MakoClient makoClient)
                : base(pixivEnumerable, MakoAPIKind.AppApi, makoClient)
            {
                this.recommendationType = recommendationType;
            }

            protected override string NextUrl()
            {
                return Entity.NextUrl;
            }

            protected override string InitialUrl()
            {
                var param = recommendationType.GetEnumMetadataContent() is string str ? $"&content_type={str}" : null;
                return $"/v1/illust/recommended?include_ranking_label=true{param}";
            }

            protected override IEnumerator<Illustration> GetNewEnumerator()
            {
                return Entity.Illusts.SelectNotNull(MakoExtensions.ToIllustration).GetEnumerator();
            }

            protected override bool ValidateResponse(RecommendResponse entity)
            {
                return Entity.Illusts.IsNotNullOrEmpty();
            }
        }
    }
}