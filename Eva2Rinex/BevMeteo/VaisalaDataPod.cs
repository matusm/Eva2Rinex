using System;
using System.Globalization;

namespace Eva2Rinex.BevMeteo
{
    public class VaisalaDataPod
    {

        CultureInfo provider = CultureInfo.InvariantCulture;
        const int ExpectedColumns = 4;
        const double dateMatchToleranceInSeconds = 120.0;

        #region Properties
        public bool IsValid { get; }
        public DateTime TimeStamp { get; private set; }
        public double Temperature => temperature;
        public double RelativeHumidity => relativeHumidity;
        #endregion

        #region Ctor
        public VaisalaDataPod(string dataLine)
        {
            IsValid = ParseDataLine(dataLine);
        }
        #endregion

        #region Public methods
        public bool MatchesDate(DateTime date)
        {
            if (!IsValid) return false;
            TimeSpan ts = TimeStamp - date;
            if (Math.Abs(ts.TotalSeconds) < dateMatchToleranceInSeconds) return true;
            return false;
        }
        #endregion

        #region Private Stuff

        private double temperature;
        private double relativeHumidity;

        private bool ParseDataLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            char[] charSeparators = { ';' }; // fields are separated by semicolons
            string[] tokens = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != ExpectedColumns) return false;
            if (!ParseEvaDate(tokens[0], tokens[1])) return false;
            if (!ParseNumericValue(tokens[2], out temperature)) return false;
            if (!ParseNumericValue(tokens[3], out relativeHumidity)) return false;
            // sometimes a courious date is recorded. We check if it the time stamp is in the future
            if (TimeStamp > DateTime.UtcNow) return false;
            if (TimeStamp < new DateTime(2020)) return false;
            return true;
        }

        private bool ParseNumericValue(string str, out double value)
        {
            value = double.NaN;
            try
            {
                value = double.Parse(str, provider);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ParseEvaDate(string dateString, string timeString)
        {
            string temporary = RemoveQuotationmarks(dateString) + " " + RemoveQuotationmarks(timeString);
            try
            {
                TimeStamp = DateTime.ParseExact(temporary, "yyyy-MM-dd HH:mm:ss", provider); // 2018-07-29 13:15:00
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private string RemoveQuotationmarks(string str)
        {
            return str.Replace("\"", string.Empty);
        }

        #endregion

    }
}
