using System;

namespace WcfApplication.Common
{
    /// <summary>
    /// На какой стороне запускается
    /// </summary>
    public enum AppSide
    {
        /// <summary>
        /// Запустить клиента.
        /// </summary>
        Client,

        /// <summary>
        /// Запускаем сервер.
        /// </summary>
        Server
    }
}