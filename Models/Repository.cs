using System.Collections.Generic;
using System.Linq;


namespace ConsoleProject.Models {

    public class Repository : IRepository {
        public IList<Milkman> GetMilkmen(){
           using(var db = new DatabaseContext()) {
               return db.Milkmen.ToList();
           }
        }

        public IList<DoctorAccount> GetDoctorAccounts() {
            using(var db = new DatabaseContext()) {
                return db.DoctorAccounts.ToList();
            }
        }
    }
}