using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Webcam_CS
{
    /// <summary>
    /// CustomizeDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomizeDialog : Window
    {
        //はい・いいえボタンを押した場合、戻る変数
        int returnValue = 0;
        public CustomizeDialog(string message)
        {
            InitializeComponent();

            txtMessage.Text = message;

        }


        public int getReturnValue()
        {
            return returnValue;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            returnValue = 1;
            DialogResult = true;
            this.Close();
        }

    }
}
