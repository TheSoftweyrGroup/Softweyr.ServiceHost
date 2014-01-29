namespace Softweyr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class StringExtensions
    {
        public static string FormatWith(this string me, params object[] args)
        {
            return string.Format(me, args);
        }
    }
}
