using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace MvcControlsToolkit.Core.Types
{
    public class MonthTypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(DateTime);
        }
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value is string ? Month.Parse(value as string) : Month.FromDateTime((DateTime)value);
        }
        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || destinationType == typeof(DateTime);
        }
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)) return ((Month)value).ToString();
            return ((Month)value).ToDateTime();
        }
    }
    [System.ComponentModel.TypeConverter("MvcControlsToolkit.Core.Types.MonthTypeConverter")]
    public struct Month: IComparable
    {
        uint _YearNumber;
        public uint YearNumber { get { return _YearNumber; } set { _YearNumber = value; } }
        uint _MonthNumber;
        public uint MonthNumber { get { return _MonthNumber; } set { _MonthNumber = value; } }

        private static Month min = new Month(1, 1);
        private static Month max = new Month(9999, 12);

        public static Month MinValue { get { return min; } }
        public static Month MaxValue { get { return max; } }

        public Month(uint yearNumber, uint monthNumber)
        {
            _YearNumber = yearNumber;
            _MonthNumber = monthNumber;
            if (_MonthNumber < 1 || _MonthNumber > 12) throw new FormatException();
            if (yearNumber < 1) throw new FormatException();
        }
        public Month AddMonths(int months)
        {
            if (months == 0) return this;
            var years = months / 12;
            months = months % 12;
            if (months < 0)
            {
                months += 12;
                years--;
            }
            return new Month((uint)(years + _YearNumber), (uint)(months + MonthNumber));

        }
        public Month AddYears(int years)
        {
            return new Month((uint)(_YearNumber + years), _MonthNumber);
        }

        public DateTime ToDateTime()
        {
            return new DateTime((int)_YearNumber, (int)_MonthNumber, 1);
        }

        public static Month FromDateTime(DateTime t)
        {
            return new Month((uint)t.Year, (uint)t.Month);
        }

        public static implicit operator Month(DateTime t)
        {
            return FromDateTime(t);
        }

        public static implicit operator DateTime(Month m)
        {
            return m.ToDateTime();
        }

        public override string ToString()
        {
            return string.Format("{0:0000}-{1:00}", _YearNumber, _MonthNumber);
        }

        public static Month Parse(string s)
        {
            return FromDateTime(DateTime.Parse(s));
        }

        public static bool TryParse(string s, out Month m)
        {
            DateTime dt;
            var res = DateTime.TryParse(s, out dt);
            if (res) m= FromDateTime(dt);
            m = min;
            return res;
        }

        public int CompareTo(object obj)
        {
            Month cm = (Month)obj;
            if (this._YearNumber < cm._YearNumber) return -1;
            if (this._YearNumber == cm._YearNumber)
            {
                if (this._MonthNumber < cm._MonthNumber) return -1;
                else if (this._MonthNumber == cm._MonthNumber) return 0;
                else return 1;
            }
            else return 1;
        }
    }
}
