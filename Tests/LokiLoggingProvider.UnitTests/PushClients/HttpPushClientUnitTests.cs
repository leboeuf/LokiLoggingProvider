namespace LokiLoggingProvider.UnitTests.PushClients
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading;
    using LokiLoggingProvider.Logger;
    using LokiLoggingProvider.PushClients;
    using Xunit;

    public class HttpPushClientUnitTests
    {
        public class Dispose
        {
            [Fact]
            public void When_DisposingMoreThanOnce_Expect_NoExceptions()
            {
                // Arrange
                HttpPushClient pushClient = new(new HttpClient());

                // Act
                Exception result = Record.Exception(() =>
                {
                    pushClient.Dispose();
                    pushClient.Dispose();
                });

                // Assert
                Assert.Null(result);
            }
        }

        public class Push
        {
            [Fact]
            public void When_PushingLogMessageEntry_Expect_HttpRequestMessage()
            {
                // Arrange
                HttpRequestHandler requestHandler = new();

                HttpClient httpClient = new(requestHandler)
                {
                    BaseAddress = new Uri("http://localhost:3100"),
                };

                HttpPushClient pushClient = new(httpClient);

                LokiLogEntry entry = new(
                    Timestamp: new DateTime(2019, 11, 30, 01, 00, 00, DateTimeKind.Utc),
                    Labels: new LabelValues
                    {
                        { "key1", "value1" },
                        { "key2", "value2" },
                    },
                    Message: "My log message.");

                // Act
                pushClient.Push(entry);

                // Assert
                Assert.Collection(
                    requestHandler.ReceivedHttpRequestMessages,
                    request =>
                    {
                        Assert.Equal(HttpMethod.Post, request.Method);
                        Assert.Equal(MediaTypeNames.Application.Json, request.Content.Headers.ContentType.MediaType);
                        Assert.Null(request.Content.Headers.ContentType.CharSet);

                        Assert.Equal(
                            "{\"streams\":[{\"stream\":{\"key1\":\"value1\",\"key2\":\"value2\"},\"values\":[[\"15750756000\",\"My log message.\"]]}]}",
                            request.Content.ReadAsStringAsync().Result);
                    });
            }

            [Fact]
            public void When_PushingLogMessageEntryWithDisposedPushClient_Expect_ObjectDisposedException()
            {
                // Arrange
                HttpPushClient pushClient = new(new HttpClient());
                pushClient.Dispose();

                LokiLogEntry entry = new(default, default, default);

                // Act
                Exception result = Record.Exception(() => pushClient.Push(entry));

                // Assert
                ObjectDisposedException objectDisposedException = Assert.IsType<ObjectDisposedException>(result);
                Assert.Equal(nameof(HttpPushClient), objectDisposedException.ObjectName);
            }
        }

        private class HttpRequestHandler : DelegatingHandler
        {
            public List<HttpRequestMessage> ReceivedHttpRequestMessages { get; } = new List<HttpRequestMessage>();

            protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.ReceivedHttpRequestMessages.Add(request);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                };
            }
        }
    }
}
