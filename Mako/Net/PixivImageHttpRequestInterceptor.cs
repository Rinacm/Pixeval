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
using System.Threading.Tasks;
using Mako.Util;

namespace Mako.Net
{
    public class PixivImageHttpRequestInterceptor : IHttpRequestInterceptor
    {
        private readonly MakoClient makoClient;

        public PixivImageHttpRequestInterceptor([InjectMarker] MakoClient makoClient)
        {
            this.makoClient = makoClient;
        }

        public Task Intercept(HttpRequestMessage message, IInterceptConfigurations configurations)
        {
            var conf = configurations as PixivRequestInterceptorConfiguration ?? throw new InvalidCastException($"{nameof(configurations)} must be {nameof(PixivRequestInterceptorConfiguration)}");
            var host = message.RequestUri.IdnHost;
            if (host == conf.ImageHost)
            {
                if (conf.MirrorHost.NotNullOrEmpty())
                {
                    var oldUri = message.RequestUri.ToString();
                    message.RequestUri = new Uri(oldUri.Replace("https://i.pximg.net", conf.MirrorHost));
                }

                if (conf.Bypass) ReplaceRequest(message);
            }

            return Task.CompletedTask;
        }

        private void ReplaceRequest(HttpRequestMessage message)
        {
            var host = message.RequestUri.IdnHost;
            var isSslSession = message.RequestUri.Scheme == "https";

            string address = makoClient.GetService<OrdinaryPixivImageDnsResolver>();
            message.RequestUri = new Uri($"http{isSslSession.ApplyIfTrue(() => "s")}://{address}{message.RequestUri.PathAndQuery}");
            message.Headers.Host = host;
        }
    }
}