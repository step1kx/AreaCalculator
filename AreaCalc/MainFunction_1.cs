﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AreaCalc
{
    [Transaction(TransactionMode.Manual)]
    public class MainFunction_1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument activeUIDocument = uiapp.ActiveUIDocument;
            UIDocument uidoc = activeUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            MainWindow myWindow = new MainWindow(doc);

            myWindow.ShowDialog();

            return Result.Succeeded;

        }
       
    }
}
