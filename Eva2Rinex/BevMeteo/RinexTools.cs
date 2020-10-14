using System;

namespace Eva2Rinex
{
    public static class RinexTools
    {
        /// <summary>
        /// this is the representation of missing data according to Meteo_format_CCTF-V1.0.pdf
        /// a more logical choice would be double.NaN
        /// </summary>
        public static double NullData => 9999.9;

        /// <summary>
        /// Formats a given string to a string of given length, padding it from the right with spaces.
        /// </summary>
        /// <remarks>According to RINEX format specifications, width is 20, 40 or 60.</remarks>
        /// <param name="str">The string to be formated</param>
        /// <param name="width">The width of the formated string.</param>
        /// <returns>A string of length width.</returns>
        public static string Consolidate(string str, int width)
        {
            string result = str.Trim();
            if (result.Length >= width)
                result = result.Substring(0, width - 1); // to improve readibility, truncate the string one character more
            return result.PadRight(width, ' '); 
        }

        /// <summary>
        /// Formats a given string to a string of length 20, padding it from the right with spaces.
        /// </summary>
        /// <param name="str">The string to be formated</param>
        /// <returns>A string of length width.</returns>
        public static string Consolidate(string str) { return Consolidate(str, 20); }

        /// <summary>
        /// Calculates the MJD from a give date.
        /// </summary>
        /// <param name="date">A date value.</param>
        /// <returns>The modified Julian date as a double value.</returns>
        public static double ModifiedJulianDate(DateTime date)
        {
            return date.ToOADate() + 15018.0;
        }

        /// <summary>
        /// Returns the integer part of the MJD as a string.
        /// </summary>
        /// <param name="date">A date value.</param>
        /// <returns>The string of the integer part.</returns>
        public static string MjdString(DateTime date)
        {
            double mjd = ModifiedJulianDate(date);
            return string.Format("{0:0.}", mjd);
        }

        /// <summary>
        /// Generates the file name for a given date according to the RINEX documentation
        /// </summary>
        /// <param name="date">The date (a day).</param>
        /// <param name="type">Rinex type.</param>
        /// <returns>The file name.</returns>
        public static string RinexFileName(DateTime date, RinexType type )
        {
            string mjd = MjdString(date);
            string mj = mjd.Substring(0,2);
            string day = mjd.Substring(mjd.Length-3,3);
            string ddd = date.DayOfYear.ToString("000");
            string yy = date.ToString("yy");
            string fileName = "";

            switch (type)
            {
                case RinexType.Unknown:
                    fileName = "";
                    break;
                case RinexType.Version3:
                    fileName = "BEV0" + ddd + "0." + yy + "M"; // ver. 3.00, modified in 3.03!
                    break;
                case RinexType.Version2:
                    fileName = "BEV0" + ddd + "0." + yy + "M"; // ver. 2.11
                    break;
                case RinexType.Bipm:
                    fileName = "BEmet_" + mj + "." + day;
                    break;
                case RinexType.Cctf:
                    fileName = "metBE" + mj + "." + day;
                    break;

            }
            return fileName;
        }
    }

    /// <summary>
    /// Enumerates the different RINEX versions covered by this application.
    /// </summary>
    public enum RinexType
    {
        Unknown,
        Version3,
        Version2,
        Bipm,
        Cctf
    }

    public enum SensorBouquet
    {
        Unknown,
        ExternalOnly,
        ExternalAndInternal
    }

}
