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

        private IRepository repository;
        private IWebDriver chromeDriver;

        private const string SITE_NAME = "http://mercury.vetrf.ru/gve";


        private BotClass() { }

        public static BotClass BotClassFactory (string driverPath, IRepository repository) {
            return new BotClass { chromeDriver = new ChromeDriver(driverPath), repository = repository };
        }


        public void Start() {
            try {
                OpenSite(); 
                InitialSettings();
                MainProcess();
            }
            catch(Exception ex) {
                bool isInternetError = ex.Message.Contains("ERR_INTERNET");
                if(isInternetError) {
                    ConsoleTextColor.RedConsoleOutput("Проблема с интернетом");

                }else {
                    ConsoleTextColor.RedConsoleOutput(ex.Message);
                }
            }
            finally {
                Stop();
            }
        }

        private void OpenSite() {
            chromeDriver.Navigate().GoToUrl(SITE_NAME);
        }

        private void InitialSettings() {
            IList<DoctorAccount> doctorAccounts = repository.GetDoctorAccounts();
            if(doctorAccounts.Count() == 0) {
                throw new Exception("Отсутвуют данные об аккаунтах");
            }
            ShowAccounts(doctorAccounts);
            Console.WriteLine("Введите ID пользователя и нажмите Enter");
            int idAccount;
            bool isCorrectIdAccount = Int32.TryParse(Console.ReadLine(), out idAccount) || doctorAccounts.All(d => d.Id != idAccount);
            while(!isCorrectIdAccount) {
                ConsoleTextColor.RedConsoleOutput("Неккоректный ID, введите заново");
            }
            DoctorAccount doctorAccount = doctorAccounts.First(d => d.Id == idAccount);
            Authorization(doctorAccount.Login, doctorAccount.Password);
            
        }

        private void MainProcess() {
            IList<Milkman> allMilkmen = repository.GetMilkmen().Where(m => CheckDoctorName(m.DoctorIDName)).ToList();  
            IList<Milkman> includedMilkmen = allMilkmen.Where(m => m.Exclud != 1).ToList();
            if(includedMilkmen.Count() == 0) {
                throw new Exception("Данные молочников отсутствуют");
            }
            int maxWord = includedMilkmen.Select(m => m.Name.Length).ToArray().Max();
            ShowMilkmen(((IList<Milkman>)includedMilkmen));
            Console.WriteLine(new string('-', maxWord + 5));
            IList<Milkman> ExcludMilkmen = allMilkmen.Where(m => m.Exclud == 1).ToList();
            if(ExcludMilkmen.Count() > 0) {
                ConsoleTextColor.YellowConsoleOutput("Следующие ЛПХ не будут оформляться:");
                ShowMilkmen(ExcludMilkmen, true);
            }
            Console.WriteLine("Нажмите Enter, чтобы оформить");
            Console.ReadLine();
            Console.WriteLine(" Name " + new string(' ', maxWord) + "Status");
            Console.WriteLine(new string('-', maxWord+3) + ' ' + new string('-', 10));
            foreach(var m in includedMilkmen) {
                PaperWork(m, maxWord);
            }
        }


        public void Stop() {
            Console.WriteLine("Программа завершилась.\n Нажите на Enter, чтобы выйти");
            Console.ReadLine();
            chromeDriver.Close();
            chromeDriver.Quit();
            Thread.Sleep(1500);
        }


        private void ShowAccounts(IList<DoctorAccount> accounts) {
            Console.WriteLine("Доступные аккаунты:");
            foreach(var a in accounts) {
                Console.WriteLine($"{a.Id}) {a.Name}");
            }
        }


        private void Authorization(string login, string password) {
            try {
                FindElement("//input[@id='username']").SendKeys(login); // логин
                FindElement("//input[@id='password']").SendKeys(password);   // пароль
                FindElement("//button[@type='submit']").Click();    // отправка формы
                Console.WriteLine("Подождите полной загрузки страницы и нажмите Enter");
                Console.ReadLine();
                string accountNameFromSite = FindElement("//div[@id='loggedas']/b").Text;
                ConsoleTextColor.GreenConsoleOutput("Вы успешно зашли: " + accountNameFromSite);
            }catch {
                ConsoleTextColor.RedConsoleOutput("Не удалось войти в систему");
            }
        }


         private void ShowMilkmen(IList<Milkman> includedMilkmen, bool isYellow = false) {
            if(isYellow)  
                Console.ForegroundColor = ConsoleColor.Yellow;
            for(int i = 0; i < includedMilkmen.Count(); i++) {
                Console.WriteLine($"{i+1}) {includedMilkmen[i].Name} ");
            }
            if(isYellow) 
                Console.ForegroundColor = ConsoleColor.Gray;
        }


        private void PaperWork(Milkman milkman, int maxWord) {
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
                IWebElement dayInputElement =  FindElement("//table[@class='innerForm']/tbody/tr[@id='PRODUCTION_DATE']/td/div/span/input[@type='text']");   // день выработки
                int todayDay = DateTime.Now.Day;
                ChangeDay(dayInputElement, todayDay);
                SelectElement monthSelectElement = new SelectElement(FindElement("//tr[@id='PRODUCTION_DATE']/td/div/span/select[@class='middle']")); // месяц выработки
                ChangeMonth(monthSelectElement, DateTime.Now.Month);

                // дата срока годности
                DateTime tomorrowDay = DateTime.Today.AddDays(1);
                dayInputElement =  FindElement("//table[@class='innerForm']/tbody/tr[@id='BEST_BEFORE_DATE']/td/div/span/input[@type='text']");   // день просрочки
                ChangeDay(dayInputElement, tomorrowDay.Day);
                monthSelectElement = new SelectElement(FindElement("//tr[@id='BEST_BEFORE_DATE']/td/div/span/select[@class='middle']")); // месяц просрочки
                ChangeMonth(monthSelectElement, tomorrowDay.Month);

                FindElement("//button[@class='positive']").Click(); // сохранить
                Thread.Sleep(1500);
                FindElement("//button[@onclick='javascript:doAction(\"transactionShowForm\", \"generateTransactionFromTemplate\", \"\")']").Click(); // создать транзакцию
                FindElement("//button[@class='positive']").Click(); // оформить
                chromeDriver.SwitchTo().Alert().Accept(); // подтверждение
                ConsoleTextColor.GreenConsoleOutput(milkman.Name + new string(' ', maxWord+5-milkman.Name.Length) + " успех");
            }
            catch {
                ConsoleTextColor.RedConsoleOutput(milkman.Name + new string(' ', maxWord+5-milkman.Name.Length) +" ошибка");
            }
            finally {
                FindElement("//a[@href='operatorui?_action=changeServicedEnterprise']").Click();
            }

        }


        private IWebElement FindElement(string xPath) {
            return chromeDriver.FindElement(By.XPath(xPath));
        }

        private void ChangeDay(IWebElement dayInputElement, int day) {
            dayInputElement.Clear();
            dayInputElement.SendKeys(day.ToString());
        }

        private void ChangeMonth(SelectElement selectElement, int month) {
            selectElement.SelectByValue((--month).ToString());  // декреминтируем 'month', чтобы соответсвовала индексу JavaScript
        }


        private bool CheckDoctorName(string doctorIdName) {
            return FindElement("//div[@id='loggedas']/b").Text.Contains(doctorIdName);
        }

    }
}