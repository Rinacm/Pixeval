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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Mako.Util
{
    /// <summary>
    /// Scope helper functions, inspired by Kotlin scope function
    /// </summary>
    [PublicAPI]
    public static class Scopes
    {
        /// <summary>
        /// Perform <see cref="Action{T}"/> on <paramref name="receiver"/> if <paramref name="receiver"/> is not null
        /// </summary>
        /// <param name="receiver">the scope receiver</param>
        /// <param name="action">the action to be performed</param>
        /// <typeparam name="R">receiver type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Let<R>(this R receiver, Action<R> action)
        {
            if (receiver != null) action(receiver);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfNull(this object obj, Action action)
        {
            if (obj == null) action();
        }

        /// <summary>
        /// Apply <see cref="Func{Boolean}"/> on <paramref name="receiver"/> and returns a boolean
        /// if <paramref name="receiver"/> is not null, otherwise returns false
        /// </summary>
        /// <param name="receiver">the scope receiver</param>
        /// <param name="function">the function to be applied</param>
        /// <typeparam name="R">receiver type</typeparam>
        /// <returns>boolean value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check<R>(this R receiver, Func<bool> function)
        {
            return receiver != null && function();
        }

        /// <summary>
        /// Apply <see cref="Func{R, U}"/> on <paramref name="receiver"/>
        /// </summary>
        /// <param name="receiver">receiver</param>
        /// <param name="function">function</param>
        /// <typeparam name="R">receiver type</typeparam>
        /// <typeparam name="U">result type</typeparam>
        /// <returns>mapped value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U Mapped<R, U>(this R receiver, Func<R, U> function)
        {
            return function(receiver);
        }

        /// <summary>
        /// Invoke <see cref="Action"/> if <paramref name="receiver"/> is true
        /// </summary>
        /// <param name="receiver">receiver</param>
        /// <param name="action">action </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfTrue(this bool receiver, Action action)
        {
            if (receiver) action();
        }

        /// <summary>
        /// Invoke <see cref="Func{R}"/> on <paramref name="receiver"/> if <paramref name="receiver"/> is true
        /// </summary>
        /// <param name="receiver">receiver</param>
        /// <param name="ifTrue">function</param>
        /// <typeparam name="R">return type</typeparam>
        /// <returns><typeparamref name="R"/> if <paramref name="receiver"/> is true, otherwise default</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R ApplyIfTrue<R>(this bool receiver, Func<R> ifTrue) where R : class
        {
            return receiver ? ifTrue() : null;
        }

        /// <summary>
        /// Create an identity function which always returns itself
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <returns>identity function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<R, R> Identity<R>()
        {
            return r => r;
        }

        /// <summary>
        /// return a <typeparamref name="T"/> if provided <see cref="value"/> is <code>true</code>
        /// otherwise return <code>null</code>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ifTrue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElseNull<T>(this bool value, T ifTrue)
        {
            return value ? ifTrue : default;
        }

        /// <summary>
        /// Attempts to invoke <see cref="Func{R}"/>, and retries until <paramref name="times"/>
        /// is exceeded
        /// </summary>
        /// <param name="body"></param>
        /// <param name="times"></param>
        /// <param name="exceptionHandler"></param>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        /// <exception cref="MaxRetryCountExceededException"></exception>
        public static R Attempts<R>(Func<R> body, int times = 3, Action<Exception> exceptionHandler = null)
        {
            var counter = 0;
            while (counter++ < times)
            {
                try
                {
                    return body();
                }
                catch (Exception e)
                {
                    exceptionHandler?.Invoke(e);
                }
            }

            throw new MaxRetryCountExceededException();
        }

        /// <summary>
        /// Asynchronously attempts to invoke <see cref="Func{R}"/>, and retries until <paramref name="times"/>
        /// is exceeded
        /// </summary>
        /// <param name="body"></param>
        /// <param name="times"></param>
        /// <param name="exceptionHandler"></param>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        /// <exception cref="MaxRetryCountExceededException"></exception>
        public static async Task<R> AttemptsAsync<R>(Func<Task<R>> body, int times = 3, Action<Exception> exceptionHandler = null)
        {
            var counter = 0;
            while (counter++ < times)
            {
                try
                {
                    return await body();
                }
                catch (Exception e)
                {
                    exceptionHandler?.Invoke(e);
                }
            }

            throw new MaxRetryCountExceededException();
        }
    }
}