using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AreaCalc
{
    internal class ValidCheckers
    {
        //private bool AreFormulasValid(string livingFormula, string usualFormula, string totalFormula)
        //{
        //    if (string.IsNullOrEmpty(livingFormula) && string.IsNullOrEmpty(usualFormula) && string.IsNullOrEmpty(totalFormula))
        //    {
        //        MessageBox.Show("Пожалуйста, введите хотя бы одну формулу.");
        //        return false;
        //    }
        //    return true;
        //}

        //private void ProcessSelectedApartment(string livingFormula, string usualFormula, string totalFormula, ref double totalViewArea)
        //{

        //    string selectedApartment = apartmentComboBox.SelectedItem?.ToString();
        //    if (string.IsNullOrEmpty(selectedApartment))
        //    {
        //        MessageBox.Show("Пожалуйста, выберите квартиру.");
        //        return;
        //    }

        //    string apartmentNumber = selectedApartment.Split(' ')[2];
        //    if (apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
        //    {
        //        var roomAreas = CalculateRoomAreas(rooms);
        //        CalculateAndUpdateRoomParameters(rooms, roomAreas, livingFormula, usualFormula, totalFormula, ref totalViewArea);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Квартира не найдена в данных.");
        //    }
        //}

        // Подсчет жилых помещений в доме (КГ.Жилые комнаты)

        //private int CalculateLivingRoomsCount(List<Room> rooms, HashSet<string> livingRoomTypes)
        //{
        //    int livingRoomsCount = 0;

        //    foreach (Room room in rooms)
        //    {
        //        Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");
        //        if (roomTypeParam != null)
        //        {
        //            string roomType = "Тип" + roomTypeParam.AsInteger().ToString();
        //            if (livingRoomTypes.Contains(roomType))
        //            {
        //                livingRoomsCount++;
        //            }
        //        }
        //    }

        //    return livingRoomsCount;
        //}

        //private void UpdateLivingRoomsParameter(List<Room> rooms, int count)
        //{
        //    foreach (Room room in rooms)
        //    {
        //        Parameter livingParam = room.LookupParameter("КГ.Жилые комнаты");
        //        if (livingParam != null && !livingParam.IsReadOnly)
        //        {
        //            livingParam.Set(count);
        //        }
        //    }
        //}

        //private void LivingRoomParameterCalculate_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string input = livingRoomsTextBox.Text;
        //        if (string.IsNullOrEmpty(input))
        //        {
        //            MessageBox.Show("Пожалуйста, укажите типы жилых помещений.");
        //            return;
        //        }

        //        HashSet<string> livingRoomTypes = new HashSet<string>(
        //            input.Split(',')
        //                 .Select(t => t.Trim())
        //                 .Where(t => !string.IsNullOrEmpty(t))
        //                 .Select(t => t.StartsWith("Тип") ? t : $"Тип{t}")
        //        );

        //        if (livingRoomTypes.Count == 0)
        //        {
        //            MessageBox.Show("Не удалось распознать типы помещений. Используйте формат: Тип1, Тип3");
        //            return;
        //        }

        //        using (Transaction tx = new Transaction(_doc, "Обновление количества жилых комнат"))
        //        {
        //            tx.Start();
        //            int processedApartments = 0;

        //            if (selectedApartmentRadioButton.IsChecked == true)
        //            {
        //                string selectedApartment = apartmentComboBox.SelectedItem?.ToString();
        //                if (string.IsNullOrEmpty(selectedApartment))
        //                {
        //                    MessageBox.Show("Пожалуйста, выберите квартиру.");
        //                    return;
        //                }

        //                string apartmentNumber = selectedApartment.Split(' ')[2];
        //                if (apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
        //                {
        //                    GetApartmentData(apartmentsData, _doc, roomTypeParam); 

        //                    if (apartmentsData.TryGetValue(apartmentNumber, out rooms))
        //                    {
        //                        int livingCount = CalculateLivingRoomsCount(rooms, livingRoomTypes);
        //                        UpdateLivingRoomsParameter(rooms, livingCount);
        //                        processedApartments++;

        //                        Parameter checkParam = rooms.First().LookupParameter("КГ.Жилые комнаты");
        //                        int actualValue = checkParam?.AsInteger() ?? -1;

        //                        MessageBox.Show($"Квартира номер {apartmentNumber} обработана!\n" +
        //                                      $"Жилые типы: {string.Join(", ", livingRoomTypes)}\n" +
        //                                      $"Найдено жилых комнат: {livingCount}\n" +
        //                                      $"Установленное значение: {actualValue}");
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show($"Не удалось обновить данные для квартиры {apartmentNumber}");
        //                    }
        //                }
        //                else
        //                {
        //                    MessageBox.Show($"Квартира {apartmentNumber} не найдена в данных");
        //                }
        //            }
        //            else if (allApartmentsOnViewRadioButton.IsChecked == true)
        //            {
        //                GetApartmentData(apartmentsData, _doc, roomTypeParam);
        //                foreach (var apartment in apartmentsData)
        //                {
        //                    int livingCount = CalculateLivingRoomsCount(apartment.Value, livingRoomTypes);
        //                    UpdateLivingRoomsParameter(apartment.Value, livingCount);
        //                    processedApartments++;
        //                }
        //                MessageBox.Show($"Обновление завершено. \nОбработано квартир: {processedApartments}. " +
        //                          $"\nЖилые типы: {string.Join(", ", livingRoomTypes)}");
        //            }
        //            else if (allApartmentsOnObjectRadioButton.IsChecked == true)
        //            {
        //                InitializeApartmentsData();
        //                foreach (var apartment in apartmentsData)
        //                {
        //                    int livingCount = CalculateLivingRoomsCount(apartment.Value, livingRoomTypes);
        //                    UpdateLivingRoomsParameter(apartment.Value, livingCount);
        //                    processedApartments++;
        //                }
        //                MessageBox.Show($"Обновление завершено. Обработано квартир: {processedApartments}. " +
        //                          $"Жилые типы: {string.Join(", ", livingRoomTypes)}");
        //            }
        //            else
        //            {
        //                MessageBox.Show("Нужен выбор режима расчета!");
        //                return;
        //            }

        //            tx.Commit();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при обновлении: {ex.Message}");
        //    }
        //}

    }
}
