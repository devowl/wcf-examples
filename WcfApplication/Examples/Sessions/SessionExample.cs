using System;
using System.ServiceModel;

using WcfApplication.Common;

namespace WcfApplication.Examples.Sessions
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IMyServiceAllowed : IMyService
    {
    }

    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IMyServiceNotAllowed : IMyService
    {
    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IMyServiceRequired : IMyService
    {
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        void Hello();
    }

    public class MyService : IMyService, IDisposable
    {
        private int _call;

        /// <inheritdoc/>
        public void Hello()
        {
            SysConsole.WriteLine("SessionId [{0}] Call [{1}]", OperationContext.Current?.SessionId, ++_call);
        }

        public void Dispose()
        {
            SysConsole.WriteLine("Goodbye [{0}]", OperationContext.Current?.SessionId);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerCallMyService : MyService
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PerSessionMyService : MyService
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SingleMyService : MyService
    {
    }

    public class SessionExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "Service sessions demonstration";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    var sessionMode = QuestionManager.Choose(
                        new[]
                        {
                            SessionMode.Allowed,
                            SessionMode.NotAllowed,
                            SessionMode.Required
                        },
                        s => s.ToString(),
                        "Choose client working SessionMode:");

                    var clientsCount = QuestionManager.Choose(
                        new[]
                        {
                            1, 2, 3
                        },
                        s => s.ToString(),
                        "How many client do we need:");

                    for (int i = 0; i < clientsCount; i ++)
                    {
                        var client = CreateClient(settings, sessionMode);
                        PeriodicExecutor(
                            client,
                            (c, j) =>
                            {
                                c.Hello();
                                if (j == 10)
                                {
                                    CloseClient(c);
                                    SysConsole.WriteQuestion("Execution completed!");
                                    return false;
                                }

                                return true;
                            });
                    }

                    Console.ReadKey();
                    break;
                case AppSide.Server:
                    var contextMode = QuestionManager.Choose(
                        new[]
                        {
                            InstanceContextMode.PerCall,
                            InstanceContextMode.PerSession,
                            InstanceContextMode.Single
                        },
                        s => s.ToString(),
                        "Choose service InstanceContextMode:");

                    var service = CreateService(settings, contextMode);
                    service.Open();
                    SysConsole.WriteQuestion("Service is working.");
                    SysConsole.WriteLine();
                    SysConsole.WriteQuestion("Awaiting client connections...");
                    SysConsole.WriteLine();
                    SysConsole.WriteLine();
                    Console.ReadKey();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ServiceHost CreateService(UserSettings settings, InstanceContextMode contextMode)
        {
            switch (contextMode)
            {
                case InstanceContextMode.PerSession:
                    return CreateServiceHost<IMyService, PerSessionMyService>(settings);
                case InstanceContextMode.PerCall:
                    return CreateServiceHost<IMyService, PerCallMyService>(settings);
                case InstanceContextMode.Single:
                    return CreateServiceHost<IMyService, SingleMyService>(settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(contextMode), contextMode, null);
            }
        }

        private IMyService CreateClient(UserSettings settings, SessionMode sessionMode)
        {
            switch (sessionMode)
            {
                case SessionMode.Allowed:
                    return CreateClient<IMyServiceAllowed>(settings);
                case SessionMode.Required:
                    return CreateClient<IMyServiceRequired>(settings);
                case SessionMode.NotAllowed:
                    return CreateClient<IMyServiceNotAllowed>(settings);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}