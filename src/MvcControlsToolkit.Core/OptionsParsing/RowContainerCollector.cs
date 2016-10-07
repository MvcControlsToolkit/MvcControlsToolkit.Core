using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public class RowContainerCollector : RowCollector
    {
        public IList<RowType> Rows { get; private set; }
        public IList<KeyValuePair<string, string>> Toolbars { get; private set; }
        private void addRow(RowType row)
        {
            if (Rows == null) Rows = new List<RowType>();
            Rows.Add(row);
        }
        private void addToolbar(KeyValuePair<string, string> toolbar)
        {
            if (Toolbars == null) Toolbars = new List<KeyValuePair<string, string>>();
            Toolbars.Add(toolbar);
        }
        private ReductionContext ctx;
        public RowContainerCollector(ReductionContext ctx) : base(ctx, null)
        {
            this.ctx = ctx;
            foreach (var item in ctx.Results)
            {
                if (item.Token == TagTokens.Row && !ctx.RowParsingDisabled) addRow(item.Result as RowType);
                if (item.Token == TagTokens.Content) addToolbar((KeyValuePair<string, string>)(item.Result) );
            }
        }
        public override object Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelper tag, DefaultTemplates defaults)
        {

            var first = ctx.RowParsingDisabled? null : base.Process(tag, defaults);
            if(first != null)
            {
                if (Rows == null) addRow(first as RowType);
                else Rows.Insert(0, first as RowType);
            }
            uint i = 0;
            if (Rows != null) foreach (var row in Rows)
                {
                    row.RowInit(Rows);
                    foreach (var col in row.Columns)
                    {
                        if (col.EditTemplate == null) col.EditTemplate = defaults.EColumnTemplate;
                        if (col.DisplayTemplate == null) col.DisplayTemplate = defaults.DColumnTemplate;
                    }
                    row.Order = i++;
                }
            return  Tuple.Create(Rows, Toolbars);
        }
    }
}
