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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Mako.Util;

namespace Mako.Net
{
    public class PixivApiAutoRefreshingHttpRequestInterceptor : IHttpRequestInterceptor
    {
        private readonly MakoClient makoClient;
        private readonly ManualResetEvent refreshing = new ManualResetEvent(true);

        public PixivApiAutoRefreshingHttpRequestInterceptor([InjectMarker] MakoClient makoClient)
        {
            this.makoClient = makoClient;
        }

        public async Task Intercept(HttpRequestMessage message, IInterceptConfigurations configurations)
        {
            var conf = configurations as PixivRequestInterceptorConfiguration ?? throw new InvalidCastException($"{nameof(configurations)} must be {nameof(PixivRequestInterceptorConfiguration)}");
            var headers = message.Headers;
            var host = message.RequestUri.IdnHost;

            if (!refreshing.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Refresh timeout");
            }

            if (makoClient.ContextualBoundedSession != null && makoClient.ContextualBoundedSession.RefreshRequired() && /* prevent recursion */ !conf.OAuthHost.IsMatch(message.RequestUri.IdnHost))
            {
                using var semaphore = new SemaphoreSlim(1, 1);
                await semaphore.WaitAsync();
                refreshing.Reset();
                await makoClient.Refresh();
                refreshing.Set();
            }

            if (conf.ApiHost.IsMatch(host))
            {
                headers.Authorization.IfNull(() => headers.Authorization = new AuthenticationHeaderValue("Bearer", conf.Token));
            }
            if (conf.BypassHost.IsMatch(host) && conf.Bypass || conf.OAuthHost.IsMatch(host))
            {
                ReplaceRequest(message);
                conf.WebApiHost.IsMatch(host).IfTrue(() => headers.TryAddWithoutValidation("Cookie", conf.Cookie));
            }
        }

        private void ReplaceRequest(HttpRequestMessage message)
        {
            var host = message.RequestUri.IdnHost;
            var isSslSession = message.RequestUri.Scheme == "https";

            string address = makoClient.GetService<OrdinaryPixivDnsResolver>();
            message.RequestUri = new Uri($"http{isSslSession.ApplyIfTrue(() => "s")}://{address}{message.RequestUri.PathAndQuery}");
            message.Headers.Host = host;
        }
    }
}