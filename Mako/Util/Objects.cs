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
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Mako.Util
{
    /// <summary>
    /// Provide a set of utilities upon a variety of types
    /// </summary>
    [PublicAPI]
    public static class Objects
    {
        /// <summary>
        /// Turns a <see cref="string"/> into <see cref="Regex"/>
        /// </summary>
        /// <param name="str">string to be transformed</param>
        /// <returns>regex</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Regex ToRegex(this string str)
        {
            return new Regex(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Acquires a byte array from <see cref="string"/> in specified <see cref="Encoding"/>.
        /// If the <see cref="Encoding"/> is not set it will use <see cref="Encoding.UTF8"/> as its
        /// default encoding
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string str, Encoding encoding = null)
        {
            return encoding?.Mapped(e => e.GetBytes(str)) ?? Encoding.UTF8.GetBytes(str);
        }

        public static string Hash<T>(this string str) where T : HashAlgorithm, new()
        {
            static string Sum(string s1, string s2) => s1 + s2;

            using var hasher = new T();
            var bytes = hasher.ComputeHash(str.GetBytes());
            return bytes.Select(b => b.ToString("x2")).Aggregate(Sum);
        }

        /// <summary>
        /// Returns a <see cref="HttpResponseMessage"/> once current request has read the header
        /// instead of full response
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> GetResponseHeader(this HttpClient client, string url)
        {
            return client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToJson(
            #nullable enable
            this object? obj,
            #nullable disable
            Formatting formatting = Formatting.None
        )
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = formatting });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FromJson<T>(this string src)
        {
            return JsonConvert.DeserializeObject<T>(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<T> GetJsonAsync<T>(this HttpClient httpClient, string url)
        {
            return (await httpClient.GetStringAsync(url)).FromJson<T>();
        }
    }
}