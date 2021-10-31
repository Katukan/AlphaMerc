using System;
using System.IO;

namespace ConsoleProject {
    public static class ConsoleSettings {
        public static void ColoringInGreen(string str) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringInYellow(string text) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringInRed(string text) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ColoringCurrentTextToYellow() {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        public static void ColoringCurrentTextToGray() {
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ShowLogo() {
            using(StreamReader stringReader = new StreamReader("logo.txt")) {
                Console.WriteLine(stringReader.ReadToEnd());
            }
        }

        public static void ShowExceptionHandling(Exception exception) {
            bool isInternetError = exception.Message.Contains("ERR_INTERNET");
            if(isInternetError) {
                ColoringInRed("Проблема с интернетом");
            }else {
                ColoringInRed(exception.Message);
            }
        }
    }
}