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
    public class PixivApiLocalizedAutoRefreshingDelegateHttpMessageHandler : HttpMessageHandler
    {
        private readonly MakoClient makoClient;
        private readonly ManualResetEvent refreshing = new(true);

        public PixivApiLocalizedAutoRefreshingDelegateHttpMessageHandler([InjectMarker] MakoClient makoClient)
        {
            this.makoClient = makoClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!refreshing.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw Errors.AuthenticationTimeout(null, makoClient.ContextualBoundedSession.RefreshToken, true, true);
            }

            request.RequestUri = new Uri(request.RequestUri!.ToString().Replace("https", "http"));
            var headers = request.Headers;
            var host = request.RequestUri!.IdnHost;

            request.Headers.TryAddWithoutValidation("Accept-Language", makoClient.ClientCulture.Name);

            if (makoClient.ContextualBoundedSession != null && makoClient.ContextualBoundedSession.RefreshRequired() && /* prevent recursion */ !MakoHttpOptions.OAuthHost.IsMatch(host))
            {
                using var semaphore = new SemaphoreSlim(1, 1);
                await semaphore.WaitAsync(cancellationToken);
                refreshing.Reset();
                await makoClient.Refresh();
                refreshing.Set();
            }

            var session = makoClient.ContextualBoundedSession;

            MakoHttpOptions.WebApiHost.IsMatch(host).IfTrue(() => headers.TryAddWithoutValidation("Cookie", session!.Cookie));

            MakoHttpOptions.ApiHost.IsMatch(host).IfTrue(() => headers.Authorization.IfNull(() => headers.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken)));

            INameResolver resolver = MakoHttpOptions.BypassHost.IsMatch(host) && session!.Bypass || MakoHttpOptions.OAuthHost.IsMatch(host)
                ? makoClient.GetService<OrdinaryPixivDnsResolver>()
                : makoClient.GetService<LocalMachineDnsResolver>();
            return await MakoHttpOptions.ConstructHttpMessageInvoker(resolver).SendAsync(request, cancellationToken);
        }
    }
}