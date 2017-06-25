using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

using WcfApplication.Common;

namespace WcfApplication.Examples.Exceptions
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class MyErrorService : IErrorService
    {
        public void MethodThrowingException()
        {
            throw new ArgumentNullException("ArgumentNullException from server");
        }

        public void MethodThrownFaultException()
        {
            throw new FaultException("New fault exception");
        }

        public string CheckClient()
        {
            return "You are fine.";
        }
    }

    [ServiceContract]
    interface IErrorService
    {
        [OperationContract]
        void MethodThrowingException();
        
        [OperationContract]
        string CheckClient();
    }

    /// <summary>
    /// Обработчик ошибок.
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        bool IErrorHandler.HandleError(Exception error)
        {
            return true;
        }

        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message message)
        {
            if (error is FaultException)
            {
                return;
            }

            var innerException = error;
            while (innerException.InnerException != null)
            {
                innerException = innerException.InnerException;
            }
            
            var wrappedException =
                (FaultException)
                    Activator.CreateInstance(
                        typeof(FaultException),
                        innerException.Message);

            message = Message.CreateMessage(version, wrappedException.CreateMessageFault(), wrappedException.Action);
        }
    }

    /// <summary>
    /// Класс для регистрации обработчика ошибок сервиса.
    /// </summary>
    public class ErrorBehavior : IEndpointBehavior
    {
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new ErrorHandler());
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class ErrorHandlerExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                return "IErrorHandler exceptions wrapper.";
            }
        }

        /// <inheritdoc/>
        public override void Execute(UserSettings settings)
        {
            switch (settings.AppSide)
            {
                case AppSide.Client:
                    var client = CreateClient<IErrorService>(settings);
                    SysConsole.WriteLine();

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.MethodThrowingException)}]...");
                            client.MethodThrowingException();
                        });

                    TrySafeCall(
                        () =>
                        {
                            SysConsole.WriteQuestionLine($"Calling >> [{nameof(client.CheckClient)}]...");
                            SysConsole.WriteLine(client.CheckClient());
                        });

                    Console.ReadKey();
                    break;
                case AppSide.Server:
                    var serviceHost = new ServiceHost(typeof(MyErrorService));
                    var servicePoint = serviceHost.AddServiceEndpoint(typeof(IErrorService), settings.Binding, settings.ServiceUrl);
                    servicePoint.Behaviors.Add(new ErrorBehavior());
                    serviceHost.Open();
                    QuestionManager.AwaitingClientConnections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
