using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;

using WcfApplication.Common;

namespace WcfApplication.Examples.Exceptions
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service : IService
    {
        public void MethodThrowingException()
        {
            throw new AccessViolationException("AccessViolationException from server IsOneWay = true");
        }

        public void OneWayMethodThrowingException()
        {
            throw new AddressAlreadyInUseException("AddressAlreadyInUseException from server");
        }

        public void Method()
        {
            SysConsole.WriteLine($"Hello [{OperationContext.Current?.SessionId}]");
        }
    }

    [ServiceContract]
    
    interface IService
    {
        [OperationContract(IsOneWay = true)]
        void MethodThrowingException();

        [OperationContract]
        void OneWayMethodThrowingException();

        [OperationContract]
        void Method();
    }

    public class ReliableSessionExceptionExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "netTcpBinding/wsHttpBinding/basicHttpBinding channel exceptions with ReliableSession [True/False]";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            var correctBindings = new[]
            {
                typeof(NetTcpBinding),
                typeof(WSHttpBinding),
                typeof(BasicHttpBinding)
            };

            var bindingType = settings.Binding.GetType();
            if (!correctBindings.Contains(bindingType))
            {
                SysConsole.WriteLine($"Binding type should be {string.Join(" or ", correctBindings.Select(binding => binding.Name))}");
                return;
            }
            
            var reliableSessionFlag = QuestionManager.Choose(
                        new[]
                        {
                            true,
                            false
                        },
                        s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToString()),
                        "Choose reliableSession mode");

            SysConsole.WriteLine();
            SysConsole.WriteQuestionLine($"ReliableSession mode [{reliableSessionFlag.ToString().ToUpper()}] in use now!", 3);

            if (bindingType == typeof(NetTcpBinding))
            {
                var netTcp = ((NetTcpBinding)settings.Binding);
                netTcp.ReliableSession.Enabled = reliableSessionFlag;
                netTcp.Security.Mode = SecurityMode.None;
            } 
            else if (bindingType == typeof(WSHttpBinding))
            {
                var wsDual = ((WSHttpBinding)settings.Binding);
                wsDual.ReliableSession.Enabled = reliableSessionFlag;
                wsDual.Security.Mode = SecurityMode.None;
            }
            else if(bindingType == typeof(BasicHttpBinding))
            {
                SysConsole.WriteLine($"[{settings.Binding.Name}] [{bindingType.Name}] not supports ReliableSession");
            }
            else
            {
                throw new NotSupportedException(bindingType.FullName);
            }

            switch (settings.AppSide)
            {
                case AppSide.Client:
                    var client = CreateClient<IService>(settings);
                    SysConsole.WriteQuestion("Single proxy channel client instance!", 1);

                    int maxCatchs = 3;
                    while (maxCatchs > 0)
                    {
                        try
                        {
                            SysConsole.WriteLine($"Call >> {nameof(IService.Method)}");
                            client.Method();
                            Thread.Sleep(2000);
                        }
                        catch (Exception exception)
                        {
                            SysConsole.WriteErrorLine(exception.Message);
                            Thread.Sleep(2000);
                        }

                        try
                        {
                            SysConsole.WriteLine($"Call >> {nameof(IService.OneWayMethodThrowingException)}");
                            client.OneWayMethodThrowingException();
                            Thread.Sleep(2000);
                        }
                        catch (Exception exception)
                        {
                            SysConsole.WriteErrorLine(exception.Message);
                            Thread.Sleep(2000);
                        }

                        try
                        {
                            SysConsole.WriteLine($"Call >> {nameof(IService.MethodThrowingException)}");
                            client.MethodThrowingException();
                            Thread.Sleep(2000);
                        }
                        catch (Exception exception)
                        {
                            SysConsole.WriteErrorLine(exception.Message);
                            Thread.Sleep(2000);
                        }
                        maxCatchs --;
                    }

                    Console.ReadKey();
                    break;
                case AppSide.Server:
                    CreateServiceHost<IService, Service>(settings).Open();
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
