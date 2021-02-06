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

using JetBrains.Annotations;

namespace Mako.Util
{
    [PublicAPI]
    public class Result<T>
    {
        public Result(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public static Result<T> Success(T value)
        {
            return new Success<T>(value);
        }

        public static Result<T> Failure => new Failure<T>(default);
    }

    [PublicAPI]
    public class Success<T> : Result<T>
    {
        public Success(T value) : base(value)
        {
        }

        public static implicit operator T(Success<T> success)
        {
            return success.Value;
        }
    }

    [PublicAPI]
    public class Failure<T> : Result<T>
    {
        public Failure(T value) : base(value)
        {
        }
    }
}