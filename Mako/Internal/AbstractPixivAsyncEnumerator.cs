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
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mako.Net;
using Mako.Util;

// ReSharper disable InvalidXmlDocComment
namespace Mako.Internal
{
    /// <summary>
    /// <para>The Pixiv app clients API mostly use the following JSON scheme:</para>
    /// <code>
    /// {
    ///     &emsp;"contents": [&lt;array-of-single-contents&gt;],
    ///     &emsp;"nextUrl": "&lt;url-to-proceed-to-next-page&gt;"
    /// }
    /// </code>
    /// Provide an subclass of <see cref="IAsyncEnumerator{T}"/> to explorer the <c>contents</c>
    /// part of a particular Pixiv API, this class serves 4 different purposes:
    /// <list type="number">
    ///     <item>
    ///         <term>Fetch: </term>
    ///         <description>
    ///         Get the JSON content from an API endpoint and deserialized into raw entities.
    ///         See <see cref="Mako.Model.IllustrationEssential"/>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Translate: </term>
    ///         <description>
    ///         Translate the raw entities into a list of simplified, clarified model such as
    ///         <see cref="Mako.Model.Illustration"/> to make it easier to manipulate
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Emit: </term>
    ///         <description>
    ///         Emit the parsed entities as <see cref="IAsyncEnumerator{T}"/>'s elements,
    ///         which gives it ability to be directly used in an async foreach loop
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Iterate: </term>
    ///         <description>
    ///         Since the API might contains more than 1 pages, this class should be able
    ///         to automatically request new JSON contents and perform the above steps until
    ///         the API cannot provide more results
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    ///     Due to the undertaken functionality of the translation layer of this class, it involves two
    ///     generic parameters, first of them(<typeparamref name="E"/>) is referring to the aforementioned
    ///     simplified model, and the later one(<typeparamref name="C"/>) represents the type of the raw
    ///     entities to which deserialized JSON corresponds
    /// </para>
    /// <example>
    ///     <code>
    ///     public static IPixivFetchEngine&lt;Entity&gt; GetEngine() => ...
    ///
    ///     var enumerator = GetEngine().GetAsyncEnumerator();
    ///     while(enumerator.MoveNext())
    ///     {
    ///         var entity = enumerator.Current;
    ///         // do something with entity
    ///     }
    ///     </code>
    /// </example>
    /// <para>
    /// <strong>NOTICE</strong>! The emitted value from this <see cref="AbstractPixivAsyncEnumerator{E,C}"/>
    /// is not guaranteed to be nonnull, check the value carefully before using it
    /// </para>
    /// </summary>
    /// <see cref="RecursivelyIterablePixivAsyncEnumerator{E,C}"/>
    /// <typeparam name="E">Model type</typeparam>
    /// <typeparam name="C">Raw entity type</typeparam>
    internal abstract class AbstractPixivAsyncEnumerator<E, C> : IAsyncEnumerator<E>
    {
        /// <summary>
        /// The <see cref="IPixivFetchEngine{E}"/> which owns this <see cref="AbstractPixivAsyncEnumerator{E,C}"/>
        /// </summary>
        protected IPixivFetchEngine<E> PixivEnumerable;

        /// <summary>
        /// The translated models of current page
        /// </summary>
        protected IEnumerator<E> CurrentEntityEnumerator { get; set; }

        /// <summary>
        /// Indicates which API is currently used
        /// </summary>
        private MakoAPIKind ApiKind { get; }

        /// <summary>
        /// Get cancellation requested or not
        /// </summary>
        protected bool IsCancellationRequested => PixivEnumerable.Cancelled;

        public virtual ValueTask DisposeAsync()
        {
            return DisposeInternal();
        }

        protected MakoClient MakoClient { get; }

        [CanBeNull]
        public virtual E Current => CurrentEntityEnumerator.Current;

        protected AbstractPixivAsyncEnumerator(IPixivFetchEngine<E> pixivEnumerable, MakoAPIKind apiKind, MakoClient makoClient)
        {
            (PixivEnumerable, ApiKind, MakoClient) = (pixivEnumerable, apiKind, makoClient);
        }

        private ValueTask DisposeInternal()
        {
            CurrentEntityEnumerator = null;
            PixivEnumerable = null;
            return default;
        }

        public abstract ValueTask<bool> MoveNextAsync();

        /// <summary>
        /// Update the value of <see cref="IPixivFetchEngine{E}.RequestedPages"/> and
        /// <see cref="CurrentEntityEnumerator"/> and other related fields after requested
        /// a new page
        /// </summary>
        /// <param name="entity">The raw entity deserialized from JSON</param>
        protected abstract void Update(C entity);

        /// <summary>
        /// Check if response is invalid/null/empty
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract bool ValidateResponse(C entity);

        /// <summary>
        /// Send an request to get JSON content(of a new page)
        /// </summary>
        /// <param name="url">Url</param>
        protected async Task<Result<C>> GetJsonResponse(string url)
        {
            var result = await MakoClient.GetMakoTaggedHttpClient(ApiKind).GetJsonAsync<C>(url);
            return ValidateResponse(result)
                ? Result<C>.Success(result)
                : Result<C>.Failure;
        }
    }
}