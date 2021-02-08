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
using System.Threading.Tasks;
using Mako.Net;
using Mako.Util;

namespace Mako.Internal
{
    /// <inheritdoc cref="AbstractPixivAsyncEnumerator{E,C}"/>
    /// <summary>
    /// <para>
    ///     After deserialized and parsed the JSON content into objects, we can acquire the
    ///     new contents of next page by sending request to the <c>nextUrl</c> and perform
    ///     the same procedure on it until <c>nextUrl</c> becomes null or empty
    /// </para>
    /// </summary>
    /// <typeparam name="E">Model type</typeparam>
    /// <typeparam name="C">Raw entity type</typeparam>
    internal abstract class RecursivelyIterablePixivAsyncEnumerator<E, C> : AbstractPixivAsyncEnumerator<E, C>
    {

        // the process procedure state transfer graph:
        //                                                                  Emit translated models
        //                                                                      MoveNextAsync()
        //                                                                      +-----------+
        //               +---------------------+        +--------------------+  | HasNext() |  +--------------------+
        //               |                     |        |                    |  |           |  |                    |
        // InitialUrl()  | Fetch JSON content  |        |  Translate the raw |  |           v  |Get url of the next |
        // +-------------+GetResponseOrThrow() +------->+       entity       +--+-----------+->+        page        |
        //               |                     |        |      Update()      |                 |      NextUrl()     |
        //               |                     |        |                    |                 |                    |
        //               +--------+------------+        +--------------------+                 +----------+---------+
        //                        ^                                                                       |
        //                        |                                                                       |
        //               NextUrl()|                                                                       |
        //                        |                           Has next page or not                        |
        //                        |                              HasNextPage()                            |
        //                        +-----------------------------------------------------------------------+

        protected C Entity { get; private set; }

        protected RecursivelyIterablePixivAsyncEnumerator(IPixivFetchEngine<E> pixivEnumerable, MakoAPIKind apiKind, MakoClient makoClient)
            : base(pixivEnumerable, apiKind, makoClient)
        {
        }

        protected abstract string NextUrl();

        protected abstract string InitialUrl();

        protected abstract IEnumerator<E> GetNewEnumerator();

        protected virtual bool HasNextPage()
        {
            return NextUrl().IsNotNullOrEmpty();
        }

        protected virtual bool HasNext()
        {
            return true;
        }

        private async ValueTask<bool> MoveNextAsyncInternal()
        {
            if (IsCancellationRequested || !HasNext())
                return false;

            if (Entity == null)
            {
                var url = InitialUrl();
                switch (await GetJsonResponse(url))
                {
                    case Success<C> success:
                        Update(success.Value);
                        break;
                    case Failure<C> _: throw Errors.EnumeratingNetworkException(url, PixivEnumerable.RequestedPages, null, MakoClient.ContextualBoundedSession.Bypass);
                    default:           throw Errors.ArgumentOutOfRange("The value must be an instance of Success<T> or Failure<T>");
                }
            }

            if (CurrentEntityEnumerator.MoveNext())
                return true;
            if (!HasNextPage())
                return false;

            if (await GetJsonResponse(NextUrl()) is Success<(Type, C)> result)
            {
                Update(result.Value.Item2);
                return true;
            }
            return false;
        }

        public override ValueTask<bool> MoveNextAsync()
        {
            return MoveNextAsyncInternal();
        }

        protected override void Update(C entity)
        {
            Entity = entity;
            CurrentEntityEnumerator = GetNewEnumerator();
            PixivEnumerable.RequestedPages++;
        }
    }
}