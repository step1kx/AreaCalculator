using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IApartmentDataProvider
    {
        (Dictionary<string, List<Autodesk.Revit.DB.Architecture.Room>> apartmentsData, List<string> errors) GetApartmentData(Autodesk.Revit.DB.Document doc, bool useActiveView);
        Dictionary<string, double> GetRoomCoefficients(Autodesk.Revit.DB.Document doc, bool useActiveView);
    }
}
