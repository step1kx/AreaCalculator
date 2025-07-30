using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Models
{
    public class ParameterUpdater : IParameterUpdater
    {
        const double squareMtoFt = 0.09203;
        public void UpdateRoomParameter(Room room, string parameterName, double value)
        {
            Parameter param = room.LookupParameter(parameterName);
            if (param != null && !param.IsReadOnly && param.StorageType == StorageType.Double)
            {
                param.Set(value); 
            }
        }

        public double GetRoomParameterValueRaw(Room room, string parameterName)
        {
            Parameter param = room.LookupParameter(parameterName);
            if (param != null && param.StorageType == StorageType.Double)
            {
                return param.AsDouble(); 
            }

            return 0;
        }

        public bool HasParameter(Room room, string paramName)
        {
            return room.LookupParameter(paramName) != null;
        }
    }
}
