using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;


namespace ConsoleProject {
    public class Program {

        static void Main(string[] args) {
            using(var db = new DatabaseContext()){
                db.Database.Migrate();
            }
            Repository repository = new Repository();
            BotClass botClass = new BotClass("/home/codename/Visual_Studio_Project/ConsoleProject", repository);
            ShowLogo();
            botClass.Start();
            botClass.Stop();
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
    }
}
