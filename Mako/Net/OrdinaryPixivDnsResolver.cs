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
using System.Linq;
using System.Net;

namespace Mako.Net
{
    /// <summary>
    /// Resolves a host to its IP address through any non-polluted DNS server, since Pixiv's real IP address
    /// has been protected under the mask of CloudFlare which further more makes it became non-retrievable,
    /// we cannot, and no longer need to retrieve the records dynamically, though the interface is reserved
    /// in case that any new measure being excavated
    /// </summary>
    public sealed class OrdinaryPixivDnsResolver : INameResolver
    {
        public IEnumerable<IPAddress> Lookup(string hostname)
        {
            yield return IPAddress.Parse("210.140.131.219");
            yield return IPAddress.Parse("210.140.131.223");
            yield return IPAddress.Parse("210.140.131.226");
        }

        public static implicit operator string(OrdinaryPixivDnsResolver resolver) => resolver.Lookup(null).First().ToString();
    }
}