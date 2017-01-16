using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OData.UriParser;

namespace MvcControlsToolkit.Core.OData.Parsers
{
    public class ODataParserBase
    {
        protected string buildPropertyAccess(SingleValuePropertyAccessNode node)
        {
            var sb = new StringBuilder();
            sb.Append((node.Property as EdmClrProperty).Property.Name);
            var currNode = node.Source as SingleValuePropertyAccessNode;
            while (currNode != null)
            {
                sb.Append('.');
                sb.Append((currNode.Property as EdmClrProperty).Property.Name);
                currNode = currNode.Source as SingleValuePropertyAccessNode;
            }
            return sb.ToString();
        }
    }
}
