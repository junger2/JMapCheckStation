using Authentication.Class;
using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JMapCheckStation
{
    public partial class JMapCheckStation : Form
    {
        public JMapCheckStation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 用户登陆情况：用户名、是否退出
        /// </summary>
        bool UserExit = false;
        //string LoginUserName = "";
        UserObject LoginUser;
        string wellcome = "";

        string sBoundingGeometryString = "";
        string sBoundingGeometryName = "";
        #region // 界面处理
        /// <summary>
        /// 界面及初始化
        /// </summary>
        Root r;
        List<Function> _functions;
        public JMapCheckStation(List<Function> functions, Root root, UserObject oLoginUser)
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(ButtonForm_FormClosed);
            this.FormClosing += new FormClosingEventHandler(ButtonForm_FormClosing);

            r = root;
            this.StartPosition = FormStartPosition.CenterScreen;
            try
            {
                this.Text = System.Configuration.ConfigurationManager.AppSettings["SystemName"];
            }
            catch { }

            try
            {
                string img = Path.Combine(Application.StartupPath, @"Images\" + System.Configuration.ConfigurationManager.AppSettings["SystemImage"]);
                if (File.Exists(img)) this.BackgroundImage = Image.FromFile(img);
            }
            catch { }

            this.BackgroundImageLayout = ImageLayout.Stretch;

            InitMenu(functions);
            //增加用户登录信息，写入配置字典中
            //LoginUserName = sLoginUserName;
            LoginUser = oLoginUser;
            wellcome = string.Format("欢迎来自{0}的{1}，您的宝贵建议都将是我们前进的目标。", LoginUser.company, LoginUser.username);
            //statusStrip1.Text = wellcome;

           // toolStripStatusLabel2.Text = wellcome;

            _functions = functions;

           // InitTreeView();


        }

        void ButtonForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!UserExit)
            {
                e.Cancel = !(MessageBox.Show(string.Format("确定退出{0}？", this.Text), "退出系统", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK);
            }
        }

        void ButtonForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        void InitMenu(List<Function> functions)
        {
            //menuStrip1.Items.Clear();
            string useInfo = "";
            try
            {
                useInfo = System.Configuration.ConfigurationManager.AppSettings["MenuUseImage"];
            }
            catch { }

            bool use = useInfo.ToUpper().Trim() == "TRUE";

            foreach (Function main in functions)
            {

                string img = Path.Combine(Application.StartupPath, @"Images\" + main.Image);
                ToolStripMenuItem yj = File.Exists(img) && use ? new ToolStripMenuItem(main.Tile, Image.FromFile(img)) : new ToolStripMenuItem(main.Tile);
                menuStrip1.Items.Add(yj);
                yj.ToolTipText = main.ToolTip;
                foreach (Function sub in main.Functions)
                {
                    img = Path.Combine(Application.StartupPath, @"Images\" + sub.Image);
                    ToolStripMenuItem ej = File.Exists(img) && use ? new ToolStripMenuItem(sub.Tile, Image.FromFile(img)) : new ToolStripMenuItem(sub.Tile);
                    ej.Tag = sub;
                    ej.ToolTipText = sub.ToolTip;
                    ej.Click += new System.EventHandler(Function_ToolstripClick);
                    yj.DropDownItems.Add(ej);
                }
            }
        }

        void Function_Click(object sender, System.EventArgs e)
        {
            object tag = (sender as Control).Tag;
            MInvoke invoke = new MInvoke(tag as Function, r, LoginUser);
            invoke.ApplicationClosed += invoke_ApplicationClosed;
            if (invoke.Run())
            {
                this.Hide();
            }
        }

        void Function_ToolstripClick(object sender, System.EventArgs e)
        {
            object tag = (sender as ToolStripItem).Tag;
            MInvoke invoke = new MInvoke(tag as Function, r, LoginUser);
            invoke.ApplicationClosed += invoke_ApplicationClosed;
            if (invoke.Run())
            {
                this.Hide();
            }
        }

        void invoke_ApplicationClosed(object sender, System.EventArgs e)
        {
            this.Show();
        }
        private void pictureBox1_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("确定退出该系统？", "退出系统", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                UserExit = true;
                Application.Exit();
            }
            else
            { return; }
        }

        #endregion

    }
}
