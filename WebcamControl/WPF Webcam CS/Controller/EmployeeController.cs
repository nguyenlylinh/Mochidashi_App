using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_Webcam_CS.Model;

namespace WPF_Webcam_CS.Controller
{
    public class EmployeeController
    {
        public Employee getEmployeeByAccountID(string accountIDParam)
        {
            Employee emp = new Employee();
            //DBへ接続する
            try
            {
                SqlConnection thisConnection = new SqlConnection(@"Server=DESKTOP-88RB75M;Database=Setsukan_SetsubiKanri;Trusted_Connection=Yes;");
                thisConnection.Open();

                string Get_Data = "select dbo.Employee.AccountID, dbo.Employee.EmployeeName,dbo.Department.DepartmentName,dbo.Section.SectionName,dbo.Project.ProjectName  from dbo.Employee " +
                                    "left join dbo.Department on dbo.Employee.DepartmentID = dbo.Department.DepartmentID " +
                                    "left join dbo.Section on dbo.Employee.SectionID= dbo.Section.DepartmentID " +
                                    "left join dbo.Project on dbo.Employee.ProjectID = dbo.Project.ProjectID " +
                                     "where AccountID  like '%" + accountIDParam + "%'";

                SqlCommand cmd = thisConnection.CreateCommand();
                cmd.CommandText = Get_Data;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Employee");
                sda.Fill(dt);
                int count = dt.Rows.Count;
                if (count >= 1)
                {
                    emp.AccountID = dt.Rows[0]["AccountID"].ToString();
                    emp.DepartmentName = dt.Rows[0]["DepartmentName"].ToString();
                    emp.SectionName = dt.Rows[0]["SectionName"].ToString();
                    emp.ProjectName = dt.Rows[0]["ProjectName"].ToString();
                    emp.EmployeeName = dt.Rows[0]["EmployeeName"].ToString();
                }
                else
                {
                    MessageBoxResult checkNotice = MessageBox.Show("アカウント名を見つけなかった! \n正しいアカウント名を入力してください！", "エラーメッセージ", MessageBoxButton.OK);
                }

            }
            catch
            {
                MessageBoxResult checkNotice = MessageBox.Show("データベースへ接続できない!", "エラーメッセージ", MessageBoxButton.OK);
                return emp;
            }
            return emp;
        }
    }
}
