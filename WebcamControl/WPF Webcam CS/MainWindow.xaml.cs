using System.Windows;
using Microsoft.Expression.Encoder.Devices;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using WPF_Webcam_CS.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RsClassDll;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text;

namespace WPF_Webcam_CS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        // RFID関連の変数
        private List<string> ontag = new List<string>();        //現在シートに載っているタグID格納
        private List<string> lastontag = new List<string>();    //前回の読み取りでシートに載っていたタグIDを格納
        object g_api_lockObject = new object();
        string g_read_cmd_header = "/reader/inventory/01";
        RsClass rc = new RsClass();

        DispatcherTimer dispatcherTimer;
        DispatcherTimer dispatcherTimerRestart;
        bool startFlg = true;                                   //初回フラグ


        //public EmployeeController empController;
        public Collection<EncoderDevice> VideoDevices { get; set; }
        public Collection<EncoderDevice> AudioDevices { get; set; }

        //貸出または返却状態
        private int isBorrow　= -1;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            resetTextBox();
            AccountName.Text = "";

            VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
            TakePhotoBtn.IsEnabled = false;
            StopCameraBtn.IsEnabled = false;

            // RFID読み込みの開始
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            // Restart時のイベント設定
            dispatcherTimerRestart = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimerRestart.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerRestart.Tick += new EventHandler(dispatcherTimer_RestartTick);

            rc.InitHttpApi("COM3");
            string text = "";

            rc.ProcessHttpApi("/reader/module/004reset", ref text);//リーダ設定リセット
            rc.ProcessHttpApi("/reader/antenna/1/008setconfig?antennastatus=1", ref text);//アンテナ有効化
            rc.ProcessHttpApi("/reader/antenna/common/006setconfig?session=0", ref text);//「タグは可能な限り応答を返す」ように設定
            rc.ProcessHttpApi("/reader/antenna/common/filter/1/010setconfig?f_start_bit=32&f_masklen_bit=16", ref text);//フィルター

            text = send_http_message(g_read_cmd_header + "3start");

            if (json_get(text, "response") == "0")
            {
                dispatcherTimer.Start();
            }
        }

        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Display webcam video
                WebcamViewer.StartPreview();
                TakePhotoBtn.IsEnabled = true;
                StopCameraBtn.IsEnabled = true;

            }
            catch (Microsoft.Expression.Encoder.SystemErrorException ex)
            {
                CustomizeDialog dialog = new CustomizeDialog("カメラが他のアプリケーションに利用されています!");
                dialog.ShowDialog();
            }
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            WebcamViewer.Width = 400;
            WebcamViewer.Height = 400;
           
            // Stop the display of webcam video.
            WebcamViewer.StopPreview();
            TakePhotoBtn.IsEnabled = false;
            StopCameraBtn.IsEnabled = false;
        }

        private void fillInTextbox(Employee empParam)
        {
            if (empParam.AccountID != null & empParam.AccountID != "")
            {
                AccountNameToSave.Text = empParam.AccountID;
                AccountName.Text = empParam.AccountID; 
                EmpName.Text = empParam.AccountID;
                DepartmentName.Text = empParam.DepartmentName;
                SectionName.Text = empParam.SectionName;
                ProjectName.Text = empParam.ProjectName;
                EmpName.Text = empParam.EmployeeName;
            }
            else
            {
                resetTextBox();
            }
        }

        private List<string> AnalysisResult(string originalResult)
        {

            string[] resultsArray = originalResult.Split(',');

            List<string> finalResults = new List<string>(resultsArray.Length);

            finalResults.AddRange(resultsArray);
            //  finalResults.Reverse();

            return finalResults;

        }
        private void TakeSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Take snapshot of webcam video.
            var filepath = WebcamViewer.TakeSnapshot();

            //VisualStudioが利用しているインタープリターのパス
            var pythonInterpreterPath = @"C:\RFIDAIsrc\LocalPredict\env\Scripts\python.exe";

            //「1. Python側を実装」にて保存したスクリプトのパス
            var pythonScriptPath = @"C:\RFIDAIsrc\LocalPredict\LocalPredict.py";

            var arguments = new List<string>
             {
            pythonScriptPath ,
            filepath   //第1引数
              };

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(pythonInterpreterPath)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = string.Join(" ", arguments),
                },
            };

            process.Start();

            //python側でprintした内容を取得
            var sr = process.StandardOutput;

            process.WaitForExit();

            var result = sr.ReadLine().ToString();

            var finalResult = AnalysisResult(result);

            process.Close();
            if (true)
            {
                //MessageBoxResult resultNotice;
                //認証できた場合
                if (finalResult.Count > 1)
                {
                    ImageSource imageSource = new BitmapImage(new Uri(filepath));
                    FaceConfirmResultDialog resultDialog = new FaceConfirmResultDialog(finalResult[0]);

                    float percentage = float.Parse(finalResult[1]) * 100;
                    
                    resultDialog.userPercentage.Text = percentage + "%";
                    resultDialog.userImage.Source = imageSource;
                   
                    if ((bool)resultDialog.ShowDialog())
                    {
                        //カメラを閉じる
                        TakePhotoBtn.IsEnabled = false;
                        StopCameraBtn.IsEnabled = false;
                        WebcamViewer.StopPreview();

                        //ユーザー情報を表示する
                        Employee emp = findEmployeeByAccountID(finalResult[0]);
                        fillInTextbox(emp);
                    }
                    //resultNotice = MessageBox.Show("あなたが" + finalResult[0] + "になる可能性は" + finalResult[1] + "です\n よろしでしょうか？", "写真認証結果メッセージ", MessageBoxButton.YesNo);
                    //switch (resultNotice)
                    //{
                    //    //もらった結果を使う
                    //    case MessageBoxResult.Yes:

                    //        //カメラを閉じる
                    //        TakePhotoBtn.IsEnabled = false;
                    //        StopCameraBtn.IsEnabled = false;
                    //        WebcamViewer.StopPreview();
                    //        //ユーザー情報を表示する
                    //        ImageSource imageSource = new BitmapImage(new Uri(filepath));
                    //        EmpImage.Source = imageSource;

                    //        WebcamViewer.Width = 0;
                    //        WebcamViewer.Height = 0;
                    //        //WebcamViewer.Visibility = 0;

                    //        Employee emp = findEmployeeByAccountID(finalResult[0]);
                    //        fillInTextbox(emp);
                    //        break;
                    //    //もらった結果を使わない
                    //    case MessageBoxResult.No:
                    //        break;
                    //}
                }
                //認証できなかった場合
                else
                {
                    CustomizeDialog dialog = new CustomizeDialog("認職できない! \n 写真を再度撮ってください!");
                    dialog.ShowDialog();
                }

            }

        }
        private bool validateAccountName(string accountParam)
        {
            if (accountParam.Trim().Length == 0)
            {
                CustomizeDialog dialog = new CustomizeDialog("アカウント名を入力してください！");
                dialog.ShowDialog();
                return false;
            }
            else
            {
                if (accountParam.Trim().Length > 20)
                {
                    CustomizeDialog dialog = new CustomizeDialog("アカウント名を20文字・数字以下入力してください！");
                    dialog.ShowDialog();
                    return false;
                }
                else if (accountParam.Contains("[") || accountParam.Contains("&") || accountParam.Contains("<") || accountParam.Contains(">") || accountParam.Contains('"') || accountParam.Contains("'") || accountParam.Contains("]"))
                {
                    CustomizeDialog dialog = new CustomizeDialog("アカウント名に文字と数字のみを入力してください！");
                    dialog.ShowDialog();
                    return false;
                }
            }
            return true;
        }
        //デッキストボックスのデータを削除する
        private void resetTextBox()
        {
            AccountNameToSave.Text = "";
            DepartmentName.Text = "";
            SectionName.Text = "";
            ProjectName.Text = "";
            EmpName.Text = "";
        }
        //ユーザー情報を探すメソッド
        private Employee findEmployeeByAccountID(string accountParam)
        {
            
            Employee emp = new Employee();
            //DBへ接続する
            try
            {
               //SqlConnection thisConnection = new SqlConnection(@"Server=DESKTOP-88RB75M;Database=Setsukan_SetsubiKanri5;Trusted_Connection=Yes;");
                SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                thisConnection.Open();

                string Get_Data = "select dbo.Employee.AccountID, dbo.Employee.EmployeeName,dbo.Department.DepartmentName,dbo.Section.SectionName,dbo.Project.ProjectName  from dbo.Employee " +
                                    "left join dbo.Department on dbo.Employee.DepartmentID = dbo.Department.DepartmentID " +
                                    "left join dbo.Section on dbo.Employee.SectionID= dbo.Section.DepartmentID " +
                                    "left join dbo.Project on dbo.Employee.ProjectID = dbo.Project.ProjectID " +
                                     "where AccountID  = '" + accountParam + "'";

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
                    CustomizeDialog dialog = new CustomizeDialog("アカウント名を見つけなかった! \n正しいアカウント名を入力してください！");
                    dialog.ShowDialog();

                }

            }
            catch
            {
                CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない!");
                dialog.ShowDialog();
                return emp;
            }
            return emp;
        }

        //検索ボタンを押す
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            resetTextBox();
            string accountInput = AccountName.Text.Trim();
            if (validateAccountName(accountInput))
            {
                Employee emp =  findEmployeeByAccountID(accountInput);
                fillInTextbox(emp);
            }     
        }
        //注文状態を保存する
        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            //貸出
            if (isBorrow == 1)
            {
                if (DepartmentName.Text != null & DepartmentName.Text != "")
                {
                    EmployeeOrder empOrderParam = new EmployeeOrder();
                    empOrderParam.AccountID = AccountNameToSave.Text;
                    if (EquipmentIdTbl.Text == null || EquipmentIdTbl.Text == "")
                    {
                        CustomizeDialog dialog = new CustomizeDialog("取られた設備の情報が削除された！");
                        dialog.ShowDialog();
                    }
                    else
                    {
                        SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                        using (thisConnection)
                        {
                            try
                            {
                                //sql実施結果
                                int result = 0;

                                //integerフォーマット変換する
                                int intequipmentID = int.Parse(EquipmentIdTbl.Text);

                                empOrderParam.EquipmentID = intequipmentID;
                                empOrderParam.OrderDate = DateTime.Now;

                                //sqlcommand
                                List<string> sqlCommandList = new List<string>();

                                // insert command
                                string sql1 = "INSERT INTO dbo.EmployeeOrder ("
                                    + " AccountID "
                                    + " ,EquipmentID "
                                    + " ,OrderDate "
                                    + " ) "
                                    + " VALUES ( "
                                    + " '" + empOrderParam.AccountID + "' "
                                    + " ," + empOrderParam.EquipmentID
                                    + " ,'" + empOrderParam.OrderDate.ToString() + "'"
                                + " )";
                                sqlCommandList.Add(sql1);
                                   


                                // update command
                                string sql2 =
                                            "UPDATE dbo.Equipment "
                                            + " SET EquipStatus = " + 0
                                            + " WHERE EquipmentID = " + empOrderParam.EquipmentID;

                                sqlCommandList.Add(sql2);

                                thisConnection.Open();

                                using (SqlTransaction trans = thisConnection.BeginTransaction())
                                {
                                    using (SqlCommand command = new SqlCommand("", thisConnection, trans))
                                    {
                                        command.CommandType = System.Data.CommandType.Text;

                                        foreach (var commandString in sqlCommandList)
                                        {
                                            command.CommandText = commandString;
                                            result = command.ExecuteNonQuery();
                                        }
                                    }
                                    if (result > 0)
                                    {
                                        trans.Commit();
                                        SuccessDialog dialog = new SuccessDialog("注文が完了しました！");
                                        dialog.ShowDialog();
                                        AccountName.Text = "";
                                        AccountNameToSave.Text = "";
                                        resetTextBox();
                                        resetEquipmentTable();
                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        CustomizeDialog dialog = new CustomizeDialog("貸出情報を登録することが失敗しました！\n再度登録してください!");
                                        dialog.ShowDialog();
                                    }

                                }

                                send_http_message(g_read_cmd_header + "3start");//インベントリ開始
                            }
                            

                            catch (FormatException) //設備IDのフォーマットエラー発生
                            {
                                CustomizeDialog dialog = new CustomizeDialog("その設備IDフォーマットが登録されていない！");
                                dialog.ShowDialog();
                            }
                            catch (Exception) //DBへ接続できない
                            {
                                CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                                dialog.ShowDialog();
                            }
                        }

                        //using (thisConnection)
                        //{
                        //    try
                        //    {
                        //        intequipmentID = int.Parse(EquipmentIdTbl.Text);

                        //        empOrderParam.EquipmentID = intequipmentID;
                        //        empOrderParam.OrderDate = DateTime.Now;

                        //        //sql実施結果
                        //        int result;

                        //        // 貸出情報を登録
                        //        string insertSql =
                        //            "INSERT INTO dbo.EmployeeOrder ("
                        //            + " AccountID "
                        //            + " ,EquipmentID "
                        //            + " ,OrderDate "
                        //            + " ) "
                        //            + " VALUES ( "
                        //            + " '" + empOrderParam.AccountID + "' "
                        //            + " ," + empOrderParam.EquipmentID
                        //            + " ,'" + empOrderParam.OrderDate.ToString() + "'"
                        //        + " )";

                        //        thisConnection.Open();

                        //        SqlCommand insertCmd = thisConnection.CreateCommand();
                        //        insertCmd.CommandText = insertSql;

                        //        SqlDataAdapter sda = new SqlDataAdapter(insertCmd);
                        //        sda.InsertCommand = insertCmd;
                        //        result = sda.InsertCommand.ExecuteNonQuery();

                        //        // 設備情報のステータスを貸し出しに変更
                        //        string updateSql =
                        //            "UPDATE dbo.Equipment "
                        //            + " SET EquipStatus = " + 0
                        //            + " WHERE EquipmentID = " + empOrderParam.EquipmentID;

                        //        SqlCommand updateCmd = thisConnection.CreateCommand();
                        //        updateCmd.CommandText = updateSql;

                        //        SqlDataAdapter updateSqlDataAdapter = new SqlDataAdapter(updateCmd);
                        //        updateSqlDataAdapter.UpdateCommand = updateCmd;
                        //        result = updateSqlDataAdapter.UpdateCommand.ExecuteNonQuery();

                        //        if (result > 0)
                        //        {

                        //            MessageBoxResult checkNotice = MessageBox.Show("注文が完了しました!", "成功メッセージ", MessageBoxButton.OK);
                        //            AccountName.Text = "";
                        //            AccountNameToSave.Text = "";
                        //            resetTextBox();
                        //            resetEquipmentTable();
                        //        }
                        //        else
                        //        {
                        //            CustomizeDialog dialog = new CustomizeDialog("貸出情報を登録することが失敗しました!");
                        //            dialog.ShowDialog();
                        //        }

                        //    }
                        //    catch (FormatException)
                        //    {
                        //        CustomizeDialog dialog = new CustomizeDialog("その設備IDフォーマットが登録されていない！");
                        //        dialog.ShowDialog();
                        //    }

                        //}


                    }

                }
                else
                {
                    CustomizeDialog dialog = new CustomizeDialog("ユーザー情報を入力してください!");
                    dialog.ShowDialog();
                }
            }
            //返却
            else if (isBorrow == 0)
            {
                
                    if (OrderIDTbl.Text != null && OrderIDTbl.Text != "" && EquipmentIdTbl.Text != null && EquipmentIdTbl.Text != "")
                    {
                        SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                        using (thisConnection)
                        {
                            try
                            {
                                //新規ユーザー定義する
                                EmployeeOrder empOrderParam = new EmployeeOrder();

                                //sql実施結果
                                int result = 0;

                                //integerフォーマット変換する
                                int intOrderID = int.Parse(OrderIDTbl.Text);
                                int intequipmentID = int.Parse(EquipmentIdTbl.Text);

                                empOrderParam.OrderID = intOrderID;
                                empOrderParam.ReturnedDate = DateTime.Now;
                                empOrderParam.EquipmentID = intequipmentID;

                                //sqlcommand
                                List<string> sqlCommandList = new List<string>();

                                //update command1
                                string sql1 =
                                    "UPDATE dbo.EmployeeOrder "
                                    + " SET ReturnedDate = '" + empOrderParam.ReturnedDate.ToString() + "'"
                                    + " WHERE EquipmentID = " + empOrderParam.EquipmentID
                                    + " AND OrderID = " + empOrderParam.OrderID;

                               sqlCommandList.Add(sql1);

                                //update command2
                                string sql2 =
                                    "UPDATE dbo.Equipment "
                                    + " SET EquipStatus = " + 1
                                    + " WHERE EquipmentID = " + empOrderParam.EquipmentID;

                                sqlCommandList.Add(sql2);

                            thisConnection.Open();

                                using (SqlTransaction trans = thisConnection.BeginTransaction())
                                {
                                    using (SqlCommand command = new SqlCommand("", thisConnection, trans))
                                    {
                                        command.CommandType = System.Data.CommandType.Text;

                                        foreach (var commandString in sqlCommandList)
                                        {
                                            command.CommandText = commandString;
                                            result = command.ExecuteNonQuery();
                                        }
                                    }
                                    if (result > 0)
                                    {
                                        trans.Commit();
                                        SuccessDialog dialog = new SuccessDialog("返却が完了しました！");
                                        dialog.ShowDialog();
                                        AccountName.Text = "";
                                        AccountNameToSave.Text = "";
                                        resetEquipmentTable();
                                        resetTextBox();
                                }
                                else
                                    {
                                        trans.Rollback();
                                        CustomizeDialog dialog = new CustomizeDialog("返却情報を登録することが失敗しました！\n再度登録してください!");
                                        dialog.ShowDialog();
                                    }

                                }
                            }
                            catch (FormatException) //注文IDのフォーマットエラー発生
                            {
                                CustomizeDialog dialog = new CustomizeDialog("その注文IDまたは設備IDフォーマットが登録されていない！");
                                dialog.ShowDialog();
                            }
                            catch (Exception) ////DBへ接続できない
                            {
                                CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない!");
                                dialog.ShowDialog();
                            }
                        }
                    }
                    else
                    {
                        CustomizeDialog dialog = new CustomizeDialog("乗せられた設備の情報が削除された!");
                        dialog.ShowDialog();
                    }

                    //EmployeeOrder empOrderParam = new EmployeeOrder();

                    //int intOrderID = 0;
                    //int intequipmentID = 0;
                    //try
                    //{
                    //    intOrderID = int.Parse(OrderIDTbl.Text);
                    //    intequipmentID = int.Parse(EquipmentIdTbl.Text);
                    //}
                    //catch (Exception)
                    //{
                    //    CustomizeDialog dialog = new CustomizeDialog("その設備の貸出時間が登録されていない!");
                    //    dialog.ShowDialog();
                    //}
                    //empOrderParam.OrderID = intOrderID;
                    //empOrderParam.ReturnedDate = DateTime.Now;
                    //empOrderParam.EquipmentID = intequipmentID;

                    //SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                    //thisConnection.Open();

                    ////sql実施結果
                    //int result;

                    //// 返却情報を登録
                    //string updateSql =
                    //    "UPDATE dbo.EmployeeOrder "
                    //    + " SET ReturnedDate = '" + empOrderParam.ReturnedDate.ToString() + "'"
                    //    + " WHERE EquipmentID = " + empOrderParam.EquipmentID
                    //    + " AND OrderID = " + empOrderParam.OrderID;

                    //SqlCommand updateCmd = thisConnection.CreateCommand();
                    //updateCmd.CommandText = updateSql;

                    //SqlDataAdapter updateSqlDataAdapter = new SqlDataAdapter(updateCmd);
                    //updateSqlDataAdapter.UpdateCommand = updateCmd;
                    //result = updateSqlDataAdapter.UpdateCommand.ExecuteNonQuery();

                    //// 設備情報のステータスを返却に変更
                    //string updateEquipmentSql =
                    //    "UPDATE dbo.Equipment "
                    //    + " SET EquipStatus = " + 1
                    //    + " WHERE EquipmentID = " + empOrderParam.EquipmentID;

                    //SqlCommand updateEquipmentCmd = thisConnection.CreateCommand();
                    //updateEquipmentCmd.CommandText = updateEquipmentSql;

                    //SqlDataAdapter updateEquipmentSqlDataAdapter = new SqlDataAdapter(updateEquipmentCmd);
                    //updateEquipmentSqlDataAdapter.UpdateCommand = updateEquipmentCmd;
                    //result = updateEquipmentSqlDataAdapter.UpdateCommand.ExecuteNonQuery();

                    //if (result > 0)
                    //{
                    //    MessageBoxResult checkNotice = MessageBox.Show("返却が完了しました!", "成功メッセージ", MessageBoxButton.OK);
                    //    AccountName.Text = "";
                    //    AccountNameToSave.Text = "";
                    //    resetEquipmentTable();
                    //    resetTextBox();
                    //}
                    //else
                    //{
                    //    CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                    //    dialog.ShowDialog();
                    //}
                

            }
            else
            {
                CustomizeDialog dialog = new CustomizeDialog("設備情報がありません！");
                dialog.ShowDialog();
            }

        }
        private void resetEquipmentTable()
        {
            OrderIDTbl.Text = "";
            EquipmentIdTbl.Text = "";
            EquipmentNameTbl.Text = "";
            OrderDateTbl.Text = "";
            ReturnedDateTbl.Text = "";
            EquipStatusTbl.Text = "";
        }

        //処理を取り消す
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            send_http_message(g_read_cmd_header + "3start");//インベントリ開始
            resetTextBox();
            AccountName.Text = "";
            TakePhotoBtn.IsEnabled = false;
            StopCameraBtn.IsEnabled = false;
        }

        // RFID関連のメソッド群
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            resetEquipmentTable();
            string text = "";
            rc.ProcessHttpApi("/reader/antenna/common/006setconfig?session=0", ref text);//「タグは可能な限り応答を返す」ように設定
            text = send_http_message(g_read_cmd_header + "3startsetconfig?continue=1");//インベントリ開始

            if (json_get(text, "response") == "0")
            {
                dispatcherTimerRestart.Start();
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ontag.Clear();
            string result = send_http_message(g_read_cmd_header + "4getlist");
        }

        void dispatcherTimer_RestartTick(object sender, EventArgs e)
        {
            string result = send_http_message(g_read_cmd_header + "4getlist");
        }

        private string send_http_message(string request)
        {
            lock (g_api_lockObject)
            {
                string result = "";
                rc.ProcessHttpApi(request, ref result);
                show_result(result);
                return result;
            }
        }
        private bool show_result(string result)
        {
            if (json_get(result, "response") == "0")
            {
                debug1.Text = "ontag";
                debug2.Text = "Lastontag";

                //lastontagを表示
                foreach (string strOnTag in lastontag)
                {
                    debug2.Text += "\n" + strOnTag;
                }

                // 現在タグリストを表示
                foreach (string strOnTag in ontag)
                {
                    debug1.Text += "\n" + strOnTag;                   
                }


                if (result.IndexOf("x001Epc") >= 0)
                {
                    show_result_taglist(result);
                    return true;
                }
                else
                {
                    // １つもタグが載っていなければ現在のタグリストをクリア
                   //ontag.Clear();

                
                    // 現在シートに乗っているタグと前回乗っていたタグの差分を算出
                    // 乗せられた（現在シートにあって、過去シートにない）
                    List<string> addTagList = ontag.Except(lastontag).ToList<string>();
                    // 取られた（過去シートにあって、現在シートにない）
                    List<string> removeTagList = lastontag.Except(ontag).ToList<string>();

                    // 画面表示用に加工
                    string format = "{0}\r\n";
                    StringBuilder sbOnTagList = new StringBuilder();
                    StringBuilder sbAddTagList = new StringBuilder();
                    StringBuilder sbRemoveTagList = new StringBuilder();

                

                    // 現在タグリスト
                        foreach (string strOnTag in ontag)
                    {
                       
                        sbOnTagList.AppendFormat(format, strOnTag);
                    }

                    // 乗せられたタグリスト
                    foreach (string strAddOnTag in addTagList)
                    {
                        
                        sbAddTagList.AppendFormat(format, strAddOnTag);
                    }

                    // 取られたタグリスト
                    foreach (string strRemoveOnTag in removeTagList)
                    {
                        sbRemoveTagList.AppendFormat(format, strRemoveOnTag);
                    }

                    // 乗せられた or 取られた場合処理をストップ
                    if (!startFlg && (addTagList.Count >= 1 || removeTagList.Count >= 1))
                    {
                        dispatcherTimer.Stop();
                        rc.ProcessHttpApi(g_read_cmd_header + "6stop", ref result);
                        lastontag = new List<string>(ontag) ;

                        // 取られた（貸出された）場合
                        if (removeTagList.Count >= 1)
                        {
                            //DBへ接続する
                            try
                            {
                                SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                                thisConnection.Open();

                                // EquipmentIDを取得
                                string selectSql =
                                    "SELECT EquipmentID, EquipmentName, EquipStatus "
                                    + " FROM dbo.Equipment "
                                    + " WHERE RFIDTagNumber = '" + removeTagList[0] + "'";

                                SqlCommand selectCmd = thisConnection.CreateCommand();
                                selectCmd.CommandText = selectSql;

                                SqlDataAdapter selectSqlDataAdapter = new SqlDataAdapter(selectCmd);
                                DataTable dt = new DataTable("Equipment");
                                selectSqlDataAdapter.Fill(dt);

                                string equipmentID = dt.Rows[0]["EquipmentID"].ToString();
                                string equipmentName = dt.Rows[0]["EquipmentName"].ToString();
                                string equipStatus = dt.Rows[0]["EquipStatus"].ToString();

                                EquipmentIdTbl.Text = equipmentID;
                                EquipmentNameTbl.Text = equipmentName;
                                EquipStatusTbl.Text = equipStatus;

                                isBorrow = 1;
                            }
                            catch (Exception ex)
                            {
                                CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                                dialog.ShowDialog();
                            }
                        }
                        // 乗せられた（返却された）場合
                        else if (addTagList.Count >= 1)
                        {
                            //DBへ接続する
                            try
                            {
                                SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                                thisConnection.Open();

                                // EquipmentIDを取得
                                string selectSql =
                                    "SELECT TOP(1) EO.OrderID, EO.EquipmentID, E.EquipmentName, EO.OrderDate, E.EquipStatus "
                                    + " FROM dbo.EmployeeOrder AS EO "
                                    + " INNER JOIN dbo.Equipment AS E "
                                    + " ON EO.EquipmentID = E.EquipmentID"
                                    + " WHERE E.RFIDTagNumber = '" + addTagList[0] + "'"
                                    + " AND E.EquipStatus = " + 0
                                    + " AND EO.ReturnedDate IS NULL"
                                    + " ORDER BY EO.OrderDate DESC";

                                SqlCommand selectCmd = thisConnection.CreateCommand();
                                selectCmd.CommandText = selectSql;

                                SqlDataAdapter selectSqlDataAdapter = new SqlDataAdapter(selectCmd);
                                DataTable dt = new DataTable("Equipment");
                                selectSqlDataAdapter.Fill(dt);

                                string equipmentID = dt.Rows[0]["EquipmentID"].ToString();
                                string equipmentName = dt.Rows[0]["EquipmentName"].ToString();
                                string equipStatus = dt.Rows[0]["EquipStatus"].ToString();
                                string orderID = dt.Rows[0]["OrderID"].ToString();
                                string orderDate = dt.Rows[0]["OrderDate"].ToString();

                                EquipmentIdTbl.Text = equipmentID;
                                EquipmentNameTbl.Text = equipmentName;
                                EquipStatusTbl.Text = equipStatus;
                                OrderIDTbl.Text = orderID;
                                OrderDateTbl.Text = orderDate;

                                isBorrow = 0;
                            }
                            catch (Exception ex)
                            {
                                CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                                dialog.ShowDialog();
                            }
                        }
                    }

                    // 現在シートに乗っているタグを前回乗っていたタグとして保持
                    lastontag.Clear();
                    foreach (string tag in ontag)
                    {
                        lastontag.Add(tag);
                    }

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private bool show_result_taglist(string result)
        {
            result = result.Replace("\r", "");
            string[] result_split = result.Split('\n');
            for (int i = 0; i < result_split.Count(); i++)
            {
                int index = result_split[i].IndexOf("Epc\":\"");
                if (index >= 0)
                {
                    // タグのID部分のみ摘出
                    string tagName = result_split[i].Substring(index + 6, 29);
                    // リストになければ追加
                    if (!ontag.Contains(tagName))
                    {
                        ontag.Add(tagName);
                    }
                    result_split[i] = "\r\n                " + result_split[i];
                }
            }

            // 現在シートに乗っているタグと前回乗っていたタグの差分を算出
            // 乗せられた（現在シートにあって、過去シートにない）
            List<string> addTagList = ontag.Except(lastontag).ToList<string>();
            // 取られた（過去シートにあって、現在シートにない）
            List<string> removeTagList = lastontag.Except(ontag).ToList<string>();

            // 画面表示用に加工
            string format = "{0}\r\n";
            StringBuilder sbOnTagList = new StringBuilder();
            StringBuilder sbAddTagList = new StringBuilder();
            StringBuilder sbRemoveTagList = new StringBuilder();

            // 現在タグリスト
            foreach (string strOnTag in ontag)
            {
                sbOnTagList.AppendFormat(format, strOnTag);
            }

            // 乗せられたタグリスト
            foreach (string strAddOnTag in addTagList)
            {
                sbAddTagList.AppendFormat(format, strAddOnTag);
            }

            // 取られたタグリスト
            foreach (string strRemoveOnTag in removeTagList)
            {
                sbRemoveTagList.AppendFormat(format, strRemoveOnTag);
            }

            // 乗せられた or 取られた場合処理をストップ
            if (!startFlg && (addTagList.Count >= 1 || removeTagList.Count >= 1))
            {
                dispatcherTimer.Stop();
                rc.ProcessHttpApi(g_read_cmd_header + "6stop", ref result);

                // 取られた（貸出された）場合
                if (removeTagList.Count >= 1)
                {
                    //DBへ接続する
                    try
                    {
                        SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                        thisConnection.Open();

                        // EquipmentIDを取得
                        string selectSql =
                            "SELECT EquipmentID, EquipmentName, EquipStatus "
                            + " FROM dbo.Equipment "
                            + " WHERE RFIDTagNumber = '" + removeTagList[0] + "'";

                        SqlCommand selectCmd = thisConnection.CreateCommand();
                        selectCmd.CommandText = selectSql;

                        SqlDataAdapter selectSqlDataAdapter = new SqlDataAdapter(selectCmd);
                        DataTable dt = new DataTable("Equipment");
                        selectSqlDataAdapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            string equipmentID = dt.Rows[0]["EquipmentID"].ToString();
                            string equipmentName = dt.Rows[0]["EquipmentName"].ToString();
                            string equipStatus = dt.Rows[0]["EquipStatus"].ToString();

                            EquipmentIdTbl.Text = equipmentID;
                            EquipmentNameTbl.Text = equipmentName;
                            EquipStatusTbl.Text = equipStatus;

                            isBorrow = 1;
                        }
                        else
                        {
                            CustomizeDialog dialog = new CustomizeDialog("取られた設備の情報がデータベースに登録されていない！");
                            dialog.ShowDialog();
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                        dialog.ShowDialog();
                    }
                }
                // 乗せられた（返却された）場合
                else if (addTagList.Count >= 1)
                {
                    //DBへ接続する
                    try
                    {
                        SqlConnection thisConnection = new SqlConnection(@"Server=tcp:bmkprojbnsqr01v.database.windows.net,1433;Initial Catalog=DB_BMK;Persist Security Info=False;User ID=bmkDataAdmin;Password=Setsubi2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                        thisConnection.Open();

                        // EquipmentIDを取得
                        string selectSql =
                            "SELECT TOP(1) EO.OrderID, EO.EquipmentID, E.EquipmentName, EO.OrderDate, E.EquipStatus "
                            + " FROM dbo.EmployeeOrder AS EO "
                            + " INNER JOIN dbo.Equipment AS E "
                            + " ON EO.EquipmentID = E.EquipmentID"
                            + " WHERE E.RFIDTagNumber = '" + addTagList[0] + "'"
                            + " AND E.EquipStatus = " + 0
                            + " AND EO.ReturnedDate IS NULL"
                            + " ORDER BY EO.OrderDate DESC";

                        SqlCommand selectCmd = thisConnection.CreateCommand();
                        selectCmd.CommandText = selectSql;

                        SqlDataAdapter selectSqlDataAdapter = new SqlDataAdapter(selectCmd);
                        DataTable dt = new DataTable("Equipment");
                        selectSqlDataAdapter.Fill(dt);


                        if (dt.Rows.Count > 0)
                        {
                            string equipmentID = dt.Rows[0]["EquipmentID"].ToString();
                            string equipmentName = dt.Rows[0]["EquipmentName"].ToString();
                            string equipStatus = dt.Rows[0]["EquipStatus"].ToString();
                            string orderID = dt.Rows[0]["OrderID"].ToString();
                            string orderDate = dt.Rows[0]["OrderDate"].ToString();

                            EquipmentIdTbl.Text = equipmentID;
                            EquipmentNameTbl.Text = equipmentName;
                            EquipStatusTbl.Text = equipStatus;
                            OrderIDTbl.Text = orderID;
                            OrderDateTbl.Text = orderDate;

                            isBorrow = 0;
                        }
                        else
                        {
                            CustomizeDialog dialog = new CustomizeDialog("乗せられた設備の貸出時間が登録されていない!");
                            dialog.ShowDialog();
                        }
                       
                    }
                    catch (Exception)
                    {
                        CustomizeDialog dialog = new CustomizeDialog("データベースへ接続できない！");
                        dialog.ShowDialog();
                    }
                }
            }

            // 現在シートに乗っているタグを前回乗っていたタグとして保持
            lastontag.Clear();
            foreach (string tag in ontag)
            {
                lastontag.Add(tag);
            }

            startFlg = false;
            return true;
        }

        private string json_get(string json, string search)//jsonをパースしてsearchの値を返す(サンプルコード丸コピ)
        {
            json = json.Replace("\r", "");
            string[] json_split = json.Split('\n');
            for (int i = 0; i < json_split.Count(); i++)
            {
                if (json_split[i].StartsWith("\"" + search + "\":"))
                {
                    return json_split[i].Substring(search.Length + 3).Replace("\"", "").Replace(",", "");
                }
            }
            return "";
        }

        private void WebcamViewer_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
