using System;
using System.Globalization;

namespace Eva2Rinex
{
    /// <summary>
    /// A container class to hold a single air parameter measurement result. Two objects of this class can be compared.
    /// </summary>
    /// <remarks>
    /// The values can be accessed by properties (pure geters) and are set by the constructor only.
    /// Missing values should be represented by <c>double.NaN</c>
    /// Stored values are TD HR PR and output is in exactly that order!
    /// </remarks>
    public class SensorDataPod : IComparable
    {

        #region Properties
        public DateTime TimeStamp { get; }
        public SensorBouquet Bouquet => GetBouquet();
        public double AirTemperature { get; }
        public double RelativeHumidity { get; }
        public double AirPressure { get; }
        public double InternalTemperature
        {
            get
            {
                if (internalTemperature.HasValue)
                    return internalTemperature.Value;
                else
                    return 9999.9;
            }
        }
        public double InternalHumidity
        {
            get
            {
                if (internalHumidity.HasValue)
                    return internalHumidity.Value;
                else
                    return 9999.9;
            }
        }
        #endregion

        #region Ctor

        /// <summary>
        /// Instantiate a <c>SensorDataPod</c> object containing all air parameter measurement results. For external sensor only.
        /// </summary>
        /// <param name="timeStamp">The time of the measurement.</param>
        /// <param name="airTemperature">Value of the air temperature in °C.</param>
        /// <param name="relativeHumidity">Value of the relative air humidity in %.</param>
        /// <param name="airPressure">Value of the air pressure in hPa.</param>
        public SensorDataPod(DateTime timeStamp, double airTemperature, double relativeHumidity, double airPressure)
        {
            TimeStamp = timeStamp;
            AirTemperature = airTemperature;
            RelativeHumidity = relativeHumidity;
            AirPressure = airPressure;
            internalTemperature = null;
            internalHumidity = null;
        }

        /// <summary>
        ///  Instantiate a <c>SensorDataPod</c> object containing all air parameter measurement results. For external and internal sensors.
        /// </summary>
        /// <param name="timeStamp">The time of the measurement.</param>
        /// <param name="airTemperature">Value of the air temperature in °C.</param>
        /// <param name="relativeHumidity">Value of the relative air humidity in %.</param>
        /// <param name="airPressure">Value of the air pressure in hPa.</param>
        /// <param name="internalTemperature">Value of the lab air temperature in °C.</param>
        /// <param name="internalHumidity">Value of the lab relative air humidity in %.</param>
        public SensorDataPod(DateTime timeStamp, double airTemperature, double relativeHumidity, double airPressure, double internalTemperature, double internalHumidity)
        {
            TimeStamp = timeStamp;
            AirTemperature = airTemperature;
            RelativeHumidity = relativeHumidity;
            AirPressure = airPressure;
            this.internalTemperature = internalTemperature;
            this.internalHumidity = internalHumidity;
        }

        #endregion

        #region Public methods

        public string ToOpenSenseMapJsonString()
        {
            string tempSensorId = "5ced2cda30705e001ad988ca";
            string humSensorId = "5cee5dd730705e001a2eb59b";
            string baroSensorId = "5cee5dd730705e001a2eb59c";
            string dateString = TimeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string tempString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.00}", AirTemperature);
            string humString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.00}", RelativeHumidity);
            string baroString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.00}", AirPressure);
            string jsonString = "";
            jsonString += "{\"sensor\":\"" + tempSensorId + "\",";
            jsonString += "\"value\":\"" + tempString + "\",";
            jsonString += "\"createdAt\":\"" + dateString + "\"},";
            jsonString += "{\"sensor\":\"" + humSensorId + "\",";
            jsonString += "\"value\":\"" + humString + "\",";
            jsonString += "\"createdAt\":\"" + dateString + "\"},";
            jsonString += "{\"sensor\":\"" + baroSensorId + "\",";
            jsonString += "\"value\":\"" + baroString + "\",";
            jsonString += "\"createdAt\":\"" + dateString + "\"}";
            return jsonString;
        }

        /// <summary>
        /// Formats the object as a data record for meterological data file according to RINEX.
        /// </summary>
        /// <returns>A string in RINEX format.</returns>
        /// <remarks>The order is fixed to TD HR PR !</remarks>
        public string ToRinexString()
        {
            //return string.Format("{0}{1,7.1}{2,7.1}{3,7.1}", //CultureInfo.InvariantCulture.NumberFormat,
            string dateString = TimeStamp.ToString("yy MM dd HH mm ss");
            dateString = dateString.Replace(" 0", "  ");
            switch (Bouquet)
            {
                case SensorBouquet.Unknown:
                    return "";
                case SensorBouquet.ExternalOnly:
                    return string.Format(CultureInfo.InvariantCulture.NumberFormat, " {0}{1,7:0.0}{2,7:0.0}{3,7:0.0}",
                                        dateString,
                                        AirTemperature,
                                        RelativeHumidity,
                                        AirPressure);
                case SensorBouquet.ExternalAndInternal:
                    return string.Format(CultureInfo.InvariantCulture.NumberFormat, " {0}{1,7:0.0}{2,7:0.0}{3,7:0.0}{4,7:0.0}{5,7:0.0}",
                                        dateString,
                                        AirTemperature,
                                        RelativeHumidity,
                                        AirPressure,
                                        InternalTemperature,
                                        InternalHumidity
                                        );
            }
            return "";
        }

        /// <summary>
        /// Compares the current instance with another object of the same kind.
        /// </summary>
        /// <param name="obj"<>An <c>SensorDataPod</c> object to compare with this instance./param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(object obj)
        {
            SensorDataPod sdp = obj as SensorDataPod;
            return TimeStamp.CompareTo(sdp.TimeStamp);
        }

        /// <summary>
        /// Returns a string that represents the state of the current object.
        /// </summary>
        /// <returns>A string representing the current object.</returns>
        public override string ToString()
        {
            return string.Format($"[SensorDataPod: TimeStamp={TimeStamp}, AirTemperature={AirTemperature}, RelativeHumidity={RelativeHumidity}, AirPressure={AirPressure}]");
        }

        #endregion

        #region Private methods
        SensorBouquet GetBouquet()
        {
            if (internalHumidity.HasValue && internalTemperature.HasValue)
                return SensorBouquet.ExternalAndInternal;
            else
                return SensorBouquet.ExternalOnly;
        }
        #endregion

        #region Private fields
        // the only way to set these values is via the constructors.
        double? internalTemperature;    // lab temperature in °C
        double? internalHumidity;       // lab relative humidity in %
        #endregion

    }
}
