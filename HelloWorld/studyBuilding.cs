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
    public partial class StudyBuilding : Form
    {
        //定义类变量
        private string pName;//名称
        public SearchInfo main;//定义详情页的主界面为SearchInfo
        private String currOperation = "";//定义当前操作
        private int currSeat;//定义当前选中教室的座位数
        int empty;//定义当前教室中的空座位数目;
        private int checkinnum = 0;//定义当前在线人数,初始化为0
        //空构造函数
        public StudyBuilding()
        {
            InitializeComponent();
        }
        public StudyBuilding(string objName, SearchInfo searchInfo)
        {
            pName = objName;
            this.main = searchInfo;//传入用户名的SearchInfo界面
            InitializeComponent();
        }

        private void studyBuilding_Load(object sender, EventArgs e)
        {
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库
            //传入教学楼的详细信息
            string strsql1 = "select * from point where 名称 ='" + this.pName + "'";
            OleDbCommand testCommand = con.CreateCommand();
            testCommand.CommandText = strsql1;
            OleDbDataReader testReader = testCommand.ExecuteReader();
            while (testReader.Read())
            {
                labelX4.Text = (string)testReader["名称"];
                labelX5.Text = (string)testReader["联系电话"];
                labelX6.Text = (string)testReader["地址"];
            }
            //传入教室列表信息
            string strsql2 = "select * from studyRoom where pName ='" + this.pName + "'";
            OleDbCommand testCommand2 = con.CreateCommand();
            testCommand2.CommandText = strsql2;
            OleDbDataReader testReader2 = testCommand2.ExecuteReader();
            while (testReader2.Read())
            {
                listBoxAdv1.Items.Add((string)testReader2["classroom"]);
            }

        }

        private void listBoxAdv1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库
            //查询当前教室的详细信息
            string strsql11 = "select * from studyRoom where pName ='" + this.pName + "'and classroom ='" + listBoxAdv1.SelectedItem.ToString() + "'";
            OleDbCommand testCommand11 = con.CreateCommand();
            testCommand11.CommandText = strsql11;
            OleDbDataReader testReader11 = testCommand11.ExecuteReader();
            while (testReader11.Read())
            {
                labelX10.Text = listBoxAdv1.SelectedItem.ToString();
                labelX11.Text = "" + (int)testReader11["座位数"];
                currSeat = (int)testReader11["座位数"];//记录当前教室的座位总数
            }
            //查询当前教室的在线用户
            listBox2Update();
            //string strsql22 = "select * from studyCheckins where pName ='" + this.pName + "'and classroom ='" + listBoxAdv1.SelectedItem.ToString() + "'";
            //OleDbCommand testCommand22 = con.CreateCommand();
            //testCommand22.CommandText = strsql22;
            //OleDbDataReader testReader22 = testCommand22.ExecuteReader();


            //统计在线用户人数
            con.Close();//关闭数据库

        }
        //check in button
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (this.main.username == null)//用户未登陆
            {
                MessageBox.Show("请先登陆！", "提示");
                UserLogin userLogin = new UserLogin(this);
                userLogin.ShowDialog();

            }
            else
            {
                string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
                OleDbConnection con = new OleDbConnection(strdb);
                con.Open();
                string strsql = "select * from studyCheckins where username='" + this.main.username + "'and pName ='" + this.pName + "'and classroom = '" + listBoxAdv1.SelectedItem.ToString() + "'";
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
                    //判断教室是否有空座位
                    if (empty == 0)
                    {
                        MessageBox.Show("当前教室已满！", "提示");
                    }
                    else
                    {
                        String sql2 = "insert into studyCheckins(username,pName,classroom,checkin) values( '" + this.main.username +
                       "' , '" + this.pName + "' , '" + listBoxAdv1.SelectedItem.ToString() + "' , '" + (int)1 + "')";
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
            }
        }
        //check out button
        private void buttonX2_Click(object sender, EventArgs e)
        {
            string strdb = "Provider=Microsoft.ACE.OLEDB.12.0;Data source=" + Application.StartupPath + "\\SampleData1.accdb";
            OleDbConnection con = new OleDbConnection(strdb);
            con.Open();//打开数据库

            //检查用户是否已经checkin
            string strsql21 = "select * from studyCheckins where username = '" + this.main.username + "'and pName ='" + this.pName + "'and classroom ='" + listBoxAdv1.SelectedItem.ToString() + "'";
            OleDbCommand testCommand21 = con.CreateCommand();
            testCommand21.CommandText = strsql21;
            OleDbDataReader testReader21 = testCommand21.ExecuteReader();
            if (testReader21.Read())//用户checkin记录存在
            {
                string strsql22 = "delete from studyCheckins where username = '" + this.main.username + "' and pName ='" + this.pName + "'and classroom ='" + listBoxAdv1.SelectedItem.ToString() + "'";
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
            string strsql33 = "select * from studyCheckins where pName ='" + this.pName + "'and classroom ='" + listBoxAdv1.SelectedItem.ToString() + "'";
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

            empty = currSeat - checkinnum2;//空座位数
            lblEmpty.Text = "" + empty;
            con.Close();//关闭数据库
        }


    }
}
