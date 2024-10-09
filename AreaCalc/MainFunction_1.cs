using Autodesk.Revit.Attributes;
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
        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        //    Document doc = uiDoc.Document;

        //    // Создаем словарь для хранения данных
        //    Dictionary<string, double> apartmentAreas = new Dictionary<string, double>();

        //    // Получаем все помещения в проекте
        //    FilteredElementCollector collector = new FilteredElementCollector(doc);
        //    collector.OfCategory(BuiltInCategory.OST_Rooms);

        //    foreach (Room room in collector)
        //    {
        //        // Получаем тип помещения и его площадь
        //        string roomType = room.LookupParameter("Помещение")?.AsString() ?? "Неизвестный тип";
        //        double roomArea = room.Area;

        //        // Сохраняем данные в словарь
        //        if (apartmentAreas.ContainsKey(roomType))
        //        {
        //            apartmentAreas[roomType] += roomArea;
        //        }
        //        else
        //        {
        //            apartmentAreas[roomType] = roomArea;
        //        }
        //    }

        //    // Выводим результаты в консоль или другое место
        //    foreach (var entry in apartmentAreas)
        //    {
        //        TaskDialog.Show("Квартирография", $"Тип квартиры: {entry.Key}, Площадь: {entry.Value} м²");
        //    }

        //    return Result.Succeeded;
        //}
    }
}
