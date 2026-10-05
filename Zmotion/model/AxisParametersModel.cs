using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.model
{
    public class AxisParametersModel
    {
        /// <summary>
        /// S曲线
        /// </summary>
        public float sramp{get;set;}
        /// <summary>
        /// 减速度
        /// </summary>
        public float decel {get;set;}
        /// <summary>
        /// 加速度
        /// </summary>
        public float accel {get;set;}
        /// <summary>
        /// 运行速度
        /// </summary>
        public float speed {get;set;}
        /// <summary>
        /// 起始速度
        /// </summary>
        public float lspeed {get;set;}
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public float units { get; set; }

    }
}
