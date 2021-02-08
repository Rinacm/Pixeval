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
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mako.Internal;
using Mako.Model;
using Mako.Net;
using Mako.Net.Protocol;
using Mako.Net.RequestModel;
using Mako.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;

// ReSharper disable once UseNameofExpression
namespace Mako
{
    /// <summary>
    /// Defines all the main functionalities by which Mako Framework can provide
    /// </summary>
    [DebuggerDisplay("Guid = {Identifier}")]
    [PublicAPI]
    public class MakoClient
    {
        private readonly string account;
        private readonly string password;

        /// <summary>
        /// Current version of Mako Framework
        /// </summary>
        public Version CurrentVersion { get; } = new Version(1, 0, 0);

        /// <summary>
        /// The bounded session of current <see cref="MakoClient"/>
        /// </summary>
        public Session ContextualBoundedSession { get; private set; }

        /// <summary>
        /// Globally Unique Identifier of current <see cref="MakoClient"/>
        /// </summary>
        public readonly Guid Identifier;

        /// <summary>
        /// Per client IoC container
        /// </summary>
        internal IServiceCollection MakoServices { get; }

        /// <summary>
        /// CultureInfo of current <see cref="MakoClient"/>
        /// </summary>
        public CultureInfo ClientCulture { get; set; }

        /// <summary>
        /// Accessor to access the instances in <see cref="MakoServices"/>
        /// </summary>
        private IServiceProvider MakoServiceProvider => MakoServices.BuildServiceProvider();

        private CancellationToken cancellationToken;

        /// <summary>
        /// Get or set the <see cref="CancellationToken"/> to cancel all enumerating operations
        /// </summary>
        public CancellationToken CancellationToken
        {
            get => cancellationToken;
            set
            {
                cancellationToken = value;
                cancellationToken.Register(() => registeredOperations.ForEach(op => op.Cancelled = true));
            }
        }

        private readonly List<ICancellable> registeredOperations = new List<ICancellable>();

        static MakoClient()
        {
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
        }

        private MakoClient()
        {
            MakoServices = new ServiceCollection();

            MakoServices.AddSingleton(this);

            // register the DNS resolver
            MakoServices.AddSingleton<OrdinaryPixivDnsResolver>();
            MakoServices.AddSingleton<OrdinaryPixivImageDnsResolver>();
            MakoServices.AddSingleton<LocalMachineDnsResolver>();

            // register the illustration comparators
            MakoServices.AddSingleton<IllustrationPopularityComparator>();
            MakoServices.AddSingleton<IllustrationPublishDateComparator>();

            // register the RequestInterceptor and the HttpClientHandler
            MakoServices.AddSingleton<PixivApiLocalizedAutoRefreshingDelegateHttpMessageHandler>();
            MakoServices.AddSingleton<PixivImageDelegateHttpMessageHandler>();
            MakoServices.AddSingleton<PixivApiDelegatedHttpClientHandler>();
            MakoServices.AddSingleton<PixivImageDelegatedHttpClientHandler>();

            // register all the required HttpClients among entire application lifetime
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoAPIKind.AppApi, GetService<PixivApiDelegatedHttpClientHandler>(), static client => client.BaseAddress = new Uri(MakoHttpOptions.AppApiBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoAPIKind.WebApi, GetService<PixivApiDelegatedHttpClientHandler>(), static client => client.BaseAddress = new Uri(MakoHttpOptions.WebApiBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoAPIKind.Auth, GetService<PixivApiDelegatedHttpClientHandler>(), client => client.BaseAddress = new Uri(MakoHttpOptions.OAuthBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoAPIKind.Image, GetService<PixivImageDelegatedHttpClientHandler>(), AddPixivImageHeaders));

            // register the HttpClientFactory as a selector to select which HttpClient shall be used
            MakoServices.AddSingleton<MakoHttpClientFactory>();

            // register the Refit services
            MakoServices.AddSingleton(RestService.For<IAppApiProtocol>(GetMakoTaggedHttpClient(MakoAPIKind.AppApi)));
            MakoServices.AddSingleton(RestService.For<IWebApiProtocol>(GetMakoTaggedHttpClient(MakoAPIKind.WebApi)));
            MakoServices.AddSingleton(RestService.For<IAuthProtocol>(GetMakoTaggedHttpClient(MakoAPIKind.Auth)));

            static void AddPixivImageHeaders(HttpClient httpClient)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");
            }
        }

        public MakoClient(string account, string password, bool bypass = true, CultureInfo clientCulture = null) : this()
        {
            (this.account, this.password, Identifier, ClientCulture) = (account, password, Guid.NewGuid(), clientCulture ?? CultureInfo.InstalledUICulture);
            ContextualBoundedSession = new Session
            {
                Bypass = bypass
            };
        }

        /// <summary>
        /// Acquires an instance of <typeparamref name="T"/> from <see cref="MakoServices"/>
        /// </summary>
        /// <typeparam name="T">instance type</typeparam>
        /// <returns>instance</returns>
        internal T GetService<T>()
        {
            return GetService<T>(typeof(T));
        }

        internal T GetService<T>(Type type)
        {
            return (T) MakoServiceProvider.GetService(type);
        }

        /// <summary>
        /// Replaces an instance in <see cref="MakoServices"/>
        /// </summary>
        /// <param name="descriptor">instance to be replaced</param>
        internal void ReplaceService(ServiceDescriptor descriptor)
        {
            MakoServices.Replace(descriptor);
        }

        /// <summary>
        /// Acquires an <see cref="MakoTaggedHttpClient"/> from <see cref="MakoServices"/>
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        internal MakoTaggedHttpClient GetMakoTaggedHttpClient(MakoAPIKind kind)
        {
            return GetService<MakoHttpClientFactory>()[kind];
        }

        /// <summary>
        /// Attempts to login by using account and password within 10 seconds
        /// </summary>
        /// <returns><see cref="Task"/>Completed when logged in or timeout</returns>
        /// <exception cref="AuthenticationTimeoutException">If it takes more than 10 seconds</exception>
        public async Task Login()
        {
            const string clientHash = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            var hash = (time + clientHash).Hash<MD5CryptoServiceProvider>();

            try
            {
                var token = await GetService<IAuthProtocol>().GetTokenByPassword(new PasswordTokenRequest { Name = account, Password = password }, time, hash);
                ContextualBoundedSession = Session.Parse(token, password, null);
            }
            catch (Exception e)
            {
                throw SelectException(e);
            }
        }

        /// <summary>
        /// Attempts to refresh the session by using refresh token within 10 seconds
        /// </summary>
        /// <returns><see cref="Task"/>Completed when refreshed or timeout</returns>
        /// <exception cref="AuthenticationTimeoutException">If it takes more than 10 seconds</exception>
        public async Task Refresh()
        {
            EnsureUserLoggedIn();
            try
            {
                var token = await GetService<IAuthProtocol>().RefreshToken(new RefreshTokenRequest { RefreshToken = ContextualBoundedSession.RefreshToken });
                ContextualBoundedSession = Session.Parse(token, password, ContextualBoundedSession);
            }
            catch (Exception e)
            {
                throw SelectException(e);
            }
        }

        /// <summary>
        /// Get a user's collection
        /// </summary>
        /// <param name="uid">User's uid</param>
        /// <param name="restrictionPolicy">
        /// Indicates the publicity, be aware this parameter is only useful when <paramref name="uid"/>
        /// is exactly referring to yourself, otherwise the result will be the same regardless of its value
        /// </param>
        /// <returns></returns>
        public IPixivFetchEngine<Illustration> Bookmarks(string uid, RestrictionPolicy restrictionPolicy)
        {
            EnsureUserLoggedIn();
            return new BookmarkEngine(this, uid, restrictionPolicy).Also(RegisterOperation);
        }

        /// <summary>
        /// Retrieves illustrations according to <see cref="keyword"/>
        /// </summary>
        /// <param name="keyword">keyword</param>
        /// <param name="start">
        /// Set the index you want to start at, which must in between 1(inclusive) and 5000(exclusive), default value is 1
        /// </param>
        /// <param name="searchCount">
        /// How many illusts will be searched, -1 to indicates you want to search as many as possible, otherwise it's
        /// value is semantically equals to <code>min(value, numbers of all possible illusts)</code>
        /// </param>
        /// <param name="searchMatchOption">Set the match method between keyword and illustration</param>
        /// <param name="illustrationSortOption">
        /// Tell the <see cref="AbstractPixivFetchEngine{E}.InsertTo"/> to manage the illustration in a proper order.
        /// This option only affects when invoking <see cref="AbstractPixivFetchEngine{E}.InsertTo"/>, it does not change
        /// the order of <see cref="IPixivFetchEngine{E}"/>
        /// </param>
        /// <param name="searchDuration"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <exception cref="ArgumentOutOfRangeException">If <see cref="start"/> is not in range [1, 5000)</exception>
        /// <returns><see cref="IPixivFetchEngine{E}"/></returns>
        public IPixivFetchEngine<Illustration> Search(
            string keyword,
            uint start = 1,
            int searchCount = -1,
            SearchMatchOption searchMatchOption = SearchMatchOption.TitleAndCaption,
            IllustrationSortOption illustrationSortOption = IllustrationSortOption.Unspecified,
            SearchDuration? searchDuration = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (start < 1 || start >= 5000)
                throw Errors.ArgumentOutOfRange($"desire range: [1, 5000), actual value: start({start})");

            EnsureUserLoggedIn();
            return new SearchEngine(this, keyword, start, searchCount, searchMatchOption, illustrationSortOption, searchDuration, startDate, endDate).Also(RegisterOperation);
        }

        /// <summary>
        /// Retrieves ranking illustrations according to <paramref name="rankOption"/> and <paramref name="date"/>
        /// </summary>
        /// <param name="rankOption">Ranking retrieve option</param>
        /// <param name="date">
        /// The date you want to retrieve, which must not greater than two days before today
        /// </param>
        /// <returns><see cref="IPixivFetchEngine{E}"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">if the date overflows</exception>
        public IPixivFetchEngine<Illustration> Ranking(RankOption rankOption, DateTime date)
        {
            if (DateTime.Today - date.Date > TimeSpan.FromDays(2))
                throw Errors.ArgumentOutOfRange($"The parameter {nameof(date)}({date.ToString(CultureInfo.CurrentCulture)})'s value must not greater than two days before today");

            EnsureUserLoggedIn();
            return new RankingEngine(rankOption, date, this).Also(RegisterOperation);
        }

        /// <summary>
        /// Gets recommendations of today
        /// </summary>
        /// <param name="illustrationSortOption">
        /// Tell the <see cref="AbstractPixivFetchEngine{E}.InsertTo"/> to manage the illustration in a proper order.
        /// This option only affects when invoking <see cref="AbstractPixivFetchEngine{E}.InsertTo"/>, it does not change
        /// the order of <see cref="IPixivFetchEngine{E}"/></param>
        /// <param name="recommendationType">
        /// Set the filter for recommendations, available values are <see cref="RecommendationType.Illustration"/>
        /// , <see cref="RecommendationType.Manga"/> and <see cref="RecommendationType.Unspecified"/>. The default
        /// value is <see cref="RecommendationType.Unspecified"/>
        /// </param>
        /// <returns></returns>
        public IPixivFetchEngine<Illustration> Recommends(IllustrationSortOption illustrationSortOption, RecommendationType recommendationType = RecommendationType.Unspecified)
        {
            EnsureUserLoggedIn();
            return new RecommendsEngine(this, illustrationSortOption, recommendationType).Also(RegisterOperation);
        }

        private void RegisterOperation(ICancellable cancellable)
        {
            registeredOperations.Add(cancellable);
        }

        /// <summary>
        /// Ensure that user has already called <see cref="Login"/> before doing some login-required action
        /// </summary>
        /// <exception cref="UserNotLoggedInException">If user is not logged in yet</exception>
        private void EnsureUserLoggedIn()
        {
            if (ContextualBoundedSession == null || ContextualBoundedSession.AccessToken.IsNullOrEmpty())
            {
                throw new UserNotLoggedInException("cannot find an appropriate session object, consider call MakoClient::Login() first");
            }
        }

        /// <summary>
        /// Selects a proper exception to thrown, this method is semantically ambiguous, for internal use only
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Exception SelectException(Exception e)
        {
            return e switch
            {
                TaskCanceledException _   => Errors.AuthenticationTimeout(account, password, true, true),
                ApiException apiException => ExamineApiException(apiException),
                _                         => e
            };

            Exception ExamineApiException(ApiException apiException)
            {
                var message = apiException.Content.FromJson<dynamic>();
                var system = message?.errors?.system;
                if (system?.code == 1508)
                {
                    var value = system.message?.Value?.ToString();
                    if (value?.StartsWith("103:"))
                        return Errors.LoginFailed(password, LoginFailedKind.Password, account);
                    if (value?.Equals("Invalid refresh token"))
                        return Errors.LoginFailed(ContextualBoundedSession.RefreshToken, LoginFailedKind.RefreshToken);
                }

                return apiException.ToMakoNetworkException(ContextualBoundedSession.Bypass);
            }
        }
    }
}