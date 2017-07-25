using System.Runtime.Serialization;

namespace WcfApplication.Examples.Duplex.Messages
{
    /// <summary>
    /// Message base.
    /// </summary>
    [KnownType(typeof(PersonalMessage))]
    [KnownType(typeof(BroadcastMessage))]
    [DataContract]
    public abstract class MessageBase
    {
        /// <summary>
        /// Message is broadcast to all clients.
        /// </summary>
        [DataMember]
        public bool IsBroadcast { get; private set; }

        /// <summary>
        /// <see cref="MessageBase"/> constructor.
        /// </summary>
        protected MessageBase(bool broadcast)
        {
            IsBroadcast = broadcast;
        }
    }
}
