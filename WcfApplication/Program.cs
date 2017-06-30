using WcfApplication.Common;
using WcfApplication.Examples.Exceptions;
using WcfApplication.Examples.MaxConnections;
using WcfApplication.Examples.MultiThreading;
using WcfApplication.Examples.Operations;
using WcfApplication.Examples.Sessions;

namespace WcfApplication
{
    /// <summary>
    /// Класс содержащий точку входа в программу.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Точка входа в программу.
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        private static void Main(string[] args)
        {
            var example = GetExample();
            var wizzard = new SettingsWizzard();
            var settings = wizzard.GetSettings();

            example.Execute(settings);
        }

        private static ExampleBase GetExample()
        {
            var examples = new ExampleBase[]
            {
                new SessionExample(), 
                new OperationsCallSequenceExample(),
                new ConnectionsExample(), 
                new ReliableSessionExceptionExample(),
                new FaultExceptionExample(),
                new ErrorHandlerExample(),
                new ConcurrencyModeExample(), 
                new ConcurrencyModeNotWorkingExample()
            };

            return QuestionManager.Choose(examples, e => e.Name, "List of samples:");

        }
    }
}