using System;
using Microsoft.EntityFrameworkCore;
using ConsoleProject.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Text.Json;
using System.Text;

namespace ConsoleProject {
    public class Program {
        static IRepository repository;
        static IWebDriver chromeDriver;
        static BotSettings botSettings;
        static BotForMercury botForMercury;

        static void Main(string[] args) {
            try {
                DatabaseMigrate();
                JsonSettingsDeserializeAsync();
                repository = new Repository();
                chromeDriver = new ChromeDriver(Directory.GetCurrentDirectory());
                botForMercury = BotForMercury.RenderBotForMercury(repository, chromeDriver, botSettings);
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

        static async void JsonSettingsDeserializeAsync() {
            using(FileStream fileStream = new FileStream("botSettings.json", FileMode.OpenOrCreate)) {
                botSettings = await JsonSerializer.DeserializeAsync<BotSettings>(fileStream);
            }
        }

        private static void ShowLogo() {
            using(StreamReader stringReader = new StreamReader("logo.txt")) {
                Console.WriteLine(stringReader.ReadToEnd());
            }
        }

        private static void ExceptionHandling(Exception exception) {
            bool isInternetError = exception.Message.Contains("ERR_INTERNET");
            if(isInternetError) {
                ConsoleTextColor.ColoringInRed("Проблема с интернетом");
            }else {
                ConsoleTextColor.ColoringInRed(exception.Message);
            }
        }
    }
}
