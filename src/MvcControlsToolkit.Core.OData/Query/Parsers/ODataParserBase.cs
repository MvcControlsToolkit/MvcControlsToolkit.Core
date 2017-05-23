using System;
using System.Collections.Generic;
using System.Reflection;
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
        protected string buildPropertyAccess(SingleValuePropertyAccessNode node, out PropertyInfo property)
        {
            var sb = new StringBuilder();
            property = (node.Property as EdmClrProperty).Property;
            sb.Append((node.Property as EdmClrProperty).Property.Name);
            var currNode = node.Source as SingleValuePropertyAccessNode;
            while (currNode != null)
            {
                sb.Append('.');
                property = (currNode.Property as EdmClrProperty).Property;
                sb.Append(property.Name);
                currNode = currNode.Source as SingleValuePropertyAccessNode;
            }
            return sb.ToString();
        }
    }
}
