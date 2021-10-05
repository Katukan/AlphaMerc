using System;
using OpenQA.Selenium;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using System.Linq;

namespace ConsoleProject {

    public class BotForMercury {

        private IRepository repository;
        private IWebDriver browserDriver;
        BotSettings botSettings;
        IList<DoctorAccount> doctorAccounts;
        int idDoctorAccount = 0;
        IList<Milkman> allMilkmen;
        IList<Milkman> includedMilkmen;
        IList<Milkman> excludMilkmen;

        private BotForMercury() { }

        public static BotForMercury RenderBotForMercury(IRepository repository, IWebDriver browserDriver, BotSettings botSettings) {
            return new BotForMercury { 
                repository = repository,
                browserDriver = browserDriver,
                botSettings = botSettings
            };
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
            doctorAccounts = repository.GetDoctorAccounts();
            CheckingCountDoctorAccounts();
            ShowDoctorAccounts();
            Console.WriteLine("Введите ID пользователя и нажмите Enter");
            ReadingAndCheckingCorrectnessAccountId();
            DoctorAccount doctorAccount = FindDoctorAccount(idDoctorAccount);
            Authorization(doctorAccount.Login, doctorAccount.Password);
        }

        private void CheckingCountDoctorAccounts() {
            if(doctorAccounts.Count() == 0) {
                throw new Exception("Отсутвуют данные об аккаунтах");
            }
        }

        private void ShowDoctorAccounts() {
            Console.WriteLine("Доступные аккаунты:");
            foreach(var d in doctorAccounts) {
                Console.WriteLine($"{d.Id}) {d.Name}");
            }
        }

        private void ReadingAndCheckingCorrectnessAccountId() {
            string idAccountFromConsole = Console.ReadLine();
            bool isNumber = Int32.TryParse(idAccountFromConsole, out idDoctorAccount);
            bool doctorAccountsHaveNotThisId = doctorAccounts.All(d => d.Id != idDoctorAccount);
            while(!isNumber || doctorAccountsHaveNotThisId) {
                ConsoleTextColor.ColoringInRed("Неккоректный ID, введите заново");
                idAccountFromConsole = Console.ReadLine();
                isNumber = Int32.TryParse(idAccountFromConsole, out idDoctorAccount);
                doctorAccountsHaveNotThisId = doctorAccounts.All(d => d.Id != idDoctorAccount);
            }
        }

        private DoctorAccount FindDoctorAccount(int idDoctorAccount) {
            return doctorAccounts.First(d => d.Id == idDoctorAccount);
        }

        private void Authorization(string login, string password) {
            try {
                SubmittingAuthorizationForm(login, password);
                Console.WriteLine("Подождите полной загрузки страницы и нажмите Enter");
                Console.ReadLine();
                string accountNameFromSite = FindElementByXpath(botSettings.accountNameFromSiteXpath).Text; 
                ConsoleTextColor.ColoringInGreen("Вы успешно зашли: " + accountNameFromSite);
            }catch {
                throw new Exception("Не удалось войти в систему");
            }
        }

        private void SubmittingAuthorizationForm(string login, string password) {
            FindElementByXpath(botSettings.userNameInputElementXpath).SendKeys(login);
            FindElementByXpath(botSettings.passwordInputElementXpath).SendKeys(password);
            FindElementByXpathAndClick(botSettings.authorizationSubmitButtonXpath);
        }

        private IWebElement FindElementByXpath(string xPath) {
            return browserDriver.FindElement(By.XPath(xPath));
        }

        private void FindElementByXpathAndClick(string xPath) {
            FindElementByXpath(xPath).Click();
        }

        private void MainProcess() {
            InitializationMilkmen();
            CheckingCountIncludedMilkmen();
            ShowMilkmen(includedMilkmen);
            DrawLine();
            ShowExcludMilkmen();
            Console.WriteLine("Нажмите Enter, чтобы оформить");
            DrawTableHead();
            PaperWorkForIncludeMilkmen();
        }

        private void InitializationMilkmen() {
            allMilkmen = repository.GetMilkmen().Where(m => CheckingDoctorName(m.DoctorIdName)).ToList();  
            includedMilkmen = allMilkmen.Where(m => m.Exclud != 1).ToList();
            excludMilkmen = allMilkmen.Where(m => m.Exclud == 1).ToList();
            foreach(var a in excludMilkmen) {
                Console.WriteLine($"{a.Name} - {a.Exclud}");
            }
        }

        private bool CheckingDoctorName(string doctorIdName) {
            return FindElementByXpath(botSettings.accountNameFromSiteXpath).Text.Contains(doctorIdName);
        }

        private void CheckingCountIncludedMilkmen() {
            if(includedMilkmen.Count() == 0) {
                throw new Exception("Данные молочников отсутствуют или они исключены");
            }
        }

        private void ShowMilkmen(IList<Milkman> milkmen, bool isYellowConsoleText = false) {
            if(isYellowConsoleText) {
                ConsoleTextColor.ColoringGrayToYellow();
            }
            for(int i = 0; i < milkmen.Count(); i++) {
                Console.WriteLine($"{i + 1}) {milkmen[i].Name} ");
            }
            if(isYellowConsoleText) {
                ConsoleTextColor.ColoringYellowToGray();
            }
        }

        private void DrawLine() {
            int longestMilkmanName = CountingLongestMilkmenName(includedMilkmen);
            Console.WriteLine(new string('-', longestMilkmanName + 5));
        }

        private int CountingLongestMilkmenName(IList<Milkman> includedMilkmen) {
            return includedMilkmen.Select(m => m.Name.Length).ToArray().Max();
        }

        private void ShowExcludMilkmen() {
            if(excludMilkmen.Count() > 0) {
                ConsoleTextColor.ColoringInYellow("Следующие ЛПХ не будут оформляться:");
                ShowMilkmen(excludMilkmen, true);
            }
        }

        private void DrawTableHead() {
            int longestMilkmanName = CountingLongestMilkmenName(includedMilkmen);
            Console.ReadLine();
            Console.WriteLine(" Name " + new string(' ', longestMilkmanName) + "Status");
            Console.WriteLine(new string('-', longestMilkmanName + 3) + ' ' + new string('-', 10));
        }

        private void PaperWorkForIncludeMilkmen() {
            int longestMilkmanName = CountingLongestMilkmenName(includedMilkmen);
            foreach(var m in includedMilkmen) {
                PaperWork(m, longestMilkmanName);
            }
        }

        private void PaperWork(Milkman milkman, int longestMilkmanName) {
            try {
                FindMilkmanInHtmlPage(milkman);
                FindReadyTemplateInHtmlPage();
                ChangeProductionDate();
                ChangeExpirationDate();
                SaveTemplate();
                ConfirmDocument();
                ShowSuccesfulStatus(milkman, longestMilkmanName);
            }
            catch {
                ShowErrorStatus(milkman, longestMilkmanName);
            }
            finally {
                ShowMilkmenListInHtmlPage();
            }
        }

        private void FindMilkmanInHtmlPage(Milkman milkman) {
            FindElementByXpathAndClick(String.Format(botSettings.milkmanInHtmlPageXpath, milkman.RuNumber));
            FindElementByXpathAndClick(botSettings.milkmanSelectonButtonXpath);
        }

        private void FindReadyTemplateInHtmlPage() {
            FindElementByXpathAndClick(botSettings.transactionButtonXpath);
            FindElementByXpathAndClick(botSettings.templateButtonXpath);
            Thread.Sleep(botSettings.millisecondsTimeout);
            FindElementByXpathAndClick(botSettings.magniferImageButtonXpath);
            FindElementByXpathAndClick(botSettings.editProductButtonXpath);
            Thread.Sleep(botSettings.millisecondsTimeout);
        }

        private void ChangeProductionDate() {
            IWebElement productionDayInputElement =  FindElementByXpath(botSettings.productionDayXpath);
            int todayDay = DateTime.Now.Day;
            ChangeDay(productionDayInputElement, todayDay);
            SelectElement productionMonthSelectElement = new SelectElement(FindElementByXpath(botSettings.productionMonthXpath));
            ChangeMonth(productionMonthSelectElement, DateTime.Now.Month);
        }

        private void ChangeDay(IWebElement dayInputElement, int day) {
            dayInputElement.Clear();
            dayInputElement.SendKeys(day.ToString());
        }

        private void ChangeMonth(SelectElement selectElement, int month) {
            int monthIndexInJavaScript = --month; 
            selectElement.SelectByValue((monthIndexInJavaScript).ToString());  
        }

        private void ChangeExpirationDate() {
            DateTime tomorrowDay = DateTime.Today.AddDays(1);
            IWebElement expirationDayInputElement =  FindElementByXpath(botSettings.expirationDayXpath);
            ChangeDay(expirationDayInputElement, tomorrowDay.Day);
            SelectElement expirationMonthSelectElement = new SelectElement(FindElementByXpath(botSettings.expirationMonthXpath));
            ChangeMonth(expirationMonthSelectElement, tomorrowDay.Month);
        }

        private void SaveTemplate() {
            FindElementByXpathAndClick(botSettings.saveTemplateButton);
        }

        private void ConfirmDocument() {
            Thread.Sleep(botSettings.millisecondsTimeout);
            FindElementByXpathAndClick(botSettings.createTransactionButtonXpath);
            Thread.Sleep(botSettings.millisecondsTimeout);
            FindElementByXpathAndClick(botSettings.formalizeButtonXpath);
            Thread.Sleep(botSettings.millisecondsTimeout);
            AcceptJavaScriptAlert();
        }

        private void AcceptJavaScriptAlert() {
            browserDriver.SwitchTo().Alert().Accept();
        }

        private void ShowSuccesfulStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleTextColor.ColoringInGreen(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) + " успех");
        }

        private void ShowErrorStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleTextColor.ColoringInRed(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) +" ошибка");
        }
        
        private void ShowMilkmenListInHtmlPage() {
            Thread.Sleep(botSettings.millisecondsTimeout);
            FindElementByXpathAndClick(botSettings.changeServicedEnterpriseLink);
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