using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.Utilities.Exceptions
{
    public class PMSShopException : Exception
    {
        public PMSShopException()
        {
        }

        public PMSShopException(string message)
            : base(message)
        {
        }

        public PMSShopException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
