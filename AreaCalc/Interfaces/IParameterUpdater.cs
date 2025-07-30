using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IParameterUpdater
    {
        void UpdateRoomParameter(Room room, string parameterName, double value);

        double GetRoomParameterValueRaw(Room room, string parameterName);

        bool HasParameter(Room room, string paramName);
    }
}
