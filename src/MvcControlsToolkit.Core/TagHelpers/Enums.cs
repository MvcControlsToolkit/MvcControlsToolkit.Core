using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public enum TemplateType {Partial,  ViewComponent, InLine, Function};
    [Flags]
    public enum Functionalities {ReadOnly=0, Edit=1, EditDetail=2, ShowDetail=4, Delete=8, Append=16, AddBefore=32, AddAfter=64, AppendDetail = 128, PrependDetail = 256, AddBeforeDetail = 512, AddAfterDetail = 1024, Prepend = 2048, GroupDetail=4096, AnyEdit=3, AnyAdd = 4080, FullInLine = 25, FullDetail=138, HasRowButtons= 5743, EditOnlyHasRowButtons = 5742, All= ~0 };
    
    public enum StandardButtons {Edit, EditDetail, ShowDetail, Delete, Append, AddBefore, AddAfter, AppendDetail, PrependDetail, AddBeforeDetail, AddAfterDetail, Prepend, Save, Undo, Redo, UndoAll, RedoAll, NextPage, LastPage, PreviousPage, FirstPage, FilterWindow, GroupWindow, SortWindow, SortAscending, SortDescending, Close, GroupDetail}
}
