using System;
using System.ComponentModel;
using System.Data;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Output;


namespace HelloWorld
{
    public partial class IntroForm : Form
    {
        public string username;//主界面的用户名称
        private IToolbarMenu m_menuMap = null;//右键菜单


        #region 构造函数
        //构造函数：未登录
        public IntroForm()
        {
            username = null;//未登录的主界面
            InitializeComponent();
            labelX8.Visible = true;
        }
        //构造函数：已登录
        public IntroForm(string uName)
        {
            this.username = uName;//用户登录后的主界面
            InitializeComponent();
            //隐藏登录和注册按钮
            buttonLogin.Visible = false;
            buttonRegister.Visible = false;
            labelX8.Text = "当前用户：" + this.username;
            labelX8.Visible = true;
        }
        #endregion
        private void IntroForm_Load(object sender, EventArgs e)
        {

            //创建右键菜单
            //m_menuMap = new ToolbarMenuClass();
            //m_menuMap.SetHook(m_mapControl);

        }




        //进入信息点查询界面
        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            SearchInfo userSearch = new SearchInfo(this.username);
            userSearch.ShowDialog();
        }
        //跳转用户注册界面
        private void buttonRegister_Click(object sender, EventArgs e)
        {
            UserRegister registerform = new UserRegister();//
            registerform.ShowDialog();
        }
        //跳转用户登录界面
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            this.Hide();
            UserLogin userLogin = new UserLogin();
            userLogin.ShowDialog();

        }

        private void buttonexit_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.ExitThread();
        }
        //进入信息点查询界面，并跳转至宿舍分类
        private void symbolBox1_Click(object sender, EventArgs e)
        {
            this.Hide();
            SearchInfo userSearch = new SearchInfo(this.username, "学生宿舍");
            userSearch.ShowDialog();


        }
        //进入信息点查询界面，并跳转至餐饮分类
        private void symbolBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
            SearchInfo userSearch = new SearchInfo(this.username, "食堂");
            userSearch.ShowDialog();
        }
        //进入统计查询界面
        private void symbolBox4_Click(object sender, EventArgs e)
        {
            if (this.username == null)//用户未登陆
            {
                this.Hide();
                Chart chart = new Chart();
                chart.ShowDialog();
            }
            else //用户已登陆
            {
                this.Hide();
                Chart chart = new Chart(this.username);
                chart.ShowDialog();
            }
            
        }
        //进入信息点查询界面，并跳转至自习室分类
        private void symbolBox3_Click(object sender, EventArgs e)
        {
            this.Hide();
            SearchInfo userSearch = new SearchInfo(this.username, "教学楼");
            userSearch.ShowDialog();
        }

     
        //跳转图书馆预定界面
        private void symbolBox6_Click(object sender, EventArgs e)
        {
            this.Hide();
            Library userLibrary = new Library(this);
            userLibrary.ShowDialog();
        }



    }
}
