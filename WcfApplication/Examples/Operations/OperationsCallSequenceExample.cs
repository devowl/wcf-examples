using System;
using System.ServiceModel;

using WcfApplication.Common;

namespace WcfApplication.Examples.Operations
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IMyService
    {
        [OperationContract]
        void Login();

        [OperationContract(IsInitiating = false)]
        void SayHello();

        [OperationContract(IsTerminating = true)]
        void Logout();
    }

    public class MyService : IMyService
    {
        private bool _isLoggedIn;

        /// <inheritdoc/>
        public void Login()
        {
            SysConsole.WriteLine(_isLoggedIn ? "You are already logged in." : "You are welcome!");
            _isLoggedIn = true;
        }

        /// <inheritdoc/>
        public void SayHello()
        {
            SysConsole.WriteLine($"Hello world! The time is {DateTime.Now.ToShortTimeString()}");
        }

        /// <inheritdoc/>
        public void Logout()
        {
            SysConsole.WriteLine("Goodbye my friend");
        }
    }
    
    public class OperationsCallSequenceExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "Required operations sequence calls";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    SysConsole.WriteLine();
                    SysConsole.WriteQuestionLine("A correct flow when we logged in -> said hello world -> logged out.", 2);
                    
                    while (true)
                    {
                        var client = CreateClient<IMyService>(settings);
                        try
                        {
                            while (true)
                            {
                                var methodName = QuestionManager.Choose(
                                    new[]
                                    {
                                        nameof(IMyService.Login),
                                        nameof(IMyService.SayHello),
                                        nameof(IMyService.Logout),
                                    },
                                    s => s.ToString(),
                                    "Choose contract action call");
                                switch (methodName)
                                {
                                    case nameof(IMyService.Login):
                                        client.Login();
                                        break;
                                    case nameof(IMyService.Logout):
                                        client.Logout();
                                        break;
                                    case nameof(IMyService.SayHello):
                                        client.SayHello();
                                        break;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            SysConsole.WriteErrorLine(exception.Message);
                            var answer = QuestionManager.Choose(
                                    new[]
                                    {
                                        "Yes",
                                        "No"
                                    },
                                    s => s.ToString(),
                                    "Create new connection?");

                            if (answer == "No")
                            {
                                break;
                            }

                        }
                    }
                    break;
                case AppSide.Server:
                    var service = CreateServiceHost<IMyService, MyService>(settings);
                    service.Open();
                    SysConsole.WriteQuestion("Service is working.");
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
