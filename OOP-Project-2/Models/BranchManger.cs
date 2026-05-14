using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class BranchManger : Employee
    {
        public BranchManger() { Position = "Branch Manager"; }
        public override string GetRole()
        {
            return "Branch Manager";
        }
    }
}
