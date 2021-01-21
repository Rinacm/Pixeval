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
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Mako.Util;

namespace Mako
{
    [PublicAPI]
    public class AuthenticationTimeoutException : Exception
    {
        public bool Refreshing { get; }

        public string Account { get; }

        public string Certificate { get; }

        public bool Bypass { get; }

        public AuthenticationTimeoutException(string account, string certificate, bool bypass, bool refreshing)
            : this($"Error occurs while attempting to authenticate {account} {bypass.IfTrue(() => "(bypassing)")} {refreshing.IfTrue(() => "(refreshing)")}")
        {
            (Account, Certificate, Bypass, Refreshing) = (account, certificate, bypass, refreshing);
        }


        protected AuthenticationTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AuthenticationTimeoutException(string message) : base(message)
        {
        }

        public AuthenticationTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}