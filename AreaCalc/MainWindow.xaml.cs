using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB.Architecture;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Controls;

namespace AreaCalc
{
    public partial class MainWindow : Window
    {
        private Autodesk.Revit.DB.Document _doc;
        public Dictionary<string, List<Room>> apartmentsData { get; private set; }
        public Dictionary<string, double> roomCoefficients { get; private set; } // Словарь с коэффициентами
        Parameter roomTypeParam;
        

        public MainWindow(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            _doc = doc;
            apartmentsData = new Dictionary<string, List<Room>>();
            roomCoefficients = new Dictionary<string, double>();
            LoadSettings();
        }


        private void LoadSettings()
        {
            SetPlaceholder(livingFormulaTextBox, Properties.Settings.Default.LivingFormula, "Пример: Тип11*0.85");
            SetPlaceholder(usualFormulaTextBox, Properties.Settings.Default.UsualFormula, "Пример: Тип11*0.85 + Тип2");
            SetPlaceholder(totalFormulaTextBox, Properties.Settings.Default.TotalFormula, "Пример: Тип11*0.85 + Тип2 + Тип3*0.3");
        }

        // Метод для установки подсказки
        private void SetPlaceholder(TextBox textBox, string setting, string placeholder)
        {
            if (string.IsNullOrEmpty(setting))
            {
                textBox.Text = placeholder;
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
            else
            {
                textBox.Text = setting;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }

            textBox.GotFocus += RemovePlaceholder;
            textBox.LostFocus += ApplyPlaceholder;
        }

        // Убираем подсказку при фокусе
        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Foreground == System.Windows.Media.Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        // Устанавливаем подсказку, если поле пустое при потере фокуса
        private void ApplyPlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                // Проверяем, какой именно TextBox вызвал событие и задаем соответствующую подсказку
                if (textBox.Name == "livingFormulaTextBox")
                {
                    textBox.Text = "Пример: Тип11*0.85";
                }
                else if (textBox.Name == "usualFormulaTextBox")
                {
                    textBox.Text = "Пример: Тип11*0.85 + Тип2";
                }
                else if (textBox.Name == "totalFormulaTextBox")
                {
                    textBox.Text = "Пример: Тип11*0.85 + Тип2 + Тип3*0.3";
                }

                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.LivingFormula = (livingFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : livingFormulaTextBox.Text;
            Properties.Settings.Default.UsualFormula = (usualFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : usualFormulaTextBox.Text;
            Properties.Settings.Default.TotalFormula = (totalFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : totalFormulaTextBox.Text;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }


        private void GetApartmentData()
        {
            apartmentsData.Clear();

            var activeView = _doc.ActiveView.Id;
            FilteredElementCollector apartmentCollector = new FilteredElementCollector(_doc, activeView)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            foreach (Room room in apartmentCollector)
            {
                Parameter apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                Parameter areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomFilterParam = room.LookupParameter("КГ.Фильтр");
                roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                //Отсеиваем помещения без номеров квартир(например, Лестничная клетка и Лифт)

                if (roomName == "Лестничная клетка" || roomName == "Лифт" )
                {
                    continue;
                }
                string roomFilter = roomFilterParam?.AsString();
                string apartmentNumber = apartmentNumberParam.AsString();
                double area = areaParam.HasValue ? areaParam.AsDouble() : 0;
                int? roomType = roomTypeParam?.AsInteger();

                if (apartmentNumberParam == null || areaParam == null || roomTypeParam == null || roomFilterParam == null)
                {
                    // Создаем список отсутствующих параметров
                    List<string> missingParams = new List<string>();

                    if (apartmentNumberParam == null)
                    {
                        missingParams.Add("КГ.Номер квартиры");
                    }

                    if (areaParam == null)
                    {
                        missingParams.Add("Площадь помещения");
                    }

                    if (roomTypeParam == null)
                    {
                        missingParams.Add("КГ.Тип помещения");
                    }

                    if(roomFilterParam == null)
                    {
                        missingParams.Add(roomFilter);
                    }
                    

                    // Преобразуем список в строку
                    string missingParamsMessage = string.Join(", ", missingParams);

                    // Показываем сообщение с отсутствующими параметрами и номером квартиры
                    MessageBox.Show($"Для квартиры с номером {apartmentNumber} не найдены следующие параметры: {missingParamsMessage}");

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
        }

        private void InitializeApartmentsData()
        {
            apartmentsData.Clear();

            var allRooms = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>();

            foreach (var room in allRooms)
            {
                var apartmentNumberParam = room.LookupParameter("КГ.Номер квартиры");
                if (apartmentNumberParam == null) continue;

                string apartmentNumber = apartmentNumberParam.AsString();
                if (string.IsNullOrEmpty(apartmentNumber)) continue;

                if (!apartmentsData.ContainsKey(apartmentNumber))
                {
                    apartmentsData[apartmentNumber] = new List<Room>();
                }

                apartmentsData[apartmentNumber].Add(room);
            }
        }

        private void GetRoomCoefficients()
        {
            // Очистка предыдущих коэффициентов, чтобы избежать дублирования
            roomCoefficients.Clear();

            var activeView = _doc.ActiveView.Id;
            var roomAll = new FilteredElementCollector(_doc, activeView)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            foreach (Room room in roomAll)
            {
                roomTypeParam = room.LookupParameter("КГ.Тип помещения");
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

        #region// Обработчики для радиокнопок
        private void SelectedApartmentRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            GetApartmentData();
            GetRoomCoefficients();
            PopulateApartmentComboBox();
        }

        private void AllApartmentsOnViewRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            GetApartmentData();
            GetRoomCoefficients();
        }

        private void AllApartmentsOnObjectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            InitializeApartmentsData();
            GetRoomCoefficients();
        }
        #endregion
       
       

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
            // Заменяем все ключи из словаря на их значения
            foreach (var entry in roomAreas)
            {
                string pattern = $@"\b{Regex.Escape(entry.Key)}\b";
                formula = Regex.Replace(formula, pattern, entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
            }

            // Заменяем все оставшиеся ключи (которые не были найдены в словаре) на 0
            formula = Regex.Replace(formula, @"\bТип\d+\b", "0");

            // Проверяем формулу на наличие недопустимых символов
            if (!Regex.IsMatch(formula, @"^[0-9A-Za-zА-Яа-яё\.\+\-\*/\(\)\s]*$"))
            {
                throw new Exception("Формула содержит недопустимые символы.");
            }

            // Вычисляем результат
            DataTable table = new DataTable();
            var result = table.Compute(formula, null);
            return Convert.ToDouble(result);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            string livingFormula = livingFormulaTextBox.Text;
            string usualFormula = usualFormulaTextBox.Text;
            string totalFormula = totalFormulaTextBox.Text;

            if (!AreFormulasValid(livingFormula, usualFormula, totalFormula))
                return;

            using (Transaction tx = new Transaction(_doc, "Расчет площадей по формулам"))
            {
                tx.Start();
                double totalViewArea = 0.0;

                if (selectedApartmentRadioButton.IsChecked == true)
                {
                    ProcessSelectedApartment(livingFormula, usualFormula, totalFormula, ref totalViewArea);
                }
                else if (allApartmentsOnViewRadioButton.IsChecked == true)
                {
                    ProcessAllApartmentsOnView(livingFormula, usualFormula, totalFormula, ref totalViewArea);
                }
                else if (allApartmentsOnObjectRadioButton.IsChecked == true)
                {
                    ProcessAllApartmentsInObject(livingFormula, usualFormula, totalFormula, ref totalViewArea);
                }

                
                tx.Commit();
            }
        }

        private bool AreFormulasValid(string livingFormula, string usualFormula, string totalFormula)
        {
            if (string.IsNullOrEmpty(livingFormula) && string.IsNullOrEmpty(usualFormula) && string.IsNullOrEmpty(totalFormula))
            {
                MessageBox.Show("Пожалуйста, введите хотя бы одну формулу.");
                return false;
            }
            return true;
        }

        private void ProcessSelectedApartment(string livingFormula, string usualFormula, string totalFormula, ref double totalViewArea)
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
                var roomAreas = CalculateRoomAreas(rooms);
                CalculateAndUpdateRoomParameters(rooms, roomAreas, livingFormula, usualFormula, totalFormula, ref totalViewArea);
            }
            else
            {
                MessageBox.Show("Квартира не найдена в данных.");
            }
        }

        private void ProcessAllApartmentsOnView(string livingFormula, string usualFormula, string totalFormula, ref double totalViewArea)
        { 
            int processed = 0;

            foreach (var room in apartmentsData)
            {
                string apartmentNumber = room.Key;
                List<Room> rooms = room.Value;

                var roomAreas = CalculateRoomAreas(rooms);
                double area = 0;
                CalculateAndUpdateRoomParameters(rooms, roomAreas, livingFormula, usualFormula, totalFormula, ref area);

                totalViewArea += area;
                processed++;
            }

            MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
        }

        private void ProcessAllApartmentsInObject(string livingFormula, string usualFormula, string totalFormula, ref double totalViewArea)
        {
            int processed = 0;
            InitializeApartmentsData();

            foreach (var roomData in apartmentsData)
            {
                List<Room> rooms = roomData.Value;
                var roomAreas = CalculateRoomAreas(rooms);
                double area = 0;
                CalculateAndUpdateRoomParameters(rooms, roomAreas, livingFormula, usualFormula, totalFormula, ref area);

                totalViewArea += area;
                processed++;
            }

            MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
        }

        private Dictionary<string, double> CalculateRoomAreas(List<Room> rooms)
        {
            return rooms
                .GroupBy(r =>
                {
                    int? type = r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0;
                    return "Тип" + type;
                })
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()), 3));
        }

        private void CalculateAndUpdateRoomParameters(List<Room> rooms, Dictionary<string, double> roomAreas, string livingFormula, string usualFormula, string totalFormula, ref double totalArea)
        {
            double livingArea = 0;
            double usualArea = 0;
            double calculatedTotalArea = 0;

            try
            {
                if (!string.IsNullOrEmpty(livingFormula))
                    livingArea = CalculateFormula(livingFormula, roomAreas);

                if (!string.IsNullOrEmpty(usualFormula))
                    usualArea = CalculateFormula(usualFormula, roomAreas);

                if (!string.IsNullOrEmpty(totalFormula))
                    calculatedTotalArea = CalculateFormula(totalFormula, roomAreas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в расчете: {ex.Message}");
                return;
            }

            foreach (Room room in rooms)
            {
                UpdateRoomParameter(room, "КГ.S.Ж", livingArea);
                UpdateRoomParameter(room, "КГ.S.ЖП.Площадь квартиры", usualArea);
                UpdateRoomParameter(room, "КГ.S.ЖПЛк.Общая площадь", calculatedTotalArea);
            }

            totalArea = calculatedTotalArea;
        }
        #region Вспомогательные методы
        private void UpdateRoomParameter(Room room, string parameterName, double value)
        {          
                Parameter param = room.LookupParameter(parameterName);
                if (param != null && !param.IsReadOnly)
                {
                    param.Set(value);
                }   
        }

        private void MovingWin(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowHintButton_Click(object sender, RoutedEventArgs args)
        {
            InitializeApartmentsData();
            var tipWindow = new TipWindow(apartmentsData);
            tipWindow.ShowDialog();
        }

        private void ShowRoomDataInputWindow_Click(object sender, RoutedEventArgs args)
        { 
            InitializeApartmentsData();
            //var roomWindow = new RoomDataInputWindow(apartmentsData);
            //roomWindow.ShowDialog();
        }
        #endregion
    }
}



