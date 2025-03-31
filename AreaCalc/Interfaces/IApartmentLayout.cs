﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Interfaces
{
    public interface IApartmentLayout
    {
        void CreateApartmentLayout(Document doc, Dictionary<string, List<Room>> apartmentsData);

    }
}
