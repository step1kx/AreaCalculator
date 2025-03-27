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
        private Dictionary<string, List<Room>> _apartmentsData;

        public TipWindow(Dictionary<string, List<Room>> apartmentsData)
        {
            InitializeComponent();
            _apartmentsData = apartmentsData;
            PopulateApartmentInfoList();
        }

        private void PopulateApartmentInfoList()
        {
            var roomTypes = new List<(int TypeId, string RoomName)>();
            var uniqueRoomNames = new HashSet<string>(); // Для отслеживания уникальных названий

            foreach (var rooms in _apartmentsData.Values)
            {
                foreach (var room in rooms)
                {
                    var roomTypeParam = room.LookupParameter("КГ.Тип помещения");
                    var roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();

                    if (roomTypeParam != null && roomName != null)
                    {
                        int roomTypeId = roomTypeParam.AsInteger();

                        // Добавляем только типы с значением > 0
                        if (roomTypeId > 0)
                        {
                            // Проверяем, есть ли уже такое название
                            if (uniqueRoomNames.Add(roomName)) // Если добавление прошло успешно, значит названия не было
                            {
                                roomTypes.Add((roomTypeId, roomName));
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                }
            }

            // Добавляем все найденные типы и названия в ListBox
            foreach (var roomType in roomTypes)
            {
                ApartmentInfoListBox.Items.Add($"Тип помещения: {roomType.TypeId} Название: {roomType.RoomName} Значение КГ.Жилые ко");
            }
        }

        // Метод для перемещения окна
        private void MovingWin(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Обработчик для кнопки отмены
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
