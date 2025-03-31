using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IDraftingServices
    {
        ViewDrafting CreateDraftingView(Document doc);
        FamilySymbol FindFamilySymbol(Document doc, string familyName);
        FamilyInstance PlaceFamilyInstance(Document doc, ViewDrafting view, FamilySymbol symbol, XYZ location);
    }
}
