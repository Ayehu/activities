using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Data;
using System.IO;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public int IsNowSelected;
        public string FirstDate;
        public string SecondDate;
        public string ReturnFormat;
        public string FirstDateFormat;
        public string SecondDateFormat;

        public ICustomActivityResult Execute()

        {
            DateTime firstDate, secondDate;
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            //
            // First date
            if (IsNowSelected == 1)
            {
                firstDate = DateTime.Now;
            }
            else
            {
                if (DateTime.TryParseExact(FirstDate, FirstDateFormat, culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out firstDate) == true)
                {
                    // Converted 
                }
                else
                {
                    // Could not convert
                    throw new ApplicationException("First date format is invalid");
                }
            }


            // 
            // Second date
            if (DateTime.TryParseExact(SecondDate, SecondDateFormat, culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out secondDate) == false)
            {
                // Could not convert -> give it a second chance
                if (DateTime.TryParse(SecondDate, culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out secondDate) == false)
                {
                    // Still cold not convert
                    throw new ApplicationException("Second date format is invalid");
                }
                else
                {
                    // Converted
                }
            }
            else
            {
                // Converted 
            }


            var dateDiff = firstDate.Subtract(secondDate);
            double result = 0;

            switch (ReturnFormat)
            {
                case "Years":
                    result = Math.Round(dateDiff.TotalDays / 365, 2);
                    break;

                case "Months":
                    result = Math.Round(dateDiff.TotalDays / 31, 2);
                    break;

                case "Days":
                    result = dateDiff.TotalDays;
                    break;

                case "Hours":
                    result = dateDiff.TotalHours;
                    break;

                case "Minutes":
                    result = dateDiff.TotalMinutes;
                    break;

                case "Seconds":
                    result = dateDiff.TotalSeconds;
                    break;

                default:
                    throw new ApplicationException("Parameter 'return type' is invalid");
                    break;
            }

            DataTable dataTable = new DataTable("resultSet");
            dataTable.Columns.Add("Result", typeof(string));
            dataTable.Rows.Add(result);


            return this.GenerateActivityResult(dataTable);

        }
    }
}
