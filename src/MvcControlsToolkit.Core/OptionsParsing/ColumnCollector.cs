using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public class ColumnCollector: BaseCollector
    {
        public Template<Column> DisplayTemplate { get; private set; }
        public Template<Column> EditTemplate { get; private set; }
        public ColumnConnectionInfos ColumnConnection { get; private set; }
        public ColumnCollector(ReductionContext ctx)
        {
            foreach(var item in ctx.Results)
            {
                if (item.Token == TagTokens.DTemplate) DisplayTemplate = item.Result as Template<Column>;
                else if (item.Token == TagTokens.ETemplate) EditTemplate = item.Result as Template<Column>;
                else if(item.Token == TagTokens.ExternalKeyConnection) ColumnConnection = item.Result as ColumnConnectionInfos;
            }
            if (DisplayTemplate == null) DisplayTemplate = ctx.Defaults.DColumnTemplate;
            if (EditTemplate == null) EditTemplate = ctx.Defaults.EColumnTemplate;
        }

        public override object Process(TagHelper tag, DefaultTemplates defaults)
        {
            var colTag = tag as ColumnBaseTagHelper;
            Column res = tag is ColumnTagHelper ? 
                new Column((colTag as ColumnTagHelper).For, DisplayTemplate, EditTemplate):
                new Column ((colTag as CustomColumnTagHelper).Name, DisplayTemplate, EditTemplate);
            res.Description = colTag.Description;
            res.ColumnTitle = colTag.ColumnTitle;
            res.DisplayFormat = colTag.DisplayFormat;
            res.EditOnly = colTag.EditOnly;
            res.Hidden = colTag.Hidden;
            res.NullDisplayText = colTag.NullDisplayText;
            res.Order = colTag.ColumnOrder;
            res.PlaceHolder = colTag.PlaceHolder;
            res.ReadOnly = colTag.ReadOnly;
            res.Widths = colTag.Widths;
            res.DetailWidths = colTag.DetailWidths;
            res.ColumnCssClass = colTag.ColumnCssClass;
            res.ColSpan = colTag.ColSpan;
            res.InputCssClass = colTag.InputCssClass;
            res.CheckboxCssClass = colTag.CheckboxCssClass;
            res.ColumnConnection = ColumnConnection;
            return res;
        }
    }
}
