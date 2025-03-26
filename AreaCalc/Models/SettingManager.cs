using AreaCalc.Interfaces;
using System.Windows.Controls;

namespace AreaCalc
{
    public class SettingsManager : ISettingsManager
    {
        public void LoadSettings(TextBox livingFormulaTextBox, TextBox usualFormulaTextBox, TextBox totalFormulaTextBox)
        {
            SetPlaceholder(livingFormulaTextBox, Properties.Settings.Default.LivingFormula, "Пример: Тип11*0.85");
            SetPlaceholder(usualFormulaTextBox, Properties.Settings.Default.UsualFormula, "Пример: Тип11*0.85 + Тип2");
            SetPlaceholder(totalFormulaTextBox, Properties.Settings.Default.TotalFormula, "Пример: Тип11*0.85 + Тип2 + Тип3*0.3");
        }

        public void SaveSettings(TextBox livingFormulaTextBox, TextBox usualFormulaTextBox, TextBox totalFormulaTextBox)
        {
            Properties.Settings.Default.LivingFormula = (livingFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : livingFormulaTextBox.Text;
            Properties.Settings.Default.UsualFormula = (usualFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : usualFormulaTextBox.Text;
            Properties.Settings.Default.TotalFormula = (totalFormulaTextBox.Foreground == System.Windows.Media.Brushes.Gray)
                ? "" : totalFormulaTextBox.Text;

            Properties.Settings.Default.Save();
        }

        private void SetPlaceholder(TextBox textBox, string setting, string placeholder)
        {
            if (string.IsNullOrEmpty(setting))
            {
                textBox.Text = placeholder;
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
            else
            {
                textBox.Text = setting;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }

            textBox.GotFocus += (sender, e) =>
            {
                if (textBox.Foreground == System.Windows.Media.Brushes.Gray)
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            };

            textBox.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    if (textBox.Name == "livingFormulaTextBox")
                        textBox.Text = "Пример: Тип11*0.85";
                    else if (textBox.Name == "usualFormulaTextBox")
                        textBox.Text = "Пример: Тип11*0.85 + Тип2";
                    else if (textBox.Name == "totalFormulaTextBox")
                        textBox.Text = "Пример: Тип11*0.85 + Тип2 + Тип3*0.3";

                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };
        }
    }
}
