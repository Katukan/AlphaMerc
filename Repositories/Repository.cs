using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleProject.DataRepositories {

    public class Repository : IRepository {

        DatabaseContext databaseContext;
        IList<Milkman> allMilkmen;
        IList<DoctorAccount> doctorAccounts;

        public Repository() {
            databaseContext = new DatabaseContext();
        }

        public IList<Milkman> AllMilkmen() {
            return databaseContext.Milkmen.ToList();
        }

        public IList<Milkman> CurrentContextMilkmen(Func<string, bool> checkingDoctorName) {
            allMilkmen = AllMilkmen().Where(m => checkingDoctorName(m.DoctorIdName)).ToList();
            return allMilkmen;
        }

        public IList<Milkman> IncludedMilkmen() {
            return allMilkmen?.Where(m => m.Exclud != 1).ToList();
        }

        public IList<Milkman> ExcludMilkmen() {
            return allMilkmen?.Where(m => m.Exclud == 1).ToList();
        }

        public IList<DoctorAccount> DoctorAccounts() {
            doctorAccounts = databaseContext.DoctorAccounts.ToList();
            return doctorAccounts;
        }

        public DoctorAccount FindDoctorAccount(int idDoctorAccount) {
            return doctorAccounts.First(d => d.Id == idDoctorAccount);
        }

        public void DatabaseMigrate() {
            using(var db = new DatabaseContext()){
                db.Database.Migrate();
            }
        }

        
    }
}