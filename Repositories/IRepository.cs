using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleProject.Models;

namespace ConsoleProject.DataRepositories {

    public interface IRepository {
        IList<Milkman> AllMilkmen();

        IList<Milkman> CurrentContextMilkmen(Func<string, bool> checkingDoctorName);

        IList<Milkman> IncludedMilkmen();

        IList<Milkman> ExcludMilkmen();

        IList<DoctorAccount> DoctorAccounts();

        DoctorAccount FindDoctorAccount(int idDoctorAccount);

        void DatabaseMigrate();
    }
}