using System;
using System.Data.OleDb;
using System.Windows.Forms;
using Core;
using System.Drawing;
using System.IO;
using Npgsql;
using System.Net;
using Authentication.Class;
using DatabaseDesignPlus;
using System.Configuration;
using System.Web;

namespace JMapCheckStation
{
    public partial class SysLogin : Form
    {
       
          // 定义参数数据库连接对象及文件地址
        public NpgsqlConnection _genericParamDbConnection = null;
        public string _genericParamDbConnectionString = "";
        string usertablename = "用户表";
        string lastlogintablename = "上次登录";
        string lastloginusername = "";

        public SysLogin()
        {
            InitializeComponent();
            this.BackgroundImageLayout = ImageLayout.Stretch;
            try
            {
                this.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"Images\" + System.Configuration.ConfigurationManager.AppSettings["LoginImage"]));
                //System.Configuration.ConfigurationSettings.AppSettings["LoginImage"]));
            }
            catch { }
            try
            {
                this.Text = System.Configuration.ConfigurationManager.AppSettings["SystemName"] + "——登录";
            }
            catch 
            {
            }
            
            ReadDbConnection();
        } 

        public void ReadDbConnection()
        {
            try
            {
                _genericParamDbConnectionString = System.Configuration.ConfigurationManager.AppSettings["Login"];
            }
            catch (System.IO.FileNotFoundException fe)
            {
                MessageBox.Show("配置文件错误！");
                Application.Exit();
            }
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            TimeSpan span = DateTime.Now.Subtract(DateTime.Parse("2016-09-16 00:00:00"));
            if (span.TotalDays >0)
            {
                MessageBox.Show("登录失败！");
                return;
            }

            string username = tb_username.Text;
            string password = tb_password.Text;

            //使用统一认证中心进行用户认证
            /*string userobjson = "";
            string url = string.Format(@"http://localhost:5155/userauth?username={0}&password={1}",HttpUtility.UrlEncode( username), HttpUtility.UrlEncode(password));
            HttpWebRequest myHttpWebRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            myHttpWebRequest.ContentType = "pplication/x-www-form-urlencoded";

            using (HttpWebResponse res = (HttpWebResponse)myHttpWebRequest.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.PartialContent)//返回为200或206
                {
                    string dd = res.ContentEncoding;
                    System.IO.Stream strem = res.GetResponseStream();
                    System.IO.StreamReader r = new System.IO.StreamReader(strem);
                    userobjson = r.ReadToEnd();
                }
            }
            */

            string dbconnection = System.Configuration.ConfigurationManager.AppSettings["Login"];
            IDatabaseReaderWriter dbReader = null;
            dbReader = new ClsPostgreSql(dbconnection);
            UserAuthenticate userauth = new UserAuthenticate(dbReader);
            //UserObject userobj = userauth.FetchUser(userobjson);

            UserObject userobj = userauth.FetchUser(username, password);
            //////////////////////////////////
            if (userobj.username == null)
            {
                MessageBox.Show("未找到对应的用户名和密码，请检查输入是否正确！");
                return;
            }

            if (userobj.authorized != "1" || userobj.authorized==null)
            {
                MessageBox.Show("您当前用户名在本机还未授权，请申请授权或等待管理员授权！");
                return;
            }

            this.Hide();
            Root root = Root.FromXML(System.IO.Path.Combine(Application.StartupPath, "Functions.xml"));
            JMapCheckStation mf = new JMapCheckStation(root.Functions, root, userobj);
            //JMapCheckStation mf = new JMapCheckStation(); 
            mf.Show();

            #region
          /*  string queryuser_sql = string.Format("select * from {0} where username = '{1}' and password = '{2}'", usertablename, username, password);
            _genericParamDbConnection = new NpgsqlConnection(_genericParamDbConnectionString);
            NpgsqlCommand cmd = new NpgsqlCommand(queryuser_sql, _genericParamDbConnection);

            _genericParamDbConnection.Open();
            try
            {
                object id = cmd.ExecuteScalar();

                if (id == null)
                    throw new InvalidOperationException();

                // 打开主窗体，并延时关闭此登陆窗体

                this.Hide();
                Root root = Root.FromXML(System.IO.Path.Combine(Application.StartupPath, "Functions.xml"));
                SurveryProductCheckMainForm mf = new SurveryProductCheckMainForm(root.Functions, root, userobj);
                mf.Show();

                _genericParamDbConnection.Close();

            }
            catch (System.InvalidOperationException e1)
            {
                //MessageBox.Show(e.Message);
                //未找到对应的用户名，请检查输入；
                MessageBox.Show("未找到对应的用户名和密码，请检查输入是否正确！");

            }

            _genericParamDbConnection.Close();
            */
            #endregion
        }

        private void btn_quit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("警告：是否退出该系统？", "退出系统", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                this.Close();
            else
                return;
        }

        private void SysLogin_Load(object sender, EventArgs e)
        {
            //登陆界面载入时从login数据库中“上次登录”中取得上次登录的用户名和密码，填入到输入框中

            string queryuser_sql = string.Format("select * from {0} ", lastlogintablename);
            _genericParamDbConnection = new NpgsqlConnection(_genericParamDbConnectionString);
            NpgsqlCommand cmd = new NpgsqlCommand(queryuser_sql, _genericParamDbConnection);

            try
            {
                _genericParamDbConnection.Open();
                object username = cmd.ExecuteScalar();

                if (username == null)
                    throw new InvalidOperationException();

                tb_username.Text = Convert.ToString(username);
                lastloginusername = tb_username.Text;

                _genericParamDbConnection.Close();

               
            }
            catch (System.InvalidOperationException e1)
            {
                //MessageBox.Show(e.Message);
                //未找到对应的用户名，请检查输入；
                //MessageBox.Show("！");
               _genericParamDbConnection.Close();
            }
            
        }

        private void SysLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (lastloginusername != "")
            {
                string updatalastlogin = string.Format("update {0} set username = '{1}',logintime = '{2}' where username = '{3}'", lastlogintablename, tb_username.Text, DateTime.Now.ToShortTimeString(), lastloginusername);

                _genericParamDbConnection = new NpgsqlConnection(_genericParamDbConnectionString);
                NpgsqlCommand cmd = new NpgsqlCommand(updatalastlogin, _genericParamDbConnection);

                _genericParamDbConnection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    _genericParamDbConnection.Close();
                }
                catch (System.InvalidOperationException e1)
                {
                    _genericParamDbConnection.Close();
                }
            }
        }
    }
}
