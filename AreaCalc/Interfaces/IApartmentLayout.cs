using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IApartmentLayout
    {
        void CreateApartmentLayout(Autodesk.Revit.DB.Document doc, Dictionary<string, List<Autodesk.Revit.DB.Architecture.Room>> apartmentsData);

    }
}
