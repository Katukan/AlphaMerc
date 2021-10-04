using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;


namespace ConsoleProject {
    public class Program {
        static IRepository repository;
        static BotForMercury botForMercury;
        static IWebDriver chromeDriver;

        static Program() {
            repository = new Repository();
            chromeDriver = new ChromeDriver(Directory.GetCurrentDirectory());
            botForMercury = BotForMercury.RenderBotForMercury(repository, chromeDriver);
        }

        static void Main(string[] args) {
            try {
                DatabaseMigrate();
                ShowLogo();
                botForMercury.Start();
            }
            catch(Exception exception) {
                ExceptionHandling(exception);
            }
            finally {
                botForMercury.Stop();
            }
        }

        private static void DatabaseMigrate() {
            using(var db = new DatabaseContext()){
                db.Database.Migrate();
            }
        }

        private static void ShowLogo() {
            Console.Write(@"
                          _       _           __  __               
                    /\   | |     | |         |  \/  |              
                   /  \  | |_ __ | |__   __ _| \  / | ___ _ __ ___ 
                  / /\ \ | | '_ \| '_ \ / _` | |\/| |/ _ \ '__/ __|
                 / ____ \| | |_) | | | | (_| | |  | |  __/ | | (__ 
                /_/    \_\_| .__/|_| |_|\__,_|_|  |_|\___|_|  \___|
                           | |                                     
                           |_|                                                
                                    
                ");
        }

        private static void ExceptionHandling(Exception exception) {
            bool isInternetError = exception.Message.Contains("ERR_INTERNET");
            if(isInternetError) {
                ConsoleTextColor.RedConsoleOutput("Проблема с интернетом");

            }else {
                ConsoleTextColor.RedConsoleOutput(exception.Message);
            }
        }
    }



}
