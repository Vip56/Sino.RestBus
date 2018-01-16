using System;

namespace Sino.RestBus.Common.Amqp
{
    /// <summary>
    /// AMQP连接信息
    /// </summary>
    public class AmqpConnectionInfo
    {
        /// <summary>
        /// AMQP连接地址
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 连接的友好名称
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
