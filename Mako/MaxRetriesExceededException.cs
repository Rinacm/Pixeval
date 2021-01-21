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
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Mako
{
    [PublicAPI]
    public class MaxRetriesExceededException : Exception
    {
        public object ExtraMessage { get; }

        public MethodInfo ExecutionBody { get; }

        public int MaxRetries { get; }

        public string Caller { get; }

        public MaxRetriesExceededException(MethodInfo executionBody, Exception cause, int maxRetries, string caller, object extraMessage)
            : this($"Max retries exceeded when attempting to invoke {executionBody} in {caller} after {maxRetries} times", cause)
        {
            (ExecutionBody, MaxRetries, Caller, ExtraMessage) = (executionBody, maxRetries, caller, extraMessage);
        }

        protected MaxRetriesExceededException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MaxRetriesExceededException([CanBeNull] string message) : base(message)
        {
        }

        public MaxRetriesExceededException([CanBeNull] string message, [CanBeNull] Exception innerException) : base(message, innerException)
        {
        }
    }
}