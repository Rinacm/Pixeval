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

using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Mako.Util;

namespace Mako.Net
{
    public static class MakoHttpOptions
    {
        public const string AppApiBaseUrl = "https://app-api.pixiv.net";

        public const string SauceNaoBaseUrl = "https://saucenao.com";

        public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";

        public const string WebApiBaseUrl = "https://www.pixiv.net";

        public const string ImageHost = "i.pximg.net";

        public static readonly Regex ApiHost = "^app-api\\.pixiv\\.net$".ToRegex();

        public static readonly Regex WebApiHost = "^(pixiv\\.net)|(www\\.pixiv\\.net)$".ToRegex();

        public static readonly Regex OAuthHost = "^oauth\\.secure\\.pixiv\\.net$".ToRegex();

        public static readonly Regex BypassHost = "^app-api\\.pixiv\\.net$|^(pixiv\\.net)|(www\\.pixiv\\.net)$".ToRegex();

        internal static HttpMessageInvoker ConstructHttpMessageInvoker(INameResolver dnsResolver)
        {
            return new(new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) =>
                {
                    var sockets = new Socket(SocketType.Stream, ProtocolType.Tcp); // disposed by networkStream
                    await sockets.ConnectAsync(await dnsResolver.Lookup(context.DnsEndPoint.Host), 443, token);
                    var networkStream = new NetworkStream(sockets, true); // disposed by sslStream
                    var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);

                    await sslStream.AuthenticateAsClientAsync(string.Empty);
                    return sslStream;
                }
            });
        }
    }
}