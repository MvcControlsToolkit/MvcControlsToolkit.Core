using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.Types;
using System.Globalization;
using MvcControlsToolkit.Core.Business;
using MvcControlsToolkit.Core.Exceptions;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DynamicRangeAttribute : ValidationAttribute
    {

        public DynamicRangeAttribute(Type targetType, string message) :
            base(message)
        {
            TargetType = targetType;

        }
        public DynamicRangeAttribute(Type targetType) :
            base(() => Resources.RangeStandardError)
        {
            TargetType = targetType;

        }
        private long toleranceMin = 0;
        private long toleranceMax = 0;
        private bool processNowToday(string dateString, bool max)
        {
            if (TargetType != typeof(DateTime)) return false;
            DateTime currDate;
            if (String.IsNullOrEmpty(dateString)) return false;
            if (dateString.StartsWith("Now"))
            {
                currDate = DateTime.Now;
                dateString = dateString.Substring(3);
            }
            else if (dateString.StartsWith("Today"))
            {
                currDate = DateTime.Today;
                dateString = dateString.Substring(5);
            }
            else
            {
                return false;
            }
            bool buildingNumber = false;
            long numberBeingBuild = 0;
            bool toInvert = false;
            foreach (char c in dateString)
            {
                if (c == '+')
                {
                    toInvert = false;
                    numberBeingBuild = 0;
                    buildingNumber = true;
                }
                else if (c == '-')
                {
                    toInvert = true;
                    numberBeingBuild = 0;
                    buildingNumber = true;
                }
                else if (buildingNumber)
                {
                    if (c.CompareTo('0') >= 0 && c.CompareTo('9') <= 0)
                    {
                        numberBeingBuild = numberBeingBuild * 10 + int.Parse(c.ToString());
                    }
                    else
                    {
                        buildingNumber = false;
                        if (toInvert) numberBeingBuild = -numberBeingBuild;
                        switch (c)
                        {
                            case 's':
                                currDate = currDate.AddSeconds(numberBeingBuild);
                                break;
                            case 'm':
                                currDate = currDate.AddMinutes(numberBeingBuild);
                                break;
                            case 'h':
                                currDate = currDate.AddHours(numberBeingBuild);
                                break;
                            case 'd':
                                currDate = currDate.AddDays(numberBeingBuild);
                                break;
                            case 'M':
                                currDate = currDate.AddMonths((int)numberBeingBuild);
                                break;
                            case 'y':
                                currDate = currDate.AddYears((int)numberBeingBuild);
                                break;

                            case 't':
                                if (max)
                                    toleranceMax = numberBeingBuild > 0 ? numberBeingBuild : -numberBeingBuild;
                                else
                                    toleranceMin = numberBeingBuild > 0 ? numberBeingBuild : -numberBeingBuild;
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            if (max) Maximum = currDate;
            else Minimum = currDate;
            return true;
        }

        private string _SMinimum;
        public string SMinimum
        {
            set
            {
                _SMinimum = value;
                if (value == null)
                {
                    Minimum = null;
                    return;
                }
                if (processNowToday(value, false))
                {
                    return;
                }
                try
                {
                    if (TargetType == typeof(Week)) Minimum = Week.Parse(value);
                    else if (TargetType == typeof(Month)) Minimum = Month.Parse(value);
                    else if (TargetType == typeof(TimeSpan)) Minimum = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                    else Minimum = Convert.ChangeType(value, TargetType, CultureInfo.InvariantCulture) as IComparable;
                }
                catch
                {
                    throw (new FormatException(Resources.InvalidFormat));
                }
            }
            get
            {
                return _SMinimum;
            }

        }
        private string _SMaximum;
        public string SMaximum
        {
            set
            {
                _SMaximum = value;
                if (value == null)
                {
                    Maximum = null;
                    return;
                }
                if (processNowToday(value, false))
                {
                    return;
                }
                try
                {
                    if (TargetType == typeof(Week)) Maximum = Week.Parse(value);
                    else if (TargetType == typeof(Month)) Maximum = Month.Parse(value);
                    else if (TargetType == typeof(TimeSpan)) Maximum = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                    else Maximum = Convert.ChangeType(value, TargetType, CultureInfo.InvariantCulture) as IComparable;
                }
                catch
                {
                    throw (new FormatException(Resources.InvalidFormat));
                }
            }
            get
            {
                return _SMaximum;
            }

        }


        Type TargetType { get; set; }
        public IComparable Minimum { get; protected set; }
        public IComparable Maximum { get; protected set; }
        public string MaxMinDisplayFormat { get; set; }
        private IEnumerable<DynamicLimit> parsedDynamicMaximum;
        private string _DynamicMaximum;
        public string DynamicMaximum
        {
            get { return _DynamicMaximum; }
            set
            {
                _DynamicMaximum = value;
                parsedDynamicMaximum = DynamicLimit.Parse(value);
            }
        }
        private IEnumerable<DynamicLimit> parsedDynamicMinimum;
        private string _DynamicMinimumm;
        public string DynamicMinimum
        {
            get { return _DynamicMinimumm; }
            set
            {
                _DynamicMinimumm = value;
                parsedDynamicMinimum = DynamicLimit.Parse(value);
            }
        }
        public string DynamicMaximumDelay { get; set; }
        public string DynamicMinimumDelay { get; set; }
        public bool Propagate {get; set;}

        private object getDelay(string delayRef, object model, object fixedDelay = null, bool invertSign = false)
        {
            if (delayRef != null && delayRef.StartsWith("!"))
            {
                if (TargetType == typeof(DateTime) || TargetType == typeof(TimeSpan) || TargetType == typeof(Week) || TargetType == typeof(Month)) fixedDelay = TimeSpan.Parse(delayRef.Substring(1), CultureInfo.InvariantCulture);
                else fixedDelay = Convert.ChangeType(delayRef.Substring(1), TargetType, CultureInfo.InvariantCulture);
            }

            object res = fixedDelay;
            if (fixedDelay == null)
            {
                if (string.IsNullOrWhiteSpace(delayRef)) return null;
                PropertyAccessor delayProp = null;
                try
                {
                    delayProp = new PropertyAccessor(model, delayRef, false);
                }
                catch
                {
                    return null;
                }
                if (delayProp == null) return null;
                res = delayProp.Value;
            }
            if (invertSign) res = changeSign(res);
            if ((TargetType == typeof(DateTime) || TargetType == typeof(TimeSpan) || TargetType == typeof(Week) || TargetType == typeof(Month)) && res != null) return Convert.ToInt64(((TimeSpan)res).TotalMilliseconds);
            return res;
        }
        private object changeSign(object value)
        {
            if (value == null) return null;
            if (TargetType == typeof(Int32))
            {
                value = -Convert.ToInt32(value);
            }
            else if (TargetType == typeof(Int16))
            {
                value = -Convert.ToInt16(value);
            }
            else if (TargetType == typeof(Int64))
            {
                value = -Convert.ToInt64(value);
            }
            else if (TargetType == typeof(UInt32))
            {
                value = -Convert.ToUInt32(value);
            }
            else if (TargetType == typeof(UInt16))
            {
                value = -Convert.ToUInt16(value);
            }
            else if (TargetType == typeof(UInt64))
            {
                value = -Convert.ToInt64(value);
            }
            else if (TargetType == typeof(byte))
            {
                value = -Convert.ToSByte(value);
            }
            else if (TargetType == typeof(sbyte))
            {
                value = -Convert.ToSByte(value);
            }
            else if (TargetType == typeof(decimal))
            {
                value = -Convert.ToDecimal(value);
            }
            else if (TargetType == typeof(float))
            {
                value = -Convert.ToSingle(value);
            }
            else if (TargetType == typeof(double))
            {
                value = -Convert.ToDouble(value);
            }
            else if (TargetType == typeof(DateTime) || TargetType == typeof(TimeSpan) || TargetType == typeof(Week) || TargetType == typeof(Month))
            {
                value = -(TimeSpan)value;

            }
            return value;
        }
        private object addDelay(string delayRef, ref IComparable value, object model, object fixedDelay = null, bool subtract = false)
        {
            if (delayRef != null && delayRef.StartsWith("!"))
            {
                if (TargetType == typeof(DateTime) || TargetType == typeof(Month) || TargetType == typeof(Week)) fixedDelay = TimeSpan.Parse(delayRef.Substring(1), CultureInfo.InvariantCulture);
                else fixedDelay = Convert.ChangeType(delayRef.Substring(1), TargetType, CultureInfo.InvariantCulture);
            }
            object toAdd = fixedDelay;
            if (fixedDelay == null)
            {
                if (string.IsNullOrWhiteSpace(delayRef)) return null;
                PropertyAccessor delayProp = null;
                try
                {
                    delayProp = new PropertyAccessor(model, delayRef, false);
                }
                catch
                {
                    return null;
                }
                if (delayProp == null) return null;
                toAdd = delayProp.Value;
            }
            if (toAdd == null) return null;
            if (subtract)
            {
                if (TargetType == typeof(Int32))
                {
                    value = Convert.ToInt32(value) - Convert.ToInt32(toAdd);
                }
                else if (TargetType == typeof(Int16))
                {
                    value = Convert.ToInt16(value) - Convert.ToInt16(toAdd);
                }
                else if (TargetType == typeof(Int64))
                {
                    value = Convert.ToInt64(value) - Convert.ToInt64(toAdd);
                }
                else if (TargetType == typeof(UInt32))
                {
                    value = Convert.ToUInt32(value) - Convert.ToUInt32(toAdd);
                }
                else if (TargetType == typeof(UInt16))
                {
                    value = Convert.ToUInt16(value) - Convert.ToUInt16(toAdd);
                }
                else if (TargetType == typeof(UInt64))
                {
                    value = Convert.ToUInt64(value) - Convert.ToUInt64(toAdd);
                }
                else if (TargetType == typeof(byte))
                {
                    value = Convert.ToByte(value) - Convert.ToByte(toAdd);
                }
                else if (TargetType == typeof(sbyte))
                {
                    value = Convert.ToSByte(value) - Convert.ToSByte(toAdd);
                }
                else if (TargetType == typeof(decimal))
                {
                    value = Convert.ToDecimal(value) - Convert.ToDecimal(toAdd);
                }
                else if (TargetType == typeof(float))
                {
                    value = Convert.ToSingle(value) - Convert.ToSingle(toAdd);
                }
                else if (TargetType == typeof(double))
                {
                    value = Convert.ToDouble(value) - Convert.ToDouble(toAdd);
                }
                else if (TargetType == typeof(TimeSpan))
                {
                    value = (TimeSpan)value - (TimeSpan)toAdd;
                    toAdd = -Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(DateTime))
                {
                    value = Convert.ToDateTime(value).Add(-(TimeSpan)toAdd);
                    toAdd = -Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(Month))
                {
                    value = Month.FromDateTime(((Month)value).ToDateTime().Add(-(TimeSpan)toAdd));
                    toAdd = -Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(Week))
                {
                    value = Week.FromDateTime(((Week)value).StartDate().Add(-(TimeSpan)toAdd));
                    toAdd = -Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
            }
            else
            {
                if (TargetType == typeof(Int32))
                {
                    value = Convert.ToInt32(value) + Convert.ToInt32(toAdd);
                }
                else if (TargetType == typeof(Int16))
                {
                    value = Convert.ToInt16(value) + Convert.ToInt16(toAdd);
                }
                else if (TargetType == typeof(Int64))
                {
                    value = Convert.ToInt64(value) + Convert.ToInt64(toAdd);
                }
                else if (TargetType == typeof(UInt32))
                {
                    value = Convert.ToUInt32(value) + Convert.ToUInt32(toAdd);
                }
                else if (TargetType == typeof(UInt16))
                {
                    value = Convert.ToUInt16(value) + Convert.ToUInt16(toAdd);
                }
                else if (TargetType == typeof(UInt64))
                {
                    value = Convert.ToUInt64(value) + Convert.ToUInt64(toAdd);
                }
                else if (TargetType == typeof(byte))
                {
                    value = Convert.ToByte(value) + Convert.ToByte(toAdd);
                }
                else if (TargetType == typeof(sbyte))
                {
                    value = Convert.ToSByte(value) + Convert.ToSByte(toAdd);
                }
                else if (TargetType == typeof(decimal))
                {
                    value = Convert.ToDecimal(value) + Convert.ToDecimal(toAdd);
                }
                else if (TargetType == typeof(float))
                {
                    value = Convert.ToSingle(value) + Convert.ToSingle(toAdd);
                }
                else if (TargetType == typeof(double))
                {
                    value = Convert.ToDouble(value) + Convert.ToDouble(toAdd);
                }
                else if (TargetType == typeof(TimeSpan))
                {
                    value = (TimeSpan)value + (TimeSpan)toAdd;
                    toAdd = Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(DateTime))
                {
                    value = Convert.ToDateTime(value).Add((TimeSpan)toAdd);
                    toAdd = Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(Month))
                {
                    value = Month.FromDateTime(((Month)value).ToDateTime().Add((TimeSpan)toAdd)); 
                    toAdd = Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
                else if (TargetType == typeof(Week))
                {
                    value = Week.FromDateTime(((Week)value).StartDate().Add((TimeSpan)toAdd));
                    toAdd = Convert.ToInt64(((TimeSpan)toAdd).TotalMilliseconds);
                }
            }
            return toAdd;
        }
        private object _lastModel;
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value == null) return ValidationResult.Success; ;
            processNowToday(_SMaximum, true); processNowToday(_SMinimum, false);
            IComparable toCheck = value as IComparable;
            if (toCheck == null) throw (new InvalidAttributeApplicationException(Resources.InvalidDynamicRangeApplication));

            if (validationContext != null)
            {
                _lastModel = validationContext.ObjectInstance;
                if (Minimum != null && toCheck.CompareTo(TargetType == typeof(DateTime) ? ((DateTime)Minimum).AddMinutes(-toleranceMin) : Minimum) < 0)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                if (Maximum != null && toCheck.CompareTo(TargetType == typeof(DateTime) ? ((DateTime)Maximum).AddMinutes(toleranceMax) : Maximum) > 0)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            else
            {
                _lastModel = null;
                if (Minimum != null && toCheck.CompareTo(TargetType == typeof(DateTime) ? ((DateTime)Minimum).AddMinutes(-toleranceMin) : Minimum) < 0)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                if (Maximum != null && toCheck.CompareTo(TargetType == typeof(DateTime) ? ((DateTime)Maximum).AddMinutes(toleranceMax) : Maximum) > 0)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            if (parsedDynamicMinimum != null && validationContext != null)
            {
                foreach (var limit in parsedDynamicMinimum)
                {
                    PropertyAccessor dynamicMinimumProp =
                        new PropertyAccessor(_lastModel = validationContext.ObjectInstance, limit.MainValue, false);

                    if (dynamicMinimumProp != null && dynamicMinimumProp.Value != null)
                    {
                        IComparable dMin = dynamicMinimumProp.Value as IComparable;

                        if (dMin == null) continue;
                        var delayString = limit.Delay ?? DynamicMinimumDelay;
                        if (delayString != null) addDelay(delayString, ref dMin, validationContext.ObjectInstance, null, limit.Subtract);

                        if (toCheck.CompareTo(dMin) < 0)
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

                    }
                }
            }
            if (parsedDynamicMaximum != null && validationContext != null)
            {
                foreach (var limit in parsedDynamicMaximum)
                {
                    PropertyAccessor dynamicMaximumProp =
                        new PropertyAccessor(_lastModel = validationContext.ObjectInstance, limit.MainValue, false);

                    if (dynamicMaximumProp != null && dynamicMaximumProp.Value != null)
                    {

                        IComparable dMax = dynamicMaximumProp.Value as IComparable;
                        if (dMax == null) continue;
                        var delayString = limit.Delay ?? DynamicMaximumDelay;
                        if (delayString != null) addDelay(delayString, ref dMax, validationContext.ObjectInstance, null, limit.Subtract);

                        if (toCheck.CompareTo(dMax) > 0)
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));


                    }
                }
            }
            return ValidationResult.Success;
        }
        string stringify(object value)
        {
            if (value == null) return "0";
            else return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
        public object GetGlobalMaximum(object model, out List<string> clientMaximum, out List<string> maxDelay)
        {
            _lastModel = model;
            processNowToday(_SMaximum, true);
            clientMaximum = new List<string>(); maxDelay = new List<string>();
            if (parsedDynamicMaximum == null) return Maximum;
            IComparable constantMaximum = Maximum;
            IComparable dMax = null;
            foreach (var limit in parsedDynamicMaximum)
            {
                if (model == null)
                {
                    clientMaximum.Add(limit.MainValue);
                    maxDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMaximumDelay, model, null, limit.Subtract)));
                    continue;
                }

                PropertyAccessor dynamicMaximumProp = null;
                try
                {
                    dynamicMaximumProp = new PropertyAccessor(model, limit.MainValue, false);
                }
                catch
                {
                    clientMaximum.Add(limit.MainValue);
                    maxDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMaximumDelay, model, null, limit.Subtract)));
                }
                if (dynamicMaximumProp != null)
                {
                    if (dynamicMaximumProp[typeof(MileStoneAttribute)].Length > 0)
                    {
                        if (dynamicMaximumProp.Value != null)
                        {
                            var currMax = dynamicMaximumProp.Value as IComparable;
                            if (currMax == null) continue;
                            addDelay(limit.Delay ?? DynamicMaximumDelay, ref currMax, model, null, limit.Subtract);
                            if (dMax == null) dMax = currMax;
                            else if (dMax.CompareTo(currMax) < 0) dMax = currMax;
                        }
                    }
                    else
                    {
                        clientMaximum.Add(limit.MainValue);
                        maxDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMaximumDelay, model, null, limit.Subtract)));
                    }
                }
            }
            object res = null;
            if (constantMaximum == null) res = dMax;
            else if (dMax == null) res = constantMaximum;
            else res = dMax.CompareTo(constantMaximum) > 0 ? constantMaximum : dMax;

            return res;

        }
        public object GetGlobalMinimum(object model, out List<string> clientMinimum, out List<string> minDelay)
        {
            _lastModel = model;
            processNowToday(_SMinimum, false);
            clientMinimum = new List<string>(); minDelay = new List<string>();
            if (parsedDynamicMinimum == null) return Minimum;
            IComparable constantMinimum = Minimum;
            IComparable dMin = null;
            foreach (var limit in parsedDynamicMinimum)
            {
                if (model == null)
                {
                    clientMinimum.Add(limit.MainValue);
                    minDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMinimumDelay, model, null, limit.Subtract)));
                    return Minimum;
                }

                PropertyAccessor dynamicMinimumProp = null;
                try
                {
                    dynamicMinimumProp = new PropertyAccessor(model, limit.MainValue, false);
                }
                catch
                {
                    clientMinimum.Add(limit.MainValue);
                    minDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMinimumDelay, model, null, limit.Subtract)));
                }
                if (dynamicMinimumProp != null)
                {
                    if (dynamicMinimumProp[typeof(MileStoneAttribute)].Length > 0)
                    {
                        if (dynamicMinimumProp.Value != null)
                        {
                            var currMin = dynamicMinimumProp.Value as IComparable;
                            if (currMin == null) continue;
                            addDelay(limit.Delay ?? DynamicMinimumDelay, ref currMin, model, null, limit.Subtract);
                            if (dMin == null) dMin = currMin;
                            else if (dMin.CompareTo(currMin) > 0) dMin = currMin;
                        }
                    }
                    else
                    {
                        clientMinimum.Add(limit.MainValue);
                        minDelay.Add(stringify(getDelay(limit.Delay ?? DynamicMinimumDelay, model, null, limit.Subtract)));
                    }
                }
            }
            object res = null;
            if (constantMinimum == null) res = dMin;
            else if (dMin == null) res = constantMinimum;
            else res = dMin.CompareTo(constantMinimum) < 0 ? constantMinimum : dMin;

            return res;
        }
        public override string FormatErrorMessage(string name)
        {
            List<string> par1, par2;
            string min = null, max = null;
            IComparable cmin = _lastModel != null ? GetGlobalMinimum(_lastModel, out par1, out par2) as IComparable ?? Minimum : Minimum;
            IComparable cmax = _lastModel != null ? GetGlobalMaximum(_lastModel, out par1, out par2) as IComparable ?? Maximum : Maximum;
            cmin = (cmin != null && TargetType == typeof(Week)) ? ((Week)cmin).StartDate() : cmin;
            cmax = (cmax != null && TargetType == typeof(Week)) ? ((Week)cmax).StartDate() : cmax;
            cmin = (cmin != null && TargetType == typeof(Month)) ? ((Month)cmin).ToDateTime() : cmin;
            cmax = (cmax != null && TargetType == typeof(Month)) ? ((Month)cmax).ToDateTime() : cmax;
            if (cmin != null)
            {
                IFormattable fmin = cmin as IFormattable;
                if (fmin != null && MaxMinDisplayFormat != null) min = fmin.ToString(MaxMinDisplayFormat, CultureInfo.CurrentCulture);
                else min = cmin.ToString();
            }
            if (cmax != null)
            {
                IFormattable fmax = cmax as IFormattable;
                if (fmax != null && MaxMinDisplayFormat != null) max = fmax.ToString(MaxMinDisplayFormat, CultureInfo.CurrentCulture);
                else max = cmax.ToString();
            }
            string[] messages = ErrorMessageString.Split(new char[] { '|' }, StringSplitOptions.None);
            string message;
            if (cmin != null && cmax != null) message = messages[0];
            else if (cmin != null) message = messages[Math.Min(messages.Length - 1, 1)];
            else if (cmax != null) message = messages[Math.Min(messages.Length - 1, 2)];
            else message = messages[Math.Min(messages.Length - 1, 3)];
            return string.Format(message, name, min ?? string.Empty, max ?? string.Empty);
        }
    }
}
