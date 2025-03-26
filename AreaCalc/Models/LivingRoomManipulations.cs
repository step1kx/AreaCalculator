using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AreaCalc.Interfaces;

namespace AreaCalc
{
    internal class LivingRoomManipulations
    {
        private readonly Document _doc;
        private readonly Dictionary<string, List<Room>> _apartmentsData;
        private readonly IApartmentDataProvider _dataProvider;

        public LivingRoomManipulations(Document doc, Dictionary<string, List<Room>> apartmentsData, IApartmentDataProvider dataProvider)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _apartmentsData = apartmentsData ?? throw new ArgumentNullException(nameof(apartmentsData));
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public int CalculateLivingRoomsCount(List<Room> rooms, HashSet<string> livingRoomTypes)
        {
            if (rooms == null || livingRoomTypes == null)
                return 0;

            int livingRoomsCount = 0;

            foreach (Room room in rooms)
            {
                Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                if (roomTypeParam != null)
                {
                    string roomType = "Тип" + roomTypeParam.AsInteger().ToString();
                    if (livingRoomTypes.Contains(roomType))
                    {
                        livingRoomsCount++;
                    }
                }
            }

            return livingRoomsCount;
        }

        public void UpdateLivingRoomsParameter(List<Room> rooms, int count)
        {
            if (rooms == null)
                return;

            foreach (Room room in rooms)
            {
                Parameter livingParam = room.LookupParameter("КГ.Жилые комнаты");
                if (livingParam != null && !livingParam.IsReadOnly)
                {
                    livingParam.Set(count);
                }
            }
        }

        public void ProcessLivingRooms(string input, string mode, string selectedApartment = null)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("Пожалуйста, укажите типы жилых помещений.");
                    return;
                }

                HashSet<string> livingRoomTypes = new HashSet<string>(
                    input.Split(',')
                         .Select(t => t.Trim())
                         .Where(t => !string.IsNullOrEmpty(t))
                         .Select(t => t.StartsWith("Тип") ? t : $"Тип{t}")
                );

                if (livingRoomTypes.Count == 0)
                {
                    MessageBox.Show("Не удалось распознать типы помещений. Используйте формат: Тип1, Тип3");
                    return;
                }

                using (Transaction tx = new Transaction(_doc, "Обновление количества жилых комнат"))
                {
                    tx.Start();
                    int processedApartments = 0;

                    switch (mode)
                    {
                        case "SelectedApartment":
                            if (string.IsNullOrEmpty(selectedApartment))
                            {
                                MessageBox.Show("Пожалуйста, выберите квартиру.");
                                return;
                            }

                            string apartmentNumber = selectedApartment.Split(' ')[2];
                            if (_apartmentsData.TryGetValue(apartmentNumber, out List<Room> rooms))
                            {
                                // Обновляем данные о квартирах
                                var (apartmentsData, errors) = _dataProvider.GetApartmentData(_doc, true);
                                _apartmentsData.Clear();
                                foreach (var kvp in apartmentsData)
                                {
                                    _apartmentsData[kvp.Key] = kvp.Value;
                                }

                                if (errors.Any())
                                {
                                    MessageBox.Show(string.Join("\n", errors));
                                }

                                if (_apartmentsData.TryGetValue(apartmentNumber, out rooms))
                                {
                                    int livingCount = CalculateLivingRoomsCount(rooms, livingRoomTypes);
                                    UpdateLivingRoomsParameter(rooms, livingCount);
                                    processedApartments++;

                                    Parameter checkParam = rooms.First().LookupParameter("КГ.Жилые комнаты");
                                    int actualValue = checkParam?.AsInteger() ?? -1;

                                    MessageBox.Show($"Квартира номер {apartmentNumber} обработана!\n" +
                                                    $"Жилые типы: {string.Join(", ", livingRoomTypes)}\n" +
                                                    $"Найдено жилых комнат: {livingCount}\n" +
                                                    $"Установленное значение: {actualValue}");
                                }
                                else
                                {
                                    MessageBox.Show($"Не удалось обновить данные для квартиры {apartmentNumber}");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Квартира {apartmentNumber} не найдена в данных");
                            }
                            break;

                        case "AllApartmentsOnView":
                            // Обновляем данные о квартирах для активного вида
                            var (viewData, viewErrors) = _dataProvider.GetApartmentData(_doc, true);
                            _apartmentsData.Clear();
                            foreach (var kvp in viewData)
                            {
                                _apartmentsData[kvp.Key] = kvp.Value;
                            }

                            if (viewErrors.Any())
                            {
                                MessageBox.Show(string.Join("\n", viewErrors));
                            }

                            foreach (var apartment in _apartmentsData)
                            {
                                int livingCount = CalculateLivingRoomsCount(apartment.Value, livingRoomTypes);
                                UpdateLivingRoomsParameter(apartment.Value, livingCount);
                                processedApartments++;
                            }
                            MessageBox.Show($"Обновление завершено.\nОбработано квартир: {processedApartments}.\n" +
                                            $"Жилые типы: {string.Join(", ", livingRoomTypes)}");
                            break;

                        case "AllApartmentsOnObject":
                            // Обновляем данные о квартирах для всего объекта
                            var (objectData, objectErrors) = _dataProvider.GetApartmentData(_doc, false);
                            _apartmentsData.Clear();
                            foreach (var kvp in objectData)
                            {
                                _apartmentsData[kvp.Key] = kvp.Value;
                            }

                            if (objectErrors.Any())
                            {
                                MessageBox.Show(string.Join("\n", objectErrors));
                            }

                            foreach (var apartment in _apartmentsData)
                            {
                                int livingCount = CalculateLivingRoomsCount(apartment.Value, livingRoomTypes);
                                UpdateLivingRoomsParameter(apartment.Value, livingCount);
                                processedApartments++;
                            }
                            MessageBox.Show($"Обновление завершено.\nОбработано квартир: {processedApartments}.\n" +
                                            $"Жилые типы: {string.Join(", ", livingRoomTypes)}");
                            break;

                        default:
                            MessageBox.Show("Нужен выбор режима расчета!");
                            return;
                    }

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}");
            }
        }
    }
}