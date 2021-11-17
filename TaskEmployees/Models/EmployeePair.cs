using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskEmployees.Models
{
    public class EmployeePair
    {
        public int EmpID1 { get; set; }
        public int EmpID2 { get; set; }
        public int ProjectID { get; set; }       
        public int DaysWorked { get; set; }
    }
}
