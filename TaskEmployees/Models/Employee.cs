using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskEmployees.Models
{
    public class Employee
    {
        public int EmpID { get; set; }
        public int ProjectID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int DaysWorked { get; set; }
    }
}
