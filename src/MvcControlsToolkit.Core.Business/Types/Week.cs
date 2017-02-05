using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MvcControlsToolkit.Core.Types
{
    public class WeekTypeConverter: System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(DateTime);
        }
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value is string ? Week.Parse(value as string) : Week.FromDateTime((DateTime)value);
        }
        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || destinationType == typeof(DateTime);
        }
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)) return ((Week)value).ToString();
            return ((Week)value).StartDate();
        }
    }
    [System.ComponentModel.TypeConverter("MvcControlsToolkit.Core.Types.WeekTypeConverter")]
    public struct  Week: IComparable, IComparable<Month>, IEquatable<Week>
    {

        uint _YearNumber;
        public uint YearNumber { get { return _YearNumber; } set { _YearNumber = value; } }
        uint _WeekNumber;
        public uint WeekNumber { get { return _WeekNumber; } set { _WeekNumber = value; } }

        private static Week min = new Week(1, 1);
        private static Week max = new DateTime(9999, 1, 1);

        public static Week MinValue { get { return min; } }
        public static Week MaxValue { get { return max; } }

        public Week(uint yearNumber, uint weekNumber)
        {
            _YearNumber = yearNumber; 
            _WeekNumber = weekNumber;
            if (!this.isValid()) throw new FormatException();
        }
        private Week(uint yearNumber, uint weekNumber, bool force)
        {
            _YearNumber = yearNumber;
            _WeekNumber = weekNumber;
            if (!force && !this.isValid()) throw new FormatException();
        }
        public bool isValid()
        {
            if (_YearNumber > 0 && _WeekNumber >= 1)
            {
                return _WeekNumber <= _FromDateTime(new DateTime((int)_YearNumber, 12, 31), true)._WeekNumber || 
                    _WeekNumber <= _FromDateTime(new DateTime((int)_YearNumber, 12, 24), true)._WeekNumber;
            }
            else return false;
            
        }
        private DateTime midDate()
        {
            var firstDay = new DateTime((int)_YearNumber, 1, 1);
            if(FromDateTime(firstDay)._WeekNumber>1) return firstDay.AddDays(7 * _WeekNumber);
            else return firstDay.AddDays(7 * (_WeekNumber-1));
        }
        public Week AddWeeks(int x)
        {
            return Week.FromDateTime(midDate().AddDays(x * 7));
        }
        public DateTime StartDate()
        {
            var mid = midDate();
            switch (mid.DayOfWeek)
            {
                case DayOfWeek.Monday: return mid;
                case DayOfWeek.Tuesday: return mid.AddDays(-1);
                case DayOfWeek.Wednesday: return mid.AddDays(-2);
                case DayOfWeek.Thursday: return mid.AddDays(-3);
                case DayOfWeek.Friday: return mid.AddDays(-4);
                case DayOfWeek.Saturday: return mid.AddDays(-5);
                default: return mid.AddDays(-6);
            }
        }
        public DateTime EndDate()
        {
            var mid = midDate();
            switch (mid.DayOfWeek)
            {
                case DayOfWeek.Monday: return mid.AddDays(6);
                case DayOfWeek.Tuesday: return mid.AddDays(5);
                case DayOfWeek.Wednesday: return mid.AddDays(4);
                case DayOfWeek.Thursday: return mid.AddDays(3);
                case DayOfWeek.Friday: return mid.AddDays(2);
                case DayOfWeek.Saturday: return mid.AddDays(1);
                default: return mid;
            }
        }
        public static Week FromDateTime(DateTime x)
        {
            return _FromDateTime(x, false);
        }
        public static Week _FromDateTime(DateTime x, bool force)
        {
            var week = System.Globalization.DateTimeFormatInfo.InvariantInfo.Calendar.GetWeekOfYear(x, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (x.Month < 12 || x.Day < 29) return new Week((uint)(x.Month > 1 || week < 52 ? x.Year : x.Year - 1), (uint)week, force);
            else
            {
                var firstDay = new DateTime(x.Year + 1, 1, 1).DayOfWeek;
                var currDay = x.Day;
                if (firstDay == DayOfWeek.Tuesday && currDay == 31 ||
                    firstDay == DayOfWeek.Wednesday && currDay > 29 ||
                    firstDay == DayOfWeek.Thursday && currDay > 28) return new Week((uint)(x.Year + 1), 1, force);
                else return new Week((uint)x.Year, (uint)week, force);
            }
        }
        public static implicit operator Week(DateTime t)
        {
            return FromDateTime(t);
        }
        public static implicit operator DateTime(Week w)
        {
            return w.StartDate();
        }
        public static bool operator <(Week x, Week y)
        {
            return x.CompareTo(y) < 0;
        }
        public static bool operator >(Week x, Week y)
        {
            return x.CompareTo(y) > 0;
        }
        public static bool operator <=(Week x, Week y)
        {
            return x.CompareTo(y) <= 0;
        }
        public static bool operator >=(Week x, Week y)
        {
            return x.CompareTo(y) >= 0;
        }
        public static bool operator ==(Week x, Week y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(Week x, Week y)
        {
            return !x.Equals(y);
        }
        public override string ToString()
        {
            return string.Format("{0:0000}-W{1:00}", _YearNumber, _WeekNumber);
        }
        public static Week Parse(string x)
        {
            if (String.IsNullOrWhiteSpace(x)) throw new ArgumentNullException();
            x = x.Trim();
            try
            {
                var index = x.IndexOf("-W");
                if (index < 0)
                {
                    DateTime? cres = null;
                    try
                    {
                        cres = Convert.ToDateTime(x);
                    }
                    catch { }
                    if (cres == null)
                        throw new FormatException();
                    else
                        return FromDateTime(cres.Value);
                }
                var year = uint.Parse(x.Substring(0, index));
                var week = uint.Parse(x.Substring(index + 2));
                var res = new Week(year, week);
                if (!res.isValid()) throw new FormatException();
                return res;
            }
            catch {
                throw new FormatException();
            }


        }

        public static bool TryParse(string x, out Week w)
        {
            w = min;
            bool fres;
            if (String.IsNullOrWhiteSpace(x)) return false;
            x = x.Trim();
            
            var index = x.IndexOf("-W");
            if (index < 0)
            {
                DateTime cres;
                fres = DateTime.TryParse(x, out cres);
                if (!fres) return fres;
                w=FromDateTime(cres);
            }
            uint year = uint.Parse(x.Substring(0, index));
            fres = uint.TryParse(x.Substring(0, index), out year);
            if (!fres || x.Length<index+3) return false;
            uint week;
            fres=uint.TryParse(x.Substring(index + 2), out week);
            if (!fres) return fres;
            w = new Week(year, week, true);
            if (!w.isValid()) return false;
            
            return true;
        }

        public int CompareTo(object obj)
        {
            Week cm = (Week)obj;
            if (this._YearNumber < cm._YearNumber) return -1;
            if (this._YearNumber == cm._YearNumber)
            {
                if (this._WeekNumber < cm._WeekNumber) return -1;
                else if (this._WeekNumber == cm._WeekNumber) return 0;
                else return 1;
            }
            else return 1;
        }

        public int CompareTo(Month other)
        {
            return this.CompareTo(other); 
        }

        public bool Equals(Week other)
        {
            return this._YearNumber == other._YearNumber &&
                this._WeekNumber == other._WeekNumber;
        }
        public override bool Equals(object obj)
        {
            return (obj is Week) && this.Equals((Week)obj);
        }
        public override int GetHashCode()
        {
            return (int)(this._YearNumber ^ this._WeekNumber);
        }
    }
}
