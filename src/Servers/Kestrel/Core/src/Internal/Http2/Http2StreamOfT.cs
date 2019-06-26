using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2
{
    internal sealed class Http2Stream<TContext> : Http2Stream
    {
        private readonly IHttpApplication<TContext> _application;

        public Http2Stream(IHttpApplication<TContext> application, Http2StreamContext context) : base(context)
        {
            _application = application;
        }

        public override void Execute()
        {
            Http2Activity.Current = new Http2Activity();
            Http2Activity.Current.StreamId = _context.StreamId;
            // REVIEW: Should we store this in a field for easy debugging?
            _ = ProcessRequestsAsync(_application);
        }
    }
}
