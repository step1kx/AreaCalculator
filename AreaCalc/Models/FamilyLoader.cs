using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AreaCalc.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace AreaCalc.Models
{
    public class FamilyLoader: IFamilyLoader
    {

        private const string FamilyName = "Квартирография - СРК - Ячейка";


        public FamilySymbol LoadFamilyFromResource(Document doc)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream familyStream = assembly.GetManifestResourceStream("AreaCalc.Resources.Квартирография - СРК - Ячейка.rfa"))
                {
                    if (familyStream == null)
                    {
                        MessageBox.Show("Файл семейства не найден в ресурсах.");
                        return null;
                    }

                    string tempFilePath = Path.Combine(Path.GetTempPath(), "Квартирография - СРК - Ячейка.rfa");
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        familyStream.CopyTo(fileStream);
                    }

                    if (doc.LoadFamily(tempFilePath, out Family loadedFamily))
                    {
                        MessageBox.Show($"Семейство успешно загружено из {tempFilePath}");

                        FamilySymbol familySymbol = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilySymbol))
                            .Cast<FamilySymbol>()
                            .FirstOrDefault(fs => fs.FamilyName == FamilyName);

                        if (familySymbol == null)
                        {
                            MessageBox.Show($"Символ семейства '{FamilyName}' не найден после загрузки.");
                        }
                        else
                        {
                            File.Delete(tempFilePath);
                            return familySymbol;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось загрузить семейство из {tempFilePath}");
                    }

                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке семейства: {ex.Message}");
                return null;
            }
        }
    }
}
