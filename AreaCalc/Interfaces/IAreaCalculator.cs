using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IAreaCalculator
    {
        double CalculateFormula(string formula, Dictionary<string, double> roomAreas);
        Dictionary<string, double> CalculateRoomAreas(List<Room> rooms);
        (double livingArea, double usualArea, double totalArea) CalculateAreas(List<Room> rooms, string livingFormula, string usualFormula, string totalFormula);
    }
}
