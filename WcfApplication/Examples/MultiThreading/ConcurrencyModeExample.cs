using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using WcfApplication.Common;

namespace WcfApplication.Examples.MultiThreading
{
    [ServiceContract]
    interface IContract
    {
        [OperationContract]
        void Method(string name);

        [OperationContract(IsOneWay = true)]
        void MethodOneWay(string name);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single)]
    class ConcurrencySingle : Service
    {
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ConcurrencyMultiple : Service
    {
    }
    
    class Service : IContract
    {
        private static DateTime _lastCall = DateTime.Now;

        public void Method(string name)
        {
            var dNow = DateTime.Now;
            Console.WriteLine($"[Server] [{name}] [{GetHashCode()}] LCall[{(int)(dNow - _lastCall).TotalSeconds}s] TID[{Thread.CurrentThread.ManagedThreadId,2}] [{dNow.ToString("HH:mm:ss")}]");
            _lastCall = DateTime.Now;
            Thread.Sleep(TimeSpan.FromSeconds(ConcurrencyModeExample.ThreadSleepTimeSeconds));
        }

        public void MethodOneWay(string name)
        {
            Method(name);
        }
    }

    public class ConcurrencyModeExample : ExampleBase
    {
        public const int ThreadSleepTimeSeconds = 5;

        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "ConcurrencyModes demonstrations (Works only when one client proxy in use)";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    IContract client = CreateClient<IContract>(settings);

                    SysConsole.WriteQuestionLine("Single client instance created!");
                    SysConsole.WriteLine();

                    new Thread(() =>
                    {
                        SysConsole.WriteInfoLine("[ON] Client request-reply method caller");
                        while (true)
                        {
                            client.Method("User1");
                        }
                    }).Start();

                    new Thread(() =>
                    {
                        SysConsole.WriteInfoLine("[ON] Client OneWay method caller");
                        while (true)
                        {
                            client.MethodOneWay("User2");
                            Thread.Sleep(1000);
                        }
                    }).Start();

                    Console.ReadKey();
                    break;
                case AppSide.Server:

                    var concurrencyMode = QuestionManager.Choose(
                        new[]
                        {
                            ConcurrencyMode.Single,
                            ConcurrencyMode.Multiple
                        },
                        s => s.ToString(),
                        "Choose service ConcurrencyMode demonstration:");


                    var host = concurrencyMode == ConcurrencyMode.Multiple
                        ? CreateServiceHost<IContract, ConcurrencyMultiple>(settings)
                        : CreateServiceHost<IContract, ConcurrencySingle>(settings);

                    host.Open();
                    SysConsole.WriteInfoLine($"Each one contract method executing at least {ThreadSleepTimeSeconds} SECONDS");
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
