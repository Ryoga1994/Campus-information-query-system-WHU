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
using ESRI.ArcGIS.NetworkAnalyst;

namespace HelloWorld
{
    public partial class SearchInfo : Form
    {
        //定义变量
        public string username;//用户名称
        string typeFlag = null;//定义类型变量由于指示查询点的类别
        bool gBool = false;//定义全局变量，判断是否需要清楚虚拟图层
        string type = null;//定义初始化信息点类型
        IEnvelope pEnvelope;//定义全局变量用于地图显示
        bool cSearch = false;　//定义bool变量用于启动缓冲区查询功能
        //定义文本框的标识
        //int textboxflag = 0;
        //IEnvelope ipEnv;
        //最短路径分析用得到的全局变量
        IFeatureWorkspace pFeatureWorkspace = null;
        INetworkDataset pNetworkDataset = null;
        IFeatureClass pInputFC = null;
        //IFeatureClass pVertexFC = null;
        IActiveView pActiveView = null;
        IMap pMap = null;
        IGraphicsContainer pGraphicsContainer = null;
        //IFeatureDataset pFeatureDataset = null;
        INAContext pNAContext = null;

        public IPoint pPoint = new ESRI.ArcGIS.Geometry.Point();//点坐标变量
        private ESRI.ArcGIS.Controls.IMapControl3 m_mapControl = null;//地图控制变量
        private string currentoperation = "";//地图当前操作类型tag
        //bool blnIsIdentifyEnable = false;//定义bool变量用于启动查看属性功能
        bool bSearch = false;　//定义bool变量用于启动多边形查询功能
        public string dianming = null;
        //右键菜单
        //private IToolbarMenu m_menuMap = null;

        #region SearchInfo构造函数
        /// <summary>
        /// 无传入参数的构造函数
        /// </summary>
        public SearchInfo()
        {
            this.username = null;
            InitializeComponent();

        }
        /// <summary>
        /// 传入用户名称的查询界面构造函数
        /// </summary>
        /// <param name="uName"></param>
        public SearchInfo(string uName)
        {
            this.username = uName;
            InitializeComponent();
        }

        /// <summary>
        /// 传入用户名称和类别的构造函数
        /// </summary>
        /// <param name="uName"></param>
        /// <param name="type"></param>
        public SearchInfo(string uName, string type)
        {
            this.username = uName;
            InitializeComponent();
            //获取特定类型的所有点
            this.type = type;//定义初始显示类别

        }
        #endregion

        #region SearchInfo 界面初始化
        /// <summary>
        /// SearchInfo界面初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchInfo_Load(object sender, EventArgs e)
        {
            //加载mxd文件
            loadMapDocunment();
            axMapControl1.Extent = axMapControl1.FullExtent;
            pEnvelope = axMapControl1.FullExtent;
            m_mapControl = (IMapControl3)axMapControl1.Object;
            //创建右键菜单
            //m_menuMap = new ToolbarMenuClass();
            listBoxDisplay(type);
            //初始化地图、网络数据集
            Initial();
            symbolBox2.Visible = false;
            tbOrigin.Visible = false;
            btRoute.Visible = false;
            //初始化窗体



        }
        #endregion

        #region 菜单栏功能键
        //返回键
        private void lblReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            IntroForm introForm = new IntroForm(this.username);
            introForm.ShowDialog();
        }

        // RadialMenu2 - 按学部获取点信息
        private void radialMenu2_ItemClick(object sender, EventArgs e)
        {
            RadialMenuItem item = sender as RadialMenuItem;
            int DistrictFlag = 0;//定义校区标识
            switch (item.Text)
            {
                case "文理学部": DistrictFlag = 1; break;
                case "工学部": DistrictFlag = 2; break;
                case "信息学部": DistrictFlag = 3; break;
                case "医学部": DistrictFlag = 4; break;
                default: break;
            }
            //地图清理
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            //axMapControl1.Extent = axMapControl1.FullExtent;
            Clearpoly();
            Clearinfo();
            Cleargraphics();

            //属性选择
            ILayer layer = getlayerbyname("point");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            int fieldIndex1 = getFieldindex(pfeatureLayer.FeatureClass, "名称");//获得图层的feature
            int fieldIndex2 = getFieldindex(pfeatureLayer.FeatureClass, "Type");
            int fieldIndex3 = getFieldindex(pfeatureLayer.FeatureClass, "地址");
            int fieldIndex4 = getFieldindex(pfeatureLayer.FeatureClass, "联系电话");
            IQueryFilter pqueryFilter = new QueryFilter();//属性值过滤
            pqueryFilter.WhereClause = "校区 =" +DistrictFlag + "";//sql中的where语句
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;

            pfeatureCursor = pfeatureClass.Search(pqueryFilter, true);//查询满足要求的项
            pfeature = pfeatureCursor.NextFeature();
            if (pfeature == null)
                MessageBox.Show("无搜索结果");
            else
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                //在groupPanel中输出第一个点的详细信息
                groupPanel1.Text = pfeature.get_Value(fieldIndex1).ToString();
                lblType.Text = "类型：" + pfeature.get_Value(fieldIndex2).ToString();
                lblAddress.Text = "地址：" + pfeature.get_Value(fieldIndex3).ToString();
                lblPhone.Text = "电话：" + pfeature.get_Value(fieldIndex4).ToString();
                //在地图上输出
                IEnvelope envelope = (IEnvelope)new Envelope();
                axMapControl1.Map.SelectFeature(pfeatureLayer, pfeature);
                envelope.XMax = pfeature.Extent.XMax + 0.001;
                envelope.XMin = pfeature.Extent.XMin - 0.001;
                envelope.YMax = pfeature.Extent.YMax + 0.001;
                envelope.YMin = pfeature.Extent.YMin - 0.001;
                axMapControl1.Extent = envelope;
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pfeature = pfeatureCursor.NextFeature();//second
            }
            //依次在ListBox中输出所有该类别的结果
            while (pfeature != null)
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                //axMapControl1.Map.SelectFeature(layer, pfeature);//在地图axMapControl1上显示查询的结果
                //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pfeature = pfeatureCursor.NextFeature();//依次输出所有查询结果
            }

        }


        //radialMenu1 - 按类型筛选点Type
        private void radialMenu1_ItemClick(object sender, EventArgs e)
        {
            RadialMenuItem item = sender as RadialMenuItem;
            string TypeFlag = null;//定义类型标识
            switch (item.Text)//item和Type对照
            {
                case "美食": TypeFlag = "食堂"; break;
                case "购物": TypeFlag = "商店"; break;
                case "景点": TypeFlag = "旅游景点"; break;
                case "自习": TypeFlag = "教学楼"; break;//修改字段对应名称
                case "图书馆": TypeFlag = "图书馆"; break;
                default: break;
            }
           //地图清理
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            //axMapControl1.Extent = axMapControl1.FullExtent;
            Clearpoly();
            Clearinfo();
            Cleargraphics();

            //属性选择
            ILayer layer = getlayerbyname("point");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            int fieldIndex1 = getFieldindex(pfeatureLayer.FeatureClass, "名称");//获得图层的feature
            int fieldIndex2 = getFieldindex(pfeatureLayer.FeatureClass, "Type");
            int fieldIndex3 = getFieldindex(pfeatureLayer.FeatureClass, "地址");
            int fieldIndex4 = getFieldindex(pfeatureLayer.FeatureClass, "联系电话");
            IQueryFilter pqueryFilter = new QueryFilter();//属性值过滤
            pqueryFilter.WhereClause = "Type ='" + TypeFlag + "'";//sql中的where语句
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;

            pfeatureCursor = pfeatureClass.Search(pqueryFilter, true);//查询满足要求的项
            pfeature = pfeatureCursor.NextFeature();
            if (pfeature == null)
                MessageBox.Show("无搜索结果");
            else
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                //在groupPanel中输出第一个点的详细信息
                groupPanel1.Text = pfeature.get_Value(fieldIndex1).ToString();
                lblType.Text = "类型：" + pfeature.get_Value(fieldIndex2).ToString();
                lblAddress.Text = "地址：" + pfeature.get_Value(fieldIndex3).ToString();
                lblPhone.Text = "电话：" + pfeature.get_Value(fieldIndex4).ToString();
                //在地图上输出
                IEnvelope envelope = (IEnvelope)new Envelope();
                axMapControl1.Map.SelectFeature(pfeatureLayer, pfeature);
                envelope.XMax = pfeature.Extent.XMax + 0.001;
                envelope.XMin = pfeature.Extent.XMin - 0.001;
                envelope.YMax = pfeature.Extent.YMax + 0.001;
                envelope.YMin = pfeature.Extent.YMin - 0.001;
                axMapControl1.Extent = envelope;
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pfeature = pfeatureCursor.NextFeature();//second
            }
            //依次在ListBox中输出所有该类别的结果
            while (pfeature != null)
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                //axMapControl1.Map.SelectFeature(layer, pfeature);//在地图axMapControl1上显示查询的结果
                //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pfeature = pfeatureCursor.NextFeature();//依次输出所有查询结果
            }

        }
       

        #endregion

        #region 地图工具栏
        //放大
        private void bubbleButton1_Click_1(object sender, ClickEventArgs e)
        {
            currentoperation = "放大";
            MessageLabel.Text = "当前操作：" + currentoperation;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;
        }
        //缩小
        private void bubbleButton2_Click_1(object sender, ClickEventArgs e)
        {
            currentoperation = "缩小";
            MessageLabel.Text = "当前操作：" + currentoperation;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;
        }
        //全图
        private void bubbleButton3_Click_1(object sender, ClickEventArgs e)
        {
            currentoperation = "全图";
            MessageLabel.Text = "当前操作：" + currentoperation;
            axMapControl1.Extent = pEnvelope;
            //axMapControl2.Extent = axMapControl2.FullExtent;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //漫游
        private void bubbleButton4_Click_1(object sender, ClickEventArgs e)
        {
            currentoperation = "漫游";
            MessageLabel.Text = "当前操作：" + currentoperation;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
        }
        //点选
        private void bubbleButton5_Click_1(object sender, ClickEventArgs e)
        {
            currentoperation = "点选";
            MessageLabel.Text = "当前操作：" + currentoperation;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //地图输出
        private void bubbleButton6_Click_1(object sender, ClickEventArgs e)
        {
            //输出地图
            try
            {
                SaveFileDialog exportdialog = new SaveFileDialog();
                exportdialog.FileName = "";
                exportdialog.Filter = "Jpg图像(*.jpg)|*.jpg|TIF图像(*.tif)|*.tif|PDF文档(*.PDF)|*.PDF||";
                if (exportdialog.ShowDialog() == DialogResult.OK)
                {
                    double iScreenDisplayResolution = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.Resolution;
                    IExporter pExporter = null;
                    if (exportdialog.FilterIndex == 0)
                    {
                        pExporter = new JpegExporterClass();
                    }
                    else if (exportdialog.FilterIndex == 1)
                    {
                        pExporter = new TiffExporterClass();
                    }
                    else if (exportdialog.FilterIndex == 2)
                    {
                        pExporter = new PDFExporterClass();
                    }
                    pExporter.ExportFileName = exportdialog.FileName;
                    pExporter.Resolution = (short)iScreenDisplayResolution;
                    tagRECT deviceRect = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame();
                    IEnvelope pDeviceEnvelope = new EnvelopeClass();
                    pDeviceEnvelope.PutCoords(deviceRect.left, deviceRect.bottom, deviceRect.right, deviceRect.top);
                    pExporter.PixelBounds = pDeviceEnvelope;
                    ITrackCancel Cancle = new CancelTrackerClass();
                    axMapControl1.ActiveView.Output(pExporter.StartExporting(), pExporter.Resolution, ref deviceRect, axMapControl1.ActiveView.Extent, Cancle);
                    Application.DoEvents();
                    pExporter.FinishExporting();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);

            }
        }

        //缓冲区查询
        private void bubbleButton7_Click(object sender, ClickEventArgs e)
        {
            if (gBool)
            {
                Clearpoly();
            }
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            //向地图控件添加内存图层
            IFeatureLayer pFeatureLayer = AddFeatureLayerByMemoryWS(axMapControl1, axMapControl1.SpatialReference);
            axMapControl1.AddLayer(pFeatureLayer, 0);
            //设置鼠标样式为十字丝
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            currentoperation = "缓冲区查询";
            MessageLabel.Text = "当前操作：" + currentoperation;
            //启动缓冲区查询功能
            cSearch = true;
        }
        //多边形查询
        private void bubbleButton8_Click(object sender, ClickEventArgs e)
        {
            if (!gBool)//无需清除多边形
            {
                //不改变鼠标样式，此时鼠标可以制造选区
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                currentoperation = "多边形查询";
                IFeatureLayer pFeatureLayer = AddFeatureLayerByMemoryWS(axMapControl1, axMapControl1.SpatialReference);
                axMapControl1.AddLayer(pFeatureLayer);//添加临时图层
            }
            else
            {
                axMapControl1.Map.ClearSelection();
                Clearpoly();
                Cleargraphics();
                //不改变鼠标样式，此时鼠标可以制造选区
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                currentoperation = "多边形查询";
                IFeatureLayer pFeatureLayer = AddFeatureLayerByMemoryWS(axMapControl1, axMapControl1.SpatialReference);
                axMapControl1.AddLayer(pFeatureLayer);//添加临时图层
            }

        }
        //清除所有选择及半透明图层
        private void bubbleButton9_Click(object sender, ClickEventArgs e)
        {
            listBoxAdv1.Items.Clear();//清除ListBox
            Clearpoly();//清除半透明图层
            Cleargraphics();//清除绘图
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Extent = pEnvelope;//恢复地图显示状态为全图
        }
        #endregion

        #region 地图状态栏
        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            // 显示当前比例尺
            scaleLable.Text = " 比例尺 1:" + ((long)this.axMapControl1.MapScale).ToString();
            // 显示当前坐标
            Coordinatelable.Text = " 当前坐标 X = " + e.mapX.ToString("#0.00000") + " Y = " + e.mapY.ToString("#0.00000") + " " + this.axMapControl1.MapUnits.ToString().Substring(4);

            //鼠标滑过显示名称
            axMapControl1.ShowMapTips = true;
            IFeatureLayer pFeatureLayer = getlayerbyname("point") as IFeatureLayer;//修改图层名称
            pFeatureLayer.DisplayField = "名称";//修改字段名称
            pFeatureLayer.ShowTips = true;
        }
        #endregion



        /// <summary>
        /// 传入名称，模糊查询结果在ListBox中输出，并显示第一个查询结果的详细信息和地图位置
        /// </summary>
        /// <param name="name"></param>
        public void Namequery(string name)
        {
            axMapControl1.Map.ClearSelection();
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            axMapControl1.Extent = pEnvelope;//全图显示
            //Clearpoly();
            listBoxAdv1.Items.Clear();
            //Cleargraphics();

            ILayer layer = getlayerbyname("point");//修改图层名称
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            int fieldIndex1 = getFieldindex(pfeatureLayer.FeatureClass, "名称");//获得图层的feature
            int fieldIndex2 = getFieldindex(pfeatureLayer.FeatureClass, "Type");
            int fieldIndex3 = getFieldindex(pfeatureLayer.FeatureClass, "地址");
            int fieldIndex4 = getFieldindex(pfeatureLayer.FeatureClass,"联系电话");
            IQueryFilter pqueryFilter = new QueryFilter();//属性值过滤
            pqueryFilter.WhereClause = "名称 like '%" + name + "%'";//sql中的where语句
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;

            pfeatureCursor = pfeatureClass.Search(pqueryFilter, false);//查询满足要求的项
            pfeature = pfeatureCursor.NextFeature();

            
            if (pfeature != null)
            {
                //添加第一个搜索结果到ListBox并在地图上输出
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                string temp = pfeature.get_Value(fieldIndex1).ToString();
                //在地图上显示第一个查询结果
                IEnvelope envelope = (IEnvelope)new Envelope();
                axMapControl1.Map.SelectFeature(pfeatureLayer, pfeature);
                envelope.XMax = pfeature.Extent.XMax + 0.001;
                envelope.XMin = pfeature.Extent.XMin - 0.001;
                envelope.YMax = pfeature.Extent.YMax + 0.001;
                envelope.YMin = pfeature.Extent.YMin - 0.001;
                axMapControl1.Extent = envelope;
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                //在groupPanel中显示第一个查询结果
                groupPanel1.Text = pfeature.get_Value(fieldIndex1).ToString();
                lblType.Text = "类型：" + pfeature.get_Value(fieldIndex2).ToString();
                lblAddress.Text = "地址：" + pfeature.get_Value(fieldIndex3).ToString();
                lblPhone.Text = "电话：" + pfeature.get_Value(fieldIndex4).ToString();
                pfeature = pfeatureCursor.NextFeature();//second
            }
            else
            {
                MessageBox.Show("无搜索结果");
            }

            while (pfeature != null)
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                axMapControl1.Map.SelectFeature(layer, pfeature);//在地图axMapControl1上显示查询的结果
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pfeature = pfeatureCursor.NextFeature();//依次输出所有查询结果
            }

        }

        

        /// <summary>
        /// 清除名称为 TransTemp 的半透明多边形图层
        /// </summary>
        private void Clearpoly()
        {
            ILayer layer = getlayerbyname("TransTemp");//半透明图层名称
            while (layer != null)
            {
                axMapControl1.Map.DeleteLayer(layer);
                layer = getlayerbyname("TransTemp");
                //IFeatureLayer flayer = layer as IFeatureLayer;
                //IFeatureClass fclass = flayer.FeatureClass;
                //IFeatureCursor fcursor = fclass.Search(null, true);
                //IFeature feature = fcursor.NextFeature();
                //while (feature != null)
                //{
                //    feature.Delete();
                //    feature = fcursor.NextFeature();
                //}
            }
        }
        /// <summary>
        /// 清除图形元素
        /// </summary>
        private void Cleargraphics()
        {
            IGraphicsContainer pgraphicscontainer = axMapControl1.Map as IGraphicsContainer;
            pgraphicscontainer.DeleteAllElements();
            IActiveView pactiveview = axMapControl1.Map as IActiveView;
            pactiveview.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }

        
        

        #region 地图输入、属性/图层获取
        /// <summary>
        /// 加载地图文件
        /// </summary>
        public void loadMapDocunment()
        {
            string filePath = Application.StartupPath + @"\\配好.mxd";
            axMapControl1.LoadMxFile(filePath, 0, Type.Missing);
            //axMapControl2.LoadMxFile(filePath, 0, Type.Missing);
        }

        /// <summary>
        /// 从axMapControl1的图层中获取指定名称的图层
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public ILayer getlayerbyname(string str)
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
        /// 获取属性字段的索引号
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

        #endregion


        #region OnMouseDown





        /// <summary>
        /// 按几何图形查询,并将图形范围内的查询结果依次放入ListBox
        /// </summary>
        /// <param name="pGeometry"></param>

        private void QueryByGeometry(IGeometry pGeometry)
        {
            axMapControl1.Map.ClearSelection();
            listBoxAdv1.Items.Clear();
            //将地图显示范围更新为当前缓冲区查询范围    
            axMapControl1.Extent = pGeometry.Envelope;
            

            ISpatialFilter pSpatialFilter = new SpatialFilter();
            pSpatialFilter.Geometry = pGeometry;//空间查询范围
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureLayer pFeatLayer = getlayerbyname("point") as IFeatureLayer;//修改图层名称
            //图层“point”中位于框内的部分
            IFeatureCursor pFeatCursor = pFeatLayer.FeatureClass.Search(pSpatialFilter, true);
            IFeature pFeat = pFeatCursor.NextFeature();

            //修改属性字段名称
            int fieldIndex1 = getFieldindex(pFeatLayer.FeatureClass, "名称");//属性字段名
            int fieldIndex2 = getFieldindex(pFeatLayer.FeatureClass, "Type");//属性字段名
            int fieldIndex3 = getFieldindex(pFeatLayer.FeatureClass, "地址");//属性字段名
            int fieldIndex4 = getFieldindex(pFeatLayer.FeatureClass, "联系电话");//属性字段名
            
            if (pFeat != null)
            {
                axMapControl1.Map.SelectFeature(pFeatLayer, pFeat);//使所选择的要素高亮显示
                listBoxAdv1.Items.Add(pFeat.get_Value(fieldIndex1));
                //将查询结果中第一项的详细信息显示在groupPanel中
                groupPanel1.Text = pFeat.get_Value(fieldIndex1).ToString();               
                lblType.Text = "类型： " + pFeat.get_Value(fieldIndex2).ToString();
                lblAddress.Text = "地址： " + pFeat.get_Value(fieldIndex3).ToString();                
                lblPhone.Text = "电话： " + pFeat.get_Value(fieldIndex4).ToString();
                pFeat = pFeatCursor.NextFeature();
            }         
            while (pFeat != null)
            {
                axMapControl1.Map.SelectFeature(pFeatLayer, pFeat);//使所选择的要素高亮显示
                //将查询项加入ListBox
                listBoxAdv1.Items.Add(pFeat.get_Value(fieldIndex1));
                pFeat = pFeatCursor.NextFeature();
            }
            //刷新地图显示

            axMapControl1.Refresh();
        }
        //在地图控件上添加透明临时图元
        private void AddTransTempEle(AxMapControl pMapCtrl, IGeometry pGeo, bool bAutoClear)
        {
            try
            {
                if (pMapCtrl == null) return;
                if (pGeo == null) return;
                if (pGeo.IsEmpty) return;
                IGeometry pPolygon = null;
                if (pGeo is IEnvelope)
                {
                    object Miss = Type.Missing;
                    pPolygon = new PolygonClass();
                    IGeometryCollection pGeoColl = pPolygon as IGeometryCollection;
                    pGeoColl.AddGeometry(pGeo, ref Miss, ref Miss);
                }
                else if (pGeo is IPolygon)
                {
                    (pGeo as ITopologicalOperator).Simplify();
                    pPolygon = pGeo;
                }
                else
                {
                    MessageBox.Show("几何实体类型不匹配", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //获取透明要素层
                IFeatureLayer pFlyr = null;
                for (int i = 0; i < pMapCtrl.LayerCount; i++)
                {
                    if (pMapCtrl.get_Layer(i).Name == "TransTemp")
                    {
                        pFlyr = pMapCtrl.get_Layer(i) as IFeatureLayer;
                        break;
                    }
                }
                //透明临时层不存在需要创建
                if (pFlyr == null)
                {
                    pFlyr = AddFeatureLayerByMemoryWS(pMapCtrl, pMapCtrl.SpatialReference);
                    if (pFlyr == null)
                    {
                        MessageBox.Show("创建透明临时图层发生异常", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    axMapControl1.AddLayer(pFlyr, axMapControl1.LayerCount);
                }
                IFeatureClass pFC = pFlyr.FeatureClass;
                if (bAutoClear)
                {
                    if (pFC.FeatureCount(null) > 0)
                    {
                        IFeatureCursor pFCursor = pFC.Search(null, false);
                        if (pFCursor != null)
                        {
                            IFeature pFeature = pFCursor.NextFeature();
                            if (pFeature != null)
                            {
                                while (pFeature != null)
                                {
                                    pFeature.Delete();
                                    pFeature = pFCursor.NextFeature();
                                }
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFCursor);
                        }
                    }
                }
                //创建要素

                IFeature pNFeature = pFC.CreateFeature();
                pNFeature.Shape = pPolygon;
                pNFeature.set_Value(pFC.FindField("Code"), "1");
                pNFeature.Store();
                pMapCtrl.Refresh(esriViewDrawPhase.esriViewGeography, pFlyr, pFlyr.AreaOfInterest);
            }
            catch (Exception Err)
            {

                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }
        /// <summary>
        /// 清理、回复详情模块至默认状态
        /// </summary>
        private void Clearinfo()
        {
            listBoxAdv1.Items.Clear();
            lblType.Text = "类型： ";
            lblAddress.Text = "地址： ";
            lblPhone.Text = "联系电话： ";
            //pictureBox1.ImageLocation = "";
            groupPanel1.Text = "名称";
        }

        #endregion
        //创建内存图层
        private IFeatureLayer AddFeatureLayerByMemoryWS(AxMapControl pMapCtrl, ISpatialReference pSReference)
        {
            try
            {
                if (pMapCtrl == null)
                    return null;
                //创建新的内存工作空间
                IWorkspaceFactory pWSF = new InMemoryWorkspaceFactoryClass();
                IWorkspaceName pWSName = pWSF.Create("", "Temp", null, 0);
                IName pName = (IName)pWSName;
                IWorkspace pMemoryWS = (IWorkspace)pName.Open();

                IField oField = new FieldClass();
                IFields oFields = new FieldsClass();
                IFieldsEdit oFieldsEdit = null;
                IFieldEdit oFieldEdit = null;
                IFeatureClass oFeatureClass = null;
                IFeatureLayer oFeatureLayer = null;
                try
                {
                    oFieldsEdit = oFields as IFieldsEdit;
                    oFieldEdit = oField as IFieldEdit;
                    oFieldEdit.Name_2 = "OBJECTID";
                    oFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                    oFieldEdit.IsNullable_2 = false;
                    oFieldEdit.Required_2 = false;
                    oFieldsEdit.AddField(oField);

                    oField = new FieldClass();
                    oFieldEdit = oField as IFieldEdit;
                    IGeometryDef pGeoDef = new GeometryDefClass();
                    IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
                    pGeoDefEdit.AvgNumPoints_2 = 5;
                    pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
                    pGeoDefEdit.GridCount_2 = 1;
                    pGeoDefEdit.HasM_2 = false;
                    pGeoDefEdit.HasZ_2 = false;
                    pGeoDefEdit.SpatialReference_2 = pSReference;
                    oFieldEdit.Name_2 = "SHAPE";
                    oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                    oFieldEdit.GeometryDef_2 = pGeoDef;
                    oFieldEdit.IsNullable_2 = true;
                    oFieldEdit.Required_2 = true;
                    oFieldsEdit.AddField(oField);

                    oField = new FieldClass();
                    oFieldEdit = oField as IFieldEdit;
                    oFieldEdit.Name_2 = "Code";
                    oFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                    oFieldEdit.IsNullable_2 = true;
                    oFieldsEdit.AddField(oField);

                    //创建要素类
                    oFeatureClass = (pMemoryWS as IFeatureWorkspace).CreateFeatureClass("Temp", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                    oFeatureLayer = new FeatureLayerClass();
                    oFeatureLayer.Name = "TransTemp";
                    oFeatureLayer.FeatureClass = oFeatureClass;

                    //创建唯一值符号化对象
                    IUniqueValueRenderer pURender = new UniqueValueRendererClass();
                    pURender.FieldCount = 1;
                    pURender.set_Field(0, "Code");
                    pURender.UseDefaultSymbol = false;
                    ISimpleFillSymbol pFillSym = new SimpleFillSymbolClass();
                    pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;
                    //设置半透明颜色
                    IRgbColor pColor = new RgbColorClass();
                    pColor.Green = 255;
                    pFillSym.Color = pColor;
                    pURender.AddValue("1", "", pFillSym as ISymbol);
                    pFillSym = new SimpleFillSymbolClass();
                    pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;

                    (oFeatureLayer as IGeoFeatureLayer).Renderer = pURender as IFeatureRenderer;
                    ILayerEffects pLyrEffect = oFeatureLayer as ILayerEffects;
                    //透明度
                    pLyrEffect.Transparency = 40;
                }
                catch (Exception Err)
                {
                    MessageBox.Show(Err.Message);
                }

                finally
                {
                    try
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oField);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFields);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldsEdit);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldEdit);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pName);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pWSF);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pWSName);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pMemoryWS);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFeatureClass);
                    }
                    catch
                    {

                    }
                    GC.Collect();
                }
                return oFeatureLayer;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }
        // 获取查询到的要素
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
        //地图点击事件
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            IGeometry pGeometry = null;
            IEnvelope objEnvelope = null;
            
            switch (currentoperation)
            {
                case "放大":                
                    objEnvelope = axMapControl1.TrackRectangle();//矩形框选区域
                    if (!objEnvelope.IsEmpty)
                    {
                        axMapControl1.Extent = objEnvelope;
                    }
                    else
                    {
                        pPoint.X = e.mapX;
                        pPoint.Y = e.mapY;
                        pPoint.Z = 0;
                        objEnvelope = axMapControl1.Extent;
                        objEnvelope.CenterAt(pPoint);
                        objEnvelope.Expand(0.5, 0.5, true);
                        axMapControl1.Extent = objEnvelope;
                    }
                    break;
                case "缩小":                 
                    objEnvelope = axMapControl1.TrackRectangle();
                    IEnvelope currentExtent = this.axMapControl1.Extent;
                    double dXmin = 0, dYmin = 0, dXmax = 0, dYmax = 0, dHeight = 0, dWidth = 0;
                    if (!objEnvelope.IsEmpty)
                    {
                        dWidth = currentExtent.Width * (currentExtent.Width / objEnvelope.Width);
                        dHeight = currentExtent.Height * (currentExtent.Height / objEnvelope.Height);

                        dXmin = currentExtent.XMin - ((objEnvelope.XMin - currentExtent.XMin) * (currentExtent.Width / objEnvelope.Width));
                        dYmin = currentExtent.YMin - ((objEnvelope.YMin - currentExtent.YMin) * (currentExtent.Height / objEnvelope.Height));
                        dXmax = dXmin + dWidth;
                        dYmax = dYmin + dHeight;
                        objEnvelope.PutCoords(dXmin, dYmin, dXmax, dYmax);
                        this.axMapControl1.Extent = objEnvelope;//显示范围
                    }
                    else
                    {
                        pPoint.X = e.mapX;
                        pPoint.Y = e.mapY;
                        pPoint.Z = 0;
                        objEnvelope = axMapControl1.Extent;
                        objEnvelope.CenterAt(pPoint);
                        objEnvelope.Expand(2, 2, true);
                        axMapControl1.Extent = objEnvelope;
                    }
                    break;
                case "漫游":
                   // if (blnIsIdentifyEnable)//???
                     //   blnIsIdentifyEnable = false;
                    axMapControl1.Pan();
                    break;
                case "点选":
                    //if (blnIsIdentifyEnable)
                      //  blnIsIdentifyEnable = false;
                    pPoint.PutCoords(e.mapX, e.mapY); // 也就是 pPoint.X = e.mapX;pPoint.Y = e.mapY;
                    ITopologicalOperator pTopo = pPoint as ITopologicalOperator;
                    pGeometry = (IGeometry)pTopo.Buffer(0.001);//点缓冲
                    QueryByGeometry(pGeometry);
                    
                    break;
                case "缓冲区查询":
                    pPoint.PutCoords(e.mapX, e.mapY);

                    Bufferquery(0.004, pPoint);
                    gBool = true;
                    currentoperation = "";//操作tag恢复初始化
                    break;
                case "多边形查询":
                    //绘制多边形进行选取
                    ILayer pLayer = getlayerbyname("point");
                    IGeometry pGeo = axMapControl1.TrackPolygon();
                    QueryByGeometry(pGeo);
                    AddTransTempEle(axMapControl1, pGeo, false);
                    axMapControl1.Extent = pGeo.Envelope;
                    axMapControl1.ActiveView.Refresh();
                   
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCustom;
                    gBool = true;
                    //操作恢复初始化
                    currentoperation = "";
                    break;
            }
           
        }
        

        //搜索键 - 名称查询
        private void btSearch_Click(object sender, EventArgs e)
        {
            if (tbSearch.Text == "")
            {
                MessageBox.Show("请输入目的地名称：", "提示");
            }
            else
            {
                string strname = tbSearch.Text.Trim();
                Namequery(strname);
                tbOrigin.Visible = true;
                btRoute.Visible = true;
                

            }
        }
       
        /// <summary>
        /// 按点和距离，缓冲区查询
        /// </summary>
        /// <param name="dis"></param>
        /// <param name="point"></param>
        public void Bufferquery(double dis, IPoint point)
        {
            axMapControl1.Map.ClearSelection();
            listBoxAdv1.Items.Clear();

            ITopologicalOperator topoope = point as ITopologicalOperator;
            IGeometry pgeometry = topoope.Buffer(dis);
            //添加半透名临时图形
            AddTransTempEle(axMapControl1, pgeometry, false);
            //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            //IFeatureLayer tempFeatureLayer = getlayerbyname("TransTemp") as IFeatureLayer;
            //axMapControl1.AddLayer(tempFeatureLayer,axMapControl1.LayerCount);
            IFeatureLayer pFeatureLayer = getlayerbyname("point") as IFeatureLayer;
            //执行查询获取符合条件的要素
            List<IFeature> pFList = GetSearchFeatures(pFeatureLayer, pgeometry);
            for (int i = 0; i < pFList.Count; i++)
            {

                IFeature pFeature = pFList[i];
                axMapControl1.Map.SelectFeature(pFeatureLayer, pFeature);
                int fieldIndex = getFieldindex(pFeatureLayer.FeatureClass, "名称");
                listBoxAdv1.Items.Add(pFeature.get_Value(fieldIndex));

                if (i == 0)
                {
                    //在groupPanel中显示第一个查询结果的详情
                    int fieldIndex2 = getFieldindex(pFeatureLayer.FeatureClass, "Type");
                    int fieldIndex3 = getFieldindex(pFeatureLayer.FeatureClass, "地址");
                    int fieldIndex4 = getFieldindex(pFeatureLayer.FeatureClass, "联系电话");
                    groupPanel1.Text = pFeature.get_Value(fieldIndex).ToString();
                    lblType.Text = "类型：" + pFeature.get_Value(fieldIndex2).ToString();
                    lblAddress.Text = "地址：" + pFeature.get_Value(fieldIndex3).ToString();
                    lblPhone.Text = "联系电话：" + pFeature.get_Value(fieldIndex4).ToString();

                }
            }
            axMapControl1.Extent = pgeometry.Envelope;
            //cSearch = false;//用于启动缓冲区查询的boolean变量
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
        }
        #region ListBox中点信息精确查询

        private void groupPanel1_Click(object sender, EventArgs e)
        {
            //检查有无选中信息点
            if (groupPanel1.Text == "名称")
            {
                MessageBox.Show("请先选择地点。", "提示");
            }
            else if (typeFlag == "教学楼")
            {
                StudyBuilding studyBuilding = new StudyBuilding(groupPanel1.Text, this);
                studyBuilding.ShowDialog();

            }
            else if (groupPanel1.Text == "图书馆文理分馆(总馆)")
            {
                Library userLibrary = new Library(this);
                userLibrary.ShowDialog();
            }
            else
            {
                ObjectDetails objectDetail = new ObjectDetails(groupPanel1.Text, this);
                objectDetail.ShowDialog();
            }
        }
        



        /// <summary>
        /// 传入类型参数，在point属性表中筛选该类型并在ListBox中输出
        /// </summary>
        /// <param name="type"></param>
        public void listBoxDisplay(string type)
        {
            ILayer layer = getlayerbyname("point");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            int fieldIndex1 = getFieldindex(pfeatureLayer.FeatureClass, "名称");//获得图层的feature
            IQueryFilter pqueryFilter = new QueryFilter();//属性值过滤
            pqueryFilter.WhereClause = "Type = '" + type + "'";//sql中的where语句
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;
            pfeatureCursor = pfeatureClass.Search(pqueryFilter, true);//查询满足要求的项
            pfeature = pfeatureCursor.NextFeature();
            if (pfeature == null)
                MessageBox.Show("无搜索结果");
            while (pfeature != null)
            {
                listBoxAdv1.Items.Add(pfeature.get_Value(fieldIndex1));//在listBox1中显示查询的属性结果
                axMapControl1.Map.SelectFeature(pfeatureLayer, pfeature);//在地图axMapControl1上显示查询的结果 
                pfeature = pfeatureCursor.NextFeature();//依次输出所有查询结果
            }


        }
        //路径搜索启动
        private void btRoute_Click(object sender, EventArgs e)
        {
            if (tbSearch.Text == "" || tbOrigin.Text == "")
                MessageBox.Show("请输入必要信息！", "提示");
            else
            {
                //textboxflag = 0;
                this.Cursor = Cursors.WaitCursor;
                //lstOutput.Items.Clear();
                //lstOutput.Items.Add("分析中...");
                string str = "名称 LIKE '%" + tbOrigin.Text + "%' OR 名称 LIKE '%" + tbSearch.Text + "%'";
                LoadNANetWorkLocations("Stops", pInputFC, 80, str);
                ESRI.ArcGIS.SystemUI.ICommand pCommand = new ControlsNetworkAnalystSolveCommandClass();
                pCommand.OnCreate(axMapControl1.Object);
                pCommand.OnClick();
                if (axMapControl1.Map.FeatureSelection == null)
                {
                    IFeatureLayer pLayer = getlayerbyname("point") as IFeatureLayer;
                    List<IFeature> pFList = GetAttributeSearchFeatures(pLayer, str);
                    for (int i = 0; i < pFList.Count; i++)
                    {
                        IFeature pFeature = pFList[i];
                        axMapControl1.Map.SelectFeature(pLayer, pFeature);
                    }
                    ESRI.ArcGIS.SystemUI.ICommand pCommandZoom = new ControlsZoomToSelectedCommandClass();
                    pCommandZoom.OnCreate(axMapControl1.Object);
                    pCommandZoom.OnClick();
                }
                else
                {
                    axMapControl1.Map.ClearSelection();
                    IFeatureLayer pLayer = getlayerbyname("point") as IFeatureLayer;
                    List<IFeature> pFList = GetAttributeSearchFeatures(pLayer, str);
                    for (int i = 0; i < pFList.Count; i++)
                    {
                        IFeature pFeature = pFList[i];
                        axMapControl1.Map.SelectFeature(pLayer, pFeature);
                    }
                    ESRI.ArcGIS.SystemUI.ICommand pCommandZoom = new ControlsZoomToSelectedCommandClass();
                    pCommandZoom.OnCreate(axMapControl1.Object);
                    pCommandZoom.OnClick();
                }
                this.Cursor = Cursors.Default;
                symbolBox2.Visible = true;
            }
        }


        // 获取属性查询到的要素
        private List<IFeature> GetAttributeSearchFeatures(IFeatureLayer pLayer, string str)
        {
            try
            {
                ////创建要素列表
                List<IFeature> pList = new List<IFeature>();
                IQueryFilter pQueryFilter = new QueryFilterClass();
                pQueryFilter.WhereClause = str;
                IFeatureCursor pFeatureCurse = pLayer.Search(pQueryFilter, false);
                IFeature pFeature = pFeatureCurse.NextFeature();
                while (pFeature != null)
                {
                    //将获取要素对象添加入要素列表
                    pList.Add(pFeature);
                    pFeature = pFeatureCurse.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCurse);
                return pList;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }

        //初始化地图、网络数据集
        private void Initial()
        {

            //this.axMapControl1.ActiveView.Clear();
            //axMapControl1.ActiveView.Refresh();
            //打开并定义工作数据集
            pFeatureWorkspace = OpenWorkspace("底图数据库.gdb") as IFeatureWorkspace;
            //打开网络数据集
            pNetworkDataset = OpenNetworkDataset_Other(pFeatureWorkspace as IWorkspace, "步行路径", "地理要素");
            //定义上下文分析
            pNAContext = CreateNAContext(pNetworkDataset);
            //定义输入要素类
            pInputFC = pFeatureWorkspace.OpenFeatureClass("point");
            // pVertexFC = pFeatureWorkspace.OpenFeatureClass("公交路径_Junctions");
            ////定义Stops的要素类并加载到工作空间中
            IFeatureLayer pVertexFL = new FeatureLayerClass();
            pVertexFL.FeatureClass = pFeatureWorkspace.OpenFeatureClass("point");
            //pVertexFL.Name = pVertexFL.FeatureClass.AliasName;
            //axMapControl1.AddLayer(pVertexFL, 0);
            ////定义路径的要素类并加载到工作空间中
            IFeatureLayer pRoadFL = new FeatureLayerClass();
            // pRoadFL.FeatureClass = pFeatureWorkspace.OpenFeatureClass("公交线路");
            //pRoadFL.Name = pRoadFL.FeatureClass.AliasName;
            //axMapControl1.AddLayer(pRoadFL, 0);

            ILayer pLayer;
            INetworkLayer pNetworkLayer = new NetworkLayerClass();
            pNetworkLayer.NetworkDataset = pNetworkDataset;
            pLayer = pNetworkLayer as ILayer;
            pLayer.Name = "Network Dataset";
            //axMapControl1.AddLayer(pLayer, 0);

            //Creat New Route
            INALayer naLayer = pNAContext.Solver.CreateLayer(pNAContext);
            pLayer = naLayer as ILayer;
            pLayer.Name = pNAContext.Solver.DisplayName;
            axMapControl1.AddLayer(pLayer, 5);

            pActiveView = axMapControl1.ActiveView;
            pMap = pActiveView.FocusMap;
            pGraphicsContainer = pMap as IGraphicsContainer;
        }

        //打开工作空间
        private IWorkspace OpenWorkspace(string strMDBName)
        {
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();// AccessWorkspaceFactoryClass();
            return pWorkspaceFactory.OpenFromFile(strMDBName, 0);
        }

        //打开网络数据集
        private INetworkDataset OpenNetworkDataset(IWorkspace workspace, string strNDName)
        {
            IWorkspaceExtensionManager pWorkspaceExtensionManager;
            IWorkspaceExtension pWorkspaceExtension;
            IDatasetContainer2 pDatasetContainer2;

            pWorkspaceExtensionManager = workspace as IWorkspaceExtensionManager;
            int iCount = pWorkspaceExtensionManager.ExtensionCount;
            for (int i = 0; i < iCount; i++)
            {
                pWorkspaceExtension = pWorkspaceExtensionManager.get_Extension(i);
                if (pWorkspaceExtension.Name.Equals("Network Dataset"))
                {
                    pDatasetContainer2 = pWorkspaceExtension as IDatasetContainer2;
                    return pDatasetContainer2.get_DatasetByName(esriDatasetType.esriDTNetworkDataset, strNDName) as INetworkDataset;
                }
            }
            return null;

        }
        private INetworkDataset OpenNetworkDataset_Other(IWorkspace workspace, string strNDName, string strRoadFeatureDataset)
        {
            IDatasetContainer3 pDatasetContainer3;
            IFeatureWorkspace pFeatureWorkspace = workspace as IFeatureWorkspace;
            IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(strRoadFeatureDataset);
            IFeatureDatasetExtensionContainer pFeatureDatasetExtensionContainer = pFeatureDataset as IFeatureDatasetExtensionContainer;
            IFeatureDatasetExtension pFeatureDatasetExtension = pFeatureDatasetExtensionContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
            pDatasetContainer3 = pFeatureDatasetExtension as IDatasetContainer3;

            if (pDatasetContainer3 == null)
                return null;
            IDataset pDataset = pDatasetContainer3.get_DatasetByName(esriDatasetType.esriDTNetworkDataset, strNDName);
            return pDataset as INetworkDataset;
        }

        //创建网络分析上下文
        private INAContext CreateNAContext(INetworkDataset networkDataset)
        {
            IDENetworkDataset pDENetworkDataset = GetDENetworkDataset(networkDataset);
            INASolver pNASolver = new NARouteSolverClass();
            INAContextEdit pNAContextEdit = pNASolver.CreateContext(pDENetworkDataset, pNASolver.Name) as INAContextEdit;
            pNAContextEdit.Bind(networkDataset, new GPMessagesClass());
            return pNAContextEdit as INAContext;
        }
        //数据的转化
        public IDENetworkDataset GetDENetworkDataset(INetworkDataset networkDataset)
        {
            IDatasetComponent dsComoponent;
            dsComoponent = networkDataset as IDatasetComponent;
            //Get the Data Element
            return dsComoponent.DataElement as IDENetworkDataset;
        }
        //根据点图层确定最短路径所用经历的点
        private void LoadNANetWorkLocations(string strNAClassName, IFeatureClass inputFC, double dSnapTolerance, string quarystr)
        {
            INAClass pNAClass;
            INamedSet pNamedSet;
            pNamedSet = pNAContext.NAClasses;
            pNAClass = pNamedSet.get_ItemByName(strNAClassName) as INAClass;

            //删除已存在的位置点
            pNAClass.DeleteAllRows();

            //创建NAClassLoader，设置捕捉容限值
            INAClassLoader pNAClassLoader = new NAClassLoaderClass();
            pNAClassLoader.Locator = pNAContext.Locator;
            if (dSnapTolerance > 0)
                pNAClassLoader.Locator.SnapTolerance = dSnapTolerance;
            pNAClassLoader.NAClass = pNAClass;

            //字段匹配
            INAClassFieldMap pNAClassFieldMap = new NAClassFieldMapClass();
            pNAClassFieldMap.CreateMapping(pNAClass.ClassDefinition, inputFC.Fields);
            pNAClassLoader.FieldMap = pNAClassFieldMap;

            //pNAClassFieldMap.set_MappedField("OBJECTID", "OBJECTID");
            //pNAClassLoader.FieldMap = pNAClassFieldMap;

            //加载网络位置点数据
            int iRows = 0;
            int iRowsLocated = 0;
            IQueryFilter pQueryFilter = new QueryFilterClass();
            pQueryFilter.WhereClause = quarystr;
            IFeatureCursor pFeatureCursor = pInputFC.Search(pQueryFilter, false);
            pNAClassLoader.Load((ICursor)pFeatureCursor, null, ref iRows, ref iRowsLocated);
            ((INAContextEdit)pNAContext).ContextChanged();
        }

        private void symbolBox2_Click(object sender, EventArgs e)
        {
            ESRI.ArcGIS.SystemUI.ICommand pCommand = new ControlsNetworkAnalystDirectionsCommandClass();
            pCommand.OnCreate(axMapControl1.Object);
            pCommand.OnClick();
        }

        private void listBoxAdv1_ItemClick(object sender, EventArgs e)
        {
            if (gBool) Clearpoly();
            axMapControl1.Map.ClearSelection();
            ILayer layer = getlayerbyname("point");
            IFeatureLayer pfeatureLayer = layer as IFeatureLayer;
            IFeatureClass pfeatureClass = pfeatureLayer.FeatureClass;
            IQueryFilter pqueryFilter = new QueryFilter();
            //??
            pqueryFilter.WhereClause = "名称='" + listBoxAdv1.SelectedItem.ToString() + "'";
            
           
            IFeatureCursor pfeatureCursor;
            IFeature pfeature = null;
            pfeatureCursor = pfeatureClass.Search(pqueryFilter, false);
            pfeature = pfeatureCursor.NextFeature();
            if (pfeature != null)
            {
                int fieldindex1 = getFieldindex(pfeatureClass, "Type");
                lblType.Text = "类型： " + pfeature.get_Value(fieldindex1).ToString();
                typeFlag = pfeature.get_Value(fieldindex1).ToString() + "";//获取点类型
                int fieldindex2 = getFieldindex(pfeatureClass, "地址");
                lblAddress.Text = "地址： " + pfeature.get_Value(fieldindex2).ToString();
                int fieldindex3 = getFieldindex(pfeatureClass, "联系电话");
                lblPhone.Text = "电话： " + pfeature.get_Value(fieldindex3).ToString();
                int fieldindex4 = getFieldindex(pfeatureClass, "名称");
                //pictureBox1.ImageLocation = System.Environment.CurrentDirectory + "\\吃货点\\" + pfeature.get_Value(fieldindex4).ToString() + ".jpg";
                groupPanel1.Text = pfeature.get_Value(fieldindex4).ToString();
                //将点选添加至搜索框



                IEnvelope envelope = (IEnvelope)new Envelope();
                axMapControl1.Map.SelectFeature(pfeatureLayer, pfeature);
                envelope.XMax = pfeature.Extent.XMax + 0.001;
                envelope.XMin = pfeature.Extent.XMin - 0.001;
                envelope.YMax = pfeature.Extent.YMax + 0.001;
                envelope.YMin = pfeature.Extent.YMin - 0.001;
                axMapControl1.Extent = envelope;
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                tbSearch.Text = groupPanel1.Text;
            }
            
        }

        /// <summary>
        /// 获取空间查询到的要素集,返回一个要素集
        /// </summary>
        /// <param name="pFeatureLayer"></param>
        /// <param name="pGeometry"></param>
        /// <returns></returns>
        private List<IFeature> GetSpatialSearchFeatures(IFeatureLayer pFeatureLayer, IGeometry pGeometry)
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

        //向ListBox中添加字段的方法
        private void AddtestToListBox(IFeatureLayer pFeatureLayer, IFeature pFeature)
        {
            axMapControl1.Map.SelectFeature(pFeatureLayer, pFeature);
            int fieldIndex = getFieldindex(pFeatureLayer.FeatureClass, "名称");
            listBoxAdv1.Items.Add(pFeature.get_Value(fieldIndex));
        }

     
        public void metroBuffer(double dis,IPoint pPoint)
        {
            //缓冲区
                    IFeatureLayer pLayer = getlayerbyname("point") as IFeatureLayer;
                    IMap pMap = axMapControl1.Map;
                    IActiveView pActView = pMap as IActiveView;
                    IPoint pt = pPoint;
                    ITopologicalOperator pTopo = pt as ITopologicalOperator;
                    IGeometry pGeo = pTopo.Buffer(dis);
                    ESRI.ArcGIS.Display.IRgbColor rgbColor = new ESRI.ArcGIS.Display.RgbColorClass();
                    rgbColor.Red = 255;
                    ESRI.ArcGIS.Display.IColor color = rgbColor; // Implicit Cast
                    ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new ESRI.ArcGIS.Display.SimpleFillSymbolClass();
                    simpleFillSymbol.Color = color;
                    ESRI.ArcGIS.Display.ISymbol symbol = simpleFillSymbol as ESRI.ArcGIS.Display.ISymbol;
                    pActView.ScreenDisplay.SetSymbol(symbol);
                    pActView.ScreenDisplay.DrawPolygon(pGeo);
                    pMap.SelectByShape(pGeo, null, false);
                    //闪动1000次
                    axMapControl1.FlashShape(pGeo, 1, 2, symbol);
                    axMapControl1.ActiveView.Refresh();
                    List<IFeature> pFList = GetSpatialSearchFeatures(pLayer, pGeo);
                    for (int i = 0; i < pFList.Count; i++)
                    {
                        IFeature pFeature = pFList[i];
                        AddtestToListBox(pLayer, pFeature);
                    }
        }

        







    }
}



        #endregion