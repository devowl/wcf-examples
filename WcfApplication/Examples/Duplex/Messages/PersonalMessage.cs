using System.Runtime.Serialization;

namespace WcfApplication.Examples.Duplex.Messages
{
    /// <summary>
    /// Personal message example.
    /// </summary>
    [DataContract]
    public class PersonalMessage : MessageBase
    {
        /// <summary>
        /// <see cref="PersonalMessage"/> Constructor.
        /// </summary>
        public PersonalMessage(string data) : base(false)
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