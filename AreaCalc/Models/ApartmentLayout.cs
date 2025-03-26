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
        private const string FamilyName = "Квартирография - СРК - Ячейка";
        private const int MaxApartmentsPerRow = 5;
        private const double CellWidth = 2.0; // Ширина ячейки в футах (примерное значение, можно настроить)
        private const double CellHeight = 1.0; // Высота ячейки в футах (примерное значение, можно настроить)
        private const double Spacing = 0.5; // Расстояние между ячейками в футах

        public void CreateApartmentLayout(Document doc, Dictionary<string, List<Room>> apartmentsData)
        {
            if (apartmentsData == null || !apartmentsData.Any())
            {
                throw new ArgumentException("Данные о квартирах отсутствуют.");
            }

            using (Transaction tx = new Transaction(doc, "Создание чертежного вида квартирографии"))
            {
                tx.Start();

                // Шаг 1: Создаём чертежный вид
                ViewDrafting draftingView = CreateDraftingView(doc);

                // Шаг 2: Находим семейство "Квартирография - СРК - Ячейка"
                FamilySymbol familySymbol = FindFamilySymbol(doc, FamilyName);
                if (familySymbol == null)
                {
                    throw new Exception($"Семейство '{FamilyName}' не найдено в проекте.");
                }

                // Активируем семейство, если оно не активно
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                // Шаг 3: Размещаем семейства в сетке
                List<FamilyInstance> familyInstances = PlaceFamilyInstances(doc, draftingView, familySymbol, apartmentsData);

                // Шаг 4: Заполняем нумерацию и параметры
                FillFamilyInstanceParameters(doc, familyInstances, apartmentsData);

                tx.Commit();
            }
        }

        private ViewDrafting CreateDraftingView(Document doc)
        {
            // Находим тип чертежного вида (Drafting View Type)
            ViewFamilyType draftingViewType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);

            if (draftingViewType == null)
            {
                throw new Exception("Не найден тип чертежного вида в проекте.");
            }

            // Создаём чертежный вид
            ViewDrafting draftingView = ViewDrafting.Create(doc, draftingViewType.Id);
            draftingView.Name = "Квартирография - Обзор";

            return draftingView;
        }

        private FamilySymbol FindFamilySymbol(Document doc, string familyName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs => fs.FamilyName == familyName);
        }

        private List<FamilyInstance> PlaceFamilyInstances(Document doc, ViewDrafting draftingView, FamilySymbol familySymbol, Dictionary<string, List<Room>> apartmentsData)
        {
            List<FamilyInstance> familyInstances = new List<FamilyInstance>();
            int apartmentCount = apartmentsData.Count;
            int columns = Math.Min(apartmentCount, MaxApartmentsPerRow);
            int rows = (int)Math.Ceiling((double)apartmentCount / MaxApartmentsPerRow);

            int index = 0;
            foreach (var apartment in apartmentsData)
            {
                int row = index / MaxApartmentsPerRow;
                int col = index % MaxApartmentsPerRow;

                // Вычисляем координаты для размещения
                double x = col * (CellWidth + Spacing);
                double y = -row * (CellHeight + Spacing); // Отрицательное значение, чтобы двигаться вверх
                XYZ location = new XYZ(x, y, 0);

                // Создаём экземпляр семейства
                FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, draftingView);
                familyInstances.Add(instance);

                index++;
            }

            return familyInstances;
        }

        private void FillFamilyInstanceParameters(Document doc, List<FamilyInstance> familyInstances, Dictionary<string, List<Room>> apartmentsData)
        {
            int index = 0;
            foreach (var apartment in apartmentsData)
            {
                string apartmentNumber = apartment.Key;
                List<Room> rooms = apartment.Value;
                FamilyInstance instance = familyInstances[index];

                // Шаг 4: Заполняем нумерацию
                Parameter numberParam = instance.LookupParameter("Номер квартиры");
                if (numberParam != null && !numberParam.IsReadOnly)
                {
                    numberParam.Set(apartmentNumber);
                }

                // Шаг 5: Заполняем параметры (пример: общая площадь, количество комнат)
                double totalArea = rooms.Sum(r => r.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble());
                Parameter areaParam = instance.LookupParameter("Общая площадь");
                if (areaParam != null && !areaParam.IsReadOnly)
                {
                    areaParam.Set(totalArea);
                }

                int roomCount = rooms.Count;
                Parameter roomCountParam = instance.LookupParameter("Количество комнат");
                if (roomCountParam != null && !roomCountParam.IsReadOnly)
                {
                    roomCountParam.Set(roomCount);
                }

                // Здесь можно добавить другие параметры, например, жилую площадь, если она вычисляется
                index++;
            }
        }
    }
}
