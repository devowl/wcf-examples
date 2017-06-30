using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace WcfApplication.Common
{
    /// <summary>
    /// Базовый класс для всех примеров.
    /// </summary>
    public abstract class ExampleBase
    {
        /// <summary>
        /// Создание экземпляра класса <see cref="ExampleBase"/>.
        /// </summary>
        protected ExampleBase()
        {
        }

        /// <summary>
        /// Название примера.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Запустить пример.
        /// </summary>
        public abstract void Execute(UserSettings settings);

        /// <summary>
        /// Безопасный вызов действия.
        /// </summary>
        /// <typeparam name="TResult">Тип результата.</typeparam>
        /// <param name="action">Действие с вызовом.</param>
        /// <param name="result">Результат вызова.</param>
        /// <returns>Удачный или нет вызов.</returns>
        protected bool TrySafeCall<TResult>(Func<TResult> action, out TResult result)
        {
            result = default(TResult);
            try
            {
                result = action();
                return true;
            }
            catch (Exception exception)
            {
                PrintError(exception);
                return false;
            }
        }

        /// <summary>
        /// Безопасный вызов действия.
        /// </summary>
        /// <param name="action">Действие с вызовом.</param>
        /// <returns>Удачный или нет вызов.</returns>
        protected bool TrySafeCall(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception exception)
            {
                PrintError(exception);
                return false;
            }
        }

        /// <summary>
        /// Безопасный вызов действия.
        /// </summary>
        /// <typeparam name="TResult">Тип результата.</typeparam>
        /// <param name="action">Действие с вызовом.</param>
        /// <returns>Удачный или нет вызов.</returns>
        protected TResult Call<TResult>(Func<TResult> action)
        {
            try
            {
                return action();
            }
            catch (Exception exception)
            {
                PrintError(exception);
                throw;
            }
        }

        /// <summary>
        /// Безопасный вызов действия.
        /// </summary>
        /// <param name="action">Действие с вызовом.</param>
        /// <returns>Удачный или нет вызов.</returns>
        protected void Call(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                PrintError(exception);
                throw;
            }
        }

        /// <summary>
        /// Создать экземпляр <see cref="ServiceHost"/>.
        /// </summary>
        /// <typeparam name="TContract">Тип контракта.</typeparam>
        /// <typeparam name="TService">Тип сервиса.</typeparam>
        /// <param name="settings">Пользовательские настройки.</param>
        /// <returns>Экземпляр <see cref="ServiceHost"/>.</returns>
        protected ServiceHost CreateServiceHost<TContract, TService>(UserSettings settings)
            where TService : TContract, new()
        {
            var serviceHost = new ServiceHost(typeof(TService));
            serviceHost.AddServiceEndpoint(typeof(TContract), settings.Binding, settings.ServiceUrl);
            return serviceHost;
        }

        /// <summary>
        /// Создать клиента для службы.
        /// </summary>
        /// <typeparam name="TContract">Тип контракта.</typeparam>
        /// <param name="settings">Пользовательские настройки.</param>
        /// <returns>Экземпляр клиента.</returns>
        protected TContract CreateClient<TContract>(UserSettings settings)
        {
            return ChannelFactory<TContract>.CreateChannel(settings.Binding, new EndpointAddress(settings.ServiceUrl));
        }

        /// <summary>
        /// Создать клиента для службы.
        /// </summary>
        /// <typeparam name="TContract">Тип контракта.</typeparam>
        /// <typeparam name="TCallback">Тип экземпляра обратного вызова.</typeparam>
        /// <param name="settings">Пользовательские настройки.</param>
        /// <param name="callback">Ссылка на экземпляр обратного вызова.</param>
        /// <returns>Экземпляр клиента.</returns>
        protected TContract CreateDuplexClient<TContract, TCallback>(UserSettings settings, out TCallback callback) 
            where TCallback : class, new()
        {
            callback = new TCallback();
            return DuplexChannelFactory<TContract>.CreateChannel(new InstanceContext(callback),
                settings.Binding,
                new EndpointAddress(settings.ServiceUrl));

        }

        /// <summary>
        /// Закрыть соединение с клиентом.
        /// </summary>
        /// <param name="client">Ссылка на клиента.</param>
        protected void CloseClient(object client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var communicationObject = client as ICommunicationObject;
            if (communicationObject != null)
            {
                // https://docs.microsoft.com/en-us/dotnet/framework/wcf/samples/avoiding-problems-with-the-using-statement
                try
                {
                    communicationObject.Close();
                }
                catch (CommunicationException)
                {
                    communicationObject.Abort();
                }
                catch (TimeoutException)
                {
                    communicationObject.Abort();
                }
                catch (Exception)
                {
                    communicationObject.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Периодический вызов метода контракта.
        /// </summary>
        /// <typeparam name="TContract">Тип контракта.</typeparam>
        /// <param name="service">Экземпляр для работы с сервисом.</param>
        /// <param name="execute">При возникновении события о вызове.</param>
        /// <param name="period">Период вызова.</param>
        protected void PeriodicExecutor<TContract>(TContract service, Func<TContract, int, bool> execute, int period = 3000)
        {
            Task.Factory.StartNew(
                () =>
                {
                    int call = 0;
                    while (true)
                    {
                        try
                        {
                            if (!execute(service, ++call))
                            {
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            SysConsole.WriteErrorLine(ex.ToString());
                        }

                        Thread.Sleep(period);
                    }
                });
        }

        private static void PrintError(Exception errorObject)
        {
            var errorType = errorObject.GetType();
            var type = errorType.IsGenericType
                ? $"{errorType.Name}<{string.Join(",", errorType.GetGenericArguments().Select(arg => arg.Name))}>"
                : errorType.Name;

            SysConsole.WriteErrorLine($"[{type}] {errorObject.Message}");
        }
    }
}