using System;


namespace ConsoleProject {

    public static class ConsoleTextColor {

        // окрашиваем надпись в зеленый
        public static void GreenConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // окрашиваем надпись в желтый
        public static void YellowConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // окрашиваем надпись в красный
        public static void RedConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }
}