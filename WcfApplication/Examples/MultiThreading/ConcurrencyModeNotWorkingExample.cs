using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using WcfApplication.Common;

namespace WcfApplication.Examples.MultiThreading
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    class ConcurrencyMultipleNotWorking : Service
    { 
    }

    /// <summary>
    /// We are using service contract from <see cref="ConcurrencyModeExample"/>.
    /// </summary>
    public class ConcurrencyModeNotWorkingExample : ExampleBase
    {
        public const int ThreadSleepTimeSeconds = 5;

        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "ConcurrencyModes.Multiple + InstanceContextMode.PerCall + MultiThread_Client_Calls not working for Request-Replies";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    IContract client = CreateClient<IContract>(settings);

                    int threads = 5;
                    SysConsole.WriteQuestionLine("Single client instance created!");
                    SysConsole.WriteQuestionLine($"{threads} thread gonna calling this client.");
                    SysConsole.WriteQuestionLine($"Expected {threads} concurrent executions.", 1);

                    for (int i = 0; i < threads; i++)
                    {
                        var localId = i;
                        new Thread(
                            () =>
                            {
                                SysConsole.WriteInfoLine("[ON] Client Request-Reply method caller");
                                while (true)
                                {
                                    client.Method($"User{localId + 1}");
                                }
                            }).Start();
                    }

                    Console.ReadKey();
                    break;
                case AppSide.Server:

                    var host = CreateServiceHost<IContract, ConcurrencyMultiple>(settings);
                    host.Open();

                    SysConsole.WriteInfoLine($"Each one contract method execute at least {ThreadSleepTimeSeconds} SECONDS");
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
