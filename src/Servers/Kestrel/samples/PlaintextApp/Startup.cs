// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PlaintextApp
{
    public class Startup
    {
        private static readonly byte[] _helloWorldBytes = Encoding.UTF8.GetBytes("Hello, World!");

        public void Configure(IApplicationBuilder app)
        {
            app.UseFileServer();
            app.Run((httpContext) =>
            {
                var payload = _helloWorldBytes;
                var response = httpContext.Response;

                response.StatusCode = 200;
                response.ContentType = "text/plain";
                response.ContentLength = payload.Length;

                return response.BodyWriter.WriteAsync(payload).GetAsTask();
            });
        }

        public static async Task Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5001, o =>
                    {
                        o.UseHttps();
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel.Https.Internal", LogLevel.Trace);
                    logging.Services.AddSingleton<ILoggerProvider, ConsoleProvider>();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            await host.StartAsync();

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.C)
                {
                    Console.Clear();
                }
            }
        }
    }

    public class ConsoleProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName);
        }

        public void Dispose()
        {
        }

        private class Logger : ILogger
        {
            private string categoryName;

            public Logger(string categoryName)
            {
                this.categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine($"[{Http2Activity.Current?.StreamId}][{logLevel.ToString().ToLower()}]: {formatter(state, exception)}");
            }
        }
    }

    internal static class ValueTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetAsTask(this in ValueTask<FlushResult> valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                // Signal consumption to the IValueTaskSource
                valueTask.GetAwaiter().GetResult();
                return Task.CompletedTask;
            }
            else
            {
                return valueTask.AsTask();
            }
        }
    }
}
