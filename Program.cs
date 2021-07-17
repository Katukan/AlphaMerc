using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;


namespace ConsoleProject {
    class Program {

        static void Main(string[] args) {
            using(var db = new DatabaseContext()){
                db.Database.Migrate();
            }
            Repository repository = new Repository();
            BotClass botClass = new BotClass("/home/codename/Visual_Studio_Project/ConsoleProject", repository);
            Console.WriteLine("AlphaMerc");
            botClass.Start();
        }
    }
}
