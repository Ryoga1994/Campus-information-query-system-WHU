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
    public partial class UserRegister : Form
    {
        private IntroForm main;
        public UserRegister()
        {
            InitializeComponent();
        }
        public UserRegister(IntroForm form1)
        {
            InitializeComponent();
            main = form1;
        }

        private void btRegister_Click(object sender, EventArgs e)
        {
            string X1 = tbUsername.Text;
            string X2 = tbPassword.Text;
            string X3 = tbpasswordconfirm.Text;
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();
            string X4 = "select username from users where username = '" + X1 + "'";//搜索重名用户
            OleDbCommand cmd = new OleDbCommand(X4, con);
            OleDbDataReader rd = cmd.ExecuteReader();

            if (X1 == "" || X2 == "" || X3 == "")
            {
                MessageBox.Show("用户名或密码不能为空!", "提示");
            }
            else if (rd.HasRows)//判断用户名是否被占用
            {
                MessageBox.Show("抱歉，该用户名已存在。", "提示"); 
                //清空已填信息
                tbUsername.Text = null;
                tbPassword.Text = null;
                tbpasswordconfirm.Text = null;     
            }
            else if (X2 != X3)
            {
                MessageBox.Show("两次密码输入不一致，请重新输入！", "提示");
                //清空密码栏
                tbPassword.Text = null;
                tbpasswordconfirm.Text = null;
            }
            else
            {
                String sql = "insert into users(username, [password]) values( '" + X1 + "' , '" + X2 + "')";
                string Strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
                OleDbConnection connection = new OleDbConnection(Strdb);
                OleDbCommand comd = new OleDbCommand(sql, connection);
                connection.Open();

                if (comd.ExecuteNonQuery() > 0) //判断插入数据是否成功;
                {
                    MessageBox.Show("注册成功!");
                    //执行要操作的语句;
                    this.Hide();
                    UserLogin loginForm = new UserLogin();//打开登录界面
                    return;
                }
                this.Hide();
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

     
 
    }
}
