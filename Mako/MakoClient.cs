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
        public IServiceCollection MakoServices { get; }

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

        static MakoClient() => AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

        private MakoClient()
        {
            MakoServices = new ServiceCollection();
            // registration ordering is important, do not change the order unless the class structure is modified

            MakoServices.AddSingleton(this);

            // register the DNS resolver
            MakoServices.AddSingleton<OrdinaryPixivDnsResolver>();
            MakoServices.AddSingleton<OrdinaryPixivImageDnsResolver>();

            // register the RequestInterceptor and the HttpClientHandler
            MakoServices.AddSingleton<PixivApiLocalizedAutoRefreshingHttpRequestInterceptor>();
            MakoServices.AddSingleton<PixivImageHttpRequestInterceptor>();
            MakoServices.AddSingleton<PixivApiInterceptedHttpClientHandler>();
            MakoServices.AddSingleton<PixivImageInterceptedHttpClientHandler>();

            // register all the required HttpClients among entire application lifetime
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoHttpClientKind.AppApi, GetService<PixivApiInterceptedHttpClientHandler>(), client => client.BaseAddress = new Uri(MakoUrls.AppApiBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoHttpClientKind.WebApi, GetService<PixivApiInterceptedHttpClientHandler>(), client => client.BaseAddress = new Uri(MakoUrls.WebApiBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoHttpClientKind.Auth, GetService<PixivApiInterceptedHttpClientHandler>(), client => client.BaseAddress = new Uri(MakoUrls.OAuthBaseUrl)));
            MakoServices.AddSingleton(MakoHttpClientFactory.Create(MakoHttpClientKind.Image, GetService<PixivImageInterceptedHttpClientHandler>(), client =>
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");
            }));

            // register the HttpClientFactory as a selector to select which HttpClient shall be used
            MakoServices.AddSingleton<MakoHttpClientFactory>();

            // register the Refit services
            MakoServices.AddSingleton(RestService.For<IAppApiProtocol>(GetMakoTaggedHttpClient(MakoHttpClientKind.AppApi)));
            MakoServices.AddSingleton(RestService.For<IWebApiProtocol>(GetMakoTaggedHttpClient(MakoHttpClientKind.WebApi)));
            MakoServices.AddSingleton(RestService.For<IAuthProtocol>(GetMakoTaggedHttpClient(MakoHttpClientKind.Auth)));
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
        internal T GetService<T>() => MakoServiceProvider.GetService<T>();

        /// <summary>
        /// Replaces an instance in <see cref="MakoServices"/>
        /// </summary>
        /// <param name="descriptor">instance to be replaced</param>
        internal void ReplaceService(ServiceDescriptor descriptor) => MakoServices.Replace(descriptor);

        /// <summary>
        /// Acquires an <see cref="MakoTaggedHttpClient"/> from <see cref="MakoServices"/>
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        internal MakoTaggedHttpClient GetMakoTaggedHttpClient(MakoHttpClientKind kind)
        {
            return GetService<MakoHttpClientFactory>()[kind];
        }

        /// <summary>
        /// Attempts to login by using account and password within 10 seconds
        /// </summary>
        /// <returns><see cref="Task"/> completed when logged in or timeout</returns>
        /// <exception cref="AuthenticationTimeoutException">if it takes more than 10 seconds</exception>
        public async Task Login()
        {
            const string clientHash = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            var hash = (time + clientHash).Hash<MD5CryptoServiceProvider>();

            try
            {
                var token = await GetService<IAuthProtocol>().GetTokenByPassword(new PasswordTokenRequest { Name = account, Password = password }, time, hash);
                ContextualBoundedSession = Session.Parse(token, password, ContextualBoundedSession);
            }
            catch (Exception e)
            {
                throw SelectException(e);
            }
        }

        /// <summary>
        /// Attempts to refresh the session by using refresh token within 10 seconds
        /// </summary>
        /// <returns><see cref="Task"/> completed when refreshed or timeout</returns>
        /// <exception cref="AuthenticationTimeoutException">if it takes more than 10 seconds</exception>
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
        /// Get a user's collection by using <see cref="RestrictionPolicy"/> to indicate the publicity,
        /// be aware that <see cref="RestrictionPolicy"/> is only useful when the <paramref name="uid"/>
        /// is exactly representing yourself, otherwise you will get the same result regardless the
        /// value of <see cref="RestrictionPolicy"/>
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="restrictionPolicy"></param>
        /// <returns></returns>
        public IPixivAsyncEnumerable<Illustration> Gallery(string uid, RestrictionPolicy restrictionPolicy)
        {
            EnsureUserLoggedIn();
            return new GalleryAsyncEnumerable(this, uid, restrictionPolicy).Also(RegisterOperation);
        }

        private void RegisterOperation(ICancellable cancellable) => registeredOperations.Add(cancellable);

        /// <summary>
        /// Ensure that user has already call Login() before doing some context-aware action
        /// </summary>
        /// <exception cref="UserNotLoggedInException">if user is not logged in yet</exception>
        private void EnsureUserLoggedIn()
        {
            if (ContextualBoundedSession == null || ContextualBoundedSession.AccessToken.IsNullOrEmpty())
            {
                throw new UserNotLoggedInException("cannot find an appropriate session object, consider call MakoClient::Login() first");
            }
        }

        /// <summary>
        /// Semantically ambiguous, for internal use only
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