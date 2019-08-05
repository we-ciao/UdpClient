using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DampingForwardUDP
{
    public class ValueCount:IComparable
    {
        public double value;
        public double count = 0;
                     
        public ValueCount(double _value)
        {
            value = _value;
        }
        public ValueCount(double _value, double _count)
        {
            value = _value;
            count = _count;
        }


        public int CompareTo(object obj)
        {
            if(obj is ValueCount)
            {
                ValueCount c = obj as ValueCount;
                return this.value.CompareTo(c.value);
            }
            else
            {
                return 0;
            }

        }
    }

    //实现对象数值比较
    public class ValueCountComparer : IEqualityComparer<ValueCount>
    {
        public bool Equals(ValueCount x, ValueCount y)
        {
            return x.value == y.value
                && x.count == y.count;
        }

        public int GetHashCode(ValueCount obj)
        {
            return 1;
        }
    }
}
