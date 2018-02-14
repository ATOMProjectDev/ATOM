using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
namespace TollOperatorClient
{
    class Program
    {
        class Client
        {
            readonly Atom.TollAuditService.TollAuditServiceClient client;
            public Client(Atom.TollAuditService.TollAuditServiceClient client)
            {
                this.client = client;
            }

            public async Task GetStream(Atom.Subscription sub)
            {
                int count = 1;
                try
                {
                    // call GetLiveStream method in server
                    using (var call = client.GetLiveStream(sub))
                    {
                        var responseStream = call.ResponseStream;

                        // get events (toll vehicle info) from server
                        while (await responseStream.MoveNext())
                        {
                            var info = responseStream.Current; // get current event
                            System.Console.WriteLine($"# {count++}: " + info);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
        }
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new Client(new Atom.TollAuditService.TollAuditServiceClient(channel));

            // create subscription with an id; generate id with each execution
            Atom.Subscription request = new Atom.Subscription { SubscriptionId = "FMPWH-" + new Random().Next(100, 999) };
            client.GetStream(request).Wait(); 
            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
