using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;


namespace ConsoleProject {
    public class Program {

        static void Main(string[] args) {
            using(var db = new DatabaseContext()){
                db.Database.Migrate();
            }
            Repository repository = new Repository();
            BotClass botClass = new BotClass(Directory.GetCurrentDirectory(), repository);
            ShowLogo();
            botClass.Start();
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
