using AreaCalc.Interfaces;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AreaCalc.Models
{
    public class ApartmentDataProvider : IApartmentDataProvider
    {
        public (Dictionary<string, List<Room>> apartmentsData, List<string> errors) GetApartmentData(Document doc, bool useActiveView)
        {
            var apartmentsData = new Dictionary<string, List<Room>>();
            var errors = new List<string>();
            FilteredElementCollector collector;

            if (useActiveView)
            {
                var activeView = doc.ActiveView.Id;
                collector = new FilteredElementCollector(doc, activeView);
            }
            else
            {
                collector = new FilteredElementCollector(doc);
            }

            var rooms = collector
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>();

            foreach (Room room in rooms)
            {
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                if (roomName == "Лестничная клетка" || roomName == "Лифт")
                    continue;

                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomFilterParam = room.LookupParameter("КГ.Фильтр");
                Parameter roomLivingParam = room.LookupParameter("КГ.Жилые комнаты");
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

                string apartmentNumber = apartmentNumberParam?.AsString();

                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null || roomFilterParam == null || roomLivingParam == null)
                {
                    List<string> missingParams = new List<string>();
                    if (apartmentNumberParam == null) missingParams.Add("КГ.Номер квартиры");
                    if (areaParam == null) missingParams.Add("Площадь помещения");
                    if (roomTypeParam == null) missingParams.Add("КГ.Тип помещения");
                    if (roomFilterParam == null) missingParams.Add("КГ.Фильтр");
                    if (roomLivingParam == null) missingParams.Add("КГ.Жилые комнаты");

                    string missingParamsMessage = $"Для комнаты '{roomName}' (ID: {room.Id}) не найдены параметры: {string.Join(", ", missingParams)}";
                    errors.Add(missingParamsMessage);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(apartmentNumber))
                {
                    if (!apartmentsData.ContainsKey(apartmentNumber))
                    {
                        apartmentsData[apartmentNumber] = new List<Room>();
                    }
                    apartmentsData[apartmentNumber].Add(room);
                }
            }

            return (apartmentsData, errors);
        }

        public Dictionary<string, double> GetRoomCoefficients(Document doc, bool useActiveView)
        {
            var coefficients = new Dictionary<string, double>();
            FilteredElementCollector collector;

            if (useActiveView)
            {
                var activeView = doc.ActiveView.Id;
                collector = new FilteredElementCollector(doc, activeView);
            }
            else
            {
                collector = new FilteredElementCollector(doc);
            }

            var rooms = collector
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>();

            foreach (Room room in rooms)
            {
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                Parameter coefficientParam = room.LookupParameter("КГ.Коэффициент площади");

                int roomTypeId = roomTypeParam?.AsInteger() ?? 0;
                double coefficient = coefficientParam?.HasValue == true ? coefficientParam.AsDouble() : 0;

                if (roomTypeParam != null && coefficientParam != null)
                {
                    if (roomTypeId > 0 && coefficient > 0)
                    {
                        string roomType = "Тип" + roomTypeId.ToString();
                        if (!coefficients.ContainsKey(roomType))
                        {
                            coefficients[roomType] = coefficient;
                        }
                    }
                }
            }

            return coefficients;
        }
    }
}
