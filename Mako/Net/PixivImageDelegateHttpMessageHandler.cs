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
using System.Threading;
using System.Threading.Tasks;
using Mako.Util;

namespace Mako.Net
{
    public class PixivImageDelegateHttpMessageHandler : HttpMessageHandler
    {
        private readonly MakoClient makoClient;

        public PixivImageDelegateHttpMessageHandler([InjectMarker] MakoClient makoClient)
        {
            this.makoClient = makoClient;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var host = request.RequestUri!.IdnHost;
            if (host == MakoHttpOptions.ImageHost)
            {
                var mirror = makoClient.ContextualBoundedSession.MirrorHost;
                mirror.IsNotNullOrEmpty().IfTrue(() =>
                {
                    var oldUri = request.RequestUri;
                    Uri newUri;
                    if (Uri.CheckHostName(mirror) is not UriHostNameType.Unknown)
                    {
                        newUri = new UriBuilder(oldUri)
                        {
                            Host = mirror
                        }.Uri;
                    }
                    else if (Uri.IsWellFormedUriString(mirror, UriKind.Absolute))
                    {
                        var mirrorUri = new Uri(mirror);
                        newUri = new UriBuilder(oldUri)
                        {
                            Host = mirrorUri.Host,
                            Scheme = mirrorUri.Scheme
                        }.Uri;
                    }
                    else
                    {
                        throw new FormatException("Expect a valid DNS host or URL");
                    }

                    request.RequestUri = newUri;
                });
            }

            INameResolver resolver = makoClient.ContextualBoundedSession.Bypass
                ? makoClient.GetService<OrdinaryPixivImageDnsResolver>()
                : makoClient.GetService<LocalMachineDnsResolver>();
            return MakoHttpOptions.ConstructHttpMessageInvoker(resolver).SendAsync(request, cancellationToken);
        }
    }
}