using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Atom;

namespace TollOperatorClient
{
    class TollPoint
    {
        public Channel Channel { get; set; }
        public string TollCode { get; set; }
    }

    class Program
    {
        static Dictionary<string, Client> clients;
        static List<TollPoint> channels;        
        
        static void Main(string[] args)
        {
            channels = new List<TollPoint>
                            {
                                //var channelCredentials = new SslCredentials(System.IO.File.ReadAllText("roots.pem"));  // Load a custom roots file.
                                // TODO: get channel ips, ports and ids from a config file/database
                                new TollPoint { Channel = new Channel("localhost:50051", /*channelCredentials*/ ChannelCredentials.Insecure), TollCode = "TOLL2" },
                                new TollPoint { Channel = new Channel("localhost:50052", /*channelCredentials*/ ChannelCredentials.Insecure), TollCode = "TOLL1" }
                            };
            clients = new Dictionary<string, Client>();

            foreach (var channel in channels)
            {
                // create subscription with an id; generate id with each execution
                // TODO: get client subscription id from a config file/database/...
                Subscription request = new Atom.Subscription { SubscriptionId = "FMPWH-" + new Random().Next(100, 999) };
                clients.Add(channel.TollCode, new Client(new TollAuditService.TollAuditServiceClient(channel.Channel), request));
            }

            foreach (var client in clients)
            {
                client.Value.Subscribe(client.Value.clientCode, client.Key);
            }
            
            Parallel.For(0, clients.Count, iterator =>
                {
                    //bool retryServerConnection = true;
                    while (true)//(retryServerConnection)
                    {
                        var tollCode = channels[iterator].TollCode;

                        try
                        {
                            clients[tollCode].GetStream(clients[tollCode].clientCode, tollCode).Wait();
                            //retryServerConnection = false;
                        }
                        catch (AggregateException ex)
                        {
                            ex.Handle(e => { Console.WriteLine($"{tollCode}: " + e.Message); return true; });
                            //retryServerConnection = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{tollCode}: " + ex.Message);
                            //retryServerConnection = true;
                        }
                    }
                });

            // unsubscribe from each server in a parallel fashion
            //Parallel.ForEach(clients, (client) => { client.Value.Unsubscribe(request); });

            // shutdown connection to each server subscribed to in a parallel manner
            //Parallel.ForEach(channels, (channel) => { channel.Channel.ShutdownAsync().Wait(); });

            #region old
            //Channel channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            //var client = new Client(new Atom.TollAuditService.TollAuditServiceClient(channel));
            // create subscription with an id; generate id with each execution
            //Atom.Subscription request = new Atom.Subscription { SubscriptionId = "FMPWH-" + new Random().Next(100, 999) };
            //client.GetStream(request).Wait(); 
            //channel.ShutdownAsync().Wait();
            #endregion

            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }

        private static void PrintRpcException(string tollId, RpcException eX)
        {
            if (eX.Status.StatusCode == StatusCode.Unavailable) //eX.Status.Detail == "Connect Failed")
                Console.WriteLine($"[{tollId}] No connection to server ");
            else if (eX.Status.StatusCode == StatusCode.Unknown) //eX.Status.Detail == "Stream removed")
                Console.WriteLine($"[{tollId}] Stream from server lost...");
            else
                Console.WriteLine(eX.Message);
        }
    }

    class Client
    {
        public readonly Subscription clientCode;
        readonly TollAuditService.TollAuditServiceClient client;

        public Client(TollAuditService.TollAuditServiceClient client, Subscription clientCode)
        {
            this.client = client;
            this.clientCode = clientCode;
        }
        
        public async Task GetStream(Subscription subscription, string tollId)
        {
            // call GetLiveStream method in server
            using (var call = client.GetLiveStream(subscription))
            {
                var responseStream = call.ResponseStream;

                // get events (toll vehicle info) from server
                while (await responseStream.MoveNext())
                {
                    var info = responseStream.Current; // get current event
                    Console.WriteLine($"CLIENT [{tollId}]: " + info);
                }
            }
        }       

        public Task Subscribe(Subscription subscription, string tollId)
        {
            try
            {
                var response = client.Subscribe(subscription);
                return Task.FromResult(response);
            }
            catch (RpcException ex)
            {
                //PrintExceptionToScreen(tollId, ex);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{subscription.SubscriptionId}: " + ex.Message);
            }
            return Task.FromResult(false);
        }

        public Task Unsubscribe(Subscription subscription)
        {
            var response = client.Unsubscribe(subscription);
            return Task.FromResult(response);
        }
    }
}
