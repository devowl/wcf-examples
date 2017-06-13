using System.ServiceModel.Channels;

namespace WcfApplication.Common
{
    /// <summary>
    /// Интерфейс предоставляющий информации о привязке.
    /// </summary>
    public interface IEndpointProvider
    {
        /// <summary>
        /// Значение привязки по умолчанию.
        /// </summary>
        Binding DefaultBinding { get; }

        /// <summary>
        /// Значение адреса сервиса по умолчанию.
        /// </summary>
        Binding DefaultServiceAddress { get; } 
    }
}