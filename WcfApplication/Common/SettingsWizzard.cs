using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;

namespace WcfApplication.Common
{
    /// <summary>
    /// Волшебник сценария создания настроек.
    /// </summary>
    public class SettingsWizzard
    {
        private const string RemoteHosts = "RemoteHosts";

        private const string Splitter = ";";

        /// <summary>
        /// Получить настройки пользователя.
        /// </summary>
        /// <returns>Экземпляр <see cref="UserSettings"/>.</returns>
        public UserSettings GetSettings()
        {
            var appSide = QuestionManager.Choose(
                new[]
                {
                    AppSide.Client,
                    AppSide.Server
                },
                side => side.ToString(),
                "Choose application type:");

            var bindingElement = QuestionManager.Choose(
                GetBindings().ToArray(),
                element => $"[{GetBindingType(element).Name}] Name [{element.Name}]",
                "Select binding name:");

            var bindingType = GetBindingType(bindingElement);
            var binding = (Binding)Activator.CreateInstance(bindingType, bindingElement.Name);

            string serviceAddress = null;
            if (appSide == AppSide.Client)
            {
                const string NewServiceAddress = "I wanna type a new address";
                serviceAddress = NewServiceAddress;

                var recentAddresses = new [] { NewServiceAddress }.Union(ReadRecentAddresses()).ToArray();
                if (recentAddresses.Length > 1)
                {
                    serviceAddress = QuestionManager.Choose(
                        recentAddresses,
                        s => s,

                        // Выберите адрес сервера из ранее использованных:
                        "Select presented before address:",

                        // Все параметры содержатся в app.config файле
                        "Its presented in app.config");
                }

                if (serviceAddress == NewServiceAddress)
                {
                    do
                    {
                        // Введите новый адрес компьютера
                        serviceAddress = QuestionManager.Read("Please type service host address (ex: localhost):");
                        using (var ping = new Ping())
                        {
                            try
                            {
                                var pingResult = ping.Send(serviceAddress);
                                if (pingResult.Status != IPStatus.Success)
                                {
                                    serviceAddress = null;
                                    SysConsole.WriteErrorLine(
                                        $"Ping status [{pingResult.Status}]. Reply from {pingResult.Address}: bytes={pingResult.Buffer.Length} time={pingResult.RoundtripTime}ms");
                                }
                            }
                            catch (PingException exception)
                            {
                                serviceAddress = null;
                                SysConsole.WriteErrorLine(exception.ToString());
                            }
                        }
                    } while (string.IsNullOrEmpty(serviceAddress));
                    SaveRecentAddress(serviceAddress);
                }
                
            }
            else if (appSide == AppSide.Server)
            {
                serviceAddress = UserSettings.DefaultServiceListenAddress;
            }

            var serviceHost = new Uri($"{binding.Scheme}://{serviceAddress}:{UserSettings.DefaultServicePort}");

            return new UserSettings(binding, appSide, serviceHost);
        }

        private static IEnumerable<string> ReadRecentAddresses()
        {
            var appRemoteHosts = ConfigurationManager.AppSettings[RemoteHosts] ?? string.Empty;
            return appRemoteHosts.Split(
                new[]
                {
                    Splitter
                },
                StringSplitOptions.RemoveEmptyEntries);
        }

        private static void SaveRecentAddress(string address)
        {
            var appRemoteHosts = ReadRecentAddresses().Union(
                new[]
                {
                    address
                }).Distinct();

            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var remoteHostSettings = configuration.AppSettings.Settings[RemoteHosts];
            
            if (remoteHostSettings != null)
            {
                configuration.AppSettings.Settings[RemoteHosts].Value = string.Join(Splitter, appRemoteHosts);
            }
            else
            {
                configuration.AppSettings.Settings.Add(RemoteHosts, address);
            }

            configuration.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private Type GetBindingType(StandardBindingElement binding)
        {
            var type = typeof(StandardBindingElement);
            var property = type.GetProperty("BindingElementType", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = (Type)property.GetValue(binding, null);
            return value;
        }

        private static IEnumerable<StandardBindingElement> GetBindings()
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(appConfig);

            if (serviceModel == null)
            {
                yield break;
            }

            var bindings =
                serviceModel.Bindings.BasicHttpBinding.Bindings.Cast<StandardBindingElement>()
                    .Union(serviceModel.Bindings.NetTcpBinding.Bindings.Cast<StandardBindingElement>())
                    .Union(serviceModel.Bindings.WSDualHttpBinding.Bindings.Cast<StandardBindingElement>())
                    .Union(serviceModel.Bindings.WSHttpBinding.Bindings.Cast<StandardBindingElement>())
                    .Union(serviceModel.Bindings.WSFederationHttpBinding.Bindings.Cast<StandardBindingElement>())
                    .Union(serviceModel.Bindings.NetMsmqBinding.Bindings.Cast<StandardBindingElement>())
                    .Union(serviceModel.Bindings.NetNamedPipeBinding.Bindings.Cast<StandardBindingElement>());

            foreach (var binding in bindings)
            {
                yield return binding;
            }
        }
    }
}
