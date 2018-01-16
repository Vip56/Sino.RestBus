using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sino.RestBus.RabbitMQ.Subscription
{
    /// <summary>
    /// Encapsulates a RabbitMQ message and its associated consumer.
    /// </summary>
    internal class MessageDispatch
    {
        public IBasicConsumer Consumer;
        public BasicDeliverEventArgs Delivery;
    }
}
