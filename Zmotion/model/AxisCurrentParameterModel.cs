using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.model
{
    public class AxisCurrentParameterModel
    {
        /// <summary>
        /// 轴当前位置
        /// </summary>
        public float AxisCurrentPosition { get; set; }
        /// <summary>
        /// 轴当前速度
        /// </summary>
        public float AxisCurrentSpeed { get; set; }
    }
}
