using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;

namespace Spreadsheet
{
    public static class Helper
    {

        public static bool IsEmpty(this object obj)
        {
            var formula = obj as Formula;
            if (formula != null)
            {
                return string.IsNullOrEmpty(formula.Expression);
            }

            var s = obj as string;
            if (s != null)
            {
                return string.IsNullOrEmpty(s);
            }

            return false;
        }

    }
}
