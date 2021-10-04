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
        IList<DoctorAccount> doctorAccounts;
        int idDoctorAccount = 0;
        IList<Milkman> allMilkmen;
        IList<Milkman> includedMilkmen;
        IList<Milkman> excludMilkmen;

        private const int millisecondsTimeout = 1500;
        private const string siteName = "http://mercury.vetrf.ru/gve";

        private BotForMercury() { }

        public static BotForMercury RenderBotForMercury(IRepository repository, IWebDriver browserDriver) {
            return new BotForMercury { 
                repository = repository,
                browserDriver = browserDriver
            };
        }

        public void Start() { 
            OpenSite(); 
            InitialSettings();
            MainProcess();
        }

        private void OpenSite() {
            browserDriver.Navigate().GoToUrl(siteName);
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
                ConsoleTextColor.RedConsoleOutput("Неккоректный ID, введите заново");
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
                string accountNameFromSite = FindElementByXpath("//div[@id='loggedas']/b").Text;
                ConsoleTextColor.GreenConsoleOutput("Вы успешно зашли: " + accountNameFromSite);
            }catch {
                throw new Exception("Не удалось войти в систему");
            }
        }

        private void SubmittingAuthorizationForm(string login, string password) {
            FindElementByXpath("//input[@id='username']").SendKeys(login);
            FindElementByXpath("//input[@id='password']").SendKeys(password);
            FindElementByXpathAndClick("//button[@type='submit']");
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
            ShowMilkmen();
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
        }

        private bool CheckingDoctorName(string doctorIdName) {
            return FindElementByXpath("//div[@id='loggedas']/b").Text.Contains(doctorIdName);
        }

        private void CheckingCountIncludedMilkmen() {
            if(includedMilkmen.Count() == 0) {
                throw new Exception("Данные молочников отсутствуют или они исключены");
            }
        }

        private void ShowMilkmen(bool isYellowConsoleText = false) {
            if(isYellowConsoleText)  
                Console.ForegroundColor = ConsoleColor.Yellow;
            for(int i = 0; i < includedMilkmen.Count(); i++) {
                Console.WriteLine($"{i + 1}) {includedMilkmen[i].Name} ");
            }
            if(isYellowConsoleText) 
                Console.ForegroundColor = ConsoleColor.Gray;
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
                ConsoleTextColor.YellowConsoleOutput("Следующие ЛПХ не будут оформляться:");
                ShowMilkmen(true);
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
                FindElementByXpathAndClick("//button[@class='positive']"); // сохранить
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
            FindElementByXpathAndClick($"//input[@value='{milkman.RuNumber}']");   // находим площадку по номеру RU
            FindElementByXpathAndClick("//button[@type='submit']");    // нажимаем выбрать
        }

        private void FindReadyTemplateInHtmlPage() {
            FindElementByXpathAndClick("//a[@href='operatorui?_action=listTransaction&formed=false&pageList=1&all=true&preview=true']"); // кнопка транзакция
            FindElementByXpathAndClick("//a[@href='javascript:onEnterMenu(true, \"\", false);']"); // шаблоны
            Thread.Sleep(millisecondsTimeout);
            FindElementByXpathAndClick("//tr[@class='first']/td[@class='control']"); // лупа
            FindElementByXpathAndClick("//a[@class='operation-link blue']"); // редактировать продукцию
            Thread.Sleep(millisecondsTimeout);
        }

        private void ChangeProductionDate() {
            IWebElement productionDayInputElement =  FindElementByXpath("//table[@class='innerForm']/tbody/tr[@id='PRODUCTION_DATE']/td/div/span/input[@type='text']");   // день выработки
            int todayDay = DateTime.Now.Day;
            ChangeDay(productionDayInputElement, todayDay);
            SelectElement productionMonthSelectElement = new SelectElement(FindElementByXpath("//tr[@id='PRODUCTION_DATE']/td/div/span/select[@class='middle']")); // месяц выработки
            ChangeMonth(productionMonthSelectElement, DateTime.Now.Month);
        }

        private void ChangeDay(IWebElement dayInputElement, int day) {
            dayInputElement.Clear();
            dayInputElement.SendKeys(day.ToString());
        }

        private void ChangeMonth(SelectElement selectElement, int month) {
            selectElement.SelectByValue((--month).ToString());  // декреминтируем 'month', чтобы соответсвовала индексу JavaScript
        }

        private void ChangeExpirationDate() {
            DateTime tomorrowDay = DateTime.Today.AddDays(1);
            IWebElement expirationDayInputElement =  FindElementByXpath("//table[@class='innerForm']/tbody/tr[@id='BEST_BEFORE_DATE']/td/div/span/input[@type='text']");   // день просрочки
            ChangeDay(expirationDayInputElement, tomorrowDay.Day);
            SelectElement expirationMonthSelectElement = new SelectElement(FindElementByXpath("//tr[@id='BEST_BEFORE_DATE']/td/div/span/select[@class='middle']")); // месяц просрочки
            ChangeMonth(expirationMonthSelectElement, tomorrowDay.Month);
        }

        private void ConfirmDocument() {
            Thread.Sleep(millisecondsTimeout);
            FindElementByXpathAndClick("//table[@class='form']/tbody/tr/td[@class='control']/div/button[3]"); // создать транзакцию
            Thread.Sleep(millisecondsTimeout);
            FindElementByXpathAndClick("//button[@class='positive']"); // оформить
            Thread.Sleep(millisecondsTimeout);
            AcceptJavaScriptAlert(); // подтверждение
        }

        private void AcceptJavaScriptAlert() {
            browserDriver.SwitchTo().Alert().Accept();
        }

        private void ShowSuccesfulStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleTextColor.GreenConsoleOutput(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) + " успех");
        }

        private void ShowErrorStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleTextColor.RedConsoleOutput(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) +" ошибка");
        }
        
        private void ShowMilkmenListInHtmlPage() {
            Thread.Sleep(millisecondsTimeout);
            FindElementByXpath("//a[@href='operatorui?_action=changeServicedEnterprise']").Click();
        }

         public void Stop() {
            Console.WriteLine("Программа завершилась.\n Нажите на Enter, чтобы выйти");
            Console.ReadLine();
            browserDriver.Close();
            browserDriver.Quit();
            Thread.Sleep(millisecondsTimeout);
        }
    }
}