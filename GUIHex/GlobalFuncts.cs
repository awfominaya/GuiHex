using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIHex
{
    public class GlobalFuncts
    {
        

        public static int ParseStringToInt(string s)
        {
            int i = 0;
            Int32.TryParse(s, out i);
            return i;
        }
    }
}
