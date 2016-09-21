using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false, Inherited =false)]
    public class ColumnLayoutAttribute: Attribute
    {
        
        public string DetailWidthsAsString { get; set; }
        public string WidthsAsString { get; set; }
        public decimal[] DetailWidths { get; private set; }
        public decimal[] Widths { get; private set; }

        public void Prepare()
        {
            if (!string.IsNullOrWhiteSpace(DetailWidthsAsString))
            {
                try
                {
                    DetailWidths = DetailWidthsAsString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => decimal.Parse(m, CultureInfo.InvariantCulture))
                        .ToArray();
                }
                catch
                {
                    throw new ArgumentException(string.Format(DefaultMessages.WrongDecimalArrayAsString, nameof(DetailWidthsAsString)), nameof(DetailWidthsAsString));
                }
            }
            if (!string.IsNullOrWhiteSpace(WidthsAsString))
            {
                try
                {
                    Widths = WidthsAsString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => decimal.Parse(m, CultureInfo.InvariantCulture))
                        .ToArray();
                }
                catch
                {
                    throw new ArgumentException(string.Format(DefaultMessages.WrongDecimalArrayAsString, nameof(WidthsAsString)), nameof(WidthsAsString));
                }
            }
        }
    }
}
