using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;

namespace AreaCalc
{
    internal class Buttons : IExternalApplication
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string tabName = "ETM";


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel;
            try
            {
                panel = application.GetRibbonPanels(tabName).First();
            }
            catch (Exception)
            {
                application.CreateRibbonTab(tabName);
            }
            panel = application.CreateRibbonPanel(tabName, "Квартирография");

            panel.AddItem(new PushButtonData(nameof(MainFunction_1), "Квартирография", assemblyLocation, typeof(MainFunction_1).FullName)
            {
                LargeImage = GetBitmapImage(Properties.Resources.area, 32, 32),
                LongDescription = "Программа для вычисления площади помещений\n" +
                                  "Она позволяет вычислять площади для всего объекта, вида или отдельной квартиры"
            });

            return Result.Succeeded;
        }


        BitmapImage GetBitmapImage(Bitmap bitmap, int width, int height)
        {
            // Создание нового битмапа с заданными размерами
            var resizedBitmap = new Bitmap(bitmap, new Size(width, height));

            using (var memory = new MemoryStream())
            {
                resizedBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        
    }
}
