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
using System.Threading;
using System.Threading.Tasks;
using Mako.Util;

namespace Mako.Net
{
    public class InterceptedHttpClientHandler : HttpClientHandler
    {
        private readonly MakoClient makoClient;
        private readonly HttpMessageInvoker delegatedHandler;

        protected InterceptedHttpClientHandler(MakoClient makoClient, HttpMessageHandler delegatedHandler)
        {
            (this.makoClient, this.delegatedHandler) = (makoClient, new HttpMessageInvoker(delegatedHandler));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Scopes.AttemptsAsync(() => base.SendAsync(request, cancellationToken), 2, request);
        }
    }
}