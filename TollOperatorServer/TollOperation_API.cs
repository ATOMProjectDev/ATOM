using System;
using Atom;
using System.Collections.Generic;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TollOperatorServer
{
    class TollOperation_API : TollAuditService.TollAuditServiceBase
    {
        // data stream to push/publish toll vehicle info from server to client
        private readonly BufferBlock<TollVehicleInfo> _buffer = new BufferBlock<TollVehicleInfo>();

        // in-memory repository to hold a streamWriter for each client subscriber and their respective callback response stream to pass data back to clients
        private static Dictionary<string, IServerStreamWriter<TollVehicleInfo>> _subscribedClients =
            new Dictionary<string, IServerStreamWriter<TollVehicleInfo>>();

        // subscribe client to toll events
        public override Task<SubscriptionResponse> Subscribe(Subscription subscription, ServerCallContext context)
        {
            var result = Subscribe(subscription.SubscriptionId);            
            return Task.FromResult(new SubscriptionResponse { Success = result });
        }

        // unsubscribe client from toll events
        public override Task<SubscriptionResponse> Unsubscribe(Subscription request, ServerCallContext context)
        {
            var result = Unsubscribe(request.SubscriptionId);
            return Task.FromResult(new SubscriptionResponse() { Success = result }); 
        }

        private bool Subscribe(string id)
        {
            // persist subscription id addition in a data repository
            try
            {
                _subscribedClients.Add(id, null);
            }
            catch (Exception)
            {
                // return false if subscription fails
                return false;
            }
            return true;
        }

        private bool Unsubscribe(string id)
        {
            // persist subscription id removal in a data repository
            try
            {
                _subscribedClients.Remove(id);
            }
            catch (Exception)
            {
                // return false if unsubscription fails
                return false;
            }
            return true;
        }

        public override /*async*/ Task GetDailyLiveCount(Subscription request, IServerStreamWriter<VehicleCount> responseStream, ServerCallContext context)
        {
            return base.GetDailyLiveCount(request, responseStream, context);
        }

        public override async Task GetLiveStream(Subscription request, IServerStreamWriter<TollVehicleInfo> responseStream, ServerCallContext context)
        {
            ////if (_subscribedClients.ContainsKey(request.SubscriptionId))
            {
                _subscribedClients[request.SubscriptionId] = responseStream;

                while (_subscribedClients.ContainsKey(request.SubscriptionId))
                {
                    //_subscribedClients[request.SubscriptionId] = responseStream;

                    // Wait on BufferBlock to receive asynchronously from target source
                    var vehicleInfo = await _buffer.ReceiveAsync();

                    foreach (var serverStreamWriter in _subscribedClients.Values)
                    {
                        try
                        {
                            await serverStreamWriter.WriteAsync(vehicleInfo);
                        }
                        catch
                        {
                            // catch exceptions thrown when connection with client is lost or bcos of other unforseen errors during server push
                        }
                    }
                }
            }
        }

        public override Task<VehicleCount> GetVehicleCount(SearchRange request, ServerCallContext context)
        {
            return base.GetVehicleCount(request, context);
        }

        public override Task<TollVehicleInfo> GetVehicleCountSummary(SearchRange request, ServerCallContext context)
        {
            return base.GetVehicleCountSummary(request, context);
        }

        public void Publish(TollVehicleInfo data)
        {
            _buffer.Post(data);
        }
    }
}
