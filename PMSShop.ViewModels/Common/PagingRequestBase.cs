﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.ViewModels.Common
{
    public class PagingRequestBase : RequestBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}