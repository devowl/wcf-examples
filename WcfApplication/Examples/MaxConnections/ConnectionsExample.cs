using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

using WcfApplication.Common;

namespace WcfApplication.Examples.MaxConnections
{
    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        void Method(string name);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MyService : IMyService
    {
        public void Method(string name)
        {
            SysConsole.WriteLine($"Hello {name}");
            Thread.Sleep(5000);
        }
    }
    /// <summary>
    /// Демонстрация указания значения maxConnections.
    /// </summary>
    public class ConnectionsExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "TCP connections limit in NetTcpBinding [maxConnections].";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            if (settings.Binding.GetType() != typeof(NetTcpBinding))
            {
                SysConsole.WriteLine("Binding should be NetTcpBinding");
                return;
            }

            var maxConnections = 
                QuestionManager.Choose(
                    new[]
                    {
                        2,
                        3,
                        4,
                        5
                    },
                    s => s.ToString(),
                    "Choose maxConnections number", 
                    "NOTE: Please use a same value on client and server sides.");
            
            var netTcpBinding = (NetTcpBinding)settings.Binding;
            netTcpBinding.MaxConnections = maxConnections;
            SysConsole.WriteQuestionLine($"{netTcpBinding.Name}.MaxConnection = {maxConnections} now");

            switch (settings.AppSide)
            {
                case AppSide.Client:
                    
                    int clients = 15;
                    SysConsole.WriteLine();
                    SysConsole.WriteLine();
                    SysConsole.WriteLine($"Starting {clients} clients.");
                    SysConsole.WriteLine();

                    // Если нет channel клиентов, которые могу работать со службой, тогда все соединения сбрасываются и maxConnections не работает
                    var holderClient = CreateClient<IMyService>(settings);

                    for (var i = 0; i < clients; i++)
                    {
                        var client = CreateClient<IMyService>(settings);
                        int iLocal = i;
                        PeriodicExecutor(client,
                            (service, j) =>
                            {
                                SysConsole.WriteQuestionLine($"Client{iLocal} >> call[{j}]");
                                service.Method($"Client{iLocal}");
                                CloseClient(service);
                                return false;
                            }, 0);
                    }

                    Console.ReadKey();
                    break;
                case AppSide.Server:
                    CreateServiceHost<IMyService, MyService>(settings).Open();
                    SysConsole.WriteQuestionLine("Unlimited method calls with OneWay.");
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
