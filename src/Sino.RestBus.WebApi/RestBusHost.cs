using Sino.RestBus.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sino.RestBus.WebApi
{
    public class RestBusHost : IDisposable
    {
        private static string machineHostName;
        readonly static string[] HTTP_RESPONSE_SERVER_HEADER = new string[] { "RestBus.WebApi" };
        private readonly IRestBusSubscriber subscriber;
        private string appVirtualPath;
        InterlockedBoolean hasStarted;
        volatile bool disposed = false;

        //TODO: Consider moving this into Common, Maybe Client (Requires a reference to System.Net.Http)
        internal static readonly ByteArrayContent _emptyByteArrayContent = new ByteArrayContent(new byte[0]);

        //TODO: Consider moving this to Common
        internal static Version VERSION_1_1 = new Version("1.1");

        public RestBusHost(IRestBusSubscriber subscriber)
        {
            this.subscriber = subscriber;
        }


        public void Start()
        {
            if (!hasStarted.SetTrueIf(false))
            {
                throw new InvalidOperationException("RestBus host has already started!");
            }
            subscriber.Start();

            Thread msgLooper = new Thread(RunLoop);
            msgLooper.Name = "RestBus WebApi Host";
            msgLooper.IsBackground = true;
            msgLooper.Start();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                subscriber.Dispose();
            }
        }

        private void RunLoop()
        {
            MessageContext context = null;
            while (true)
            {
                try
                {
                    context = subscriber.Dequeue();
                }
                catch (Exception e)
                {
                    if (!(e is ObjectDisposedException || e is OperationCanceledException))
                    {
                    }

                    if (disposed)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                var cancellationToken = CancellationToken.None;
                Task.Factory.StartNew((Func<object, Task>)Process, Tuple.Create(context, cancellationToken), cancellationToken);

            }
        }

        private async Task Process(object state)
        {
            try
            {
                var typedState = (Tuple<MessageContext, CancellationToken>)state;
                await ProcessRequest(typedState.Item1, typedState.Item2);
            }
            catch (Exception ex)
            {
                //TODO: SHouldn't occur (the called method should be safe): Log execption and return a server error
            }
        }

        private async Task ProcessRequest(MessageContext restbusContext, CancellationToken cancellationToken)
        {
            subscriber.SendResponse(restbusContext, CreateResponsePacketFromMessage(CreateResponseMessageFromException(new InvalidOperationException()), subscriber));
        }

        private static bool TryGetHttpRequestMessage(HttpRequestPacket packet, string virtualPath, string hostname, out HttpRequestMessage request)
        {
            try
            {
                request = new HttpRequestMessage
                {
                    Content = packet.Content == null ? _emptyByteArrayContent : new ByteArrayContent(packet.Content),
                    Version = packet.Version == "1.1" ? VERSION_1_1 : new Version(packet.Version),
                    Method = new HttpMethod(packet.Method ?? "GET"),
                    RequestUri = packet.BuildUri(virtualPath, hostname)
                };

                packet.PopulateHeaders(request.Content.Headers, request.Headers);
            }
            catch
            {
                request = null;
                return false;
            }

            return true;
        }

        private static HttpResponsePacket CreateResponsePacketFromMessage(HttpResponseMessage responseMsg, IRestBusSubscriber subscriber)
        {
            var responsePkt = responseMsg.ToHttpResponsePacket();
            responsePkt.Headers["Server"] = HTTP_RESPONSE_SERVER_HEADER;

            return responsePkt;
        }

        private static HttpResponseMessage CreateResponseMessageFromException(Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Exception: \r\n\r\n");
            sb.Append(ex.Message);
            sb.Append("\r\n\r\nStackTrace: \r\n\r\n");
            sb.Append(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.Append("Inner Exception: \r\n\r\n");
                sb.Append(ex.InnerException.Message);
                sb.Append("\r\n\r\nStackTrace: \r\n\r\n");
                sb.Append(ex.InnerException.StackTrace);
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(sb.ToString()),
                ReasonPhrase = "An unexpected exception was thrown."
            };

        }
    }
}
