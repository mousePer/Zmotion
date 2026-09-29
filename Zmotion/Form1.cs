using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zmotion.BaseMotion;
using Zmotion.help;
using Zmotion.model;

namespace Zmotion
{
    public partial class Form1 : UIForm

    {
        ParentMotion parentMotion;
        int[] iaxis = { 0, 1, 3 };
        public Form1()
        {
            InitializeComponent();
            parentMotion = new ZmotionServer();
            //搜索网口IP
            List<string> list = parentMotion.SearchEthlist();
            uiComboBox1.DataSource = list;
            //加载错误码
            ErrorCodeHelper.GetErrorCodeDicWithFile();

        }
        //连接
        private void uiButton1_Click(object sender, EventArgs e)
        {
            AppResultHelper<bool> appResultHelper = parentMotion.Open(uiComboBox1.Text);
            if (!appResultHelper.isSuccessful)
            {
                this.ShowErrorTip(appResultHelper.message);
                return;
            }
            this.ShowSuccessTip(appResultHelper.message);
            //成功修改控件为绿色
            toolStripStatusLabel2.BackColor = Color.Green;
            //连接成功开启定时器使能
            timer1.Enabled = true;
            //连接成功开始设置轴限位
            foreach (int axis in iaxis)
            {
                appResultHelper = parentMotion.SetAxis(axis);
                if (!appResultHelper.isSuccessful)
                {
                    this.ShowErrorTip(appResultHelper.message);
                    return;
                }
            }
        }
        //运动
        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            GetAxisParameters();
            Button button = sender as Button;
            AppResultHelper<bool> appResultHelper = parentMotion.ContinuousMotion(GetAxisParameters(), button.Tag.ToString());
            if (!appResultHelper.isSuccessful)
            {
                this.ShowErrorTip(appResultHelper.message);
                return;
            }
        }
        //停止运动
        private void button6_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            AppResultHelper<bool> appResultHelper = parentMotion.StopMotion(button.Tag.ToString());
        }

        private AxisParametersModel GetAxisParameters()   
        {
            AxisParametersModel axisParametersModel = new AxisParametersModel();
            axisParametersModel.units = Convert.ToSingle(TextBox_units.Text);
            axisParametersModel.lspeed = Convert.ToSingle(TextBox_lspeed.Text);
            axisParametersModel.speed = Convert.ToSingle(TextBox_speed.Text);
            axisParametersModel.accel =Convert.ToSingle(TextBox_accel.Text);
            axisParametersModel.decel = Convert.ToSingle(TextBox_decel.Text);
            axisParametersModel.sramp =Convert.ToSingle(TextBox_sramp.Text);
            return axisParametersModel;
        }
        //在定时器中执行轴限位状态
        private void timer1_Tick(object sender, EventArgs e)
        {
            //读取轴IO点限位状态
            foreach (Control item in groupBox1.Controls)
            {

                if(item is PictureBox)
                {
                    //解析出轴的IO点
                    string axisIO = item.Name.Split('_')[2];
                    AppResultHelper<uint> appResultHelper = parentMotion.GetAxisIOStatus(int.Parse(axisIO));
                    if (!appResultHelper.isSuccessful)
                    {
                        this.ShowErrorTip(appResultHelper.message);
                    }
                    //由于电平取反，所以这里颜色显示反转
                    if (appResultHelper.data == 0)
                    {
                        item.BackColor = Color.Red;
                    }
                    else
                    {
                        item.BackColor = Color.Green;
                    }
                }
            }
            //读取轴参数
            for (int i=0; i< iaxis.Length; i++)
            {
                AppResultHelper<AxisCurrentParameterModel> appResultHelper = parentMotion.GetAxisCurrentParameter(iaxis[i]);
                if (!appResultHelper.isSuccessful)
                { 
                    this.ShowErrorTip(appResultHelper.message);
                }
                //获取当前轴参数
                AxisCurrentParameterModel axisCurrent = appResultHelper.data;
                //显示轴参数
                statusStrip1.Items["ts_Location_"+i].Text = axisCurrent.AxisCurrentPosition.ToString();
                statusStrip1.Items["ts_Speed_"+i].Text = axisCurrent.AxisCurrentSpeed.ToString();
            }

        }
    }
}
