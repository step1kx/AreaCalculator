using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaCalc.Models
{
    public class DraftingServices : IDraftingServices
    {
        public ViewDrafting CreateDraftingView(Document doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc), "Документ Revit не может быть null.");
            }

            // Находим тип чертежного вида
            ViewFamilyType draftingViewType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);

            if (draftingViewType == null)
            {
                throw new Exception("Не найден тип чертежного вида в проекте.");
            }

            ViewDrafting draftingView = ViewDrafting.Create(doc, draftingViewType.Id);
            if (draftingView == null)
            {
                throw new Exception("Не удалось создать чертежный вид.");
            }

            draftingView.Name = "Квартирография - Схема расположения квартир";
            return draftingView;
        }

        public FamilySymbol FindFamilySymbol(Document doc, string familyName)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc), "Документ Revit не может быть null.");
            }

            if (string.IsNullOrWhiteSpace(familyName))
            {
                throw new ArgumentException("Имя семейства не может быть пустым.", nameof(familyName));
            }

            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs => fs.FamilyName == familyName);
        }

        public FamilyInstance PlaceFamilyInstance(Document doc, ViewDrafting view, FamilySymbol symbol, XYZ location)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc), "Документ Revit не может быть null.");
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view), "Чертежный вид не может быть null.");
            }

            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol), "Семейство не может быть null.");
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location), "Координаты не могут быть null.");
            }

            if (!symbol.IsActive)
            {
                symbol.Activate();
                doc.Regenerate();
            }

            FamilyInstance instance = doc.Create.NewFamilyInstance(location, symbol, view);
            if (instance == null)
            {
                throw new Exception("Не удалось разместить экземпляр семейства.");
            }

            return instance;
        }
    }
}
