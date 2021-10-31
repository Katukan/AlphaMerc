using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleProject.Models;

namespace ConsoleProject.BotClasses {
    public class ConsoleWork {
        
        public void ShowDoctorAccounts(IList<DoctorAccount> doctorAccounts) {
            Console.WriteLine("Доступные аккаунты:");
            foreach(var d in doctorAccounts) {
                Console.WriteLine($"{d.Id}) {d.Name}");
            }
        }

        public void ShowMilkmen(IList<Milkman> milkmen, bool isYellowConsoleText = false) {
            if(isYellowConsoleText) {
                ConsoleSettings.ColoringCurrentTextToYellow();
            }
            for(int i = 0; i < milkmen.Count(); i++) {
                Console.WriteLine($"{i + 1}) {milkmen[i].Name} ");
            }
            if(isYellowConsoleText) {
                ConsoleSettings.ColoringCurrentTextToGray();
            }
        }

        public void ShowExcludMilkmen(IList<Milkman> excludMilkmen) {
            if(excludMilkmen.Count() > 0) {
                ConsoleSettings.ColoringInYellow("Следующие ЛПХ не будут оформляться:");
                ShowMilkmen(excludMilkmen, true);
            }
        }

        public void DrawLine(int longestMilkmanName) {
            Console.WriteLine(new string('-', longestMilkmanName + 5));
        }

        public void DrawTableHead(int longestMilkmanName) {
            Console.ReadLine();
            Console.WriteLine(" Name " + new string(' ', longestMilkmanName) + "Status");
            Console.WriteLine(new string('-', longestMilkmanName + 3) + ' ' + new string('-', 10));
        }

        public void ShowSuccesfulStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleSettings.ColoringInGreen(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) + " успех");
        }

        public void ShowErrorStatus(Milkman milkman, int longestMilkmanName) {
            ConsoleSettings.ColoringInRed(milkman.Name + new string(' ', longestMilkmanName+5-milkman.Name.Length) +" ошибка");
        }
    }
}