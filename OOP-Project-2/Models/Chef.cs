using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Chef : Employee
    {

        public Chef() { Position = "Chef"; }


        public override string GetRole()
        {
            return "Chef";
        }
    }
}
