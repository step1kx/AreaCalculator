using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Models
{
    public class ParameterUpdater : IParameterUpdater
    {
        public void UpdateRoomParameter(Room room, string parameterName, double value)
        {
            Parameter param = room.LookupParameter(parameterName);
            if (param != null && !param.IsReadOnly)
            {
                param.Set(value);
            }
        }
    }
}
