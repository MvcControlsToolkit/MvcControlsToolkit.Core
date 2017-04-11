using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Views
{
    public class Endpoint
    {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Delete = "DELETE";
        public const string Patch = "PATCH";
        public string BaseUrl { get; set; }
        public string Verb { get; set; }
        public bool AccpetsJson {get; set;}
        public bool ReturnsJson { get; set; }
        public string BearerToken { get; set; }
        public string AjaxId { get; set; }
    }
}
