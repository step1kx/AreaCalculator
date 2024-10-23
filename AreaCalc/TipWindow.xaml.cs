using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AreaCalc
{
    /// <summary>
    /// Логика взаимодействия для TipWindow.xaml
    /// </summary>
    public partial class TipWindow : Window
        
    {
        private Dictionary<string, Dictionary<string, double>> _apartments;

        private Dictionary<string, List<Room>> _apartmentsData;

        public TipWindow(Dictionary<string, List<Room>> apartmentsData)
        {
            InitializeComponent();
            _apartmentsData = apartmentsData;
            PopulateApartmentInfoList();
        }

        // Метод для заполнения ListBox информацией о квартирах и помещениях
        private void PopulateApartmentInfoList()
        {
            foreach (var apartment in _apartmentsData)
            {
                // Добавляем номер квартиры
                ApartmentInfoListBox.Items.Add($"Квартира {apartment.Key}:");

                // Обрабатываем каждую комнату
                foreach (var room in apartment.Value)
                {
                    Parameter roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                    Parameter areaParam = room.LookupParameter("Площадь");
                    Parameter roomFilterParam = room.LookupParameter("КГ.Фильтр");

                    int roomTypeId = roomTypeParam.AsInteger();

                    // Получаем значения параметров
                    double roomArea = areaParam != null ? areaParam.AsDouble() : 0;

                    string roomFilter = roomFilterParam.AsString();

                    // Добавляем информацию о помещении
                    ApartmentInfoListBox.Items.Add($"  Помещение: {room.Name}, Тип: {roomTypeId} Площадь: {roomArea} м² Фильтр: {roomFilter}");
                }

                ApartmentInfoListBox.Items.Add(""); // Пустая строка для разделения квартир
            }
        }

        private void CopyFormulaButton_Click(object sender, RoutedEventArgs e)
        {
            // Копируем текст формулы в буфер обмена
            //Clipboard.SetText(FormulaTextBox.Text);
            MessageBox.Show("Формула скопирована в буфер обмена!");
        }



    }
}
