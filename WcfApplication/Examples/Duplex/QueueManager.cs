using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using WcfApplication.Examples.Duplex.Exceptions;
using WcfApplication.Examples.Duplex.Messages;

namespace WcfApplication.Examples.Duplex
{
    /// <summary>
    /// Clients queue manager.
    /// </summary>
    public class QueueManager : IDisposable
    {
        private readonly TimeSpan _clientTimeout;

        private const int CheckPeriod = 1000;

        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();

        private bool _isDisposed;

        /// <summary>
        /// <see cref="QueueManager"/> Constructor.
        /// </summary>
        /// <param name="clientTimeout">Time since of last <see cref="GetMessages"/> request for timeout.</param>
        public QueueManager(TimeSpan clientTimeout)
        {
            if (clientTimeout == TimeSpan.Zero)
            {
                throw new IndexOutOfRangeException(nameof(clientTimeout));
            }

            _clientTimeout = clientTimeout;
            var aliveChecker = new Task(AliveChecker, TaskCreationOptions.LongRunning);
            aliveChecker.Start();
        }

        /// <summary>
        /// Send messages to clients.
        /// </summary>
        /// <param name="clientId">Client identity.</param>
        /// <param name="messages">Messages sequence.</param>
        public bool Send(string clientId, params MessageBase[] messages)
        {
            Client client;
            if (!_clients.TryGetValue(clientId, out client))
            {
                return false;
            }

            foreach (var message in messages)
            {
                client.Push(message);
            }

            return true;
        }

        /// <summary>
        /// Send messages to all clients.
        /// </summary>
        /// <param name="messages">Messages sequence.</param>
        public bool SendToAll(params MessageBase[] messages)
        {
            foreach (var message in messages)
            {
                foreach (var pair in _clients.ToArray())
                {
                    var client = pair.Value;
                    client.Push(message);
                }
            }

            return true;
        }

        /// <summary>
        /// Add new client.
        /// </summary>
        /// <param name="clientId">Client identity.</param>
        public void AddClient(string clientId)
        {
            _clients.TryAdd(clientId, new Client(clientId));
        }
        
        /// <summary>
        /// Get client messages.
        /// </summary>
        /// <param name="clientId">Client identity.</param>
        /// <param name="sleep">Sleep timeout.</param>
        /// <returns>Message sequence or empty sequence in the case no messages.</returns>
        public IEnumerable<MessageBase> GetMessages(string clientId, TimeSpan sleep)
        {
            Client client;
            if (!_clients.TryGetValue(clientId, out client))
            {
                throw new ClientNotFoundedException(clientId);
            }

            client.WaitOne(sleep);
            client.Reset();
            return client.Pop();
        }

        /// <summary>
        /// Get client identities (for internal use, <see cref="SendToAll"/>).
        /// </summary>
        /// <returns>Client identities.</returns>
        internal IEnumerable<string> GetClients()
        {
            return _clients.ToArray().Select(client => client.Key);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _isDisposed = true;
        }

        private void AliveChecker()
        {
            do
            {
                Thread.Sleep(CheckPeriod);
                foreach (var client in _clients.ToArray())
                {
                    var timeGone = DateTime.Now - client.Value.LastPopTime;
                    if (timeGone > _clientTimeout)
                    {
                        Client tempClient;
                        _clients.TryRemove(client.Key, out tempClient);
                        Console.WriteLine($"CID {client.Key} Timeout [{timeGone}].");
                    }
                }
            } while (!_isDisposed);
        }

        private class Client
        {
            /// <summary>
            /// Messages queue.
            /// </summary>
            private readonly ConcurrentQueue<MessageBase> _messages = new ConcurrentQueue<MessageBase>();

            private readonly ManualResetEvent _event;

            /// <summary>
            /// Last messages pop time
            /// </summary>
            public DateTime LastPopTime { get; private set; }

            /// <summary>
            /// <see cref="Client"/> Constructor.
            /// </summary>
            public Client(string clientId)
            {
                ClientId = clientId;
                _event = new ManualResetEvent(false);
                LastPopTime = DateTime.Now;
            }

            /// <summary>
            /// Client Identity.
            /// </summary>
            public string ClientId { get; private set; }

            /// <summary>
            /// Set event. 
            /// </summary>
            public void WaitOne(TimeSpan timeout)
            {
                _event.WaitOne(timeout);
            }

            /// <summary>
            /// Reset event. 
            /// </summary>
            public void Reset()
            {
                _event.Reset();
            }

            /// <summary>
            /// Push message in queue.
            /// </summary>
            /// <param name="messages">Messages reference.</param>
            public void Push(params MessageBase[] messages)
            {
                foreach (var message in messages)
                {
                    _messages.Enqueue(message);
                }

                _event.Set();
            }

            /// <summary>
            /// Pop messages.
            /// </summary>
            /// <returns>Message sequence.</returns>
            public IEnumerable<MessageBase> Pop()
            {
                MessageBase message;
                while (_messages.TryDequeue(out message))
                {
                    yield return message;
                }

                LastPopTime = DateTime.Now;
            }
        }
    }
}