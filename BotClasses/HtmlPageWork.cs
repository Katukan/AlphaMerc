using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ConsoleProject.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ConsoleProject.BotClasses {
    public class HtmlPageWork {
        IWebDriver browserDriver;
        BotSettings botSettings;
        ConsoleWork consoleWork;

        public HtmlPageWork(IWebDriver browserDriver, BotSettings botSettings, ConsoleWork consoleWork) {
            this.browserDriver = browserDriver;
            this.botSettings = botSettings;
            this.consoleWork = consoleWork;
        }
        
        public void SubmittingAuthorizationForm(string login, string password) {
            FindElementByXpath(botSettings.userNameInputElementXpath).SendKeys(login);
            FindElementByXpath(botSettings.passwordInputElementXpath).SendKeys(password);
            FindElementByXpathAndClick(botSettings.authorizationSubmitButtonXpath);
        }

        public IWebElement FindElementByXpath(string xPath) {
            return browserDriver.FindElement(By.XPath(xPath));
        }

        public void FindElementByXpathAndClick(string xPath) {
            FindElementByXpath(xPath).Click();
        }

        public void PaperWorkForIncludedMilkmen(int longestMilkmanName, IList<Milkman> includedMilkmen) {
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
                consoleWork.ShowSuccesfulStatus(milkman, longestMilkmanName);
            }
            catch {
                consoleWork.ShowErrorStatus(milkman, longestMilkmanName);
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

        private void ChangeDay(IWebElement dayInputElement, int day) {
            dayInputElement.Clear();
            dayInputElement.SendKeys(day.ToString());
        }

        private void ChangeMonth(SelectElement selectElement, int month) {
            int monthIndexInJavaScript = --month; 
            selectElement.SelectByValue((monthIndexInJavaScript).ToString());  
        }

        private void AcceptJavaScriptAlert() {
            browserDriver.SwitchTo().Alert().Accept();
        }

        private void ShowMilkmenListInHtmlPage() {
            Thread.Sleep(botSettings.millisecondsTimeout);
            FindElementByXpathAndClick(botSettings.changeServicedEnterpriseLink);
        }
    }
}