//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.Windows;
//using System.Diagnostics;
//using Autodesk.Revit.DB.Architecture;
//using System.Linq;
//using System.Collections.ObjectModel;

//namespace AreaCalc
//{
//    public partial class MainWindow : Window
//    {
//        private Autodesk.Revit.DB.Document _doc;
//        private Dictionary<string, List<Room>> apartmentsData;
//        private List<Room> apartments;
//        private Dictionary<string, string> typeFromFormula;

//        public MainWindow(Autodesk.Revit.DB.Document doc)
//        {
//            InitializeComponent();
//            _doc = doc;
//            apartmentsData = new Dictionary<string, List<Room>>();
//            apartments = new List<Room>();
//            typeFromFormula = new Dictionary<string, string>();
//            GetApartmentData();
//        }

//        private void GetApartmentData()
//        {
//            var activeView = _doc.ActiveView.Id;
//            FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
//                .OfCategory(BuiltInCategory.OST_Rooms)
//                .WhereElementIsNotElementType();





//        }

//        private double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
//        {
//            foreach (var entry in roomAreas)
//            {
//                formula = formula.Replace(entry.Key, entry.Value.ToString());
//            }

//            if (!Regex.IsMatch(formula, @"^[0-9a-zA-Z\.\+\-\*/\(\)\s]+$"))
//            {
//                throw new Exception("Формула содержит недопустимые символы.");
//            }

//            System.Data.DataTable table = new System.Data.DataTable();
//            return Convert.ToDouble(table.Compute(formula, null));
//        }

//        private void CalculateButton_Click(object sender, EventArgs e)
//        {

//        }

//        private void CancelButton_Click(object sender, RoutedEventArgs e)
//        {
//            DialogResult = false;
//            Close();
//        }
//    }
//}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB.Architecture;

namespace AreaCalc
{
    public partial class MainWindow : Window
    {
        private Autodesk.Revit.DB.Document _doc;
        private Dictionary<string, List<Room>> apartmentsData;
        private List<Room> apartments;

        public MainWindow(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            _doc = doc;
            apartmentsData = new Dictionary<string, List<Room>>();
            apartments = new List<Room>();
            GetApartmentData();
            PopulateApartmentComboBox();
        }

        //private void GetApartmentData()
        //{
        //    var activeView = _doc.ActiveView.Id;
        //    FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
        //        .OfCategory(BuiltInCategory.OST_Rooms)
        //        .WhereElementIsNotElementType();

        //    foreach (Room room in apartmentCollector)
        //    {
        //        Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
        //        Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
        //        Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

        //        if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
        //        {
        //            MessageBox.Show("Один из параметров не найден!");


        //        }

        //        string apartmentNumber = apartmentNumberParam?.AsString() ?? "Не найден";
        //        double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
        //        string roomType = roomTypeParam?.AsString() ?? "Не найден";


        //        if (apartmentNumber != null)
        //        {
        //            apartments.Add(room);
        //        }

        //        //apartmentsData[apartmentNumber].Add(room);
        //    }

        //    foreach (Room room in apartments)
        //    {
        //        Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
        //        Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
        //        Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

        //        if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
        //        {
        //            MessageBox.Show("Один из параметров не найден!");

        //            continue;
        //        }

        //        string apartmentNumber = apartmentNumberParam.AsString();
        //        double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
        //        string roomType = roomTypeParam?.AsString() ?? "Не найден";
        //        try
        //        {
        //            if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
        //            {
        //                apartmentsData[apartmentNumber] = new List<Room>();

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("gdfdgfdg");
        //        }

        //        apartmentsData[apartmentNumber].Add(room);
        //    }

        //}

        //private void GetApartmentData()
        //{
        //    var activeView = _doc.ActiveView.Id;
        //    FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
        //        .OfCategory(BuiltInCategory.OST_Rooms)
        //        .WhereElementIsNotElementType();

        //    foreach (Room room in apartmentCollector)
        //    {
        //        Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
        //        Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
        //        Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

        //        if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
        //        {
        //            MessageBox.Show("Один из параметров не найден!");

        //            continue;
        //        }

        //        string apartmentNumber = apartmentNumberParam.AsString();
        //        double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
        //        string roomType = roomTypeParam.AsString();
        //        //if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
        //        //{
        //        //    apartmentsData[apartmentNumber] = new List<Room>();

        //        //}
        //        if (apartmentNumber != null)
        //        {
        //            apartments.Add(room);
        //        }

        //        //apartmentsData[apartmentNumber].Add(room);
        //    }

        //    foreach (Room room in apartments)
        //    {
        //        Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
        //        Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
        //        Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

        //        if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
        //        {
        //            MessageBox.Show("Один из параметров не найден!");

        //            continue;
        //        }

        //        string apartmentNumber = apartmentNumberParam.AsString();
        //        double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
        //        string roomType = roomTypeParam.AsString();
        //        try
        //        {
        //            if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
        //            {
        //                apartmentsData[apartmentNumber] = new List<Room>();

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }

        //        apartmentsData[apartmentNumber].Add(room);
        //    }

        //}

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

                // Логирование параметров для отладки
                string roomType = roomTypeParam?.AsString();

                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
                {
                    MessageBox.Show("Один из параметров не найден!");
                    continue;
                }

                string apartmentNumber = apartmentNumberParam.AsString() ?? "Не найден";
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;

                if (apartmentNumber != null)
                {
                    apartments.Add(room);
                }
            }

            foreach (Room room in apartments)
            {
                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");

                // Проверяем и логируем
                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
                {
                    MessageBox.Show("Один из параметров не найден!");
                    continue;
                }

                string apartmentNumber = apartmentNumberParam.AsString();
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
                string roomType = roomTypeParam?.AsString();

                
                if (!apartmentsData.ContainsKey(apartmentNumber) && apartmentNumber != null)
                {
                    apartmentsData[apartmentNumber] = new List<Room>();
                }

                apartmentsData[apartmentNumber].Add(room);
            }
        }





        private double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
        {
            foreach (var entry in roomAreas)
            {
                // Замена типа помещения на его площадь
                // Например, "Тип2" заменяется на "5.0"
                string pattern = $@"\b{Regex.Escape(entry.Key)}\b"; // Обратите внимание на границы слова
                formula = Regex.Replace(formula, pattern, entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
            }

            // Проверка на допустимые символы
            if (!Regex.IsMatch(formula, @"^[0-9A-Za-zА-Яа-яё\.\+\-\*/\(\)\s]*$"))
            {
                throw new Exception("Формула содержит недопустимые символы.");
            }

            DataTable table = new DataTable();
            try
            {
                var result = table.Compute(formula, null);
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка вычисления формулы: {ex.Message}");
            }
        }

        private void PopulateApartmentComboBox()
        {
            apartmentComboBox.Items.Clear();
            foreach (var kvp in apartmentsData)
            {
                apartmentComboBox.Items.Add($"Квартира № {kvp.Key} (Помещений: {kvp.Value.Count})"); // Добавление номера квартиры и количества помещений
            }

        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем формулы из текстовых полей
            string livingFormula = livingFormulaTextBox.Text.Trim();
            string usualFormula = usualFormulaTextBox.Text.Trim();
            string totalFormula = totalFormulaTextBox.Text.Trim();

            // Проверяем, что хотя бы одна формула введена
            if (string.IsNullOrEmpty(livingFormula) && string.IsNullOrEmpty(usualFormula) && string.IsNullOrEmpty(totalFormula))
            {
                MessageBox.Show("Пожалуйста, введите хотя бы одну формулу.");
                return;
            }

            // Начинаем транзакцию
            using (Transaction tx = new Transaction(_doc, "Расчет площадей по формулам"))
            {
                tx.Start();
                double totalViewArea = 0.0;

                if (selectedApartmentRadioButton.IsChecked == true)
                {
                    // Получаем номер выбранной квартиры из ComboBox
                    string selectedApartment = apartmentComboBox.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(selectedApartment))
                    {
                        MessageBox.Show("Пожалуйста, выберите квартиру.");
                        return;
                    }

                    // Извлекаем номер квартиры
                    string apartmentNumber = selectedApartment.Split(' ')[2]; // Извлекаем номер квартиры

                    if (apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
                    {
                        // Группируем площади по типам помещений
                        Dictionary<string, double> roomAreas = rooms
                            .GroupBy(r =>
                            {
                                string type = r.LookupParameter("КГ.Тип помещения")?.AsString() ?? "Неизвестный тип";
                                return "Тип" + type; // Формируем ключ с добавлением "Тип"
                            })
                            .ToDictionary(g => g.Key, g => g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()));

                        MessageBox.Show("Содержимое roomAreas:\n" + string.Join("\n", roomAreas.Select(kv => $"Тип: {kv.Key}, Площадь: {kv.Value}")));
                        // Рассчитываем площади по формулам
                        double livingArea = 0;
                        double usualArea = 0;
                        double totalArea = 0;

                        try
                        {
                            if (!string.IsNullOrEmpty(livingFormula))
                            {
                                livingArea = CalculateFormula(livingFormula, roomAreas);
                            }
                            if (!string.IsNullOrEmpty(usualFormula))
                            {
                                usualArea = CalculateFormula(usualFormula, roomAreas);
                            }
                            if (!string.IsNullOrEmpty(totalFormula))
                            {
                                totalArea = CalculateFormula(totalFormula, roomAreas);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка в квартире {apartmentNumber}: {ex.Message}");
                            return; // Завершаем метод при ошибке
                        }

                        // Записываем результаты в параметры Revit
                        Room firstRoom = rooms.FirstOrDefault();
                        if (firstRoom != null)
                        {
                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
                        }

                        totalViewArea = totalArea; // Обновляем общую площадь на общую площадь выбранной квартиры
                    }
                    else
                    {
                        MessageBox.Show("Квартира не найдена в данных.");
                    }
                }
                else if (allApartmentsRadioButton.IsChecked == true)
                {
                    // Обработка всех квартир
                    int processed = 0;

                    foreach (var room in apartmentsData)
                    {
                        string apartmentNumber = room.Key;
                        List<Room> rooms = room.Value;

                        // Группируем площади по типам помещений
                        Dictionary<string, double> roomAreas = rooms
                            .GroupBy(r => r.LookupParameter("КГ.Тип помещения")?.AsString() ?? "Неизвестный тип")
                            .ToDictionary(g => g.Key, g => g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()));

                        // Рассчитываем площади по формулам
                        double livingArea = 0;
                        double usualArea = 0;
                        double totalArea = 0;

                        try
                        {
                            if (!string.IsNullOrEmpty(livingFormula))
                            {
                                livingArea = CalculateFormula(livingFormula, roomAreas);
                            }
                            if (!string.IsNullOrEmpty(usualFormula))
                            {
                                usualArea = CalculateFormula(usualFormula, roomAreas);
                            }
                            if (!string.IsNullOrEmpty(totalFormula))
                            {
                                totalArea = CalculateFormula(totalFormula, roomAreas);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка в квартире {apartmentNumber}: {ex.Message}");
                            continue; // Переходим к следующей квартире
                        }

                        // Записываем результаты в параметры Revit
                        Room firstRoom = rooms.FirstOrDefault();
                        if (firstRoom != null)
                        {
                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
                        }

                        totalViewArea += totalArea; // Добавляем к общей площади
                        processed++;
                    }

                    MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
                }

                tx.Commit();
                totalViewAreaTextBlock.Text = $"Общая площадь: {UnitUtils.ConvertFromInternalUnits(totalViewArea, DisplayUnitType.DUT_SQUARE_METERS):F2} м²";
            }

            // Закрываем окно после завершения расчета
            this.Close();
        }

        

        // Метод для обновления параметра в комнате
        private void UpdateRoomParameter(Room room, string parameterName, double value)
        {
            Parameter param = room.LookupParameter(parameterName);
            if (param != null && !param.IsReadOnly)
            {
                param.Set(value);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}





