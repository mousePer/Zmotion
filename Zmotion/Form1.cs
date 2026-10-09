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
using LogProject;

namespace Zmotion
{
    public partial class Form1 : UIForm

    {
        ParentMotion parentMotion;
        List<int> iaxis = new List<int>() { 1, 3, 0 };

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
                LogHelper.WriteLog(appResultHelper.message);
                return;
            }
            this.ShowSuccessTip(appResultHelper.message);
            LogHelper.WriteLog(appResultHelper.message);
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
                    LogHelper.WriteLog(appResultHelper.message);
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
                LogServer.Instance.WriteLog(appResultHelper.message);
                return;
            }
        }
        //停止运动
        private void button6_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            AppResultHelper<bool> appResultHelper = parentMotion.StopMotion(button.Tag.ToString());
        }
        /// <summary>
        /// 设置轴参数
        /// </summary>
        /// <returns></returns>
        private AxisParametersModel GetAxisParameters()
        {
            AxisParametersModel axisParametersModel = new AxisParametersModel();
            axisParametersModel.units = Convert.ToSingle(TextBox_units.Text);
            axisParametersModel.lspeed = Convert.ToSingle(TextBox_lspeed.Text);
            axisParametersModel.speed = Convert.ToSingle(TextBox_speed.Text);
            axisParametersModel.accel = Convert.ToSingle(TextBox_accel.Text);
            axisParametersModel.decel = Convert.ToSingle(TextBox_decel.Text);
            axisParametersModel.sramp = Convert.ToSingle(TextBox_sramp.Text);
            return axisParametersModel;
        }
        #region 在定时器中轮询轴状态和参数
        //在定时器中执行轴限位状态
        private void timer1_Tick(object sender, EventArgs e)
        {
            //读取轴IO点限位状态
            foreach (Control item in groupBox1.Controls)
            {

                if (item is PictureBox)
                {
                    //解析出轴的IO点
                    string axisIO = item.Name.Split('_')[2];
                    AppResultHelper<uint> appResultHelper = parentMotion.GetAxisIOStatus(int.Parse(axisIO));
                    if (!appResultHelper.isSuccessful)
                    {
                        this.ShowErrorTip(appResultHelper.message);
                        LogHelper.WriteLog(appResultHelper.message);
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
            for (int i = 0; i < iaxis.Count; i++)
            {
                AppResultHelper<AxisCurrentParameterModel> appResultHelper = parentMotion.GetAxisCurrentParameter(iaxis[i]);
                if (!appResultHelper.isSuccessful)
                {
                    this.ShowErrorTip(appResultHelper.message);
                    LogHelper.WriteLog(appResultHelper.message);
                }
                //获取当前轴参数
                AxisCurrentParameterModel axisCurrent = appResultHelper.data;
                //显示轴参数
                statusStrip1.Items["ts_Location_" + i].Text = axisCurrent.AxisCurrentPosition.ToString();
                statusStrip1.Items["ts_Speed_" + i].Text = axisCurrent.AxisCurrentSpeed.ToString();
                //TODO:编写控制器连接状态变化

            }
        }
        #endregion

        #region 启动轴运动
        //启动轴运动
        private void uiButton3_Click(object sender, EventArgs e)
        {
            //设置轴参数
            AxisParametersModel axisParametersModel = GetAxisParameters();
            Dictionary<int, float> axisMotionDic;
            bool flowControl = GetAxisMotionDic(out axisMotionDic);
            if (!flowControl)
            {
                LogHelper.WriteLog("轴运动参数获取错误");
                return;
            }
            AppResultHelper<bool> appResultHelper;
            //判断是相对运动还是绝对运动
            if (rb_RelativeMotion.Checked)
            {
                appResultHelper = parentMotion.RelativeMotion(axisParametersModel, axisMotionDic);
            }
            else
            {
                appResultHelper = parentMotion.AbsoluteMotion(axisParametersModel, axisMotionDic);
            }
            if (!appResultHelper.isSuccessful)
            {
                LogHelper.WriteLog(appResultHelper.message);
                this.ShowErrorTip(appResultHelper.message);
            }
        }
        #endregion

        /// <summary>
        /// 获取当前轴的轴号和距离存入字典
        /// </summary>
        /// <param name="axisMotionDic">轴号和距离的字典</param>
        /// <returns>是否成功获取轴号和距离</returns>
        private bool GetAxisMotionDic(out Dictionary<int, float> axisMotionDic)
        {
            //搜集复选框中选中的轴
            axisMotionDic = new Dictionary<int, float>();
            foreach (Control control in uiGroupBox1.Controls)
            {
                //若不是复选框则跳过，若复选框没有选中则跳过,若复选框标记为空则跳过
                if (!(control is UICheckBox) || !(control as UICheckBox).Checked)
                {
                    continue;
                }
                if (control.Tag == null)
                {
                    this.ShowErrorTip("轴号获取失败");
                }
                //提起出控件标记出来的轴号和距离存入字典
                int axisNum = int.Parse(control.Tag.ToString());
                if (!float.TryParse(uiGroupBox4.Controls["txb_" + axisNum].Text, out float axisDistance))
                {
                    this.ShowErrorTip("请输入正确的距离");
                    return false;
                }
                //float.TryParse(uiGroupBox4.Controls["txb_" + axisNum].Text, out float axisDistance);
                axisMotionDic.Add(axisNum, axisDistance);
            }

            return true;
        }

        //停止轴运动
        private void uiButton4_Click(object sender, EventArgs e)
        {
            AppResultHelper<bool> appResultHelper = parentMotion.StopMotion();
            if (!appResultHelper.isSuccessful)
            {
                LogHelper.WriteLog(appResultHelper.message);
                this.ShowErrorTip(appResultHelper.message);
            }
        }
        //测试
        private void button7_Click(object sender, EventArgs e)
        {
            AppResultHelper<bool> appResultHelper = parentMotion.Test();
            if (!appResultHelper.isSuccessful)
            {
                this.ShowErrorTip(appResultHelper.message);
            }
        }
        //一键回原点
        private async void uiButton2_Click(object sender, EventArgs e)
        {
            uiButton2.Enabled = false;
            //搜集复选框中选中的轴(收集需要回原点的轴)
            Dictionary<int, float> axisMotionDic = new Dictionary<int, float>();
            foreach (Control control in uiGroupBox1.Controls)
            {
                //若不是复选框则跳过，若复选框没有选中则跳过,若复选框标记为空则跳过
                if (!(control is UICheckBox) || !(control as UICheckBox).Checked)
                {
                    continue;
                }
                if (control.Tag == null)
                {
                    this.ShowErrorTip("轴号获取失败");
                }
                //提起出控件标记出来的轴号和距离存入字典
                int axisNum = int.Parse(control.Tag.ToString());
                axisMotionDic.Add(axisNum, -500000);
            }
            try
            {
                AppResultHelper<bool> appResultHelper = await parentMotion.BackOriginALLAsync(GetAxisParameters(), axisMotionDic, 600000);
                if (appResultHelper.isSuccessful)
                {
                    LogHelper.WriteLog("回原点成功");
                    this.ShowSuccessTip("回原点成功");
                }
                else
                {
                    LogHelper.WriteLog(appResultHelper.message);
                    this.ShowErrorTip(appResultHelper.message);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                this.ShowErrorTip(ex.Message);
            }
            finally
            {
                //最终强制停止运动，防止异常导致运动无法停止
                parentMotion.StopMotion();
                uiButton2.Enabled = true;
            }
        }
        //直线插补
        private void uiButton5_Click(object sender, EventArgs e)
        {
            //获取轴参数
            AxisParametersModel axisParametersModel = GetAxisParameters();
            //获取轴号和距离的字典
            GetAxisMotionDic(out Dictionary<int, float> axisMotionDic);
            if (axisMotionDic.Count < 2)
            {
                this.ShowErrorTip("直线插补至少需要两个轴");
                return;
            }
            AppResultHelper<bool> appResultHelper = parentMotion.AbsLine(axisParametersModel, axisMotionDic);
            if (!appResultHelper.isSuccessful)
            {
                LogHelper.WriteLog(appResultHelper.message);
                this.ShowErrorTip(appResultHelper.message);
            }
        }
        //圆弧插补
        private void uiButton6_Click(object sender, EventArgs e)
        {
            //获取轴参数
            AxisParametersModel axisParametersModel = GetAxisParameters();
            //搜集复选框中选中的轴
            Dictionary<int, float> axisMotionDic = new Dictionary<int, float>();
            List<float> middleList = new List<float>();
            foreach (Control control in uiGroupBox1.Controls)
            {
                //若不是复选框则跳过，若复选框没有选中则跳过,若复选框标记为空则跳过
                if (!(control is UICheckBox) || !(control as UICheckBox).Checked)
                {
                    continue;
                }
                if (control.Tag == null)
                {
                    this.ShowErrorTip("轴号获取失败");
                }
                //提起出控件标记出来的轴号和距离存入字典
                int axisNum = int.Parse(control.Tag.ToString());
                //收集该轴的中间点距离存入集合
                if (!float.TryParse(uiGroupBox4.Controls["uiTxb_" + axisNum].Text, out float middle))
                {
                    this.ShowErrorTip("距离不能为空");
                }
                middleList.Add(middle);
                if (!float.TryParse(uiGroupBox4.Controls["txb_" + axisNum].Text, out float axisDistance))
                {
                    this.ShowErrorTip("请输入正确的距离");
                }
                axisMotionDic.Add(axisNum, axisDistance);
            }
            if (axisMotionDic.Count < 2)
            {
                this.ShowErrorTip("圆弧插补至少需要两个轴");
                return;
            }
            AppResultHelper<bool> appResultHelper = parentMotion.AbsCircle(GetAxisParameters(), axisMotionDic, middleList);
            if (!appResultHelper.isSuccessful)
            {
                LogHelper.WriteLog(appResultHelper.message);
                this.ShowErrorTip(appResultHelper.message);
            }
        }
    }
}
