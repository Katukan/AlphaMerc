using System;

namespace ConsoleProject {
    public static class ConsoleTextColor {
        public static void ColoringInGreen(string str) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringInYellow(string str) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringInRed(string str) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringGrayToYellow() {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        public static void ColoringYellowToGray() {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}