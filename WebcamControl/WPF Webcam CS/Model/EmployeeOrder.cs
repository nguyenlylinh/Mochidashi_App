using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Webcam_CS.Model
{
    public class EmployeeOrder
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string AccountID { get; set; }
        public int EquipmentID { get; set; }
        public DateTime ReturnCompleteDate { get;set;}
        public DateTime EstimateReturnDate { get; set; }
        public DateTime ReturnedDate { get; set; }
        public string Comment { get; set; }

    }
}
