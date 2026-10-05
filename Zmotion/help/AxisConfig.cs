using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.help
{
    public static class AxisConfig
    {
        //加载轴编号
        public static List<int> GetAxisNum()
        {
            List<int> axisList = new List<int>();
            Array array = Enum.GetValues(typeof(AxisNumberEnum));
            for (int i = 0; i < array.GetLength(0); i++)
            {
                // 取出枚举项，转成int
                AxisNumberEnum enumItem = (AxisNumberEnum)array.GetValue(i);
                int axisNum = (int)enumItem;
                axisList.Add(axisNum);
            }
            return axisList;
        }
    }
}
