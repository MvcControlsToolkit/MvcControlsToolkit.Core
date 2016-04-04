using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.Http;
namespace MvcControlsToolkit.Core.Options.Providers
{
    public class ConfigurationProvider: IOptionsProvider
    {
        virtual public bool CanSave
        {
            get
            {
                return false;
            }
        }
        virtual public bool Enabled(HttpContext ctx)
        {
            return true;
        }
        public uint Priority { get; set; } 
        public IConfiguration Configuration { get; set; } 
        public string Prefix { get; set; }

        public bool AutoSave { get; set; }

        public string SourcePrefix { get; set; }

        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            throw new NotImplementedException();
        }
        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            if (string.IsNullOrWhiteSpace(SourcePrefix)) SourcePrefix = Prefix;
            var path = SourcePrefix.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            IConfiguration currConfiguration = Configuration;
            List<IOptionsProvider> res = new List<IOptionsProvider>();
            for (int i=0; i<path.Length; i++)
            {
                currConfiguration = currConfiguration.GetChildren().Where(m => m.Key == path[i]).FirstOrDefault();
                if (currConfiguration == null) return res;

            }
            loadSection(dict, Prefix, currConfiguration as ConfigurationSection, res);
            return res;
        }
        private void loadSection(IOptionsDictionary dict, string prefix, IConfigurationSection section, List<IOptionsProvider> res)
        {
            if (section.Value != null)
            {
                var pres = dict.AddOption(this, prefix, section.Value, Priority);
                if (pres != null) res.Add(pres);
            }
            foreach(var child in section.GetChildren())
            {
                loadSection(dict, prefix + "." + child.Key, child, res);
            }
        }
        public ConfigurationProvider(string prefix, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            if (configuration == null) throw new ArgumentNullException("configuration");
            Prefix = prefix;
            Configuration = configuration;
        }
    }
}
