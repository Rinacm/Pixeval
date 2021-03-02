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

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mako.Util
{
    /// <summary>
    /// Additional signatures generation for login purpose, thanks to <a href="https://github.com/Notsfsssf">@Notsfsssf</a>'s
    /// reverse engineering.
    /// See <a href="https://github.com/Notsfsssf/pixez-flutter/blob/master/android/app/src/main/kotlin/com/perol/pixez/CodeGen.kt">CodeGen.kt</a>
    /// to get more information.
    /// The replication of above codes is permitted by <a href="https://github.com/Notsfsssf">@Notsfsssf</a>
    /// </summary>
    internal static class AuthSignature
    {
        public static string GetCodeVer()
        {
            var bytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes); // hope for an async version :(
            return bytes.ToURLSafeBase64String();
        }

        public static async Task<string> GetCodeChallenge(string code)
        {
            var bArr = code.GetBytes(Encoding.ASCII);
            var csp = new SHA256CryptoServiceProvider();
            await using var bStream = new MemoryStream(bArr);
            var resultBytes = await csp.ComputeHashAsync(bStream);
            return resultBytes.ToURLSafeBase64String();
        }
    }
}