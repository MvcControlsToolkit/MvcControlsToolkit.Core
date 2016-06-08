using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class RequestTransformationsRegister
    {
        private static Regex indexDetector = new Regex(@"(^|\.|\])[0-9]+[\.\[$]");
        private Dictionary<string, string> allPrefixes = new Dictionary<string, string>();
        public void Add(string prefix, string index)
        {
            allPrefixes[prefix] = index;
        }
        public string GetIndex(string prefix)
        {
            string res;
            if (allPrefixes.TryGetValue(prefix, out res)) return res;
            else return null;
        }
        public void Fill(Microsoft.AspNetCore.Http.HttpRequest x)
        {
            if (x.HasFormContentType)
            {
                foreach (var y in x.Form.Select(m => m.Key))
                {
                    foreach (Match match in indexDetector.Matches(y))
                    {
                        var val = match.Value;
                        if (val[0] == '.' || val[0] == ']') val = val.Substring(1);
                        if (val[val.Length - 1] == '.' || val[val.Length - 1] == '[') val = val.Substring(0, val.Length - 1);
                        if (match.Index == 0)
                            this.Add(string.Empty, val);
                        else
                            this.Add(y.Substring(0, match.Index), val);
                    }
                }
            }
        }
    }
}
