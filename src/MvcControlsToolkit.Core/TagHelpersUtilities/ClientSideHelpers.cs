using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public static class ClientSideHelpers
    {
        public static string getClientType(ModelMetadata metaData)
        {
            var type = metaData.UnderlyingOrModelType;


            var hint = (metaData.DataTypeName ?? metaData.TemplateHint)?.ToLowerInvariant();
            string ctype = "text";
            if (hint == "color") ctype = hint;
            else if (type == typeof(bool)) ctype = "checkbox";
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal) ||
                metaData.IsEnum)
            {
                ctype = "number";
            }
            else if (type == typeof(DateTime))
            {
                if (hint == "date")
                {
                    ctype = "date";
                }
                else if (hint == "time")
                {
                    ctype = "time";
                }
                else
                {
                    ctype = "datetime-local";
                }

            }
            else if (type == typeof(DateTimeOffset))
            {
                ctype = "datetime-local";
            }
            else if (type == typeof(TimeSpan) && hint == "time")
            {
                ctype = "time";
            }
            else if (type == typeof(Week))
            {
                ctype = "week";
            }
            else if (type == typeof(Month))
            {
                ctype = "month";
            }
            
            return ctype;
        }
    }
}
