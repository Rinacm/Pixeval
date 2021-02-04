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
using System.Collections.Generic;
using JetBrains.Annotations;
using Mako.Net;
using Mako.Net.ResponseModel;
using Mako.Util;

namespace Mako
{
    [PublicAPI]
    public class Session
    {
        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Token expiration
        /// </summary>
        public DateTime ExpireIn { get; set; }

        /// <summary>
        /// Current access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Current refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Avatar
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// User id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's registered e-mail address
        /// </summary>
        public string MailAddress { get; set; }

        /// <summary>
        /// Account for login
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Password for login
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates current user is Pixiv Premium or not
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// WebAPI cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Automatically bypass GFW or not, default is set to true.
        /// If you are currently living in China Mainland, turn it on to make sure
        /// you can use Mako without using any kind of proxy, otherwise you will
        /// need a proper proxy server to bypass the GFW
        /// </summary>
        public bool Bypass { get; set; }

        /// <summary>
        /// Mirror server's host of image downloading
        /// </summary>
        public string MirrorHost { get; set; }

        /// <summary>
        /// The time of current session's refresh
        /// </summary>
        public DateTime TokenRefreshed { get; set; }

        /// <summary>
        /// Indicates which tags should be strictly exclude when performing a query operation
        /// </summary>
        public ISet<string> ExcludeTags { get; } = new HashSet<string>();

        /// <summary>
        /// Indicates which tags should be strictly include when performing a query operation
        /// </summary>
        public ISet<string> IncludeTags { get; } = new HashSet<string>();

        /// <summary>
        /// Any illust with less bookmarks will be filtered out
        /// </summary>
        public int MinBookmark { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }

        public bool RefreshRequired()
        {
            return AccessToken.IsNullOrEmpty() || DateTime.Now - TokenRefreshed >= TimeSpan.FromMinutes(50);
        }

        public IInterceptConfigurations ToPixivInterceptConfiguration()
        {
            return new PixivRequestInterceptorConfiguration
            {
                Token = AccessToken,
                Cookie = Cookie,
                Bypass = Bypass,
                MirrorHost = MirrorHost
            };
        }

        public static Session Parse(TokenResponse tokenResponse, string password, Session previousSession)
        {
            var response = tokenResponse.ToResponse;
            var session = (Session) previousSession.MemberwiseClone();
            session.TokenRefreshed = DateTime.Now;
            session.Name = response.User.Name;
            session.ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn);
            session.AccessToken = response.AccessToken;
            session.RefreshToken = response.RefreshToken;
            session.AvatarUrl = response.User.ProfileImageUrls.Px170X170;
            session.Id = response.User.Id.ToString();
            session.MailAddress = response.User.MailAddress;
            session.Account = response.User.Account;
            session.Password = password;
            session.IsPremium = response.User.IsPremium;
            return session;
        }

#if DEBUG
        public void Invalidate()
        {
            TokenRefreshed = DateTime.MinValue;
        }
#endif
    }
}