using AreaCalc.Interfaces;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace AreaCalc.Models
{
    public class AreaCalculator : IAreaCalculator
    {

        private double RoundToThreeAndThenTwo(double value)
        {
            double roundedToThree = Math.Round(value, 2);
            return roundedToThree;
        }

        public double CalculateFormula(string formula, Dictionary<string, double> roomAreas)
        {
            
            //if (!Regex.IsMatch(formula, @"^[0-9A-Za-zА-Яа-яё\.\+\-\*/\(\)\s]*$"))
            //{
            //    throw new Exception("Формула пуста или содержит недопустимые символы.");
            //}

            string processedFormula = formula;

            foreach (var entry in roomAreas)
            {
                string pattern = $@"\b{Regex.Escape(entry.Key)}\b";
                processedFormula = Regex.Replace(processedFormula, pattern, entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
            }

            processedFormula = Regex.Replace(processedFormula, @"\bТип\d+\b", "0");

            DataTable table = new DataTable();
            var result = table.Compute(processedFormula, null);
            double resultValue = Convert.ToDouble(result);
            return RoundToThreeAndThenTwo(resultValue);
        }
        public Dictionary<string, double> CalculateRoomAreas(List<Room> rooms)
        {
            return rooms
                .GroupBy(r => "Тип" + (r.LookupParameter("КГ.Тип помещения")?.AsInteger() ?? 0))
                .ToDictionary(
                    g => g.Key,
                    g => RoundToThreeAndThenTwo(
                        g.Sum(room =>
                        {
                            var param = room.LookupParameter("Скрипт.Площадь помещения");
                            return param != null && param.HasValue ? param.AsDouble() : 0;
                        })
                    )
                );
        }


        public (double livingArea, double usualArea, double totalArea) CalculateAreas(List<Room> rooms, string livingFormula, string usualFormula, string totalFormula)
        {

            double livingArea = 0;
            double usualArea = 0;
            double totalArea = 0;
            var roomAreas = CalculateRoomAreas(rooms);

            if (!string.IsNullOrEmpty(livingFormula))
                livingArea = CalculateFormula(livingFormula, roomAreas);


            if (!string.IsNullOrEmpty(usualFormula))
                usualArea = CalculateFormula(usualFormula, roomAreas);


            if (!string.IsNullOrEmpty(totalFormula))
            {
                totalArea = CalculateFormula(totalFormula, roomAreas);
            }

            return (livingArea, usualArea, totalArea);
        }
    }
}
