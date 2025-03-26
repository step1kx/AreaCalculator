using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AreaCalc.Interfaces
{
    public interface ISettingsManager
    {
        void LoadSettings(TextBox livingFormulaTextBox, TextBox usualFormulaTextBox, TextBox totalFormulaTextBox);
        void SaveSettings(TextBox livingFormulaTextBox, TextBox usualFormulaTextBox, TextBox totalFormulaTextBox);
    }
}
