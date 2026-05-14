using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Waiter : Employee
    {
        public Waiter() { Position = "Waiter"; }


        public override string GetRole()
        {
            return "Waiter";
        }
    }
}
