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
using System.Reflection;
using Refit;

namespace Mako.Util
{
    /// <summary>
    /// A helper factory class to create <see cref="Exception"/> objects
    /// </summary>
    internal static class Errors
    {
        public static LoginFailedException LoginFailed(string certificate, LoginFailedKind kind, string account = null)
        {
            return new LoginFailedException(certificate, kind, account);
        }

        public static MakoNetworkException NetworkException(string message, string url, bool bypass)
        {
            return new MakoNetworkException(message, bypass, url);
        }

        public static MakoNetworkException EnumeratingNetworkException(string url, int pageRequested, string extraMessage, bool bypass)
        {
            return new MakoNetworkException($"Current Requesting Url: {url}. Current Page Requested: {pageRequested}. {extraMessage} {bypass.IfTrue(() => "(bypassing)")}", bypass, url);
        }

        public static AuthenticationTimeoutException AuthenticationTimeout(string account, string certificate, bool bypass, bool refreshing)
        {
            return new AuthenticationTimeoutException(account, certificate, bypass, refreshing);
        }

        public static MaxRetriesExceededException MaxRetriesExceeded(MethodInfo executionBody, Exception cause, int maxRetries, string caller, object extraMessage)
        {
            return new MaxRetriesExceededException(executionBody, cause, maxRetries, caller, extraMessage);
        }

        public static MakoNetworkException ToMakoNetworkException(this ApiException apiException, bool bypass)
        {
            return new MakoNetworkException(apiException.Message, bypass, apiException.Uri.ToString(), apiException);
        }

        public static ArgumentOutOfRangeException ArgumentOutOfRange(string message)
        {
            return new ArgumentOutOfRangeException(message);
        }
    }
}