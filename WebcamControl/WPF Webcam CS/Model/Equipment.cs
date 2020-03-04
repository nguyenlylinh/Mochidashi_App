using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Webcam_CS.Model
{
    public class Equipment
    {
        public int EquipmentID { get; set; }
        public string RFIDTagNumber { get; set; }
        public string EquipmentName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int EquipStatus { get; set; }
    }
}
