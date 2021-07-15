using System.Collections.Generic;
using System.Linq;

namespace ConsoleProject.Models {

    public interface IRepository {
        IList<Milkman> GetMilkmen();

        IList<DoctorAccount> GetDoctorAccounts();
    }
}