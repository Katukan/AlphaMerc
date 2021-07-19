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
        int todayDay, maxWord, idAccount;

        // конструктор
        public BotClass(string driverPath, IRepository repo) {
            driver = new ChromeDriver(driverPath);
            repository = repo;
            todayDay = DateTime.Now.Day;
        }

    // вспомогательные методы
        private IWebElement FindElement(string xPath) {
            return driver.FindElement(By.XPath(xPath));
        }

        // окрашиваем надпись в зеленый
        private void GreenConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // окрашиваем надпись в желтый
        private void YellowConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // окрашиваем надпись в красный
        private void RedConsoleOutput(string str) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // меняет день 
        private void ChangeDay(IWebElement dayInput, int day) {
            dayInput.Clear();
            dayInput.SendKeys(day.ToString());
        }

        // менят месяц
        private void ChangeMonth(SelectElement selectElement, int month) {
            selectElement.SelectByValue((--month).ToString());  // декреминтируем 'month', чтобы соответсвовала индексу JavaScript
        }

        // сравниваем имя пользователя
        private bool CheckDoctorName(string doctorIdName) {
            return FindElement("//div[@id='loggedas']/b").Text.Contains(doctorIdName);
        }

        // выводим молокосборщиков
        private void ShowMilkmen(IList<Milkman> milkmen, bool isYellow = false) {
            if(isYellow)  
                Console.ForegroundColor = ConsoleColor.Yellow;
            for(int i = 0; i < milkmen.Count(); i++) {
                Console.WriteLine($"{i+1}) {milkmen[i].Name} ");
            }
            if(isYellow) 
                Console.ForegroundColor = ConsoleColor.Gray;
        }

        // выводим пользователей
        private void ShowAccounts(IList<DoctorAccount> accounts) {
            Console.WriteLine("Доступные аккаунты:");
            foreach(var account in accounts) {
                Console.WriteLine($"{account.Id}) {account.Name}");
            }
        }


    // основные методы

        // начало
        public void Start() {
            try {
                OpenSite(); 
                EnterSite();
                MainProcess();
            }
            catch(Exception ex) {
                if(ex.Message.Contains("ERR_INTERNET")) {
                    RedConsoleOutput("Проблема с интернетом");
                }else {
                    RedConsoleOutput(ex.Message);
                }
            }
            finally {
                Stop();
            }
        }

        // заходим на сайт
        private void OpenSite() {
            driver.Navigate().GoToUrl("http://mercury.vetrf.ru/gve");
        }

        // получаем нужного пользователя и заходим в систему "Меркурий ГВЭ"
        private void EnterSite() {
            IList<DoctorAccount> doctorAccounts = repository.GetDoctorAccounts();
            if(doctorAccounts.Count() == 0) {
                throw new Exception("Отсутвуют данные об аккаунтах");
            }
            ShowAccounts(doctorAccounts);
            Console.WriteLine("Введите ID пользователя и нажмите Enter");
            while(!Int32.TryParse(Console.ReadLine(), out idAccount) || doctorAccounts.All(d => d.Id != idAccount)) {
                RedConsoleOutput("Неккоректный ID, введите заново");
            }
            DoctorAccount doctorAccount = doctorAccounts.First(d => d.Id == idAccount);
            Authorization(doctorAccount.Login, doctorAccount.Password);
            
        }

        // Авторизация
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

        // оформление ВСД через шаблон для одного пользователя
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
                Thread.Sleep(1500);
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
        
        // оформление ВСД для нескольких пользователей
        private void MainProcess() {
            IList<Milkman> allMilkmen = repository.GetMilkmen().Where(m => CheckDoctorName(m.DoctorIDName)).ToList();  
            IList<Milkman> milkmen = allMilkmen.Where(m => m.Exclud != 1).ToList();
            if(milkmen.Count() == 0) {
                throw new Exception("Данные молочников отсутствуют");
            }
            maxWord = milkmen.Select(m => m.Name.Length).ToArray().Max();
            ShowMilkmen(((IList<Milkman>)milkmen));
            Console.WriteLine(new string('-', maxWord + 5));
            IList<Milkman> ExcludMilkmen = allMilkmen.Where(m => m.Exclud == 1).ToList();
            if(ExcludMilkmen.Count() > 0) {
                YellowConsoleOutput("Следующие ЛПХ не будут оформляться:");
                ShowMilkmen(ExcludMilkmen, true);
            }
            Console.WriteLine("Нажмите Enter, чтобы оформить");
            Console.ReadLine();
            Console.WriteLine(" Name " + new string(' ', maxWord) + "Status");
            Console.WriteLine(new string('-', maxWord+3) + ' ' + new string('-', 10));
            foreach(var milkman in milkmen) {
                PaperWork(milkman);
            }
        }

        // остановка, и закрытия окон
        public void Stop() {
            Console.WriteLine("Программа завершилась.\n Нажите на Enter, чтобы выйти");
            Console.ReadLine();
            driver.Close();
            driver.Quit();
            Thread.Sleep(1500);
        }
    }
}