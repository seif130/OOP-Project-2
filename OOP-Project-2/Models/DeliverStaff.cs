using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class DeliverStaff
    {
        public int StaffId { get; set; }

        public string FullName { get; set; }

        public string VehicleType { get; set; }

        public string LicenceNumber { get; set; }

        public string AssignedArea { get; set; }

        public int BranchId { get; set; }

        public bool IasAvailable { get; set; }
    }
}
