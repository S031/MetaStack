﻿using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Logging
{
    public class LogScopeInfo
    {
        public LogScopeInfo()
        {
        }

        public string Text { get; set; }
        public MapTable<string, object> Properties { get; set; }
    }
}
