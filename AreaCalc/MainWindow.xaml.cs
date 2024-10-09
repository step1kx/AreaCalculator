using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Diagnostics;
using Autodesk.Revit.DB.Architecture;
using System.Linq;
using System.Collections.ObjectModel;

namespace AreaCalc
{
    public partial class MainWindow : Window
    {
        private Autodesk.Revit.DB.Document _doc;
        private Dictionary<string, List<Room>> apartmentsData;
        private List<Room> apartments;
        private Dictionary<string, string> typeFromFormula;

        public MainWindow(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            _doc = doc;
            apartmentsData = new Dictionary<string, List<Room>>();
            apartments = new List<Room>();
            typeFromFormula = new Dictionary<string, string>();
            GetApartmentData();
        }

        private void GetApartmentData()
        {
            var activeView = _doc.ActiveView.Id;
            FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();


            foreach (Room room in apartmentCollector)
            {
                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
                {
                    MessageBox.Show("Один из параметров не найден!");

                    continue;
                }

                string apartmentNumber = apartmentNumberParam.AsString();
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
                string roomType = roomTypeParam.AsString();
                //if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
                //{
                //    apartmentsData[apartmentNumber] = new List<Room>();

                //}
                if (apartmentNumber != null)
                {
                    apartments.Add(room);
                }



                //apartmentsData[apartmentNumber].Add(room);
            }

            foreach (Room room in apartments)
            {
                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
                {
                    MessageBox.Show("Один из параметров не найден!");

                    continue;
                }

                string apartmentNumber = apartmentNumberParam.AsString();
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
                string roomType = roomTypeParam.AsString();
                try
                {
                    if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
                    {
                        apartmentsData[apartmentNumber] = new List<Room>();

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }



                apartmentsData[apartmentNumber].Add(room);

            }

            apartmentComboBox.Items.Clear(); // Очистка предыдущих значений
            foreach (var kvp in apartmentsData)
            {
                apartmentComboBox.Items.Add(kvp.Key + "             " + kvp.Value.Count()); // Добавление номера квартиры
            }
        }

        private double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
        {
            foreach (var entry in roomAreas)
            {
                formula = formula.Replace(entry.Key, entry.Value.ToString());
            }

            if (!Regex.IsMatch(formula, @"^[0-9a-zA-Z\.\+\-\*/\(\)\s]+$"))
            {
                throw new Exception("Формула содержит недопустимые символы.");
            }

            System.Data.DataTable table = new System.Data.DataTable();
            return Convert.ToDouble(table.Compute(formula, null));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}




