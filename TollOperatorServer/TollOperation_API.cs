using System;
using Atom;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TollOperatorServer
{
    class TollOperation_API : TollAuditService.TollAuditServiceBase
    {
        private readonly BufferBlock<TollVehicleInfo> _buffer = new BufferBlock<TollVehicleInfo>();

        private Dictionary<string, IServerStreamWriter<TollVehicleInfo>> _subscriberWritersMap =
            new Dictionary<string, IServerStreamWriter<TollVehicleInfo>>();

        public override Task<SubscriptionResponse> Subscribe(Subscription subscription, ServerCallContext context)
        {
            var result = Subscribe(subscription.SubscriptionId);            
            return Task.FromResult(new SubscriptionResponse { Success = result });
        }

        public override Task<SubscriptionResponse> Unsubscribe(Subscription request, ServerCallContext context)
        {
            _subscriberWritersMap.Remove(request.SubscriptionId);
            return Task.FromResult(new SubscriptionResponse() { Success = true }); // **********
        }

        private bool Subscribe(string id)
        {
            // persist subscription id in a data repository
            try
            {
                _subscriberWritersMap.Add(id, null);
            }
            catch (Exception ex)
            {
                // return false if subscription fails
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
            // in-memory repository to hold a streamWriter for each subscriber
            _subscriberWritersMap[request.SubscriptionId] = responseStream;

            while (_subscriberWritersMap.ContainsKey(request.SubscriptionId))
            {
                // Wait on BufferBlock from TPL Dataflow
                var @event = await _buffer.ReceiveAsync();
                foreach (var serverStreamWriter in _subscriberWritersMap.Values)
                {
                    await serverStreamWriter.WriteAsync(@event);
                }
            }
            //var responses = TollOperationSimulator.VehicleInfoGenerator.GenerateVehicleInfo(18, 0);
            //foreach (var response in responses)
            //{
            //    await responseStream.WriteAsync(response);
            //}
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
