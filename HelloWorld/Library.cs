using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Output;

namespace HelloWorld
{
    public partial class Library : Office2007Form
    {
        public string username;//定义用户名称
        public IntroForm introForm;//定义从主界面进入的用户
        public SearchInfo searchInfo;//定义从搜索界面进入的用户
        public int bookedSeat;//定义当前已被预订的座位号
        public int type;//定义返回参数类型

        public Library()
        {
            InitializeComponent();
        }
        //传入用户名称的构造函数 = 从Login返回的Library构造函数
        public Library(string uName)
        {
            type = 0;//代表无返回源 = Library在login时重新生成
            this.username = uName;
            InitializeComponent();
        }
        //传入主界面参数的构造函数
        public Library(IntroForm intro)
        {
            type = 1;//定义主界面层级为1
            this.introForm = intro;
            this.username = intro.username;
            InitializeComponent();
        }
        //定义从查询界面进入的构造函数
        public Library(SearchInfo search)
        {
            type = 2;//定义查询界面层级为2
            this.searchInfo = search;
            this.username = search.username;
            InitializeComponent();
        }


        private void Library_Load(object sender, EventArgs e)
        {
            loadMapDocunment2();
            axMapControl1.Extent = axMapControl1.FullExtent;
            //检查用户登录状态
            if (this.username == null)//用户未登录
            {
                panel1.Visible = false;//隐藏用户预约状态panel1
            }
            //检查用户预约状态
            else if (HasBooked(this.username))//存在用户预约记录
            {
                panel1.Visible = true;
                labelX2.Text = this.username;
                labelX7.Text = Convert.ToString(bookedSeat);
            }
            else//用户已登录但未预约
            {
                panel1.Visible = false;
                labelX8.Text = this.username;
                labelX5.Text = null;

            }
        }



        /// <summary>
        /// 读入地图文件library.mxd
        /// </summary>
        public void loadMapDocunment2()
        {
            string filePath = Application.StartupPath + @"\\library\\library.mxd";
            axMapControl1.LoadMxFile(filePath, 0, Type.Missing);
            //axMapControl2.LoadMxFile(filePath, 0, Type.Missing);
        }

        /// <summary>
        /// 获取指定名称的图层
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public ILayer getlayerbyname2(string str)
        {
            ILayer layer = null;
            for (int i = 0; i < axMapControl1.LayerCount; i++)  //从axMapControl1的图层中获取指定名称的图层
            {
                if (axMapControl1.get_Layer(i).Name == str)
                {
                    layer = axMapControl1.get_Layer(i);
                    break;
                }
            }
            return layer;
        }
        /// <summary>
        /// 选座点缓冲
        /// </summary>
        /// <param name="dis"></param>
        /// <param name="point"></param>
        public void Bufferquery2(double dis, IPoint point)
        {

            axMapControl1.Map.ClearSelection();
            axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            ITopologicalOperator topoope = point as ITopologicalOperator;
            IGeometry pgeometry = topoope.Buffer(dis);
            IFeatureLayer pFeatureLayer = getlayerbyname2("seat") as IFeatureLayer;
            //执行查询获取符合条件的要素
            List<IFeature> pFList = GetSearchFeatures(pFeatureLayer, pgeometry);
            for (int i = 0; i < pFList.Count; i++)
            {
                IFeature pFeature = pFList[i];
                axMapControl1.Map.SelectFeature(pFeatureLayer, pFeature);
                int fieldIndex = getFieldindex(pFeatureLayer.FeatureClass, "Id");
                labelX5.Text = "" + (int)pFeature.get_Value(fieldIndex);
            }
            //图书馆选座不需要更新地图显示界面
            //axMapControl1.Extent = pgeometry.Envelope;


            //cSearch = false;//用于启动缓冲区查询的boolean变量
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
        }

        /// <summary>
        /// 获取指定图层，指定范围内的属性列表
        /// </summary>
        /// <param name="pFeatureLayer"></param>
        /// <param name="pGeometry"></param>
        /// <returns></returns>
        private List<IFeature> GetSearchFeatures(IFeatureLayer pFeatureLayer, IGeometry pGeometry)
        {
            try
            {
                //创建要素列表
                List<IFeature> pList = new List<IFeature>();
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                IQueryFilter pQueryFilter = pSpatialFilter as ISpatialFilter;
                pSpatialFilter.Geometry = pGeometry;
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                IFeatureCursor pFeatureCursor = pFeatureLayer.Search(pQueryFilter, false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                while (pFeature != null)
                {
                    //将获取要素对象添加入要素列表
                    pList.Add(pFeature);
                    pFeature = pFeatureCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                return pList;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }
        /// <summary>
        /// 获取指定属性字段的索引号
        /// </summary>
        /// <param name="pfeatureclass"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public int getFieldindex(IFeatureClass pfeatureclass, string fieldName)//传入属性类，属性名称
        {
            IField pfield = null;
            int index = -1;
            for (int j = 0; j < pfeatureclass.Fields.FieldCount; j++)
            {
                pfield = pfeatureclass.Fields.get_Field(j);
                if (pfield.Name == fieldName)
                {
                    index = j;
                    break;
                }
            }
            return index;
        }
        //选座
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {

            IPoint pPoint = new PointClass();//创建点实例 = 座位选取
            pPoint.PutCoords(e.mapX, e.mapY); // 也就是 pPoint.X = e.mapX;pPoint.Y = e.mapY;       

            ITopologicalOperator pTopo = pPoint as ITopologicalOperator;
            //IGeometry pGeometry;//定义选座缓冲区范围
            IGeometry pGeometry = (IGeometry)pTopo.Buffer(1);//点缓冲区
            QueryByGeometry2(pGeometry);

        }
        /// <summary>
        /// 按几何空间查询
        /// </summary>
        /// <param name="pGeometry"></param>
        private void QueryByGeometry2(IGeometry pGeometry)
        {
            axMapControl1.Map.ClearSelection();
            ISpatialFilter pSpatialFilter = new SpatialFilter();
            pSpatialFilter.Geometry = pGeometry;//空间查询范围
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureLayer pFeatLayer = getlayerbyname2("seat") as IFeatureLayer;
            //图层“seat”中位于选择区域内的部分
            IFeatureCursor pFeatCursor = pFeatLayer.FeatureClass.Search(pSpatialFilter, true);
            IFeature pFeat = pFeatCursor.NextFeature();

            //修改属性字段名称
            int fieldIndex1 = getFieldindex(pFeatLayer.FeatureClass, "Id");//属性字段名
            int fieldIndex2 = getFieldindex(pFeatLayer.FeatureClass, "status");//属性字段名
            //int fieldIndex3 = getFieldindex(pFeatLayer.FeatureClass, "地址");//属性字段名
            //int fieldIndex4 = getFieldindex(pFeatLayer.FeatureClass, "联系电话");//属性字段名
            while (pFeat != null)
            {
                axMapControl1.Map.SelectFeature(pFeatLayer, pFeat);//使所选择的要素高亮显示
                axMapControl1.ActiveView.Refresh();

                object a = pFeat.get_Value(fieldIndex1);
                string id = a.ToString();

                //selectedSiteName = pFeat.get_Value(2);
                //labelX1.Text ="类型："+type;
                object b = pFeat.get_Value(fieldIndex2);
                string status = b.ToString();//状态status记录当前座位状态

                if (status == "1")//当前座位已被预订
                {
                    MessageBox.Show("当前座位已被预订！", "提示");
                }
                else
                {
                    labelX5.Text = id;
                }
                pFeat = pFeatCursor.NextFeature();
            }
        }
        //提交选中
        private void buttonX1_Click(object sender, EventArgs e)
        {
            //检查当前有无选中座位
            if (labelX5.Text =="")
            {
                MessageBox.Show("请先选择座位", "提示");
            }
            else if (this.username == null)//检查用户是否登录
            {
                MessageBox.Show("请先登录！", "提示");
                UserLogin userLogin = new UserLogin(this);
                userLogin.ShowDialog();
                this.Hide();
            }
            else
            {
                //检查当前用户有无已经预定的座位
                if (!HasBooked(this.username))//无预约记录
                {
                    BookSeat(Convert.ToInt32(labelX5.Text));//预约当前选中的座位
                    labelX2.Text = this.username;
                    labelX7.Text = labelX5.Text;
                    panel1.Visible = true;              
                }
                else//无效code
                {
                    MessageBox.Show("您已存在预定记录！", "提示");
                }


            }
        }
        /// <summary>
        /// 传入座位号，Lock该座位status = 1（已预约）
        /// </summary>
        /// <param name="name"></param>
        public void BookSeat(int seatID)
        {
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            axMapControl1.Extent = axMapControl1.FullExtent;
            //获取需要Edit的图层
            ILayer layer = getlayerbyname2("seat");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = "Id = " + seatID + "";
            IFeatureCursor pFeatureCursor = pfeatureLayer.FeatureClass.Search(pQueryFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();

            //使要素处于编辑状态
            IDataset dataset = (IDataset)pfeatureClass;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            if (pFeature != null && this.username != null)//用户已登录，该座位存在
            {
                int field2 = pFeature.Fields.FindField("status");
                int field3 = pFeature.Fields.FindField("username");
                pFeature.set_Value(field3, this.username);
                pFeature.set_Value(field2, 1);//设置当前座位状态为“已被预约”
                bookedSeat = seatID;//记录当前被预订的座位号
                axMapControl1.Refresh();
                MessageBox.Show("预约成功！", "提示");
            }
            pFeature.Store();
            workspaceEdit.StopEditing(true);
            workspaceEdit.StopEditOperation();
        }
        /// <summary>
        /// 检查当前用户的预约状态，存在预约返回true,否则返回false
        /// </summary>
        /// <param name="uName"></param>
        /// <returns></returns>
        public Boolean HasBooked(string uName)
        {
            ILayer layer = getlayerbyname2("seat");//修改图层名称
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            int fieldIndex1 = getFieldindex(pfeatureLayer.FeatureClass, "username");//获得图层的feature
            IQueryFilter pqueryFilter = new QueryFilter();//属性值过滤
            pqueryFilter.WhereClause = "username  = '" + this.username + "'";//sql中的where语句
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;

            pfeatureCursor = pfeatureClass.Search(pqueryFilter, false);//查询满足要求的项
            pfeature = pfeatureCursor.NextFeature();
            if (pfeature != null)//存在预约记录
            {
                int field1 = pfeature.Fields.FindField("Id");//查询用户预约的座位号
                bookedSeat = Convert.ToInt32(pfeature.get_Value(field1));//记录被预订的座位号
                return true;
            }
            else//不存在返回false
            {
                return false;
            }
        }
        /// <summary>
        /// 传入用户名和座位ID，取消预约
        /// </summary>
        /// <param name="uName"></param>
        /// <param name="seatID"></param>
        public void cancelBook(string uName, int seatID)
        {
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            axMapControl1.Extent = axMapControl1.FullExtent;
            //获取需要Edit的图层
            ILayer layer = getlayerbyname2("seat");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = "Id = " + seatID + "";//找ID对应的座位
            IFeatureCursor pFeatureCursor = pfeatureLayer.FeatureClass.Search(pQueryFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();

            //使要素处于编辑状态
            IDataset dataset = (IDataset)pfeatureClass;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            if (pFeature != null && this.username != null)//用户已登录，该座位存在
            {
                int field2 = pFeature.Fields.FindField("status");
                int field3 = pFeature.Fields.FindField("username");
                pFeature.set_Value(field3, null);//设置当前座位的用户名为空
                pFeature.set_Value(field2, 0);//设置当前座位状态为“可选”
                bookedSeat = seatID;//记录当前被预订的座位号
                axMapControl1.Refresh();
                MessageBox.Show("您已取消对该座位的预约！", "提示");
                //更新界面显示状态
                panel1.Visible = false;
                labelX8.Text = this.username;
                //labelX5.Text = null;
            }
            pFeature.Store();
            workspaceEdit.StopEditing(true);
            workspaceEdit.StopEditOperation();
        }
        //取消预约
        private void buttonX2_Click(object sender, EventArgs e)
        {
            cancelBook(this.username, bookedSeat);

        }

        private void lbl_return_Click(object sender, EventArgs e)
        {
            //判断用户是否已登录
            if (type == 0&&this.username!=null)//用户在Library界面点开Login并登录
            {
                this.Hide();
                introForm = new IntroForm(this.username);
                introForm.ShowDialog();
            }
            else if (type == 1)//Library来自主界面
            {
                this.Hide();
                introForm = new IntroForm(this.username);
                introForm.ShowDialog();
            }
            else if (type == 2)//Library来自SearchInfo界面
            {
                this.Hide();
                searchInfo = new SearchInfo(this.username,"图书馆");
                searchInfo.ShowDialog();
            }
            

        }



    }
}
