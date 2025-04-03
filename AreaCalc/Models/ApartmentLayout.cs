using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace AreaCalc.Models
{
    public class ApartmentLayout : IApartmentLayout
    {
        private readonly IDraftingServices _draftingService;
        private readonly IFamilyLoader _familyLoader;

        private const string FAMILY_NAME = "Квартирография - СРК - Ячейка";
        private const int MAX_APARTMENTS_PER_ROW = 5; // количество ячеек в ряду
        private const double CELL_WIDTH = 3.0; // ширина ячейки
        private const double CELL_HEIGHT = 2.0; // высота ячейки
        private const double SPACING = 5; // расстояние между ячейками


        public ApartmentLayout(DraftingServices draftingServices, FamilyLoader familyLoader)
        {
            _draftingService = draftingServices ?? throw new ArgumentNullException(nameof(draftingServices));
            _familyLoader = familyLoader ?? throw new ArgumentNullException(nameof(familyLoader));  
        }

        

        public void CreateApartmentLayout(Document doc, Dictionary<string, List<Room>> apartmentsData)
        {
            using (Transaction tx = new Transaction(doc, "Создание чертежного вида квартирографии"))
            {
                tx.Start();

                ViewDrafting draftingView = _draftingService.CreateDraftingView(doc);

                FamilySymbol familySymbol = _draftingService.FindFamilySymbol(doc, FAMILY_NAME);
                if (familySymbol == null)
                {
                    MessageBox.Show($"Семейство '{FAMILY_NAME}' не найдено в проекте. Пытаемся загрузить из ресурсов...");
                    familySymbol = _familyLoader.LoadFamilyFromResource(doc);
                    if (familySymbol == null)
                    {
                        throw new Exception($"Не удалось загрузить семейство '{FAMILY_NAME}' из ресурсов.");
                    }
                }

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                List<FamilyInstance> familyInstances = PlaceFamilyInstances(doc, draftingView, familySymbol, apartmentsData);

                FillFamilyInstanceParameters(doc, familyInstances, apartmentsData);

                tx.Commit();
            }
        }


        private List<FamilyInstance> PlaceFamilyInstances(Document doc, ViewDrafting draftingView, FamilySymbol familySymbol, Dictionary<string, List<Room>> apartmentsData)
        {
            List<FamilyInstance> familyInstances = new List<FamilyInstance>();

            var sortedApartments = apartmentsData
                .OrderBy(kvp => int.Parse(kvp.Key))
                .ToList();

            int apartmentCount = sortedApartments.Count;
            int columns = Math.Min(apartmentCount, MAX_APARTMENTS_PER_ROW);
            int rows = (int)Math.Ceiling((double)apartmentCount / MAX_APARTMENTS_PER_ROW);

            int index = 0;
            foreach (var apartment in sortedApartments)
            {
                int row = index / MAX_APARTMENTS_PER_ROW;
                int col = index % MAX_APARTMENTS_PER_ROW;

                double x = col * (CELL_WIDTH + SPACING);
                double y = row * (CELL_HEIGHT + SPACING);
                XYZ location = new XYZ(x, y, 0);

                FamilyInstance instance = _draftingService.PlaceFamilyInstance(doc, draftingView, familySymbol, location);
                familyInstances.Add(instance);

                index++;
            }

            return familyInstances;
        }

        private void FillFamilyInstanceParameters(Document doc, List<FamilyInstance> familyInstances, Dictionary<string, List<Room>> apartmentsData)
        {
            int index = 0;
            foreach (var apartment in apartmentsData.OrderBy(kvp => int.Parse(kvp.Key)))
            {
                string apartmentNumber = apartment.Key;
                List<Room> rooms = apartment.Value;
                FamilyInstance instance = familyInstances[index];

                Parameter numberParam = instance.LookupParameter("КГ.СРК.Номер квартиры");
                if (numberParam != null && !numberParam.IsReadOnly)
                {
                    numberParam.Set(apartmentNumber);
                }

                Room firstRoom = rooms.FirstOrDefault();
                if (firstRoom != null)
                {
                    Parameter typeParam = firstRoom.LookupParameter("КГ.Тип квартиры");
                    if (typeParam != null)
                    {
                        string apartmentType = typeParam.AsString();
                        Parameter familyTypeParam = instance.LookupParameter("КГ.СРК.Тип квартиры");
                        if (familyTypeParam != null && !familyTypeParam.IsReadOnly)
                        {
                            familyTypeParam.Set(apartmentType);
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
                        familyTotalAreaParam.Set(totalArea);
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
                        familyLivingAreaParam.Set(livingArea);
                    }
                }

                index++;
            }
        }
    }
}
