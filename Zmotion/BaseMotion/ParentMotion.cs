using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zmotion.help;
using Zmotion.model;

namespace Zmotion.BaseMotion
{
    public abstract class ParentMotion
    {
        public AxisParametersModel AxisParameters { get; set; }
        public ParentMotion()
        {
            AxisParameters = new AxisParametersModel();
        }
        /// <summary>
        /// 搜索网口IP
        /// </summary>
        /// <returns></returns>
        public abstract List<string> SearchEthlist();
        /// <summary>
        /// 连接网口
        /// </summary>
        /// <returns></returns>
        public abstract AppResultHelper<bool> Open(string ip);
        /// <summary>
        /// 连续运动
        /// </summary>
        /// <param name="axisParametersModel"></param>
        /// <param name="functional"></param>
        /// <returns></returns>
        public abstract AppResultHelper<bool> ContinuousMotion(AxisParametersModel axisParametersModel,string functional);
        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="axisParametersModel"></param>
        /// <param name="functional"></param>
        /// <returns></returns>
        public abstract AppResultHelper<bool> StopMotion(string functional);
        /// <summary>
        /// 获取轴IO列表
        /// </summary>
        /// <param name="iaxis">轴号</param>
        /// <returns></returns>
        public abstract List<int> GetAxisIOList(int iaxis);
        /// <summary>
        /// 限位监控
        /// </summary>
        /// <param name="axisIO">轴的IO列表</param>
        /// <returns></returns>
        public abstract AppResultHelper<uint> GetAxisIOStatus(int axisIO);
        /// <summary>
        /// 设置轴(轴的限位)
        /// </summary>
        /// <param name="iaxis"></param>
        /// <returns></returns>
        public abstract AppResultHelper<bool> SetAxis(int iaxis);
        /// <summary>
        /// 获取当前轴参数
        /// </summary>
        /// <param name="iaxis"></param>
        /// <returns></returns>
        public abstract AppResultHelper<AxisCurrentParameterModel> GetAxisCurrentParameter(int iaxis);
        /// <summary>
        /// 轴相对运动
        /// </summary>
        /// <param name="axisMotionDic">轴号与相应距离的字典</param>
        /// <returns></returns>
        public abstract AppResultHelper<bool> RelativeMotion(Dictionary<int, float> axisMotionDic);
        /// <summary>
        /// 轴绝对运动
        /// </summary>
        /// <param name="axisMotionDic">轴号与相应距离的字典</param>
        /// <returns></returns>
        public abstract AppResultHelper<bool> AbsoluteMotion(Dictionary<int, float> axisMotionDic);
        
    }
}
