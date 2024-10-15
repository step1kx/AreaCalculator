﻿//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.Windows;
//using System.Linq;
//using System.Data;
//using Autodesk.Revit.DB.Architecture;
//using System.Diagnostics;
//using System.Windows.Input;
//using System.Windows.Controls;

//namespace AreaCalc
//{
//    public partial class MainWindow : Window
//    {
//        private Autodesk.Revit.DB.Document _doc;
//        private Dictionary<string, List<Room>> apartmentsData;
//        private List<Room> apartments;
//        Parameter roomTypeParam;
//        int? roomType;
//        private Dictionary<string, double> roomTypeCoefficients;

//        public MainWindow(Autodesk.Revit.DB.Document doc)
//        {
//            InitializeComponent();
//            _doc = doc;
//            apartmentsData = new Dictionary<string, List<Room>>();
//            apartments = new List<Room>();
//            GetApartmentData();
//            PopulateApartmentComboBox();
//        }

//        private void MovingWin(object sender, EventArgs e)
//        {
//            if (Mouse.LeftButton == MouseButtonState.Pressed)
//            {
//                this.DragMove();
//            }
//        }

//        private void GetApartmentData()
//        {
//            var activeView = _doc.ActiveView.Id;
//            FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
//                .OfCategory(BuiltInCategory.OST_Rooms)
//                .WhereElementIsNotElementType();

//            foreach (Room room in apartmentCollector)
//            {
//                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
//                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
//                roomTypeParam = room.LookupParameter("КГ.Тип помещения");
//                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
//                // Отсеиваем помещения без номеров квартир (например, Лестничная клетка и Лифт)

//                //if (roomName == "Лестничная клетка" || roomName == "Лифт")
//                //{
//                //    MessageBox.Show($"Skipping room: {roomName}");
//                //    continue;
//                //}

//                // Проверяем наличие параметров
//                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
//                {
//                    MessageBox.Show("Один из параметров не найден!");
//                    continue;
//                }



//                string apartmentNumber = apartmentNumberParam.AsString();
//                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
//                roomType = roomTypeParam?.AsInteger();

//                if (roomType == null)
//                {
//                    MessageBox.Show($"Тип помещения: {roomType}");

//                }

//                // Проверяем номер квартиры и добавляем в словарь
//                if (!string.IsNullOrWhiteSpace(apartmentNumber))
//                {
//                    // Если ключа нет, создаем новую запись
//                    if (!apartmentsData.ContainsKey(apartmentNumber))
//                    {
//                        apartmentsData[apartmentNumber] = new List<Room>();
//                    }

//                    // Добавляем комнату к квартире
//                    apartmentsData[apartmentNumber].Add(room);
//                }
//                //else
//                //{
//                //    MessageBox.Show("Apartment number is empty or whitespace!");
//                //}
//            }

//        }





//        private double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
//        {
//            foreach (var entry in roomAreas)
//            {
//                // Замена типа помещения на его площадь
//                // Например, "Тип2" заменяется на "5.0"
//                string pattern = $@"\b{Regex.Escape(entry.Key)}\b"; // Обратите внимание на границы слова
//                formula = Regex.Replace(formula, pattern, entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
//            }

//            // Проверка на допустимые символы
//            if (!Regex.IsMatch(formula, @"^[0-9A-Za-zА-Яа-яё\.\+\-\*/\(\)\s]*$"))
//            {
//                throw new Exception("Формула содержит недопустимые символы.");
//            }

//            DataTable table = new DataTable();
//            try
//            {
//                var result = table.Compute(formula, null);
//                return Convert.ToDouble(result);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Ошибка вычисления формулы: {ex.Message}");
//            }
//        }

//        private void PopulateApartmentComboBox()
//        {
//            apartmentComboBox.Items.Clear();
//            foreach (var kvp in apartmentsData)
//            {
//                apartmentComboBox.Items.Add($"Квартира № {kvp.Key} (Помещений: {kvp.Value.Count})"); // Добавление номера квартиры и количества помещений
//            }

//        }

//        private void CalculateButton_Click(object sender, RoutedEventArgs e)
//        {
//            // Получаем формулы из текстовых полей
//            string livingFormula = livingFormulaTextBox.Text.Trim();
//            string usualFormula = usualFormulaTextBox.Text.Trim();
//            string totalFormula = totalFormulaTextBox.Text.Trim();

//            // Проверяем, что хотя бы одна формула введена
//            if (string.IsNullOrEmpty(livingFormula) && string.IsNullOrEmpty(usualFormula) && string.IsNullOrEmpty(totalFormula))
//            {
//                MessageBox.Show("Пожалуйста, введите хотя бы одну формулу.");
//                return;
//            }

//            // Начинаем транзакцию
//            using (Transaction tx = new Transaction(_doc, "Расчет площадей по формулам"))
//            {
//                tx.Start();
//                double totalViewArea = 0.0;

//                if (selectedApartmentRadioButton.IsChecked == true)
//                {
//                    // Получаем номер выбранной квартиры из ComboBox
//                    string selectedApartment = apartmentComboBox.SelectedItem?.ToString();
//                    if (string.IsNullOrEmpty(selectedApartment))
//                    {
//                        MessageBox.Show("Пожалуйста, выберите квартиру.");
//                        return;
//                    }

//                    // Извлекаем номер квартиры
//                    string apartmentNumber = selectedApartment.Split(' ')[2]; // Извлекаем номер квартиры

//                    if (apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
//                    {
//                        // Группируем площади по типам помещений
//                        Dictionary<string, double> roomAreas = rooms
//                            .Where(r =>
//                            {
//                                string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
//                                return roomName != "Лестничная клетка" && roomName != "Лифт"; // Отсеивание ненужных помещений
//                            })
//                            .GroupBy(r =>
//                            {
//                                int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
//                                return "Тип" + type; // Формируем ключ с добавлением "Тип"
//                            })
//                            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));

//                        MessageBox.Show("Содержимое roomAreas:\n" + string.Join("\n", roomAreas.Select(kv => $"Тип: {kv.Key}, Площадь: {kv.Value}")));
//                        // Рассчитываем площади по формулам
//                        double livingArea = 0;
//                        double usualArea = 0;
//                        double totalArea = 0;

//                        try
//                        {
//                            if (!string.IsNullOrEmpty(livingFormula))
//                            {
//                                livingArea = CalculateFormula(livingFormula, roomAreas);
//                            }
//                            if (!string.IsNullOrEmpty(usualFormula))
//                            {
//                                usualArea = CalculateFormula(usualFormula, roomAreas);
//                            }
//                            if (!string.IsNullOrEmpty(totalFormula))
//                            {
//                                totalArea = CalculateFormula(totalFormula, roomAreas);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            MessageBox.Show($"Ошибка в квартире {apartmentNumber}: {ex.Message}");
//                            return; // Завершаем метод при ошибке
//                        }

//                        // Записываем результаты в параметры Revit
//                        Room firstRoom = rooms.FirstOrDefault();
//                        if (firstRoom != null)
//                        {
//                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
//                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
//                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
//                        }

//                        totalViewArea = totalArea; // Обновляем общую площадь на общую площадь выбранной квартиры
//                    }
//                    else
//                    {
//                        MessageBox.Show("Квартира не найдена в данных.");
//                    }
//                }
//                else if (allApartmentsOnViewRadioButton.IsChecked == true)
//                {
//                    // Обработка всех квартир
//                    int processed = 0;

//                    foreach (var room in apartmentsData)
//                    {
//                        string apartmentNumber = room.Key;
//                        List<Room> rooms = room.Value;

//                        // Группируем площади по типам помещений
//                        Dictionary<string, double> roomAreas = rooms
//                            .Where(r =>
//                            {
//                                string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
//                                return roomName != "Лестничная клетка" && roomName != "Лифт"; // Отсеивание ненужных помещений
//                            })
//                            .GroupBy(r =>
//                            {
//                                int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
//                                return "Тип" + type; // Формируем ключ с добавлением "Тип"
//                            })
//                            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));

//                        // Рассчитываем площади по формулам
//                        double livingArea = 0;
//                        double usualArea = 0;
//                        double totalArea = 0;

//                        try
//                        {
//                            if (!string.IsNullOrEmpty(livingFormula))
//                            {
//                                livingArea = CalculateFormula(livingFormula, roomAreas);
//                            }
//                            if (!string.IsNullOrEmpty(usualFormula))
//                            {
//                                usualArea = CalculateFormula(usualFormula, roomAreas);
//                            }
//                            if (!string.IsNullOrEmpty(totalFormula))
//                            {
//                                totalArea = CalculateFormula(totalFormula, roomAreas);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            MessageBox.Show($"Ошибка в квартире {apartmentNumber} (все хорошо, там просто не посчитается площадь): {ex.Message}");
//                            continue; // Переходим к следующей квартире
//                        }

//                        // Записываем результаты в параметры Revit
//                        Room firstRoom = rooms.FirstOrDefault();
//                        if (firstRoom != null)
//                        {
//                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
//                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
//                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
//                        }

//                        totalViewArea += totalArea; // Добавляем к общей площади
//                        processed++;
//                    }

//                    MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
//                }
//                else if (allApartmentsOnObjectRadioButton.IsChecked == true)
//                {
//                    int processedRooms = 0;

//                    // Получаем все помещения в проекте
//                    var allRooms = new FilteredElementCollector(MainFunction_1.doc)
//                        .OfCategory(BuiltInCategory.OST_Rooms)
//                        .WhereElementIsNotElementType()
//                        .ToElements()
//                        .Cast<Room>()
//                        .Where(r =>
//                        {
//                            string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
//                            return roomName != "Лестничная клетка" && roomName != "Лифт"; // Отсеивание ненужных помещений
//                        })
//                        .ToList();

//                    // Группируем площади по типам помещений
//                    Dictionary<string, double> roomAreas = allRooms
//                        .GroupBy(r =>
//                        {
//                            int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
//                            return "Тип" + type; // Формируем ключ с добавлением "Тип"
//                        })
//                        .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));

//                    // Рассчитываем площади по формулам
//                    double livingArea = 0;
//                    double usualArea = 0;
//                    double totalArea = 0;

//                    try
//                    {
//                        if (!string.IsNullOrEmpty(livingFormula))
//                        {
//                            livingArea = CalculateFormula(livingFormula, roomAreas);
//                        }
//                        if (!string.IsNullOrEmpty(usualFormula))
//                        {
//                            usualArea = CalculateFormula(usualFormula, roomAreas);
//                        }
//                        if (!string.IsNullOrEmpty(totalFormula))
//                        {
//                            totalArea = CalculateFormula(totalFormula, roomAreas);
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        MessageBox.Show($"Ошибка при расчете площадей на объекте: {ex.Message}");
//                        return; // Завершение метода, если произошла ошибка
//                    }

//                    // Записываем результаты в параметры всех помещений
//                    foreach (var room in allRooms)
//                    {
//                        UpdateRoomParameter(room, "КГ.S.Помещения с коэфф.", livingArea);
//                        UpdateRoomParameter(room, "КГ.S.ЖП.Площадь квартиры", usualArea);
//                        UpdateRoomParameter(room, "КГ.S.ЖПЛк.Общая площадь", totalArea);
//                        processedRooms++;
//                    }

//                    MessageBox.Show($"Расчет завершен. Обработано помещений: {processedRooms}");
//                }

//                tx.Commit();

//            }


//        }



//        // Метод для обновления параметра в комнате
//        private void UpdateRoomParameter(Room room, string parameterName, double value)
//        {
//            Parameter param = room.LookupParameter(parameterName);
//            if (param != null && !param.IsReadOnly)
//            {
//                param.Set(value);
//            }
//        }

//        private void CancelButton_Click(object sender, RoutedEventArgs e)
//        {
//            DialogResult = false;
//            Close();
//        }


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB.Architecture;
using System.Windows.Input;
using System.Diagnostics;

namespace AreaCalc
{
    public partial class MainWindow : Window
    {
        private Autodesk.Revit.DB.Document _doc;
        private Dictionary<string, List<Room>> apartmentsData;
        private Dictionary<string, double> roomCoefficients; // Словарь с коэффициентами
        Parameter roomTypeParam;

        public MainWindow(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            _doc = doc;
            apartmentsData = new Dictionary<string, List<Room>>();
            roomCoefficients = new Dictionary<string, double>();
            GetApartmentData();
            PopulateApartmentComboBox();
            GetRoomCoefficients(); // Получаем коэффициенты
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
                roomTypeParam = room.LookupParameter("КГ.Тип помещений");
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                //Отсеиваем помещения без номеров квартир(например, Лестничная клетка и Лифт)

                //if (roomName == "Лестничная клетка" || roomName == "Лифт")
                //{
                //    MessageBox.Show($"Skipping room: {roomName}");
                //    continue;
                //}

                //if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null)
                //{
                //    MessageBox.Show("Один из параметров не найден!");
                //    continue;
                //}

                string apartmentNumber = apartmentNumberParam.AsString();
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
                int? roomType = roomTypeParam?.AsInteger();

                if (!string.IsNullOrWhiteSpace(apartmentNumber))
                {
                    if (!apartmentsData.ContainsKey(apartmentNumber))
                    {
                        apartmentsData[apartmentNumber] = new List<Room>();
                    }
                    apartmentsData[apartmentNumber].Add(room);
                }
            }
        }

        private void MovingWin(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


        private void GetRoomCoefficients()
        {
            // Очистка предыдущих коэффициентов, чтобы избежать дублирования
            roomCoefficients.Clear();

            var activeView = _doc.ActiveView.Id;
            var roomAll = new FilteredElementCollector(_doc, activeView)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Room>();

            foreach (var room in roomAll)
            {
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                Parameter coefficientParam = room.LookupParameter("КГ.Коэффициент площади");

                int roomTypeId = roomTypeParam.AsInteger();
                double coefficient = coefficientParam.HasValue ? coefficientParam.AsDouble() : 0;

                // Проверка на наличие параметров
                if (roomTypeParam != null && coefficientParam != null)
                {
                    

                    // Условие для добавления типа помещения и коэффициента
                    if (roomTypeId > 0 && coefficient > 0) // Пропускаем типы с 0 и отсутствующими коэффициентами
                    {
                        string roomType = "Тип" + roomTypeId.ToString();

                        // Если коэффициент не был добавлен, добавляем его
                        if (!roomCoefficients.ContainsKey(roomType))
                        {
                            roomCoefficients[roomType] = coefficient;
                            MessageBox.Show($"Добавлен коэффициент: {roomType} = {coefficient}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Недопустимые значения для комнаты '{room.Name}'. Тип: Тип{roomTypeId}, Коэффициент: {coefficient}");
                    }
                }
                else
                {
                    // Вывод сообщения для отладки, если параметры отсутствуют
                    Debug.WriteLine($"Для комнаты '{room.Name}' отсутствуют необходимые параметры.");
                }
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

        private double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
        {
            foreach (var entry in roomAreas)
            {
                string pattern = $@"\b{Regex.Escape(entry.Key)}\b";
                formula = Regex.Replace(formula, pattern, entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
            }

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

        

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            string livingFormula = livingFormulaTextBox.Text.Trim();
            string usualFormula = usualFormulaTextBox.Text.Trim();
            string totalFormula = totalFormulaTextBox.Text.Trim();

            if (string.IsNullOrEmpty(livingFormula) && string.IsNullOrEmpty(usualFormula) && string.IsNullOrEmpty(totalFormula))
            {
                MessageBox.Show("Пожалуйста, введите хотя бы одну формулу.");
                return;
            }

            using (Transaction tx = new Transaction(_doc, "Расчет площадей по формулам"))
            {
                tx.Start();
                double totalViewArea = 0.0;

                if (selectedApartmentRadioButton.IsChecked == true)
                {
                    string selectedApartment = apartmentComboBox.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(selectedApartment))
                    {
                        MessageBox.Show("Пожалуйста, выберите квартиру.");
                        return;
                    }

                    string apartmentNumber = selectedApartment.Split(' ')[2];

                    if (apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
                    {
                        Dictionary<string, double> roomAreas = rooms
                            .GroupBy(r =>
                            {
                                int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
                                return "Тип" + type;
                            })
                            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));

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
                            return;
                        }

                        Room firstRoom = rooms.FirstOrDefault();
                        if (firstRoom != null)
                        {
                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
                        }

                        totalViewArea = totalArea;
                    }
                    else
                    {
                        MessageBox.Show("Квартира не найдена в данных.");
                    }
                }
                else if (allApartmentsOnViewRadioButton.IsChecked == true)
                {
                    int processed = 0;

                    foreach (var room in apartmentsData)
                    {
                        string apartmentNumber = room.Key;
                        List<Room> rooms = room.Value;

                        Dictionary<string, double> roomAreas = rooms
                            .GroupBy(r =>
                            {
                                int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
                                return "Тип" + type;
                            })
                            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));

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
                            continue;
                        }

                        Room firstRoom = rooms.FirstOrDefault();
                        if (firstRoom != null)
                        {
                            UpdateRoomParameter(firstRoom, "КГ.S.Помещения с коэфф.", livingArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖП.Площадь квартиры", usualArea);
                            UpdateRoomParameter(firstRoom, "КГ.S.ЖПЛк.Общая площадь", totalArea);
                        }

                        totalViewArea += totalArea;
                        processed++;
                    }

                    MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
                }

                tx.Commit();
            }
        }

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

        private void selectedApartmentRadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ShowHintButton_Click(object sender, RoutedEventArgs args)
        {

        }
    }
}



