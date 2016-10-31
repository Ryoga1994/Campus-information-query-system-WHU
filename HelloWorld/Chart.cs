using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelloWorld
{
    public partial class Chart : Form
    {
        public string username;

        public Chart()
        {
            this.username = null;
            InitializeComponent();
        }

        //传入用户名称参数的构造函数
        public Chart(string uName)
        {
            this.username = uName;
            InitializeComponent();
        }
        //武汉大学各学部热度分布图
        private void buttonX1_Click(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = Application.StartupPath + @"\吃货点\武汉大学各学部热度分布图.jpg";
            textBox2.Visible = true;
            textBox1.Visible = false;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = Application.StartupPath + @"\吃货点\武汉大学各学部资源分布.jpg";
            textBox2.Visible = false;
            textBox1.Visible = true;
        }

      
        //界面初始化
        private void Chart_Load(object sender, EventArgs e)
        {
          
            pictureBox1.ImageLocation = Application.StartupPath + @"\吃货点\小封面.jpg";
            textBox1.Visible = false;
            textBox2.Visible = false;

        }
        //返回键


        private void lblReturn_Click(object sender, EventArgs e)
        {
            if (this.username == null)//用户未登陆
            {
                this.Hide();
                IntroForm intro = new IntroForm();
                intro.ShowDialog();
            }
            else
            {
                this.Hide();
                IntroForm intro = new IntroForm(this.username);
                intro.ShowDialog();
            }
        }
    }
}
