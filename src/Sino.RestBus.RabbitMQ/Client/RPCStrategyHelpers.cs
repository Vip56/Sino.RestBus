using RabbitMQ.Client.Events;
using Sino.RestBus.Common;
using Sino.RestBus.RabbitMQ.ChannelPooling;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sino.RestBus.RabbitMQ.Client
{
    internal class RPCStrategyHelpers
    {
        internal const string DIRECT_REPLY_TO_QUEUENAME_ARG = "amq.rabbitmq.reply-to";
        internal const int HEART_BEAT = 30;

        //TODO: Consider moving this to Common
        internal static Version VERSION_1_1 = new Version("1.1");

        internal static void WaitForResponse(HttpRequestMessage request, ExpectedResponse arrival, TimeSpan requestTimeout, AmqpModelContainer model, bool closeModel, CancellationToken cancellationToken, TaskCompletionSource<HttpResponseMessage> taskSource, Action cleanup)
        {
            var localVariableInitLock = new object();

            lock (localVariableInitLock)
            {
                //TODO: Have cancellationToken signal WaitHandle so that threadpool stops waiting.

                RegisteredWaitHandle callbackHandle = null;
                callbackHandle = ThreadPool.RegisterWaitForSingleObject(arrival.ReceivedEvent.WaitHandle,
                    (state, timedOut) =>
                    {
                        //TODO: Investigate, this memorybarrier might be unnecessary since the thread is released from the threadpool
                        //after deserializationException and responsePacket is set.
                        Interlocked.MemoryBarrier(); //Ensure non-cached versions of arrival are read
                        try
                        {
                            //TODO: Check Cancelation Token when it's implemented

                            if (closeModel) model.Close();
                            SetResponseResult(request, timedOut, arrival, taskSource);

                            lock (localVariableInitLock)
                            {
                                callbackHandle.Unregister(null);
                            }

                        }
                        catch
                        {
                            //TODO: Log this: 
                            // the code in the try block should be safe so this catch block should never be called, 
                            // hoewever, this delegate is called on a separate thread and should be protected.
                        }
                        finally
                        {
                            cleanup();
                        }
                    },
                        null,
                        requestTimeout == System.Threading.Timeout.InfiniteTimeSpan ? System.Threading.Timeout.Infinite : (long)requestTimeout.TotalMilliseconds,
                        true);

            }
        }

        internal static void ReadAndSignalDelivery(ExpectedResponse expected, BasicDeliverEventArgs evt)
        {
            try
            {
                expected.Response = HttpResponsePacket.Deserialize(evt.Body);
            }
            catch (Exception ex)
            {
                expected.DeserializationException = ex;
            }

            //NOTE: The ManualResetEventSlim.Set() method can be called after the object has been disposed
            //So no worries about the Timeout disposing the object before the response comes in.
            expected.ReceivedEvent.Set();
        }

        private static void SetResponseResult(HttpRequestMessage request, bool timedOut, ExpectedResponse arrival, TaskCompletionSource<HttpResponseMessage> taskSource)
        {
            if (timedOut)
            {
                taskSource.SetCanceled();
            }
            else
            {
                if (arrival.DeserializationException == null)
                {
                    if (arrival.Response == null)
                    {
                        //TODO: Log this -- Critical issue (or just assert)
                    }
                }

                HttpResponseMessage msg;
                if (arrival.DeserializationException == null && TryGetHttpResponseMessage(arrival.Response, out msg))
                {
                    msg.RequestMessage = request;
                    taskSource.SetResult(msg);
                }
                else
                {
                    taskSource.SetException(RestBusClient.GetWrappedException("An error occurred while reading the response.", arrival.DeserializationException));
                }
            }
        }

        private static bool TryGetHttpResponseMessage(HttpResponsePacket packet, out HttpResponseMessage response)
        {
            try
            {
                response = new HttpResponseMessage
                {
                    Content = packet.Content == null ? RestBusClient._emptyByteArrayContent : new ByteArrayContent(packet.Content),
                    Version = packet.Version == "1.1" ? VERSION_1_1 : new Version(packet.Version),
                    ReasonPhrase = packet.StatusDescription,
                    StatusCode = (System.Net.HttpStatusCode)packet.StatusCode
                };

                packet.PopulateHeaders(response.Content.Headers, response.Headers);
            }
            catch
            {
                response = null;
                return false;
            }

            return true;
        }
    }
}
