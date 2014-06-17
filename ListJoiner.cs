using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeChecker
{
    class ListJoiner
    {
        private string _Joined;
        public ListJoiner(string[] inList, string inSeparator, string inPrefix)
        {
            for(UInt32 t=0; t < inList.Length; t++)
            {
                if (t != 0){
                    _Joined += inSeparator;
                }

                _Joined += inPrefix;
                _Joined += inList [t];
            }
        }

        internal string JoinedList
        {
            get { return _Joined; }

        }
    }
}
