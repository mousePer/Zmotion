using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zmotion.help;
using Zmotion.model;

namespace Zmotion.BaseMotion
{
    public class ZmotionServer : ParentMotion
    {
        /// <summary>
        /// 网口连接句柄
        /// </summary>
        private IntPtr g_handle;
        //public bool isConnect = false;
        public bool isConnect => !string.IsNullOrEmpty(g_handle.ToString());
        private List<int> codeList = new List<int>();
        /// <summary>
        /// 连续运动
        /// </summary>
        /// <param name="axisParametersModel">参数模型</param>
        /// <param name="functional">轴号与运动方向功能码</param>
        /// <returns></returns>
        public override AppResultHelper<bool> ContinuousMotion(AxisParametersModel axisParametersModel, string functional)
        {
            if (string.IsNullOrEmpty(g_handle.ToString()))
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            else
            {
                //轴手动运动前，关闭所有轴运动
                AppResultHelper<bool> appResultHelper = this.StopMotion();
                if (!appResultHelper.isSuccessful)
                {
                    return appResultHelper;
                }
                functional = functional.Trim();
                if (string.IsNullOrEmpty(functional))
                {
                    return AppResultHelper<bool>.Fail("功能码为空");
                }
                if (!functional.Contains(","))
                {
                    return AppResultHelper<bool>.Fail("功能码格式错误");
                }
                string[] functionals = functional.Split(',');
                if (functionals.Length != 2)
                {
                    return AppResultHelper<bool>.Fail("功能码格式错误");
                }
                int iaxis = int.Parse(functionals[0]);
                int direction = int.Parse(functionals[1]);
                try
                {
                    SetAxisParameters(axisParametersModel, iaxis);
                    int result = zmcaux.ZAux_Direct_Single_Vmove(g_handle, iaxis, direction);
                    return AppResultHelper<bool>.ResultValidation(result);
                }
                catch (Exception ex)
                {
                    return AppResultHelper<bool>.Fail("轴运动发生错误，" + ex.Message);
                }

            }
        }
  
        /// <summary>
        /// 连接网口
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override AppResultHelper<bool> Open(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return AppResultHelper<bool>.Fail("请输入IP");
            }
            int result = zmcaux.ZAux_OpenEth(ip, out g_handle);
            if (result != 0 || string.IsNullOrEmpty(g_handle.ToString()))
            {
                return AppResultHelper<bool>.Fail(result);
            }
            //isConnect = true;
            return AppResultHelper<bool>.Success();
          }

        /// <summary>
        /// 获取所有网口 IP
        /// </summary>
        /// <returns></returns>
        public override List<string> SearchEthlist()
        {
            StringBuilder ipaddrlist = new StringBuilder();
            int result = 0;
            // 最多尝试 5 次(预防网口偶尔搜索不到)
            for (int searchTimes = 1; searchTimes <= 5; searchTimes++)
            {
                result = zmcaux.ZAux_SearchEthlist(ipaddrlist, 1024, 1000);
                string ipStr = ipaddrlist.ToString().Trim();
                // 搜索成功
                if (!string.IsNullOrEmpty(ipStr))
                {
                    // 多个 IP 用空格分隔
                    if (ipStr.Contains(" "))
                    {
                        return ipStr
                            .Split(' ')
                            .ToList();
                    }
                    // 单个 IP
                    return new List<string> { ipStr };
                }

                // 失败后等待后再次重新搜索
                System.Threading.Thread.Sleep(200);
            }
            if (result != 0)
            {
                return new List<string>() { AppResultHelper<bool>.Fail(result).message };
            }
            // 连续 5 次都没搜到
            return new List<string>();
        }
        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="axisParametersModel"></param>
        /// <param name="functional"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override AppResultHelper<bool> StopMotion(string functional)
        {
            if (string.IsNullOrEmpty(g_handle.ToString()))
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            else
            {
                functional = functional.Trim();
                if (string.IsNullOrEmpty(functional))
                {
                    return AppResultHelper<bool>.Fail("功能码为空");
                }
                if (!functional.Contains(","))
                {
                    return AppResultHelper<bool>.Fail("功能码格式错误");
                }
                string[] functionals = functional.Split(',');
                if (functionals.Length != 2)
                {
                    return AppResultHelper<bool>.Fail("功能码格式错误");
                }
                int iaxis = int.Parse(functionals[0]);
                try
                {
                    /**
                     * 停止运动
                     * iaxis:轴号
                     * 0:取消当前运动
                     * 1:取消缓冲运动
                     * 2:取消当前运动和缓冲运动
                     * 3:立即中断脉冲发送
                     */
                    //这里的2表示取消当前运动和缓冲运动
                    int result = zmcaux.ZAux_Direct_Single_Cancel(g_handle, iaxis, 2);
                    //验证结果
                    return AppResultHelper<bool>.ResultValidation(result);
                }
                catch (Exception ex)
                {
                    return AppResultHelper<bool>.Fail("轴停止发生错误，" + ex.Message);
                }
            }
        }
        

        /// <summary>
        /// 设置轴参数
        /// </summary>
        /// <param name="axisParametersModel">轴模型</param>
        /// <param name="iaxis">轴号</param>
        private void SetAxisParameters(AxisParametersModel axisParametersModel, int iaxis)
        {
            //设置轴参数
            //设置脉冲当量（units）                
            zmcaux.ZAux_Direct_SetUnits(g_handle, iaxis, axisParametersModel.units);
            //设置轴起始速度，单位为 units/s     
            zmcaux.ZAux_Direct_SetLspeed(g_handle, iaxis, axisParametersModel.lspeed);
            //设置轴速度，单位为 units/s         
            zmcaux.ZAux_Direct_SetSpeed(g_handle, iaxis, axisParametersModel.speed);
            //设置加速度，单位为 units /s       
            zmcaux.ZAux_Direct_SetAccel(g_handle, iaxis, axisParametersModel.accel);
            //设置减速度，单位为 units /s
            zmcaux.ZAux_Direct_SetDecel(g_handle, iaxis, axisParametersModel.decel);
            //设置 S 曲线设置。0-梯形加减速       
            zmcaux.ZAux_Direct_SetSramp(g_handle, iaxis, axisParametersModel.sramp);
        }
        /// <summary>
        /// 获取轴IO列表
        /// </summary>
        /// <param name="iaxis">轴号</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override List<int> GetAxisIOList(int iaxis)
        {
            switch (iaxis)
            {
                case 0:
                    return new List<int> { 11, 13, 12 };
                case 1:
                    return new List<int> { 8, 9, 10 };
                case 3:
                    return new List<int> { 0, 1, 2 };
                default:
                    return new List<int>();
            }
        }
        /// <summary>
        /// 限位监控
        /// </summary>
        /// <param name="axisIO">轴号列表</param>
        /// <returns></returns>
        public override AppResultHelper<uint> GetAxisIOStatus(int axisIO)
        {
            if (isConnect == false)
            {
                return AppResultHelper<uint>.Fail("请先连接网口");
            }
            uint status = 0;
            int reult = zmcaux.ZAux_Direct_GetIn(g_handle, axisIO, ref status);
            return AppResultHelper<uint>.ResultValidation(reult, status);
        }
        /// <summary>
        /// 设置轴（轴限位）
        /// </summary>
        /// <param name="iaxis"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override AppResultHelper<bool> SetAxis(int iaxis)
        {
            if (!isConnect) {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            List<int> IOlist = GetAxisIOList(iaxis);
            //电平取反
            for(int i=1; i<IOlist.Count; i++)
            {
                zmcaux.ZAux_Direct_SetInvertIn(g_handle, IOlist[i], 1);
            }
            //设置轴减减速度
            //设置0轴快减减速度，触发限位时，按这个速度减速
            List<int> codeList = new List<int>()
            {
                zmcaux.ZAux_Direct_SetFastDec(g_handle, iaxis, 1000),      //轴快减减速度
                zmcaux.ZAux_Direct_SetDatumIn(g_handle, iaxis, IOlist[0]),        //原点in(0)
                zmcaux.ZAux_Direct_SetFwdIn(g_handle, iaxis, IOlist[1]),          //正限位in(2)
                zmcaux.ZAux_Direct_SetRevIn(g_handle, iaxis, IOlist[2])          //负限位in(3) 
            };
            //验证结果
            return AppResultHelper<bool>.ResultValidation(codeList);
        }
        /// <summary>
        /// 获取当前轴参数
        /// </summary>
        /// <param name="iaxis"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override AppResultHelper<AxisCurrentParameterModel>  GetAxisCurrentParameter(int iaxis)
        {
            //读取轴的速度和位置
            float curpos = 0;
            float curspeed = 0;
            List<int> codeList = new List<int>()
            {
                zmcaux.ZAux_Direct_GetDpos(g_handle, iaxis, ref curpos),     //获取当前轴坐标
                zmcaux.ZAux_Direct_GetVpSpeed(g_handle, iaxis, ref curspeed) //获取当前轴速度
            };
            //将轴参数（坐标，速度）封装成模型
            AxisCurrentParameterModel axisCurrentParameterModel = new AxisCurrentParameterModel()
            {
                isConnect = isConnect,
                AxisCurrentPosition = curpos,
                AxisCurrentSpeed = curspeed
            };
            //验证结果
            return AppResultHelper<AxisCurrentParameterModel>.ResultValidation(codeList, axisCurrentParameterModel);
        }
        /// <summary>
        /// 轴相对运动
        /// </summary>
        /// <param name="axisMotionDic">轴与相应距离的字典</param>
        /// <returns></returns>
        public override AppResultHelper<bool> RelativeMotion(AxisParametersModel axisParametersModel, Dictionary<int, float> axisMotionDic)
        {
            if (!isConnect)
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            List<int> axisKeyList = axisMotionDic.Keys.ToList();
            List<int> resultList = new List<int>();
            foreach (int iaxis in axisKeyList)
            {
                //判断轴是否正在运动
                AppResultHelper<bool> isMotionResult = IsMotion(iaxis);
                if (!isMotionResult.isSuccessful)
                {
                    return AppResultHelper<bool>.Fail(isMotionResult.message);
                }
                //设置轴参数
                SetAxisParameters(axisParametersModel, iaxis);
                //轴相对运动，并将轴的结果添加到列表中
                float v = axisMotionDic[iaxis];
                resultList.Add(zmcaux.ZAux_Direct_Single_Move(g_handle, iaxis, v));
            }
            return AppResultHelper<bool>.ResultValidation(resultList);
        }
        /// <summary>
        /// 轴绝对运动
        /// </summary>
        /// <param name="axisMotionDic">轴与相应距离的字典</param>
        /// <returns></returns>
        public override AppResultHelper<bool> AbsoluteMotion(AxisParametersModel axisParametersModel, Dictionary<int, float> axisMotionDic)
        {
            if (!isConnect)
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            List<int> axisKeyList = axisMotionDic.Keys.ToList();
            List<int> resultList = new List<int>();
            foreach (int iaxis in axisKeyList)
            {
                //判断轴是否正在运动
                AppResultHelper<bool> isMotionResult = IsMotion(iaxis);
                if (!isMotionResult.isSuccessful)
                {
                    return AppResultHelper<bool>.Fail(isMotionResult.message);
                }
                //设置轴参数
                SetAxisParameters(axisParametersModel, iaxis);
                //轴相对运动，并将轴的结果添加到列表中
                resultList.Add(zmcaux.ZAux_Direct_Single_MoveAbs(g_handle, iaxis, axisMotionDic[iaxis]));
            }
            return AppResultHelper<bool>.ResultValidation(resultList);
        }
        /// <summary>
        /// 轴运动状态  
        /// </summary>
        /// <param name="iaxis">轴号</param>
        /// <returns>正在运行为flase  停止行为true</returns>
        public override AppResultHelper<bool> IsMotion(int iaxis)
        {
            if (!isConnect)
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            int runstate = -1;
            //获取当太时轴运行状态，
            int result = zmcaux.ZAux_Direct_GetIfIdle(g_handle, iaxis, ref runstate);
            if(result != 0)
            {
                return AppResultHelper<bool>.Fail("获取轴运行状态失败");
            }
            if (runstate == 0)
            {
                return AppResultHelper<bool>.Fail("轴正在运行");
            }
            return AppResultHelper<bool>.Success();
        }
        /// <summary>
        /// 停止轴运动
        /// </summary>
        /// <returns></returns>
        public override AppResultHelper<bool> StopMotion()
        {
            if (!isConnect)
            {
                return AppResultHelper<bool>.Fail("请先连接网口");
            }
            int result = zmcaux.ZAux_Direct_Rapidstop(g_handle, 2);
            return AppResultHelper<bool>.ResultValidation(result);
        }

        public override AppResultHelper<bool> Test()
        {
            int result = zmcaux.ZAux_Direct_Single_Move(g_handle, 3, 1000);
            return AppResultHelper<bool>.ResultValidation(result);
        }
    }
}
