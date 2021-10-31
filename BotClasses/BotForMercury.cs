using System;
using OpenQA.Selenium;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using System.Linq;
using ConsoleProject.DataRepositories;

namespace ConsoleProject.BotClasses {

    public class BotForMercury {
        ConsoleWork consoleWork;
        private HtmlPageWork htmlPageWork;
        private IRepository repository;
        private IWebDriver browserDriver;
        BotSettings botSettings;
        IList<DoctorAccount> doctorAccounts;
        int idDoctorAccount = 0;
        IList<Milkman> allMilkmen;
        IList<Milkman> includedMilkmen;
        IList<Milkman> excludMilkmen;

        private BotForMercury(IRepository repository, IWebDriver browserDriver, BotSettings botSettings) { 
            this.repository = repository;
            this.browserDriver = browserDriver;
            this.botSettings = botSettings;
            this.consoleWork = new ConsoleWork();
            htmlPageWork = new HtmlPageWork(browserDriver, botSettings, consoleWork);
        }

        public static BotForMercury RenderBotForMercury(IRepository repository, IWebDriver browserDriver, BotSettings botSettings) {
            return new BotForMercury(repository, browserDriver, botSettings);
        }

        public void Start() { 
            OpenSite(); 
            InitialSettings();
            MainProcess();
        }

        private void OpenSite() {
            browserDriver.Navigate().GoToUrl(botSettings.siteName);
        }

        private void InitialSettings() {
            doctorAccounts = repository.DoctorAccounts();
            CheckingCountDoctorAccounts();
            consoleWork.ShowDoctorAccounts(doctorAccounts);   
            Console.WriteLine("Введите ID пользователя и нажмите Enter");
            ReadingAndCheckingCorrectnessAccountId();
            DoctorAccount doctorAccount = repository.FindDoctorAccount(idDoctorAccount);
            Authorization(doctorAccount.Login, doctorAccount.Password);
        }

        private void CheckingCountDoctorAccounts() {
            if(doctorAccounts.Count() == 0) {
                throw new Exception("Отсутвуют данные об аккаунтах");
            }
        }

        public void ReadingAndCheckingCorrectnessAccountId() {
            string idAccountFromConsole = Console.ReadLine();
            bool isNumber = Int32.TryParse(idAccountFromConsole, out idDoctorAccount);
            bool doctorAccountsHaveNotThisId = doctorAccounts.All(d => d.Id != idDoctorAccount);
            while(!isNumber || doctorAccountsHaveNotThisId) {
                ConsoleSettings.ColoringInRed("Неккоректный ID, введите заново");
                idAccountFromConsole = Console.ReadLine();
                isNumber = Int32.TryParse(idAccountFromConsole, out idDoctorAccount);
                doctorAccountsHaveNotThisId = doctorAccounts.All(d => d.Id != idDoctorAccount);
            }
        }

        private void Authorization(string login, string password) {
            try {
                htmlPageWork.SubmittingAuthorizationForm(login, password);
                Console.WriteLine("Подождите полной загрузки страницы и нажмите Enter");
                Console.ReadLine();
                string accountNameFromSite = htmlPageWork.FindElementByXpath(botSettings.accountNameFromSiteXpath).Text;
                ConsoleSettings.ColoringInGreen("Вы успешно зашли: " + accountNameFromSite);
            }catch {
                throw new Exception("Не удалось войти в систему");
            }
        }

        private void MainProcess() {
            InitializationMilkmen();
            CheckingCountIncludedMilkmen();
            consoleWork.ShowMilkmen(includedMilkmen);
            int longestMilkmanName = CountingLongestMilkmenName(includedMilkmen);
            consoleWork.DrawLine(longestMilkmanName);
            consoleWork.ShowExcludMilkmen(excludMilkmen); 
            Console.WriteLine("Нажмите Enter, чтобы оформить");
            consoleWork.DrawTableHead(longestMilkmanName);
            htmlPageWork.PaperWorkForIncludedMilkmen(longestMilkmanName, includedMilkmen);
        }

        private int CountingLongestMilkmenName(IList<Milkman> includedMilkmen) {
            return includedMilkmen.Select(m => m.Name.Length).ToArray().Max();
        }

        private void InitializationMilkmen() {
            allMilkmen = repository.CurrentContextMilkmen(CheckingDoctorName);
            includedMilkmen = repository.IncludedMilkmen();
            excludMilkmen = repository.ExcludMilkmen();
        }

        private bool CheckingDoctorName(string doctorIdName) {
            return htmlPageWork.FindElementByXpath(botSettings.accountNameFromSiteXpath).Text.Contains(doctorIdName);
        }

        private void CheckingCountIncludedMilkmen() {
            if(includedMilkmen.Count() == 0) {
                throw new Exception("Данные о молочниках отсутствуют");
            }
        }
        
         public void Stop() {
            Console.WriteLine("Программа завершилась.\n Нажите на Enter, чтобы выйти");
            Console.ReadLine();
            browserDriver.Close();
            browserDriver.Quit();
            Thread.Sleep(botSettings.millisecondsTimeout);
        }
    }
}