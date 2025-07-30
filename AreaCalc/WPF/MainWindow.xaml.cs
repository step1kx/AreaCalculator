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
using Autodesk.Revit.UI;

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
            _apartmentLayout = new ApartmentLayout(new DraftingServices(), new FamilyLoader());
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
                    var allErrors = new HashSet<string>(); 
                    int processed = 0; 

                    foreach (var apartment in _apartmentsData)
                    {
                        double area = 0;
                        try
                        {
                            ProcessApartment(apartment.Value, livingFormula, usualFormula, totalFormula, ref area);
                            totalViewArea += area;
                            processed++;
                        }
                        catch (Exception ex)
                        {
                            allErrors.Add($"{ex.Message}");
                        }
                    }

                    if (allErrors.Any())
                    {
                        string errorMessage = "Обнаружены следующие ошибки:\n" + string.Join("\n", allErrors);
                        MessageBox.Show(errorMessage);
                    }

                    if (processed > 0) 
                    {
                        MessageBox.Show($"Расчет завершен. Обработано квартир: {processed}");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обработать ни одной квартиры.");
                    }
                }
                else
                {
                    MessageBox.Show("Вы не выбрали режим для работы (режимы представлены выше)");
                    return;
                }

                tx.Commit();
            }
        }

        private void ProcessApartment(List<Autodesk.Revit.DB.Architecture.Room> rooms, string livingFormula, string usualFormula, string totalFormula, ref double totalArea)
        {
            try
            {
                foreach (var room in rooms)
                {
                    var areaParam = room.get_Parameter(BuiltInParameter.ROOM_AREA);
                    if (areaParam == null)
                        throw new Exception("У помещения отсутствует встроенный параметр ROOM_AREA.");

                    double areaMeters = areaParam.AsDouble() * 0.092903;
                    double roundedArea = RoundToThreeAndThenTwo(areaMeters);

                    if (!_parameterUpdater.HasParameter(room, "Скрипт.Площадь помещения"))
                        throw new Exception("У помещения отсутствует параметр 'Скрипт.Площадь помещения'.");

                    _parameterUpdater.UpdateRoomParameter(room, "Скрипт.Площадь помещения", roundedArea);
                }

                var (livingArea, usualArea, calculatedTotalArea) = _areaCalculator.CalculateAreas(rooms, livingFormula, usualFormula, totalFormula);

                foreach (var room in rooms)
                {
                            // Проверка технических параметров
                     string[] techParams = {
                    "Скрипт.Жилая площадь",
                    "Скрипт.Площадь квартиры",
                    "Скрипт.Общая площадь"
                };

                    foreach (var param in techParams)
                    {
                        if (!_parameterUpdater.HasParameter(room, param))
                            throw new Exception($"У помещения отсутствует параметр '{param}'.");
                    }

                    _parameterUpdater.UpdateRoomParameter(room, "Скрипт.Жилая площадь", RoundToThreeAndThenTwo(livingArea));
                    _parameterUpdater.UpdateRoomParameter(room, "Скрипт.Площадь квартиры", RoundToThreeAndThenTwo(usualArea));
                    _parameterUpdater.UpdateRoomParameter(room, "Скрипт.Общая площадь", RoundToThreeAndThenTwo(calculatedTotalArea));

                    double livingRaw = _parameterUpdater.GetRoomParameterValueRaw(room, "Скрипт.Жилая площадь");
                    double usualRaw = _parameterUpdater.GetRoomParameterValueRaw(room, "Скрипт.Площадь квартиры");
                    double totalRaw = _parameterUpdater.GetRoomParameterValueRaw(room, "Скрипт.Общая площадь");

                            // Проверка КГ-параметров
                    string[] kgParams = {
                    "КГ.S.Ж",
                    "КГ.S.ЖП.Площадь квартиры",
                    "КГ.S.ЖПЛк.Общая площадь"
                };

                    foreach (var param in kgParams)
                    {
                        if (!_parameterUpdater.HasParameter(room, param))
                            throw new Exception($"У помещения отсутствует параметр '{param}'.");
                    }

                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.Ж", livingRaw * 10.7639);
                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.ЖП.Площадь квартиры", usualRaw * 10.7639);
                    _parameterUpdater.UpdateRoomParameter(room, "КГ.S.ЖПЛк.Общая площадь", totalRaw * 10.7639);
                }

                totalArea = calculatedTotalArea;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка в расчете площадей:\n{ex.Message}");
                throw; // если хочешь, можно убрать повторный throw
            }
        }


        private double RoundToThreeAndThenTwo(double value)
        {
            double roundedToThree = Math.Round(value, 2);
            return roundedToThree;
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
                MessageBox.Show("Вы не выбрали режим для работы (режимы представлены выше)");
                return; 
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
                    MessageBox.Show("Чертежный вид не может быть создан.\nПожалуйста, выберите режим расчета \'Все квартиры на объекте\'.");
                    return;
                }
                _apartmentLayout.CreateApartmentLayout(_doc, _apartmentsData);
                MessageBox.Show("Чертежный вид с квартирографией успешно создан!" 
                                + $"\nКвартир обработано: {_apartmentsData.Count}", "Успешное создание чертежного вида");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}" +
                "\nЧертеж уже существует!");
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
                    MessageBox.Show("Чертежный вид не может быть обновлен.\nПожалуйста, выберите режим расчета 'Все квартиры на объекте'.");
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





