using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using TaskEmployees.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskEmployees
{
    public class HomeController : Controller
    {
        public static List<EmployeePair> employeePairList = new List<EmployeePair>();
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoadData()
        {
            try
            {
                var result = new { data = employeePairList };
                return Json(result);
            }
            catch (Exception e)
            {
                return new EmptyResult();
            }
        }

        private Dictionary<List<int>, Dictionary<int, int>> MapPairsToDaysWorkedTogether(Dictionary<int, List<Employee>> projectToEmployees)
        {
            Dictionary<List<int>, Dictionary<int, int>> employeePairToProjectWorkedTogether = new Dictionary<List<int>, Dictionary<int, int>>();
            
            foreach (var project in projectToEmployees)
            {
                List<Employee> employees = project.Value;

                for (int i = 0; i < employees.Count; i++)
                {
                    for (int j = i + 1; j < employees.Count; j++)
                    {
                        List<int> keys = new List<int> { employees[i].EmpID, employees[j].EmpID };
                        if (employeePairToProjectWorkedTogether.ContainsKey(keys))
                        {
                            var projectToDaysWorkedTogether = employeePairToProjectWorkedTogether[keys];
                            projectToDaysWorkedTogether[project.Key] = CalculateDaysWorkedTogether(employees[i], employees[j]);
                        }
                        else
                        {
                            var projectToDaysWorkedTogether = new Dictionary<int, int>();
                            projectToDaysWorkedTogether[project.Key] = CalculateDaysWorkedTogether(employees[i], employees[j]);
                            employeePairToProjectWorkedTogether.Add(keys, projectToDaysWorkedTogether);                            
                        }
                    }

                }
            }

            return employeePairToProjectWorkedTogether;
        }

        private List<int> GetPairWorkedLongestTogether(Dictionary<List<int>, Dictionary<int, int>> employeePairToProjectWorkedTogether)
        {
            var maxDays = 0;
            List<int> maxPair = new List<int>();
            foreach(var pair in employeePairToProjectWorkedTogether)
            {
                var projectToDaysWorked = pair.Value;
                var pairDays = projectToDaysWorked.Sum(x => x.Value);
                if (pairDays > maxDays)
                {
                    maxDays = pairDays;
                    maxPair = pair.Key;
                }
            }
            return maxPair;
        }

        public IActionResult UploadFile(IFormFile file)
        {
            if (file.Length > 0)
            {
                employeePairList.Clear();
                var projectToEmployees = ReadFileAndPopulateEmployees(file);
                var pairToDaysWorkedTogether = MapPairsToDaysWorkedTogether(projectToEmployees);
                List<int> pairWorkedLongest = GetPairWorkedLongestTogether(pairToDaysWorkedTogether);
                if (pairWorkedLongest.Count > 0)
                {
                    PrepareEmployeePairForDisplay(pairWorkedLongest, pairToDaysWorkedTogether[pairWorkedLongest]);
                }
                ViewData["grid"] = "show";
                return PartialView("Index");
            }
            return NotFound();
        }

        private Dictionary<int, List<Employee>> ReadFileAndPopulateEmployees(IFormFile file)
        {
            var result = new StringBuilder();
            Dictionary<int, List<Employee>> projectToEmployees = new Dictionary<int, List<Employee>>();
            using (StreamReader reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                    var employeeRecord = result.ToString();
                    string[] emp = employeeRecord.Trim().Split(',');

                    Employee employee = new Employee()
                    {
                        EmpID = Int32.Parse(emp[0].Trim()),
                        ProjectID = Int32.Parse(emp[1].Trim()),
                        DateFrom = DateTime.Parse(emp[2].Trim()),
                        DateTo = ParseDateTo(emp[3].Trim())
                    };
                    AddEmployeeToProject(projectToEmployees, employee);
                    result.Clear();
                }
                return projectToEmployees;
            }
        }
        private DateTime ParseDateTo(string dateTo)
        {
            DateTime dateToDate = DateTime.Today;
            if (dateTo != "NULL")
            {
                dateToDate = Convert.ToDateTime(dateTo);
            }
            return dateToDate;
        }

        private void AddEmployeeToProject(Dictionary<int, List<Employee>> projectToEmployees, Employee employee)
        {
            if (projectToEmployees.ContainsKey(employee.ProjectID))
            {
                projectToEmployees[employee.ProjectID].Add(employee);
            }
            else
            {
                projectToEmployees.Add(employee.ProjectID, new List<Employee>());
                projectToEmployees[employee.ProjectID].Add(employee);
            }
        }



        private int CalculateDaysWorkedTogether(Employee first, Employee second)
        {
            var startDate = (first.DateFrom > second.DateFrom ? first.DateFrom : second.DateFrom);
            var endDate = (first.DateTo > second.DateTo ? second.DateTo : first.DateTo);
            if (startDate < endDate)
            {
                var workdays = (int)(endDate - startDate).TotalDays;
                return workdays;
            }
            else
            {
                return 0;
            }
        }

        public void PrepareEmployeePairForDisplay(List<int> employeerPair, Dictionary<int, int> projectToDaysWorked)
        {
           foreach(var project in projectToDaysWorked)
            {
                employeePairList.Add(new EmployeePair()
                {
                    EmpID1 = employeerPair[0],
                    EmpID2 = employeerPair[1],
                    ProjectID = project.Key,
                    DaysWorked = project.Value
                });
            }
        }
    }


}
