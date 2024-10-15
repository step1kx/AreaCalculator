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
        public TipWindow()
        {
            InitializeComponent();
            
        }

        private void CopyFormulaButton_Click(object sender, RoutedEventArgs e)
        {
            // Копируем текст формулы в буфер обмена
            Clipboard.SetText(FormulaTextBox.Text);
            MessageBox.Show("Формула скопирована в буфер обмена!");
        }



    }
}
