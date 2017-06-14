using System;

namespace WcfApplication.Common
{
    /// <summary>
    /// Расширения для <see cref="Console"/>.
    /// </summary>
    public static class SysConsole
    {
        /// <summary>
        /// Написать с новой строки текст определённого цвета.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <param name="writeDelegate">Используемая функция для вывода.</param>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        private static void Write(ConsoleColor color, Action<string, object[]> writeDelegate, string text, params object[] arguments)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            writeDelegate(text, arguments);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// Написать с новой строки текст вопроса.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        public static void WriteQuestionLine(string text, params object[] arguments)
        {
            Write(ConsoleColor.Yellow, Console.WriteLine, text, arguments);
        }

        /// <summary>
        /// Написать текст вопроса.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        public static void WriteQuestion(string text, params object[] arguments)
        {
            Write(ConsoleColor.Yellow, Console.Write, text, arguments);
        }

        /// <summary>
        /// Написать с новой строки текст ошибки.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        public static void WriteErrorLine(string text, params object[] arguments)
        {
            Write(ConsoleColor.Red, Console.WriteLine, text, arguments);
        }

        /// <summary>
        /// Написать с новой строки обычный текст.
        /// </summary>
        public static void WriteLine()
        {
            Write(ConsoleColor.White, Console.WriteLine, string.Empty);
        }

        /// <summary>
        /// Написать с новой строки обычный текст.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        public static void WriteLine(string text = null, params object[] arguments)
        {
            Write(ConsoleColor.White, Console.WriteLine, text, arguments);
        }

        /// <summary>
        /// Написать с новой строки обычный текст.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="arguments">Аргументы для формата.</param>
        public static void Write(string text, params object[] arguments)
        {
            Write(ConsoleColor.White, Console.Write, text, arguments);
        }

        /// <summary>
        /// Введите любой символ.
        /// </summary>
        public static void PressAnyKey()
        {
            Write(ConsoleColor.Yellow, Console.Write, "Press any key...");
            Console.ReadKey();
        }
    }
}