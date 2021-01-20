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

// ReSharper disable UnusedMemberInSuper.Global
namespace Mako.Net
{
    /// <summary>
    /// The configurations which might be helpful when <see cref="IHttpRequestInterceptor"/>
    /// trying to intercept a request
    /// </summary>
    public interface IInterceptConfigurations
    {
        /// <summary>
        /// string-based key value pair to represents ordinary configurations
        /// </summary>
        IReadOnlyDictionary<string, string> Configurations { get; }
    }
}