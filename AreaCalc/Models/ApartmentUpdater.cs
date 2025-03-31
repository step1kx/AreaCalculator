using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AreaCalc.Models
{
    internal class ApartmentUpdater : IApartmentUpdater
    {
        private readonly IDraftingServices _draftingService;

        private const int MaxApartmentsPerRow = 5;
        private const double CellWidth = 3.0;
        private const double CellHeight = 2.0;
        private const double Spacing = 5;

        private const string DRAFTING_NAME = "Квартирография - Схема расположения квартир";
        private const string FAMILY_NAME = "Квартирография - СРК - Ячейка";

        public ApartmentUpdater(IDraftingServices draftingService)
        {
            _draftingService = draftingService ?? throw new ArgumentNullException(nameof(draftingService));
        }

        public void UpdateApartmentLayout(Document doc, Dictionary<string, List<Room>> apartmentsData)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc), "Документ Revit не может быть null.");
            }

            if (apartmentsData == null || !apartmentsData.Any())
            {
                throw new ArgumentException("Данные о квартирах отсутствуют.");
            }

            using (Transaction tx = new Transaction(doc, "Обновление чертежного вида квартирографии"))
            {
                tx.Start();

                ViewDrafting draftingView = FindDraftingView(doc);
                if (draftingView == null)
                {
                    throw new Exception("Чертежный вид 'Квартирография - Схема расположения квартир' не найден.");
                }

                IEnumerable<FamilyInstance> instances = new FilteredElementCollector(doc, draftingView.Id)
                     .OfClass(typeof(FamilyInstance))
                     .ToElements()
                     .Cast<FamilyInstance>()
                     .Where(i => i.Symbol.FamilyName.Equals(FAMILY_NAME, StringComparison.OrdinalIgnoreCase));

                // MessageBox.Show($"Найдено экземпляров 'Квартирография - СРК - Ячейка': {instances.Count()}"); отладка

                if (!instances.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Проверка имён семейств:");
                    foreach (var instance in new FilteredElementCollector(doc, draftingView.Id)
                        .OfClass(typeof(FamilyInstance))
                        .ToElements()
                        .Cast<FamilyInstance>())
                    {
                        MessageBox.Show($"Семейство: {instance.Symbol.FamilyName}");
                    }
                    throw new Exception("В чертежном виде не найдено экземпляров семейства 'Квартирография - СРК - Ячейка'.");
                }

                var instanceMap = instances.ToDictionary(
                    i => i.LookupParameter("КГ.СРК.Номер квартиры")?.AsString() ?? "",
                    i => i,
                    StringComparer.OrdinalIgnoreCase);

                var sortedApartments = apartmentsData
                    .OrderBy(kvp => int.Parse(kvp.Key))
                    .ToList();

                int index = 0;
                foreach (var apartment in sortedApartments)
                {
                    string apartmentNumber = apartment.Key;
                    List<Room> rooms = apartment.Value;

                    FamilyInstance instance = instances
                        .FirstOrDefault(i => i.LookupParameter("КГ.СРК.Номер квартиры")?.AsString() == apartmentNumber);

                    if (instance == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Экземпляр для квартиры {apartmentNumber} не найден.");
                        continue;
                    }

                    UpdateFamilyInstanceParameters(instance, rooms);
                    index++;
                }

                tx.Commit();
            }
        }

        private ViewDrafting FindDraftingView(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ViewDrafting))
                .Cast<ViewDrafting>()
                .FirstOrDefault(v => v.Name == DRAFTING_NAME);
        }

        private void UpdateFamilyInstanceParameters(FamilyInstance instance, List<Room> rooms)
        {
            if (instance == null || rooms == null || !rooms.Any())
            {
                return;
            }

            Room firstRoom = rooms.FirstOrDefault();
            if (firstRoom == null)
            {
                return;
            }

            Parameter numberParam = instance.LookupParameter("КГ.СРК.Номер квартиры");
            if (numberParam != null && !numberParam.IsReadOnly)
            {
                string currentNumber = numberParam.AsString();
                string newNumber = firstRoom.LookupParameter("КГ.Номер квартиры")?.AsString();
                if (!string.IsNullOrWhiteSpace(newNumber) && currentNumber != newNumber)
                {
                    numberParam.Set(newNumber);
                }
            }

            Parameter typeParam = firstRoom.LookupParameter("КГ.Тип квартиры") ?? firstRoom.LookupParameter("КГ.Тип помещения");
            if (typeParam != null && typeParam.HasValue)
            {
                string apartmentType = typeParam.AsString();
                Parameter familyTypeParam = instance.LookupParameter("КГ.СРК.Тип квартиры");
                if (familyTypeParam != null && !familyTypeParam.IsReadOnly)
                {
                    string currentType = familyTypeParam.AsString();
                    if (currentType != apartmentType)
                    {
                        familyTypeParam.Set(apartmentType);
                    }
                }
            }

            double totalArea = 0;
            Parameter totalAreaParam = firstRoom.LookupParameter("КГ.S.ЖПЛк.Общая площадь");
            if (totalAreaParam != null && totalAreaParam.HasValue)
            {
                totalArea = totalAreaParam.AsDouble();
            }
            Parameter familyTotalAreaParam = instance.LookupParameter("КГ.СРК.S.ЖПЛк.Общая площадь");
            if (familyTotalAreaParam != null && !familyTotalAreaParam.IsReadOnly)
            {
                double currentTotalArea = familyTotalAreaParam.AsDouble();
                if (!currentTotalArea.Equals(totalArea))
                {
                    familyTotalAreaParam.Set(totalArea);
                }
            }

            double livingArea = 0;
            Parameter livingAreaParam = firstRoom.LookupParameter("КГ.S.Ж");
            if (livingAreaParam != null && livingAreaParam.HasValue)
            {
                livingArea = livingAreaParam.AsDouble();
            }
            Parameter familyLivingAreaParam = instance.LookupParameter("КГ.СРК.S.Ж");
            if (familyLivingAreaParam != null && !familyLivingAreaParam.IsReadOnly)
            {
                double currentLivingArea = familyLivingAreaParam.AsDouble();
                if (!currentLivingArea.Equals(livingArea))
                {
                    familyLivingAreaParam.Set(livingArea);
                }
            }
        }


    }
}
