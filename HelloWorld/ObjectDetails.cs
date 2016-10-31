using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Data.OleDb;


namespace HelloWorld
{
    public partial class ObjectDetails : Form
    {
        //定义类变量
        private string pName;//名称
        public SearchInfo main;//定义详情页的主界面为SearchInfo
        private String currOperation = "";//定义当前操作
        private int checkinnum = 0;//定义当前在线人数,初始化为0
        //空构造函数？？？？
        public ObjectDetails()
        {
            InitializeComponent();
        }
        public ObjectDetails(String objName, SearchInfo searchForm)
        {
            pName = objName;//传入地点名称的详情页
            main = searchForm;//传入用户名称的SearchInfo界面
            InitializeComponent();
        }

        private void ObjectDetails_Load(object sender, EventArgs e)
        {
            //隐藏写评价panel组
            panel1.Visible = false;

            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库
            //传入信息点的详细信息
            string strsql1 = "select * from point where 名称 ='" + this.pName + "'";
            OleDbCommand testCommand = con.CreateCommand();
            testCommand.CommandText = strsql1;
            OleDbDataReader testReader = testCommand.ExecuteReader();
            while (testReader.Read())
            {
                groupPanel1.Text = this.pName;
                labelX6.Text = (string)testReader["Type"];
                labelX7.Text = (string)testReader["联系电话"];
                labelX8.Text = (string)testReader["地址"];
                pictureBox1.ImageLocation = Application.StartupPath + @"\吃货点\" + this.pName + ".jpg";
            }
            //传入信息点评价内容
            string strsql2 = "select * from comments where pName = '" + this.pName + "'";
            OleDbCommand testCommand2 = con.CreateCommand();
            testCommand2.CommandText = strsql2;
            OleDbDataReader testReader2 = testCommand2.ExecuteReader();
            while (testReader2.Read())
            {
                listBoxAdv1.Items.Add((string)testReader2["comment"]);
            }
            //传入当前签到状态
            string strsql3 = "select * from checkins where pName = '" + this.pName + "'";
            OleDbCommand testCommand3 = con.CreateCommand();
            testCommand3.CommandText = strsql3;
            OleDbDataReader testReader3 = testCommand3.ExecuteReader();
            while (testReader3.Read())
            {
                if ((int)testReader3["checkin"] == 1)//状态为checkin的用户
                {
                    listBoxAdv2.Items.Add((string)testReader3["username"]);
                    checkinnum++;
                }
            }
            lblCheckin.Text = " " + checkinnum;
            con.Close();//关闭数据库
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (this.main.username == null)//用户未登陆
            {
                MessageBox.Show("请先登陆！", "提示");
                UserLogin userLogin = new UserLogin(this);//调用从ObjectDetails界面传入的UserLogin构造函数
                userLogin.ShowDialog();
 
            }



            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();
            string strsql = "select * from checkins where username='" + this.main.username + "'and pName ='" + this.pName + "'";
            OleDbCommand testCommand = con.CreateCommand();
            testCommand.CommandText = strsql;
            OleDbDataReader testReader = testCommand.ExecuteReader();
            if (testReader.Read())
            {
                if ((int)testReader["checkin"] == 1)//当前在线
                {
                    MessageBox.Show("您已在这里checkin！", "提示");
                }
            }
            else
            {
                String sql2 = "insert into checkins(username,pName,checkin) values( '" + this.main.username + "' , '" + this.pName + "' , '" + (int)1 + "')";
                OleDbCommand comd2 = new OleDbCommand(sql2, con);
                //con.Open();
                if (comd2.ExecuteNonQuery() > 0) //判断数据插入是否成功
                {
                    MessageBox.Show("用户 check in 已成功!");
                    listBox2Update();
                    return;
                }


            }
        }
        //从该点check out
        private void buttonX2_Click(object sender, EventArgs e)
        {
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库

            //检查用户是否已经checkin
            string strsql21 = "select * from checkins where username='" + this.main.username + "'and pName ='" + this.pName + "'";
            OleDbCommand testCommand21 = con.CreateCommand();
            testCommand21.CommandText = strsql21;
            OleDbDataReader testReader21 = testCommand21.ExecuteReader();
            if (testReader21.Read())//用户checkin记录存在
            {
                string strsql22 = "delete from checkins where username = '" + this.main.username + "' and pName= '" + this.pName + "'";
                OleDbCommand comd = new OleDbCommand(strsql22, con);
                //con.Open();
                if (comd.ExecuteNonQuery() > 0) //判断数据删除是否成功
                {
                    MessageBox.Show("用户checkout已成功!", "提示");
                    listBox2Update();//更新当前在线用户列表
                    //执行要操作的语句
                    return;
                }
            }
            else
            {
                MessageBox.Show("您没有在这里checkin!", "提示");
            }
            con.Close();//关闭数据库

        }
        /// <summary>
        /// 更新当前在线用户列表
        /// </summary>
        private void listBox2Update()
        {
            listBoxAdv2.Items.Clear();
            int checkinnum2 = 0;
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库
            string strsql33 = "select * from checkins where pName = '" + this.pName + "'";
            OleDbCommand testCommand33 = con.CreateCommand();
            testCommand33.CommandText = strsql33;
            OleDbDataReader testReader33 = testCommand33.ExecuteReader();
            while (testReader33.Read())
            {
                if ((int)testReader33["checkin"] == 1)//状态为checkin的用户
                {
                    listBoxAdv2.Items.Add((string)testReader33["username"]);
                    checkinnum2++;
                }
            }
            lblCheckin.Text = " " + checkinnum2;
            con.Close();//关闭数据库
        }
        //添加评价
        private void buttonX3_Click(object sender, EventArgs e)
        {
            //检查用户是否登录
            if (this.main.username == null)
            {
                MessageBox.Show("请先登录");
                UserLogin userLogin = new UserLogin(this);
                userLogin.ShowDialog();
            }
            else
            {
                panel1.Visible = true;
            }
        }

        /// <summary>
        /// 更新当前评价列表
        /// </summary>
        public void CommentsUpdate()
        {
            listBoxAdv1.Items.Clear();
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库
            string strsql = "select * from comments where pName = '" + this.pName + "'";
            OleDbCommand testCommand = con.CreateCommand();
            testCommand.CommandText = strsql;
            OleDbDataReader testReader = testCommand.ExecuteReader();
            while (testReader.Read())
            {
                listBoxAdv1.Items.Add((string)testReader["comment"]);

            }
            con.Close();//关闭数据库

        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            //获取评分和评价内容
            int rating = ratingStar1.Rating;
            string comment = textBoxX1.Text;
            if (textBoxX1.Text == "")//用户没有填写评价内容
            {
                MessageBox.Show("来嘛来嘛说点啥呗~小哥请你吃四喜丸子~≧▽≦~","提示");
            }
            else
            {
                //链接数据库
                string Strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
                String sql = "insert into comments(username, pName,comment,rating) values( '" + this.main.username + "' , '" + this.pName + "' , '" + comment + "' , " + rating + ")";
                OleDbConnection connection = new OleDbConnection(Strdb);
                OleDbCommand comd = new OleDbCommand(sql, connection);
                connection.Open();

                //判断插入数据是否成功;
                if (comd.ExecuteNonQuery() > 0)
                {
                    MessageBox.Show("评价成功!");
                    CommentsUpdate();
                    panel1.Visible = false;
                }
            }          
        }
    }
}
