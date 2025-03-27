using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace Restaurant
{
    public partial class Form1 : Form
    {
        /******************************************
         * 2019-12-27:初版
         *            功能:1.備餐
         *                 2.刷餐()
         *                 3.刪除異常(BaseBook、FactDinner、ExpenseLog)
         * 2020-01-07:修改刪除異常BUG，新增訂餐刪除
         * 2020-01-08:修改刪除異常BUG，增加刪除條件(餐別)
         * 2020-01-09:修改刷餐BUG，增加更新BaseBook旗標
         * 2020-04-23:修改刷餐BUG，取消午餐晚餐BUG
         * 2020-07-16:修改訂餐BUG
         * 2020-09-01:修改刷餐BUG，廠區未正確帶入
         * 2020-11-24:修改物件命名規則、刷餐資料提示
         * 2021-01-12:增加跨廠區功能
         * 2021-03-17:備餐增加跨廠區功能
         * 2022-01-04:增加常日班批次中午訂餐功能
         * 2022-01-11:修改訂晚餐bug，欄位名稱錯誤
         * 2023-01-04:修改年訂餐log時間錯誤 改為點功能當日
         * 2023-09-06:修改訂餐log為RPersonal代表此程式訂餐，並因應FactBook Table調整，新增pType欄位
         * 2024-12-23:新增刪除異常判斷若，有欄位為空，則不進行刪除操作
         * 2025-01-01:新增選擇區間訂餐
         * 2025-01-03:新增日期區間選擇訂餐或刪除
         ******************************************/


        //正式用

        public string ReleaseCon = "Data Source=SQLName;Initial Catalog=databaseName;User ID=sa;Password=";

        public string DebugCon = "Data Source=SQLName;Initial Catalog=databaseName;User ID=sa;Password=";




        public Form1()
        {
            InitializeComponent();
            this.Text = "不能說的訂餐(二林專用)";
            this.label1.Text = "卡號/工號/姓名: ";
            this.label2.Text = "日期: ";
            this.label3.Text = "卡號: ";
            this.label4.Text = "工號: ";
            this.label5.Text = "姓名: ";
            this.label6.Text = "部門: ";
            this.btnSearch.Text = "查詢人事資料";
            this.btnBaseBook.Text = "個人備餐";
            this.btnFactDinner.Text = "個人刷餐";
            this.btnDelete.Text = "刪除異常";
            this.Dinner.Text = "訂餐查詢";
            this.ClearBtn.Text = "清除";
            this.btnYear.Text = "全年訂餐";

            toolStripStatusLabel1.Text = "資料庫:正式區";
            toolStripStatusLabel2.Text = "Ver.1.5.5";
            toolStripStatusLabel3.Text = "Release Date:2023-09-06";
            //this.ControlBox = false; 關閉控制列(縮小/放大/關閉)
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            //this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (UserIDTextBox.Text == string.Empty ||
                CardNoTextBox.Text == string.Empty ||
                NameTextBox.Text == string.Empty ||
                DepartmentTextBox.Text == string.Empty)
            {
                btnBaseBook.Enabled = false;
                btnFactDinner.Enabled = false;
                btnDelete.Enabled = false;
                btnYear.Enabled = false;
                Dinner.Enabled = false;
                btnChoose.Enabled = false;
                btnDelChos.Enabled = false;
                checkBox1.Enabled = false;
                dateTimePicker2.Enabled = false;
                dateTimePicker3.Enabled = false;
            }
#if DEBUG
            toolStripStatusLabel1.Text = "測試區";
#endif
        }

        /// <summary>
        /// 查詢個人資料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnSearch_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty) return;
            string connstring = ReleaseCon;
            string sql = $@"select top(100) CardNo, UserID, Name, Department, dbs from Employee where CardNo like '%{textBox1.Text}%'
                or UserID like '%{textBox1.Text}%' or Name Like '%{textBox1.Text}%' order by Department asc ;";
#if DEBUG
            connstring = DebugCon;
#endif

            DateTime time = this.dateTimePicker1.Value;
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            List<Employee> EmpLs = new List<Employee>();
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                using (SqlDataAdapter sda = new SqlDataAdapter(sql, conn))
                {
                    sda.Fill(ds, "Employee");
                    dt = ds.Tables["Employee"];
                    dataGridView1.DataSource = dt;
                }
                conn.Close();
                conn.Dispose();
            }
        }

        private void btnDinner_Click(object sender, EventArgs e)
        {
            string Conn = ReleaseCon;
#if DEBUG
            Conn = DebugCon;
#endif

            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }

            int Lunch = 0, Supper = 0;
            if (radioButton1.Checked) Lunch = 1;
            if (radioButton2.Checked) Supper = 1;
            string sql = $"select * from Dinner where DinnerDate = '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}';";
            using (SqlConnection sqc = new SqlConnection(Conn))
            {
                sqc.Open();
                SqlCommand cmd = new SqlCommand(sql);
                cmd.Connection = sqc;
                if (cmd.ExecuteScalar() == null)
                {
                    // insert
                    sql = $@"insert into Dinner values ('{Guid.NewGuid()}', '{UserIDTextBox.Text}', '{NameTextBox.Text}', '{CardNoTextBox.Text}'
                        , '{DepartmentTextBox.Text}', '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}', '', 0, {Lunch}, {Supper}, 0, '1','0', '{FactIdTextBox.Text}'
                        ,'{UserIDTextBox.Text}','{dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 08:42:15"}','RPersonal');";
                }
                else
                {
                    // update
                    if (Lunch == 1)
                        sql = $"update Dinner set Lunch = {Lunch} where DinnerDate = '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}'";
                    if (Supper == 1)
                        sql = $"update Dinner set Supper = {Supper} where DinnerDate = '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}' and UserID = '{UserIDTextBox.Text}'and dbs = '{FactIdTextBox.Text}' ";
                }
                cmd.CommandText = sql;
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("訂餐成功" + dateTimePicker1.Text, "成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "錯誤");
                }
                finally
                {
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// 個人備餐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBaseBook_Click(object sender, EventArgs e)
        {

            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }

            if (UserIDTextBox.Text == string.Empty)
                return;
            string date = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string dinnerdata = string.Empty;
            string tmp = "";
            string Conn = ReleaseCon;
#if DEBUG
            Conn = DebugCon;
#endif

            int DType = 0;

            if (radioButton1.Checked)
                DType = 2;
            else
                DType = 3;
            dinnerdata = $"select UserID from BaseBook where UserID = '{UserIDTextBox.Text}' and BDate = '{date}' and DType = {DType}";
            using (SqlConnection sqc = new SqlConnection(Conn))
            {
                sqc.Open();
                SqlCommand cmd = new SqlCommand(dinnerdata, sqc);
                try
                {
                    //tmp = cmd.ExecuteScalar().ToString();
                    if (cmd.ExecuteScalar() != null)
                    { MessageBox.Show("已有備餐資料", "錯誤"); return; }

                    DialogResult dr = MessageBox.Show("是否新增備餐資料", "測試", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        string InsertDinner = $@"insert into BaseBook values ('{Guid.NewGuid()}', '{date}', {DType}, '{CardNoTextBox.Text}',
                                    '{UserIDTextBox.Text}', '{NameTextBox.Text}', '{DepartmentTextBox.Text}', '', 1, 0, 2, 0, '{FactIdTextBox.Text}', 0);";
                        try
                        {
                            if (SqlExecute(InsertDinner) == true)
                            {
                                MessageBox.Show("新增備餐成功: " + dateTimePicker1.Value.ToString("yyyy-MM-dd"), "訂餐");
                            }
                            else
                                MessageBox.Show("新增備餐失敗", "訂餐");
                        }
                        catch (Exception ex) { MessageBox.Show("新增失敗" + ex.Source, "錯誤"); }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("查詢失敗: " + ex.Message, "錯誤");
                }
            }
        }

        /// <summary>
        /// 刷餐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFactDinner_Click(object sender, EventArgs e)
        {

            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }

            DateTime t = DateTime.Now;
            string DType = string.Empty;
            string DinnerTime = string.Empty;
            if (radioButton1.Checked)
            { DType = "2"; DinnerTime = "12:10:23"; }
            else
            { DType = "3"; DinnerTime = "17:01:52"; }
            string sql = $@"update BaseBook set DNum = 1, chkup = 1 where UserID = '{UserIDTextBox.Text}' and BDate = '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}' and DType = '{DType}'
                        Insert Into FactDinner Values( '{Guid.NewGuid()}', '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}',
                        '{DinnerTime}', '{DType}', '{CardNoTextBox.Text}', '{UserIDTextBox.Text}', N'{NameTextBox.Text}', N'{DepartmentTextBox.Text}', '2', '0', '', '0', '0', N'{FactIdTextBox.Text}','ID');";
            DialogResult dr = MessageBox.Show("是否新增刷餐資料", "刷餐", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                try
                {
                    if (SqlExecute(sql) == true)
                        MessageBox.Show("新增刷餐成功", "刷餐");
                    else
                        MessageBox.Show("新增刷餐失敗", "錯誤");
                }
                catch
                {
                    MessageBox.Show("新增刷餐失敗", "錯誤");
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int DType = 0;
            string DelS = "";
            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }
            if (radioButton1.Checked)
            { DType = 2; DelS = "Lunch = 0"; }
            else
            { DType = 3; DelS = "Supper = 0"; }
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            //測試時拿掉[RESERVE].[db_owner].[ExpenseLog]該行，測試DB沒建Table
            string sql = $@"delete BaseBook   where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and BDate      = '{date}' and DType = {DType};
                            delete FactDinner where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DDate      = '{date}' and DType = {DType};
                            delete [RESERVE].[db_owner].[ExpenseLog] where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DinnerDate = '{date}' and DType = {DType};";
            sql += $"update Dinner set {DelS} where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DinnerDate = '{date}'";

            DialogResult dr = MessageBox.Show($"是否取消訂餐，刪除備餐、用餐、異常\n日期: {date}\n姓名: {NameTextBox.Text}", "刪除異常資料", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                try
                {
                    if (SqlExecute(sql) == true)
                        MessageBox.Show("刪除成功", "刪除訂餐");
                    else
                        MessageBox.Show("刪除失敗", "刪除訂餐");
                }
                catch
                {
                    MessageBox.Show("刪除失敗", "刪除訂餐");
                }
            }
        }

        //清除按鍵
        //private void ClearBtn_Click(object sender, EventArgs e)
        //{
        //    textBox1.Text = "";
        //    CardNoTextBox.Text = "";
        //    UserIDTextBox.Text = "";
        //    NameTextBox.Text = "";
        //    DepartmentTextBox.Text = "";
        //    FactIdTextBox.Text = "";
        //    Dinner.Enabled = false;
        //    btnBaseBook.Enabled = false;
        //    btnFactDinner.Enabled = false;
        //    btnDelete.Enabled = false;
        //    radioButton1.Checked = false;
        //    radioButton2.Checked = false;
        //    btnYear.Enabled = false;
        //    btnChoose.Enabled = false;
        //    btnDelChos.Enabled = false;
        //    btnDinner.Enabled = false;
        //    dataGridView1.DataSource = null;
        //    checkBox1.Checked = false;
        //    dateTimePicker1.Value = DateTime.Now.Date;
        //    dateTimePicker2.Value = DateTime.Now.Date;
        //    dateTimePicker3.Value = DateTime.Now.Date;

        //}

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            // 清除一次所有欄位
            ClearAllFields();

            // 手動再觸發一次清除操作（即第二次操作）
            ClearAllFields();
        }

        private void ClearAllFields()
        {
            textBox1.Text = "";
            CardNoTextBox.Text = "";
            UserIDTextBox.Text = "";
            NameTextBox.Text = "";
            DepartmentTextBox.Text = "";
            FactIdTextBox.Text = "";
            Dinner.Enabled = false;
            btnBaseBook.Enabled = false;
            btnFactDinner.Enabled = false;
            btnDelete.Enabled = false;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            btnYear.Enabled = false;
            btnChoose.Enabled = false;
            btnDelChos.Enabled = false;
            btnDinner.Enabled = false;
            dataGridView1.DataSource = null;
            checkBox1.Checked = false;
            checkBox1.Enabled = false;
            dateTimePicker1.Value = DateTime.Now.Date;
            dateTimePicker2.Value = DateTime.Now.Date;
            dateTimePicker3.Value = DateTime.Now.Date;

            // 確保所有事件被刷新
            Application.DoEvents();
        }


        private void Dinner_Click(object sender, EventArgs e)
        {
            string sql = $@"select UserID, Lunch as 午餐, Supper as 晚餐, DinnerDate from Dinner where UserID = '{UserIDTextBox.Text}' and dbs = 'T4'
                and ( DinnerDate Between '{dateTimePicker1.Value.AddDays(-3).ToString("yyyy-MM-dd")}' and '{dateTimePicker1.Value.AddDays(3).ToString("yyyy-MM-dd")}');";
            DataTable dt = SqlReader(sql);
            dataGridView1.DataSource = dt;
        }

        //全年訂餐
        private void btnYear_Click(object sender, EventArgs e)
        {
            string Conn = ReleaseCon;
#if DEBUG
            Conn = DebugCon;
#endif

            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }

            DateTime currentDate = dateTimePicker1.Value; //從選擇的日期取時間
            DateTime centuryEnd = DateTime.Parse(dateTimePicker1.Value.ToString("yyyy" + "-01-01"));//設定結束日為1/1
            centuryEnd = centuryEnd.AddYears(1);//加一年

            long elapsedTicks = centuryEnd.Ticks - currentDate.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            int alldays = elapsedSpan.Days; //今年剩下的天數

            string nDays = currentDate.ToString("yyyy-MM-dd");

            int Lunch = 0, Supper = 0;
            if (radioButton1.Checked) Lunch = 1;
            if (radioButton2.Checked) Supper = 1;
            DialogResult dr = MessageBox.Show("警告！此功能只適用於常日班，並且將會從選擇日期訂餐直到年底結束，請謹慎使用！", "特殊功能", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                using (SqlConnection sqc = new SqlConnection(Conn))
                {
                    sqc.Open();
                    string Sql = $"select * from Dinner where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}';";
                    SqlCommand cmd = new SqlCommand(Sql);

                    for (int i = 0; i < alldays; i++)
                    {
                        #region 多天訂餐

                        //取訂餐資料
                        string sql = $"select * from Dinner where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}';";
                        cmd = new SqlCommand(sql);
                        cmd.Connection = sqc;
                        cmd.CommandText = sql;
                        //取假日
                        string dsql = $"select * from Holiday where OFFDAY ='{nDays}' ";//'{dateTimePicker1.Value.ToString("yyyy-MM-dd")}'
                        SqlCommand dcmd = new SqlCommand(dsql);
                        dcmd.Connection = sqc;
                        dcmd.CommandText = dsql;

                        if (dcmd.ExecuteScalar() == null)
                        {
                            if (cmd.ExecuteScalar() == null)
                            {
                                // insert
                                sql = $@"insert into Dinner values ('{Guid.NewGuid()}', '{UserIDTextBox.Text}', '{NameTextBox.Text}', '{CardNoTextBox.Text}'
                            , '{DepartmentTextBox.Text}', '{nDays}', '', 0, {Lunch}, {Supper}, 0, '1','0', '{FactIdTextBox.Text}'
                            ,'{UserIDTextBox.Text}','{dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 08:42:15"}','NWPersonal');";
                            }
                            else
                            {
                                // update
                                if (Lunch == 1)
                                    sql = $"update Dinner set Lunch = {Lunch} where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}'";
                                if (Supper == 1)
                                    sql = $"update Dinner set Dinner = {Supper} where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}'and dbs = '{FactIdTextBox.Text}' ";
                            }
                            cmd.CommandText = sql;
                            try
                            {
                                cmd.ExecuteNonQuery();
                                //MessageBox.Show("訂餐成功" + currentDate, "成功");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "錯誤");
                            }
                        }
                        currentDate = currentDate.AddDays(1);
                        nDays = currentDate.ToString("yyyy-MM-dd");
                        //MessageBox.Show(Convert.ToString(dcmd.ExecuteScalar()));

                        #endregion 多天訂餐
                    }
                    cmd.Dispose();
                }
                MessageBox.Show("訂餐成功", "成功");
            }
        }

        //選擇日期區間訂餐
        private void btnChoose_Click(object sender, EventArgs e)
        {

            string Conn = ReleaseCon;
#if DEBUG
            Conn = DebugCon;
#endif
            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) || string.IsNullOrEmpty(FactIdTextBox.Text))
            {
                MessageBox.Show("請選擇人員，並填寫完整資料", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空，則不進行刪除操作
            }

            DateTime startDate = dateTimePicker2.Value;  // 獲取開始日期
            DateTime endDate = dateTimePicker3.Value;    // 獲取结束日期

            // 檢查開始日期和結束日期是否選擇有效日期
            if ( startDate < DateTime.Now.Date || endDate < DateTime.Now.Date)
            {
                MessageBox.Show("請選擇有效的開始日期!(日期不能小於今天)", "日期錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // 如果未選擇有效日期，則退出
            }

            //檢查開始日期是否小於結束日期
            if (startDate >= endDate)
            {
                MessageBox.Show("開始日期不能等於或晚於結束日期！", "日期錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // 如果日期不符合，直接return
            }


            long elapsedTicks = endDate.Date.Ticks - startDate.Date.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            int alldays = elapsedSpan.Days;  // 獲取開始日期到結束日期之間的天數



            int Lunch = 0, Supper = 0;
            if (radioButton1.Checked) Lunch = 1;
            if (radioButton2.Checked) Supper = 1;

            DialogResult dr = MessageBox.Show("此功能將會從選擇的開始日期訂餐直到結束日期，請謹慎使用！", "特殊功能", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                using (SqlConnection sqc = new SqlConnection(Conn))
                {
                    sqc.Open();

                    for (int i = 0; i <= alldays; i++)
                    {
                        // 如果是周六或周日，則跳過
                        if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            startDate = startDate.AddDays(1);
                            continue; // 跳過周六或周日
                        }

                        string nDays = startDate.ToString("yyyy-MM-dd");

                        SqlCommand cmd = null;
                        try
                        {
                            cmd = new SqlCommand();  // 在每次循環中創建新的 SqlCommand
                            cmd.Connection = sqc;

                            string sql = $"select * from Dinner where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}';";
                            cmd.CommandText = sql;

                            // 檢查假日表
                            string dsql = $"select * from Holiday where OFFDAY ='{nDays}'";
                            SqlCommand dcmd = new SqlCommand(dsql);
                            dcmd.Connection = sqc;

                            if (dcmd.ExecuteScalar() == null)  // 不是假日
                            {
                                if (cmd.ExecuteScalar() == null)  // 如果沒有紀錄，則插入新紀錄
                                {
                                    sql = $@"insert into Dinner values ('{Guid.NewGuid()}', '{UserIDTextBox.Text}', '{NameTextBox.Text}', '{CardNoTextBox.Text}'
                                , '{DepartmentTextBox.Text}', '{nDays}', '', 0, {Lunch}, {Supper}, 0, '1','0', '{FactIdTextBox.Text}'
                                ,'{UserIDTextBox.Text}','{DateTime.Now.ToString("yyyy-MM-dd") + " 08:45:15"}','NWPersonal');";
                                }
                                else  // 如果有紀錄，更新
                                {
                                    if (Lunch == 1)
                                        sql = $"update Dinner set Lunch = {Lunch} where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}'";
                                    if (Supper == 1)
                                        sql = $"update Dinner set Dinner = {Supper} where DinnerDate = '{nDays}' and UserID = '{UserIDTextBox.Text}' and dbs = '{FactIdTextBox.Text}' ";
                                }
                                cmd.CommandText = sql;

                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "錯誤");
                                }
                            }

                            // 增加一天
                            startDate = startDate.AddDays(1);
                            nDays = startDate.ToString("yyyy-MM-dd");

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"執行資料庫時出現錯誤: {ex.Message}", "錯誤");
                        }
                        finally
                        {
                            // 確保命令對象始終被釋放
                            if (cmd != null)
                            {
                                cmd.Dispose();
                            }
                        }
                    }
                }
                MessageBox.Show("訂餐成功", "成功");
            }
        }

        //選擇日期區間刪除
        private void btnDelChos_Click(object sender, EventArgs e)
        {
            int DType = 0;
            string DelS = "";

            // 檢查是否有必要的欄位為空
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(CardNoTextBox.Text) ||
                string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(DepartmentTextBox.Text) ||
                string.IsNullOrEmpty(FactIdTextBox.Text) ||
                dateTimePicker2.Value > dateTimePicker3.Value)  // 檢查日期範圍
            {
                MessageBox.Show("請選擇人員，並填寫完整資料，並確保開始日期小於結束日期", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 若有欄位為空或日期不符合，則不進行刪除操作
            }

            // 設置用餐類型
            if (radioButton1.Checked)
            {
                DType = 2;
                DelS = "Lunch = 0";
            }
            else
            {
                DType = 3;
                DelS = "Supper = 0";
            }

            DateTime startDate = dateTimePicker2.Value;  // 獲取開始日期
            DateTime endDate = dateTimePicker3.Value;    // 獲取結束日期

            DialogResult dr = MessageBox.Show($"是否取消訂餐，刪除備餐、用餐、異常\n姓名: {NameTextBox.Text} ，工號:{UserIDTextBox.Text}\n日期範圍: {startDate.ToString("yyyy-MM-dd")} 到 {endDate.ToString("yyyy-MM-dd")}",
                                              "刪除異常資料", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    // 用來記錄是否刪除成功
                    bool allDeletedSuccessfully = true;

                    // 從開始日期到結束日期，逐天刪除
                    while (startDate <= endDate)
                    {
                        string date = startDate.ToString("yyyy-MM-dd");

                        // 編寫SQL語句
                        string sql = $@"
                    delete BaseBook where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and BDate = '{date}' and DType = {DType};
                    delete FactDinner where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DDate = '{date}' and DType = {DType};
                    delete [RESERVE].[dbo_owner].[ExpenseLog] where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DinnerDate = '{date}' and DType = {DType};
                    update Dinner set {DelS} where UserID = '{UserIDTextBox.Text}' and CardNo = '{CardNoTextBox.Text}' and DinnerDate = '{date}'";

                        // 執行刪除操作
                        bool success = SqlExecute(sql);

                        // 如果有一次刪除失敗，則標記為 false
                        if (!success)
                        {
                            allDeletedSuccessfully = false;
                        }

                        // 增加一天，繼續刪除下一天
                        startDate = startDate.AddDays(1);
                    }

                    // 根據是否所有操作都成功，顯示相應的訊息
                    if (allDeletedSuccessfully)
                    {
                        MessageBox.Show($"成功刪除 {dateTimePicker2.Value.ToString("yyyy-MM-dd")} 到 {endDate.ToString("yyyy-MM-dd")} 的訂餐資料", "刪除訂餐");
                    }
                    else
                    {
                        MessageBox.Show($"刪除過程中有部分資料未能成功刪除，請檢查錯誤", "刪除訂餐");
                    }
                }
                catch
                {
                    MessageBox.Show("刪除失敗", "刪除訂餐");
                }
            }
        }


        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CardNoTextBox.Text = dataGridView1.Rows[e.RowIndex].Cells["CardNo"].Value.ToString();
            UserIDTextBox.Text = dataGridView1.Rows[e.RowIndex].Cells["UserID"].Value.ToString();
            NameTextBox.Text = dataGridView1.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            DepartmentTextBox.Text = dataGridView1.Rows[e.RowIndex].Cells["Department"].Value.ToString();
            FactIdTextBox.Text = dataGridView1.Rows[e.RowIndex].Cells["dbs"].Value.ToString();
            //button2.Enabled = true;
            //button3.Enabled = true;
            //button4.Enabled = true;
            Dinner.Enabled = true;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                btnDinner.Enabled = true;
                btnBaseBook.Enabled = true;
                btnFactDinner.Enabled = true;
                btnDelete.Enabled = true;
                btnYear.Enabled = true;
                checkBox1.Enabled = true;
            }
            if (radioButton2.Checked)
            {
                btnDinner.Enabled = true;
                btnBaseBook.Enabled = true;
                btnFactDinner.Enabled = true;
                btnDelete.Enabled = true;
                btnYear.Enabled = false;
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // 您可以在此處檢查 CheckBox 是否被選中，並執行相應的操作
            if (checkBox1.Checked)
            {
                // 做些操作，當 CheckBox 被選中時
                dateTimePicker2.Enabled = true;
                dateTimePicker3.Enabled = true;
                btnChoose.Enabled = true;
                btnDinner.Enabled = false;
                btnBaseBook.Enabled = false;
                btnFactDinner.Enabled = false;
                btnDelete.Enabled = false;
                btnYear.Enabled = false;
                btnDelChos.Enabled = true;
                
            }
            else
            {
                // 做些操作，當 CheckBox 沒有被選中時
                btnChoose.Enabled = false;
                dateTimePicker2.Enabled = false;
                dateTimePicker3.Enabled = false;
                btnDelChos.Enabled = false;
                btnDinner.Enabled = true;
                btnBaseBook.Enabled = true;
                btnFactDinner.Enabled = true;
                btnDelete.Enabled = true;
                btnYear.Enabled = true;

            }
        }




        private bool SqlExecute(string sql)
        {
            bool isOK = false;
            string connstring = ReleaseCon;
#if DEBUG
            connstring = DebugCon;
#endif
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    cmd.ExecuteNonQuery();
                    isOK = true;
                }
                catch (Exception ex) { }
                finally { conn.Close(); }
            }
            return isOK;
        }

        private DataTable SqlReader(string sql)
        {
            DataTable dt;
            string connstring = ReleaseCon;
#if DEBUG
            connstring = DebugCon;
#endif
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(sql, conn))
                {
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    dt = ds.Tables[0];
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

        private void CardNoTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void UserIDTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}