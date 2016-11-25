﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.HttpTransport.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Internals.Extensions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Serialization;
    using TestFramework;
    using Util;


    [TestFixture]
    public class HostingRequestResponse_Specs :
        AsyncTestFixture
    {
        Uri _hostAddress;

        [Test]
        [Explicit]
        public async Task Should_create_a_host_that_responds_to_requests()
        {
            _hostAddress = new Uri("http://localhost:8080");

            var busControl = Bus.Factory.CreateUsingHttp(cfg =>
            {
                var mainHost = cfg.Host(_hostAddress, h =>
                {
                    h.Method = HttpMethod.Post;
                });

                cfg.ReceiveEndpoint(mainHost, ep =>
                {
                    ep.Consumer<HttpRequestConsumer>();
                });
            });

            await busControl.StartAsync(TestCancellationToken);
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new Request {Value = "Hello"};
                    var envelope = new HttpMessageEnvelope(request, TypeMetadataCache<Request>.MessageTypeNames);
                    envelope.RequestId = NewId.NextGuid().ToString();
                    envelope.DestinationAddress = _hostAddress.ToString();
                    envelope.ResponseAddress = _hostAddress.ToString();

                    var messageBody = JsonConvert.SerializeObject(envelope, JsonMessageSerializer.SerializerSettings);

                    var content = new StringContent(messageBody, Encoding.UTF8, "application/vnd.masstransit+json");

                    var result = await client.PostAsync(_hostAddress, content);

                    await Console.Out.WriteLineAsync("Request complete");

                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await busControl.StopAsync().WithTimeout(TimeSpan.FromSeconds(30));
            }

//            var mc = new MessageRequestClient<Ping, Pong>(bus, new Uri("http://requestb.in/15alnbk1"), TimeSpan.FromMinutes(500));

            //          await mc.Request(new Ping(), default(CancellationToken));
        }


        public class HttpRequestConsumer :
            IConsumer<Request>
        {
            public async Task Consume(ConsumeContext<Request> context)
            {
                await Console.Out.WriteLineAsync($"Received Request: {context.Message.Value}");

    //            context.Respond(new Response {RequestValue = context.Message.Value, ResponseValue = $"{context.Message.Value}, World."});
            }
        }


        public class Request
        {
            public string Value { get; set; }
        }


        public class Response
        {
            public string RequestValue { get; set; }
            public string ResponseValue { get; set; }
        }
    }


    public class HttpMessageEnvelope :
        MessageEnvelope
    {
        public HttpMessageEnvelope(object message, string[] messageTypeNames)
        {
            ConversationId = NewId.NextGuid().ToString();
            MessageId = NewId.NextGuid().ToString();

            MessageType = messageTypeNames;

            Message = message;

            Headers = new Dictionary<string, object>();

            Host = HostMetadataCache.Host;
        }

        public string MessageId { get; set; }
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string ConversationId { get; set; }
        public string InitiatorId { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string ResponseAddress { get; set; }
        public string FaultAddress { get; set; }
        public string[] MessageType { get; }
        public object Message { get; }
        public DateTime? ExpirationTime { get; set; }
        public IDictionary<string, object> Headers { get; }
        public HostInfo Host { get; }
    }
}