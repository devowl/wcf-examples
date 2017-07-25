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
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        private static void Write(ConsoleColor color, Action<string> writeDelegate, string text, int newLines = 0)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            writeDelegate(text);
            Console.ForegroundColor = currentColor;
            for (int i = 0; i < newLines; i++)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Написать с новой строки информационный текст.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void WriteInfoLine(string text, int newLines = 0)
        {
            Write(ConsoleColor.Magenta, Console.WriteLine, text, newLines);
        }

        /// <summary>
        /// Написать с новой строки текст вопроса.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void WriteQuestionLine(string text, int newLines = 0)
        {
            Write(ConsoleColor.Yellow, Console.WriteLine, text, newLines);
        }

        /// <summary>
        /// Написать текст вопроса.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void WriteQuestion(string text, int newLines = 0)
        {
            Write(ConsoleColor.Yellow, Console.Write, text, newLines);
        }

        /// <summary>
        /// Написать с новой строки текст ошибки.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void WriteErrorLine(string text, int newLines = 0)
        {
            Write(ConsoleColor.Red, Console.WriteLine, text, newLines);
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
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void WriteLine(string text, int newLines = 0)
        {
            Write(ConsoleColor.White, Console.WriteLine, text, newLines);
        }

        /// <summary>
        /// Написать с новой строки обычный текст.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="newLines">Количество новых строк после вывода текущей</param>
        public static void Write(string text, int newLines = 0)
        {
            Write(ConsoleColor.White, Console.Write, text, newLines);
        }

        /// <summary>
        /// Введите любой символ.
        /// </summary>
        public static void PressAnyKey()
        {
            Write(ConsoleColor.Yellow, Console.Write, "Press any key...");
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}