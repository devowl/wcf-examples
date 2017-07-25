using System.Runtime.Serialization;

namespace WcfApplication.Examples.Duplex.Messages
{
    /// <summary>
    /// Broadcast message example.
    /// </summary>
    [DataContract]
    public class BroadcastMessage : MessageBase
    {
        /// <summary>
        /// <see cref="BroadcastMessage"/> Constructor.
        /// </summary>
        public BroadcastMessage(string data) : base(true)
        {
            Data = data;
        }

        /// <summary>
        /// Some data.
        /// </summary>
        [DataMember]
        public string Data { get; private set; }
    }
}