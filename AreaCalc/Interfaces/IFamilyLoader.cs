using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IFamilyLoader
    {
        FamilySymbol LoadFamilyFromResource(Document doc);
    }
}
