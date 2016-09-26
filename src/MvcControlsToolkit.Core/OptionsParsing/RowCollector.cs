using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public class RowCollector: BaseCollector
    {
        public Template<RowType> DisplayTemplate { get; private set; }
        public Template<RowType> EditTemplate { get; private set; }
        public IList<Column> Columns { get; private set; }
        public IList<ModelExpression> RemoveColumns { get; private set; }
        RowType inherit;
        private void addColumn(Column col)
        {
            if (Columns == null) Columns = new List<Column>();
            Columns.Add(col);
        }
        private void removeColumn(Column col)
        {
            if (RemoveColumns == null) RemoveColumns = new List<ModelExpression>();
            RemoveColumns.Add(col.For);
        }
        private void removeExpression(ModelExpression exp)
        {
            if (RemoveColumns == null) RemoveColumns = new List<ModelExpression>();
            RemoveColumns.Add(exp);
        }
        public override object Process(TagHelper tag, DefaultTemplates defaults)
        {
            var rowTag = tag as RowTypeTagHelper;
            var expression = rowTag.For;
            if (this is RowContainerCollector && expression.Metadata.IsEnumerableType)
            {
                var explorer = expression.ModelExplorer.GetExplorerForExpression(expression.Metadata.ElementMetadata, null);
                expression = new ModelExpression(string.Empty, explorer);
            }
            var keyName = rowTag.KeyName != null ? rowTag.KeyName.Metadata.PropertyName : null;
            RowType result = null;
            if (inherit != null)
            {
                result= new RowType(expression, inherit, keyName ?? inherit.KeyName,
                    defaults.IsDetail,
                    Columns,
                    RemoveColumns,
                    defaults.RenderHiddenFields);
                
            }
            else if (Columns != null || rowTag.AllProperties)
            {
                result= new RowType(expression, keyName, defaults.IsDetail,
                    Columns, rowTag.AllProperties, RemoveColumns, defaults.RenderHiddenFields);
            }
            else return null;
            result.RequiredFunctionalities = rowTag.RequiredFunctionalities;
            result.CustomButtons = rowTag.CustomButtons;
            result.RowCssClass = rowTag.RowCssClass;
            result.LocalizationType = rowTag.LocalizationType;
            result.InputCssClass = rowTag.InputCssClass;
            result.CheckboxCssClass = rowTag.CheckboxCssClass;
            return result;
        }

        public RowCollector(ReductionContext ctx, RowType inherit)
        {
            this.inherit = inherit;
            foreach (var item in ctx.Results)
            {
                if (item.Token == TagTokens.DTemplate) DisplayTemplate = item.Result as Template<RowType>;
                else if (item.Token == TagTokens.ETemplate) EditTemplate = item.Result as Template<RowType>;
                else if (item.Token == TagTokens.Column)
                {
                    if (item.SubToken > 0)
                    {
                        var col = item.Result as Column;
                        addColumn(col);
                        if (col.ColumnConnection != null) removeExpression(col.ColumnConnection.DisplayProperty);
                    }
                    else removeColumn(item.Result as Column);
                }
            }
            if (DisplayTemplate == null) DisplayTemplate = ctx.Defaults.DRowTemplate;
            if (EditTemplate == null) EditTemplate = ctx.Defaults.ERowTemplate;
        }
    }
}
