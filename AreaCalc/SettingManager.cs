using System.Windows.Controls;

namespace AreaCalc
{
    public static class SettingsManager
    {
        public static string LivingFormula
        {
            get => Properties.Resources.LivingFormula;
            set
            {
                Properties.Settings.Default.LivingFormula = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string UsualFormula
        {
            get => Properties.Settings.Default.UsualFormula;
            set
            {
                Properties.Settings.Default.UsualFormula = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string TotalFormula
        {
            get => Properties.Settings.Default.TotalFormula;
            set
            {
                Properties.Settings.Default.TotalFormula = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
