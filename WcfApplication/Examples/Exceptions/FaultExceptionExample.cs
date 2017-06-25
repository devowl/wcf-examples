using System;
using System.ServiceModel;

using WcfApplication.Common;

namespace WcfApplication.Examples.Exceptions
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class MyService : IMyService {
        public void MethodFaultContract() {
            throw new FaultException<ArgumentNullException>(new ArgumentNullException(), "Something null");
        }

        public void MethodWrongFaultContract()
        {
            throw new FaultException<ArgumentNullException>(new ArgumentNullException(), "Something null");
        }

        public void MethodThrownFaultException() {
            throw new FaultException("New fault exception");
        }

        public string CheckClient() {
            return "You are fine.";
        }
    }

    [ServiceContract]
    interface IMyService {
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void MethodFaultContract();

        [OperationContract]
        [FaultContract(typeof(InvalidCastException))]
        void MethodWrongFaultContract();

        [OperationContract]
        void MethodThrownFaultException();

        [OperationContract]
        string CheckClient();
    }

    public class FaultExceptionExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "FaultException. Simple methods calls.";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    var client = CreateClient<IMyService>(settings);
                    SysConsole.WriteLine();

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.MethodFaultContract)}]...");
                            client.MethodFaultContract();
                        });

                    SysConsole.WriteLine();

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.MethodWrongFaultContract)}]...");
                            client.MethodWrongFaultContract();
                        });

                    SysConsole.WriteLine();

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.MethodThrownFaultException)}]...");
                            client.MethodThrownFaultException();
                        });

                    SysConsole.WriteLine();

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine("Validating client service proxy:");
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.CheckClient)}]...");
                            SysConsole.WriteLine(client.CheckClient());
                        });
                    Console.ReadKey();
                    break;
                case AppSide.Server:
                    CreateServiceHost<IMyService, MyService>(settings).Open();
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
