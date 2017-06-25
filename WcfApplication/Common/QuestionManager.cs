using System;
using System.Collections.Generic;
using System.Linq;

namespace WcfApplication.Common
{
    /// <summary>
    /// Управленец вопросами.
    /// </summary>
    public static class QuestionManager
    {
        /// <summary>
        /// Считать ответ пользователя на вопрос.
        /// </summary>
        /// <param name="question">Текст вопроса.</param>
        /// <returns>Введённое значение.</returns>
        public static string Read(string question)
        {
            string buffer = null;
            do
            {
                SysConsole.WriteQuestionLine(question);
                buffer = (Console.ReadLine() ?? string.Empty).Trim();
            } while (string.IsNullOrEmpty(buffer));

            return buffer;
        }

        /// <summary>
        /// Выбор одного варианта из списка.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="header">Header для вопроса.</param>
        /// <param name="results">Список вариантов.</param>
        /// <param name="footer">Footer для вопроса.</param>
        /// <param name="getName">Получения имени для вывода</param>
        /// <returns></returns>
        public static TResult Choose<TResult>(
            IEnumerable<TResult> results,
            Func<TResult, string> getName,
            string header = null,
            string footer = null)
        {
            var arrayResult = results.ToArray();
            if (!arrayResult.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(results));
            }

            int selectedIndex;
            do
            {
                if (!string.IsNullOrEmpty(header))
                {
                    SysConsole.WriteQuestionLine(header);
                }

                for (int i = 0; i < arrayResult.Length; i++)
                {
                    SysConsole.WriteLine($"  {i + 1}) {getName(arrayResult[i])}");
                }

                if (!string.IsNullOrEmpty(footer))
                {
                    SysConsole.WriteQuestionLine(footer);
                }

                SysConsole.Write($"Number between 1 and {arrayResult.Length}: ");
                if (arrayResult.Length < 10)
                {
                    selectedIndex = (int)char.GetNumericValue(Console.ReadKey().KeyChar) - 1;
                    Console.WriteLine();
                }
                else
                {
                    
                    var input = Console.ReadLine();
                    int.TryParse(input, out selectedIndex);
                    selectedIndex -= 1;
                }

                Console.WriteLine();
            }
            while (selectedIndex < 0 || selectedIndex >= arrayResult.Length);
            
            return arrayResult[selectedIndex];
        }

        /// <summary>
        /// Вывод текста ожидания клиента.
        /// </summary>
        public static void AwaitingClientConnections()
        {
            SysConsole.WriteLine();
            SysConsole.WriteQuestion("Awaiting client connections...");
            Console.ReadKey();
        }
    }
}