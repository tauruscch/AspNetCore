// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Features
{
    public class Http2Activity
    {
        public int StreamId { get; set; }

        private static AsyncLocal<Http2Activity> _current = new AsyncLocal<Http2Activity>();
        public static Http2Activity Current
        {
            get
            {
                return _current.Value;
            }
            set
            {
                _current.Value = value;
            }
        }
    }

    public interface IHttp2StreamIdFeature
    {
        int StreamId { get; }
    }
}
