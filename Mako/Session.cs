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
        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        public string MailAddress { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public DateTime CookieCreation { get; set; }

        public bool IsPremium { get; set; }

        public string Cookie { get; set; }

        public bool Bypass { get; set; } = true;

        public string MirrorHost { get; set; }

        public DateTime TokenRefreshed { get; set; }

        public ISet<string> ExcludeTags { get; } = new HashSet<string>();

        public ISet<string> IncludeTags { get; } = new HashSet<string>();

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

        public static Session Parse(TokenResponse tokenResponse, string password, Session previousSession = null)
        {
            var response = tokenResponse.ToResponse;
            var session = (Session) previousSession?.MemberwiseClone() ?? new Session();
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
            TokenRefreshed -= TimeSpan.FromHours(1);
        }
#endif
    }
}