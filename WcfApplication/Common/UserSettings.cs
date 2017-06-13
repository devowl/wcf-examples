using System;
using System.ServiceModel.Channels;

namespace WcfApplication.Common
{
    /// <summary>
    /// Настройки пользователя.
    /// </summary>
    public sealed class UserSettings
    {
        /// <summary>
        /// Значение адреса для прослушивания по умолчанию.
        /// </summary>
        public const string DefaultServiceListenAddress = "0.0.0.0";

        /// <summary>
        /// Значение порта по умолчанию.
        /// </summary>
        public const int DefaultServicePort = 5099;

        /// <summary>
        /// Создание экземпляра класса <see cref="UserSettings"/>.
        /// </summary>
        public UserSettings(Binding binding, AppSide appSide, Uri serviceUrl)
        {
            Binding = binding;
            AppSide = appSide;
            ServiceUrl = serviceUrl;
        }

        /// <summary>
        /// Значение <see cref="Binding"/> для сервиса.
        /// </summary>
        public Binding Binding { get; private set; }

        /// <summary>
        /// Тип приложения.
        /// </summary>
        public AppSide AppSide { get; private set; }

        /// <summary>
        /// URL сервиса для клиента.
        /// </summary>
        public Uri ServiceUrl { get; private set; }
    }
}