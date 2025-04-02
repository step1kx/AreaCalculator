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
using System.Threading.Tasks;
using Grid = System.Windows.Controls.Grid;
using AreaCalc.Interfaces;
using AreaCalc.Models;

namespace AreaCalc
{
    public partial class MainWindow : Window
    {
        private readonly Document _doc;
        private readonly IApartmentDataProvider _dataProvider;
        private readonly IAreaCalculator _areaCalculator;
        private readonly IParameterUpdater _parameterUpdater;
        private readonly ISettingsManager _settingsManager;
        private readonly LivingRoomManipulations _livingRoomManipulations;

        private Dictionary<string, List<Room>> _apartmentsData;
        private Dictionary<string, double> _roomCoefficients;
        private readonly IApartmentLayout _apartmentLayout;
        private readonly IApartmentUpdater _apartmentUpdater;

        public MainWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            _apartmentsData = new Dictionary<string, List<Room>>();
            _roomCoefficients = new Dictionary<string, double>();

            _dataProvider = new ApartmentDataProvider();
            _areaCalculator = new AreaCalculator();
            _parameterUpdater = new ParameterUpdater();
            _settingsManager = new SettingsManager();
            _livingRoomManipulations = new LivingRoomManipulations(_doc, _apartmentsData, _dataProvider);
            _apartmentLayout = new ApartmentLayout(new DraftingServices());
            _apartmentUpdater = new ApartmentUpdater(new DraftingServices());

            _settingsManager.LoadSettings(livingFormulaTextBox, usualFormulaTextBox, totalFormulaTextBox);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _settingsManager.SaveSettings(livingFormulaTextBox, usualFormulaTextBox, totalFormulaTextBox);
        }

        private void SelectedApartmentRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var (apartmentsData, errors) = _dataProvider.GetApartmentData(_doc, true);
            //_apartmentsData.Clear();
            foreach (var kvp in apartmentsData)
            {
                _apartmentsData[kvp.Key] = kvp.Value;
            }
            _roomCoefficients = _dataProvider.GetRoomCoefficients(_doc, true);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors));
            }

            PopulateApartmentComboBox();
        }

        private void AllApartmentsOnViewRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var (apartmentsData, errors) = _dataProvider.GetApartmentData(_doc, true);
            _apartmentsData.Clear();
            foreach (var kvp in apartmentsData)
            {
                _apartmentsData[kvp.Key] = kvp.Value;
            }
            _roomCoefficients = _dataProvider.GetRoomCoefficients(_doc, true);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors));
            }
        }

        private void AllApartmentsOnObjectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var (apartmentsData, errors) = _dataProvider.GetApartmentData(_doc, false);
            _apartmentsData.Clear();
            foreach (var kvp in apartmentsData)
            {
                _apartmentsData[kvp.Key] = kvp.Value;
            }
            _roomCoefficients = _dataProvider.GetRoomCoefficients(_doc, false);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors));
            }
        }

        private void PopulateApartmentComboBox()
        {
            apartmentComboBox.Items.Clear();
            foreach (var kvp in _apartmentsData)
            {
                apartmentComboBox.Items.Add($"Квартира № {kvp.Key} (Помещений: {kvp.Value.Count})");
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            string livingFormula = livingFormulaTextBox.Text;
            string usualFormula = usualFormulaTextBox.Text;
            string totalFormula = totalFormulaTextBox.Text;

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
                    if (_apartmentsData.TryGetValue(apartmentNumber, out List<Autodesk.Revit.DB.Architecture.Room> rooms))
                    {
                        ProcessApartment(rooms, livingFormula, usualFormula, totalFormula, ref totalViewArea);
                    }
                    else
                    {
                        MessageBox.Show("Квартира не найдена в данных.");
                    }
                }
                else if (allApartmentsOnViewRadioButton.IsChecked == true || allApartmentsOnObjectRadioButton.IsChecked == true)
                {
                    int processed = 0;
                    foreach (var apartment in _apartmentsData)
                    {
                        double area = 0;
                        ProcessApartment(apartment.Value, livingFormula, usualFormula, totalFormula, ref area);
                        totalViewArea += area;
                        processed++;
                    }
                    MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
                }
                else
                {
                    MessageBox.Show("Нужен выбор режима расчета!");
                    return;
                }

                tx.Commit();
            }
        }

        private void ProcessApartment(List<Autodesk.Revit.DB.Architecture.Room> rooms, string livingFormula, string usualFormula, string totalFormula, ref double totalArea)
        {
            try
            {
                var (livingArea, usualArea, calculatedTotalArea) = _areaCalculator.CalculateAreas(rooms, livingFormula, usualFormula, totalFormula);

                foreach (var room in rooms)
                {
                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.Ж", livingArea);
                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.ЖП.Площадь квартиры", usualArea);
                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.ЖПЛк.Общая площадь", calculatedTotalArea);
                }

                totalArea = calculatedTotalArea;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в расчете: {ex.Message}");
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
            var (apartmentsData, errors) = _dataProvider.GetApartmentData(_doc, false);
            _apartmentsData.Clear();
            foreach (var kvp in apartmentsData)
            {
                _apartmentsData[kvp.Key] = kvp.Value;
            }

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors));
            }

            var tipWindow = new TipWindow(_apartmentsData);
            tipWindow.ShowDialog();
        }

        private void LivingRoomParameterCalculate_Click(object sender, RoutedEventArgs e)
        {
            string mode = selectedApartmentRadioButton.IsChecked == true ? "SelectedApartment" :
                          allApartmentsOnViewRadioButton.IsChecked == true ? "AllApartmentsOnView" :
                          allApartmentsOnObjectRadioButton.IsChecked == true ? "AllApartmentsOnObject" : null;

            if (mode == "SelectedApartment")
            {
                _livingRoomManipulations.ProcessLivingRooms(livingRoomsTextBox.Text, mode, apartmentComboBox.SelectedItem?.ToString());
            }
            else if (mode != null)
            {
                _livingRoomManipulations.ProcessLivingRooms(livingRoomsTextBox.Text, mode);
            }
            else
            {
                MessageBox.Show("Нужен выбор режима расчета!");
            }
        }

        private void CreateLayoutButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (selectedApartmentRadioButton.IsChecked == true || allApartmentsOnViewRadioButton.IsChecked == true)
                {
                    MessageBox.Show("Выбрать можно только: " +
                        "\n- Квартиры на объекте");
                    return;
                }

                if (!_apartmentsData.Any())
                {
                    MessageBox.Show("Данные о квартирах отсутствуют. Пожалуйста, выберите режим расчета (выбор представлен выше).");
                    return;
                }
                _apartmentLayout.CreateApartmentLayout(_doc, _apartmentsData);
                MessageBox.Show("Чертежный вид с квартирографией успешно создан!" 
                                + $"\nКвартир обработано: {_apartmentsData.Count}", "Успешное создание чертежного вида");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}" + "\nЧертеж уже существует!");
            }
        }

        private void UpdateLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedApartmentRadioButton.IsChecked == true || allApartmentsOnViewRadioButton.IsChecked == true)
                {
                    MessageBox.Show("Выбрать можно только: " +
                        "\n- Квартиры на объекте");
                    return;
                }

                if (!_apartmentsData.Any())
                {
                    MessageBox.Show("Данные о квартирах отсутствуют. Пожалуйста, выберите режим расчета (выбор представлен выше).");
                    return;
                }

                var (newApartmentsData, errors) = _dataProvider.GetApartmentData(_doc, false);
                _apartmentsData.Clear();
                foreach (var kvp in newApartmentsData)
                {
                    _apartmentsData[kvp.Key] = kvp.Value;
                }
                if (errors.Any())
                {
                    MessageBox.Show(string.Join("\n", errors));
                }

                _apartmentUpdater.UpdateApartmentLayout(_doc, _apartmentsData);
                MessageBox.Show("Чертежный вид успешно обновлён!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении чертежного вида: {ex.Message}");
            }
        }
    }
}





