using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using WcfApplication.Common;
using WcfApplication.Examples.Duplex.Exceptions;
using WcfApplication.Examples.Duplex.Messages;

namespace WcfApplication.Examples.Duplex
{
    /// <summary>
    /// Service contract.
    /// </summary>
    [ServiceContract]
    public interface IMyService
    {
        /// <summary>
        /// Client method.
        /// </summary>
        [OperationContract]
        string Login(string login, string password);

        /// <summary>
        /// Get callback message call.
        /// </summary>
        [OperationContract]
        IEnumerable<MessageBase> GetMessages(string clientId, TimeSpan timeout);
    }

    /// <summary>
    /// Service instance.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MyService : IMyService
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Callback manager instance.
        /// </summary>
        public static QueueManager Manager { get; private set; }

        static MyService()
        {
            Manager = new QueueManager(TimeSpan.FromSeconds(30));
            var emulator = new TestMessageEmulator(Manager);
            emulator.Start();
        }

        /// <inheritdoc/>
        public string Login(string login, string password)
        {
            // Validating login & password
            var clientId = Random.Next(1000000, 9999999).ToString();
            Manager.AddClient(clientId);
            return clientId;
        }

        /// <inheritdoc/>
        [FaultContract(typeof(ClientNotFoundedException))]
        public IEnumerable<MessageBase> GetMessages(string clientId, TimeSpan timeout)
        {
            return Manager.GetMessages(clientId, timeout);
        }
    }

    /// <summary>
    /// BasicHttpBinding Callback example.
    /// </summary>
    public class BasicHttpCallbackExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "BasicHttpBinding callback emulation.";
            }
        }

        public override void Execute(UserSettings settings)
        {
            if(!(settings.Binding is BasicHttpBinding))
            {
                SysConsole.WriteErrorLine($"Supports {nameof(BasicHttpBinding)} only.");
                return;
            }

            switch (settings.AppSide)
            {
                case AppSide.Client:
                    var clients = QuestionManager.Choose(
                        new[]
                        {
                            1,
                            2,
                            3
                        },
                        (o) => o.ToString(),
                        "Choose clients count");

                    // Creates callback processors
                    for (int i = 0; i < clients; i++)
                    {
                        // proxy client
                        var client = CreateClient<IMyService>(settings);

                        // client thread
                        Task.Factory.StartNew(InitClient, client);
                    }

                    SysConsole.PressAnyKey();
                    SysConsole.WriteLine(null, 2);
                    break;
                case AppSide.Server:
                    CreateServiceHost<IMyService, MyService>(settings).Open();
                    QuestionManager.AwaitingClientConnections();
                    break;
            }
        }

        private void InitClient(object clientObj)
        {
            // Each one client processing service messages on call GetMessages()
            var client = clientObj as IMyService;

            // 1. Login method call.
            var clientId = client.Login(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            try
            {
                do
                {
                    // 2. Infinity GetMessage requests
                    var callbackMessages = client.GetMessages(clientId, TimeSpan.FromSeconds(10));

                    // 3. Processing messages
                    foreach (var callbackMessage in callbackMessages)
                    {
                        var broadcast = callbackMessage as BroadcastMessage;
                        if (broadcast != null)
                        {
                            ProcessBroadcast(clientId, broadcast);
                            continue;
                        }

                        var personal = callbackMessage as PersonalMessage;
                        if (personal != null)
                        {
                            ProcessPersonal(clientId, personal);
                            continue;
                        }

                        SysConsole.WriteQuestionLine($"Unknown message {callbackMessage.GetType()}");
                    }
                } while (true);
            }
            catch (ClientNotFoundedException ex)
            {
                SysConsole.WriteErrorLine($"Client disconnected {ex.ClientId}. Re-login required...");
            }
        }
        
        private void ProcessPersonal(string clientId, PersonalMessage personal)
        {
            SysConsole.WriteInfoLine($"CID [{clientId}] Data [{personal.Data}]");
        }

        private void ProcessBroadcast(string clientId, BroadcastMessage broadcast)
        {
            SysConsole.WriteInfoLine($"CID [{clientId}] Data [{broadcast.Data}]");
        }
    }
}
