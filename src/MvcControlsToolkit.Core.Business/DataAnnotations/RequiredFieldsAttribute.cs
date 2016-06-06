using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Reflection;


namespace MvcControlsToolkit.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiredFieldsAttribute: ValidationAttribute
    {
        public RequiredFieldsAttribute() 
            : base()
        {
        }
        public RequiredFieldsAttribute(string message)
            : base(message)
        {
        }
        public string Fields
        {
            get;
            set;
        }
        public bool RejectEmptyIEnumerables
        {
            get;
            set;
        }
        private string allErrorFieldNames = null;
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                allErrorFieldNames);
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            string[] fields = Fields.Split(',');
            List<string> errorFields = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (string x in fields)
            {
                string field = x.Trim();

                PropertyAccessor po = new PropertyAccessor(value, field, false);

                object ob = po.Value;
                if (ob == null || (RejectEmptyIEnumerables && (ob is IEnumerable) && !(ob as IEnumerable).GetEnumerator().MoveNext()))
                {
                    errorFields.Add(field);
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(po.DisplayName);
                    continue;
                }
                if (!ob.GetType().GetTypeInfo().IsValueType) continue;
                if (ob.Equals(Activator.CreateInstance(ob.GetType())))
                {
                    errorFields.Add(field);
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(po.DisplayName);
                }
            }
            if (errorFields.Count == 0) return ValidationResult.Success;
            allErrorFieldNames = sb.ToString();
            return new ValidationResult(null, errorFields);
        }
    }
}
