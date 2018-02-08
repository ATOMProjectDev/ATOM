using System;
using Atom;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using System.Threading.Tasks;

namespace TollOperatorServer
{
    class TollOperation_API : TollAuditService.TollAuditServiceBase
    {
        public override /*async*/ Task GetLiveCount(Empty request, IServerStreamWriter<VehicleCount> responseStream, ServerCallContext context)
        {
            return base.GetLiveCount(request, responseStream, context);
        }

        public override async Task GetLiveStream(Empty request, IServerStreamWriter<VehicleInfo> responseStream, ServerCallContext context)
        {
            var responses = TollOperationSimulator.VehicleInfoGenerator.GenerateVehicleInfo(18, 0);
            foreach (var response in responses)
            {
                await responseStream.WriteAsync(response);
            }
        }

        public override Task<VehicleCount> GetVehicleCount(TimeRange request, ServerCallContext context)
        {
            return base.GetVehicleCount(request, context);
        }

        public override Task<VehicleInfo> GetVehicleCountSummary(TimeRange request, ServerCallContext context)
        {
            return base.GetVehicleCountSummary(request, context);
        }
    }
}
