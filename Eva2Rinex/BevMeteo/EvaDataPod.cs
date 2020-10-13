using System;
using System.Globalization;

namespace Eva2Rinex
{
    public class EvaDataPod : IComparable
    {
        CultureInfo provider = CultureInfo.InvariantCulture;
        const int ExpectedColumns = 12; // after update in August 2020, was 11 before
        const double nullData = 9999.9; // according to Meteo_format_CCTF-V1.0.pdf

        #region Ctor
        public EvaDataPod(string dataLine)
        {
            IsValid = ParseDataLine(dataLine);
        }
        #endregion

        #region Properties
        public bool IsValid { get; }
        public DateTime TimeStamp { get; private set; }
        public double Temperature1 => temperature1;
        public double Temperature2 => temperature2;
        public double AbsolutePressure => absolutePressure;
        public double RelativeHumidity => relativeHumidity;
        public double AbsoluteHumidity => absoluteHumidity;
        public double Dewpoint => dewpoint;
        public double MixingRatio => mixingRatio;
        public double Frostpoint => frostpoint;
        public double AirFlow => airFlow;
        public double FanPower => fanPower;
        #endregion

        #region Private methods

        private bool ParseDataLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            char[] charSeparators = { ';' }; // fields are separated by semicolons
            string[] tokens = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != ExpectedColumns) return false;
            if (!ParseEvaDate(tokens[0], tokens[1])) return false;
            if (!ParseNumericValue(tokens[2], out temperature1)) return false;
            if (!ParseNumericValue(tokens[3], out temperature2)) return false;
            if (!ParseNumericValue(tokens[4], out absolutePressure)) return false;
            if (!ParseNumericValue(tokens[5], out relativeHumidity)) return false;
            if (!ParseNumericValue(tokens[6], out absoluteHumidity)) return false;
            if (!ParseNumericValue(tokens[7], out dewpoint)) return false;
            if (!ParseNumericValue(tokens[8], out mixingRatio)) return false; // this parameter was introduced in August 2020
            if (!ParseNumericValue(tokens[9], out frostpoint)) return false;
            if (!ParseNumericValue(tokens[10], out airFlow)) return false;
            if (!ParseNumericValue(tokens[11], out fanPower)) return false;
            // sometimes a courious date is recorded. We check if it the time stamp is in the future
            if (TimeStamp > DateTime.UtcNow) return false;
            if (TimeStamp < new DateTime(2020)) return false;
            return true;
        }

        private bool ParseNumericValue(string str, out double value)
        {
            value = nullData;
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

        public int CompareTo(object obj)
        {
            EvaDataPod edp = obj as EvaDataPod;
            return TimeStamp.CompareTo(edp.TimeStamp);
        }

        public override string ToString()
        {
            if (IsValid)
                return string.Format("{0} {1} {2} {3}", TimeStamp, temperature1, relativeHumidity,  absolutePressure);
            else
                return "< invalid text line >";
        }

        #region Private fields
        double temperature1;        // the air temperature (analog) in °C
        double temperature2;        // the air temperature (digital) in °C
        double absolutePressure;    // the barometric pressure in hPa
        double relativeHumidity;    // the relative humidity in %
        double absoluteHumidity;    // the absolute humidity in g/m³
        double mixingRatio;         // in g/kg
        double dewpoint;            // in °C
        double frostpoint;          // in °C
        double airFlow;             // in %
        double fanPower;            // in %
        #endregion

    }
}
