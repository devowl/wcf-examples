using System;

namespace WcfApplication.Examples.Duplex.Exceptions
{
    /// <summary>
    /// Client not founded exception.
    /// </summary>
    [Serializable]
    public class ClientNotFoundedException : Exception
    {
        /// <summary>
        /// <see cref="ClientNotFoundedException"/> Constructor.
        /// </summary>
        public ClientNotFoundedException(string clientId)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// Client Identity.
        /// </summary>
        public string ClientId { get; private set; }
    }
}