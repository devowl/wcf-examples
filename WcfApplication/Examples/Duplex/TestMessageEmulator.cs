using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WcfApplication.Examples.Duplex.Messages;

namespace WcfApplication.Examples.Duplex
{
    /// <summary>
    /// Emu
    /// </summary>
    public class TestMessageEmulator
    {
        private readonly QueueManager _manager;

        private readonly int _threads;

        /// <summary>
        /// <see cref="TestMessageEmulator"/> Constructor.
        /// </summary>
        public TestMessageEmulator(QueueManager manager, int threads = 1)
        {
            _manager = manager;
            _threads = threads;
        }

        /// <summary>
        /// Begin emulation.
        /// </summary>
        public void Start()
        {
            for (int i = 0; i < _threads; i ++)
            {
                Task.Factory.StartNew(EmulateCalls);
            }
        }

        private void EmulateCalls()
        {
            var randomizer = new Random();
            do
            {
                var randomValue = randomizer.Next(1, 5000);

                if (randomValue % 2 == 0)
                {
                    var broadcast = new BroadcastMessage($"{nameof(BroadcastMessage)} message at [{DateTime.Now.ToString("HH:mm:ss")}]");
                    _manager.SendToAll(broadcast);
                }
                else
                {
                    var personal = new PersonalMessage($"{nameof(PersonalMessage)} message at [{DateTime.Now.ToString("HH:mm:ss")}]");
                    var clients = _manager.GetClients().ToArray();
                    if (clients.Any())
                    {
                        var clientRandom = new Random();
                        var clientId = clients[clientRandom.Next(0, clients.Length)];
                        _manager.Send(clientId, personal);
                    }
                }

                int timeout = 5000 + randomValue;
                Thread.Sleep(timeout);
            } while (true);
        }
    }
}
