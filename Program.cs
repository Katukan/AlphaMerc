using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using ConsoleProject.DataRepositories;
using ConsoleProject.ProjectSettings;
using ConsoleProject.BotClasses;

namespace ConsoleProject {
    public class Program {
        static IRepository repository;
        static IWebDriver chromeDriver;
        static BotSettings botSettings;
        static BotForMercury botForMercury;

        static void Main(string[] args) {
            try {
                repository = new Repository();
                chromeDriver = new ChromeDriver(Directory.GetCurrentDirectory());
                JsonSetting.JsonSettingsDeserializeAsync(botSettings);
                repository.DatabaseMigrate();
                botForMercury = BotForMercury.RenderBotForMercury(repository, chromeDriver, botSettings);
                ConsoleSettings.ShowLogo();
                botForMercury.Start();
            }
            catch(Exception exception) {
                ConsoleSettings.ShowExceptionHandling(exception);
            }
            finally {
                botForMercury.Stop();
            }
        }        
    }
}
