using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using System.Linq;

namespace ConsoleProject {

    public class BotClass {

        IRepository repository;
        IWebDriver driver;
        IWebElement dayInput;
        SelectElement selectElement;
        DateTime tomorrow;
        int todayDay;
        int maxWord;

        public BotClass(string driverPath, IRepository repo) {
            driver = new ChromeDriver(driverPath);
            repository = repo;
            todayDay = DateTime.Now.Day;
        }

        // вспомогательные методы
        private IWebElement FindElement(string xPath) {
            return driver.FindElement(By.XPath(xPath));
        }

        private void GreenConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void YellowConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void RedConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void ChangeDay(IWebElement dayInput, int day) {
            dayInput.Clear();
            dayInput.SendKeys(day.ToString());
        }

        private void ChangeMonth(SelectElement selectElement, int month) {
            selectElement.SelectByValue((--month).ToString());
        }

        private bool CheckDoctorName(string doctorIdName) {
            return FindElement("//div[@id='loggedas']/b").Text.Contains(doctorIdName);
        }

        private void ShowMilkmen(IList<Milkman> milkmen, bool isYellow = false) {
            if(isYellow) 
                Console.ForegroundColor = ConsoleColor.Yellow;
            for(int i = 0; i < milkmen.Count(); i++) {
                Console.WriteLine($"{i+1}) {milkmen[i].Name} {milkmen[i].DoctorIDName}");
            }
            if(isYellow) 
                Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void ShowAccounts(IList<DoctorAccount> accounts) {
            Console.WriteLine("Доступные аккаунты:");
            foreach(var account in accounts) {
                Console.WriteLine($"{account.Id}) {account.Name}");
            }
        }


        // основные методы
        public void OpenSite() {
            driver.Navigate().GoToUrl("http://mercury.vetrf.ru/gve");
        }

        public void EnterSite() {
            IList<DoctorAccount> doctorAccounts = repository.GetDoctorAccounts();
            ShowAccounts(doctorAccounts);
            Console.WriteLine("Введите ID пользователя и нажмите Enter");
            int idAccount = Int32.Parse(Console.ReadLine());
            DoctorAccount doctorAccount = repository.GetDoctorAccounts().First(d => d.Id == idAccount);
            Authorization(doctorAccount.Login, doctorAccount.Password);
        }

        private void Authorization(string login, string password) {
            try {
                FindElement("//input[@id='username']").SendKeys(login); // логин
                FindElement("//input[@id='password']").SendKeys(password);   // пароль
                FindElement("//button[@type='submit']").Click();    // отправка формы
                Console.WriteLine("Подождите полной загрузки страницы и нажмите Enter");
                Console.ReadLine();
                GreenConsoleOutput("Вы успешно зашли: " + FindElement("//div[@id='loggedas']/b").Text);
            }catch {
                RedConsoleOutput("Не удалось войти в систему");
            }
        }

        private void PaperWork(Milkman milkman) {
            try {
                FindElement($"//input[@value='{milkman.RuNumber}']").Click();   // находим площадку по номеру RU
                FindElement("//button[@type='submit']").Click();    // нажимаем выбрать

                FindElement("//a[@href='operatorui?_action=listTransaction&formed=false&pageList=1&all=true&preview=true']").Click(); // кнопка транзакция
                FindElement("//a[@href='javascript:onEnterMenu(true, \"\", false);']").Click(); // шаблоны
                Thread.Sleep(1500);
                FindElement("//tr[@class='first']/td[@class='control']").Click(); // лупа
                FindElement("//a[@class='operation-link blue']").Click(); // редактировать продукцию
                Thread.Sleep(1500);

                // дата выработки продукции
                dayInput =  FindElement("//table[@class='innerForm']/tbody/tr[@id='PRODUCTION_DATE']/td/div/span/input[@type='text']");   // день выработки
                ChangeDay(dayInput, todayDay);
                selectElement = new SelectElement(FindElement("//tr[@id='PRODUCTION_DATE']/td/div/span/select[@class='middle']")); // месяц выработки
                ChangeMonth(selectElement, DateTime.Now.Month);

                // дата срока годности
                tomorrow = DateTime.Today.AddDays(1);
                dayInput =  FindElement("//table[@class='innerForm']/tbody/tr[@id='BEST_BEFORE_DATE']/td/div/span/input[@type='text']");   // день просрочки
                ChangeDay(dayInput, tomorrow.Day);
                selectElement = new SelectElement(FindElement("//tr[@id='BEST_BEFORE_DATE']/td/div/span/select[@class='middle']")); // месяц просрочки
                ChangeMonth(selectElement, tomorrow.Month);

                FindElement("//button[@class='positive']").Click(); // сохранить
                Thread.Sleep(2000);
                FindElement("//button[@onclick='javascript:doAction(\"transactionShowForm\", \"generateTransactionFromTemplate\", \"\")']").Click(); // создать транзакцию
                //FindElement("//button[@class='positive']").Click(); // оформить
                //driver.SwitchTo().Alert().Accept(); // подтверждение
                GreenConsoleOutput(milkman.Name + new string(' ', maxWord+5-milkman.Name.Length) + " успех");
            }
            catch {
                RedConsoleOutput(milkman.Name + new string(' ', maxWord+5-milkman.Name.Length) +" ошибка");
            }
            finally {
                FindElement("//a[@href='operatorui?_action=changeServicedEnterprise']").Click();
            }

        }
        
        public void MainProcess() {
            IList<Milkman> milkmen = repository.GetMilkmen().Where(m => CheckDoctorName(m.DoctorIDName) && m.Exclud != 1).ToList();
            maxWord = milkmen.Select(m => m.Name.Length).ToArray().Max();
            ShowMilkmen(((IList<Milkman>)milkmen));
            YellowConsoleOutput(new string('-', maxWord + 5));
            YellowConsoleOutput("Следующие ЛПХ не будут оформляться:");
            ShowMilkmen(repository.GetMilkmen().Where(m => m.Exclud == 1).ToList(), true);
            Console.WriteLine("Нажмите Enter, чтобы оформить");
            Console.ReadLine();
            Console.WriteLine(" Name " + new string(' ', maxWord) + "Status");
            Console.WriteLine(new string('-', maxWord+3) + ' ' + new string('-', 10));
            foreach(var milkman in milkmen) {
                PaperWork(milkman);
            }
            
        }
    }
}