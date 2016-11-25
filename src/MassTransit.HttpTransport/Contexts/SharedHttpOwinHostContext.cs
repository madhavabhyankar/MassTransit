// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.HttpTransport.Contexts
{
    using System;
    using System.Threading;
    using GreenPipes;
    using Hosting;
    using Util;


    public class SharedHttpOwinHostContext :
        OwinHostContext,
        IDisposable
    {
        readonly CancellationToken _cancellationToken;
        readonly OwinHostContext _context;
        readonly ITaskParticipant _participant;

        public SharedHttpOwinHostContext(OwinHostContext context, CancellationToken cancellationToken, ITaskScope scope)
        {
            _context = context;
            _cancellationToken = cancellationToken;


            _participant = scope.CreateParticipant($"{TypeMetadataCache<SharedHttpOwinHostContext>.ShortName} - {context.HostSettings.ToDebugString()}");
            _participant.SetReady();
        }

        public void Dispose()
        {
            _participant.SetComplete();
        }

        public CancellationToken CancellationToken => _cancellationToken;

        public bool HasPayloadType(Type contextType)
        {
            return _context.HasPayloadType(contextType);
        }

        public bool TryGetPayload<TPayload>(out TPayload payload) where TPayload : class
        {
            return _context.TryGetPayload(out payload);
        }

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory) where TPayload : class
        {
            return _context.GetOrAddPayload(payloadFactory);
        }

        public HttpHostSettings HostSettings => _context.HostSettings;

        public void StartHttpListener(HttpConsumerAction controller)
        {
            _context.StartHttpListener(controller);
        }

        public void StopHttpListener()
        {
            _context.StopHttpListener();
        }
    }
}