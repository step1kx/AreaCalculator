using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Models
{
    public class ApartmentLayout : IApartmentLayout
    {
        private readonly IDraftingServices _draftingService;

        private const string FamilyName = "Квартирография - СРК - Ячейка";
        private const int MaxApartmentsPerRow = 5; // количество ячеек в ряду
        private const double CellWidth = 3.0; // ширина ячейки
        private const double CellHeight = 2.0; // высота ячейки
        private const double Spacing = 5; // расстояние между ячейками


        public ApartmentLayout(DraftingServices draftingServices)
        {
            _draftingService = draftingServices ?? throw new ArgumentNullException(nameof(draftingServices));
        }

        

        public void CreateApartmentLayout(Document doc, Dictionary<string, List<Room>> apartmentsData)
        {
            if (apartmentsData == null || !apartmentsData.Any())
            {
                throw new ArgumentException("Данные о квартирах отсутствуют.");
            }

            using (Transaction tx = new Transaction(doc, "Создание чертежного вида квартирографии"))
            {
                tx.Start();

                ViewDrafting draftingView = _draftingService.CreateDraftingView(doc);

                FamilySymbol familySymbol = _draftingService.FindFamilySymbol(doc, FamilyName);
                if (familySymbol == null)
                {
                    throw new Exception($"Семейство '{FamilyName}' не найдено в проекте.");
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
            int columns = Math.Min(apartmentCount, MaxApartmentsPerRow);
            int rows = (int)Math.Ceiling((double)apartmentCount / MaxApartmentsPerRow);

            int index = 0;
            foreach (var apartment in sortedApartments)
            {
                int row = index / MaxApartmentsPerRow;
                int col = index % MaxApartmentsPerRow;

                double x = col * (CellWidth + Spacing);
                double y = row * (CellHeight + Spacing);
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
