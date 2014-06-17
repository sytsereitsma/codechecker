using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeChecker
{
    class ArgumentListJoiner
    {
        private string _Joined;
        public ArgumentListJoiner(string[] inList, string inSeparator, string inPrefix)
        {
            for(UInt32 t=0; t < inList.Length; t++)
            {
                if (t != 0){
                    _Joined += inSeparator;
                }

                _Joined += inPrefix;

                if (inList[t].Contains(" "))
                {
                    _Joined += "'" + inList[t] + "'";
                }
                else
                {
                    _Joined += inList[t];
                }
            }
        }

        internal string JoinedList
        {
            get { return _Joined; }

        }
    }
}
