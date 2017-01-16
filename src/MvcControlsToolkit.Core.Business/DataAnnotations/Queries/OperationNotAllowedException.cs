using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    public class OperationNotAllowedException: Exception
    {
        private string _PropertyName { get; set; }
        private string _OperationName { get; set; }
        public string PropertyName { get {return _PropertyName; } }
        public string OperationName { get { return _OperationName; } }
        public OperationNotAllowedException(string propertyName, string operationName = null)
        {
            _PropertyName = propertyName;
            _OperationName = operationName;
        }
        public override string Message
        {
            get
            {
                if( OperationName == null )
                    return string.Format(Resources.PropertyNotAllowed, PropertyName);
                else if(PropertyName == null)
                    return string.Format(Resources.NotSupportedOperation, OperationName);
                else
                    return string.Format(Resources.NotSupportedOperationOn, OperationName, PropertyName);

            }
        }
    }
    //public class SortingNotAllowedException: OperationNotAllowedException
    //{
    //    public SortingNotAllowedException(string propertyName)
    //        :base(propertyName)
    //    {
            
    //    }
    //}
    //public class GroupingNotAllowedException : OperationNotAllowedException
    //{
    //    public GroupingNotAllowedException(string propertyName)
    //        : base(propertyName)
    //    {

    //    }
    //}
}
