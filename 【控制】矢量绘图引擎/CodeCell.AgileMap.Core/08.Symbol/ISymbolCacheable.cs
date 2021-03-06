﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CodeCell.AgileMap.Core
{
    public interface ISymbolCacheable
    {
        bool NeedCache { get; set; }
        Image GetCacheSymbol();
        void ClearCache();
    }
}
