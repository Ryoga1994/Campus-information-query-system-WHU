using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace HelloWorld
{
    public partial class UserLogin : Form
    {
        int type;//定义返回参数类型
        IntroForm introForm;
        ObjectDetails objectDetails;
        Library userLibrary;
        StudyBuilding studyBuilding;
        //从主界面传入的用户登录函数
        public UserLogin()
        {
            type = 1;//主界面层级为1
            InitializeComponent();
        }

        //从ObjectDeatils界面传入的用户登录函数
        public UserLogin(ObjectDetails form3)
        {
            type = 3;//ObejctDetails界面层级为3
            this.objectDetails = form3;
            InitializeComponent();
        }
        
        //从studyBuilding界面传入的用户登陆函数
        public UserLogin(StudyBuilding form4)
        {
            type = 4;//studyBuilding界面的层级为4
            this.studyBuilding = form4;
            InitializeComponent();
        }
        //从Library界面传入的用户登录函数
        public UserLogin(Library form2)
        {
            type = 2;//Library界面的层级为2
            this.userLibrary = form2;
            InitializeComponent();
        }

        //登录
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (textBoxX1.Text == "" || textBoxX2.Text == "")
                MessageBox.Show("用户名或密码不能为空!", "提示");
            else
            {
                string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
                OleDbConnection con = new OleDbConnection(strdb);
                con.Open();
                string strsql = "select * from users where username='" + textBoxX1.Text + "'and password='" + textBoxX2.Text + "'";
                OleDbCommand cmd = new OleDbCommand(strsql, con);
                OleDbDataReader rd = cmd.ExecuteReader();//？？？

                if (!rd.HasRows)
                {

                    MessageBox.Show("用户名或密码错误,请重新输入!", "提示");
                    return;
                }
                else
                {
                    this.Hide();
                    //判断返回参数类型
                    if (type == 1)//传入函数来自主界面
                    {
                        this.introForm = new IntroForm(textBoxX1.Text);
                        this.introForm.ShowDialog();
                    }
                    else if (type == 2)//传入函数来自Library界面
                    {
                        this.userLibrary = new Library(textBoxX1.Text);
                        this.userLibrary.ShowDialog();

                         
                    }
                    else if (type == 3)//传入函数来自ObjectDeatils界面
                    {
                        this.objectDetails.main.username = textBoxX1.Text;//传入用户名称
                        //this.objectDetails.ShowDialog();
                    }
                    else if (type == 4)//传入函数来自StudyBuilding界面
                    {
                        this.studyBuilding.main.username = textBoxX1.Text;
                    }


                }
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Dispose();
            //IntroForm introForm = new IntroForm();
            //introForm.ShowDialog();
            //Application.ExitThread();//??
        }
    }
}
