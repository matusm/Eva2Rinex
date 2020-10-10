using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Eva2Rinex
{
    public class SensorMetaData
    {

        #region Properties
        public string ProgramName { get; set; }
        public string AgencyName { get; set; }
        public string StationName { get; set; }
        public string StationNumber { get; set; }
        public List<string> Comments { get; set; } = new List<string>();
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double PositionH { get; set; }
        #endregion

        #region Ctor
        public SensorMetaData(RinexType version)
        {
            ProgramName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            this.version = version;
            switch (version)
            {
                case RinexType.Unknown:
                case RinexType.Version3:
                case RinexType.Version2:
                case RinexType.Bipm:
                    metSensors = new MeteoSensorDescription[3];
                    break;
                case RinexType.Cctf:
                    metSensors = new MeteoSensorDescription[5];
                    break;
                default:
                    break;
            }
            SetBevSpecificMetaData(this.version);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds a comment line to the meta data. Must be called in advance to ToRinex().
        /// </summary>
        /// <param name="comment">The comment to be added.</param>
        public void AddComment(string comment)
        {
            Comments.Add(comment.Trim());
        }

        /// <summary>
        /// Generates the file header. AddComment() must be called in advance.
        /// </summary>
        /// <returns>A single string representing the file header.</returns>
        public string ToRinex()
        {
            if (metSensors.Length < 3)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            AddPositionToComments(version);
            sb.AppendLine(GenerateVersionHeaderInfo(version));
            sb.AppendLine(GenerateProgramVersionAndAgencyInfo(version));

            // marker
            switch (version)
            {
                case RinexType.Version3:
                case RinexType.Version2:
                    sb.AppendLine(RinexTools.Consolidate(StationName, 60) + "MARKER NAME");
                    if (!string.IsNullOrWhiteSpace(StationNumber))
                        sb.AppendLine(RinexTools.Consolidate(StationNumber, 60) + "MARKER NUMBER");
                    break;
                case RinexType.Bipm:
                case RinexType.Cctf:
                case RinexType.Unknown:
                    break;
            }

            foreach (var s in Comments)
                sb.AppendLine(RinexTools.Consolidate(s, 60) + "COMMENT");
            sb.AppendLine(GenerateLabNameInfo(version));

            // sensor info
            // sb.AppendLine("     3    TD    HR    PR                                    # / TYPES OF OBSERV");

            string typeOfObs = string.Format("{0,6}", metSensors.Length);
            foreach (var sensor in metSensors)
                typeOfObs += string.Format("  {0,4}", sensor.GetObservationType(version));
            sb.AppendLine(typeOfObs.PadRight(60, ' ') + "# / TYPES OF OBSERV");
            foreach (var sensor in metSensors)
                sb.AppendLine(sensor.ToRinex(version));

            // sensor position (not for BIPM / CCTF files)
            switch (version)
            {
                case RinexType.Version3:
                case RinexType.Version2:
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0,14:0.0000}{1,14:0.0000}{2,14:0.0000}{3,14:0.0000} PR SENSOR POS XYZ/H", PositionX, PositionY, PositionZ, PositionH));
                    break;
                case RinexType.Bipm:
                case RinexType.Cctf:
                case RinexType.Unknown:
                    break;
            }

            sb.AppendLine(new string(' ', 60) + "END OF HEADER");
            return sb.ToString();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// The BIPM and CCTF file format specification does not include sensor position. Since the height is important for barometric pressure measurements it is included as a comment.
        /// </summary>
        /// <param name="version">The Rinex version.</param>
        void AddPositionToComments(RinexType version)
        {
            if (version == RinexType.Bipm || version == RinexType.Cctf)
                AddComment(string.Format(CultureInfo.InvariantCulture.NumberFormat, "Height of PR sensor {0:F1} m", PositionH));
        }

        void SetBevSpecificMetaData(RinexType version)
        {
            AgencyName = "BEV";
            StationName = "BEV";
            StationNumber = "3";
            bipmStationCode = "BE1_";
            if (version == RinexType.Cctf)
            {
                // the concept of external versus internal sensors was introduced in CCTF
                metSensors[0] = new MeteoSensorDescription("TE", "KRONEIS EVA700", "SN:700.092-12590592", 0.1);
                metSensors[1] = new MeteoSensorDescription("HE", "KRONEIS EVA700", "SN:700.092-12590592", 2.0);
                metSensors[2] = new MeteoSensorDescription("PR", "KRONEIS EVA700", "SN:700.092-12590592", 0.3);
                metSensors[3] = new MeteoSensorDescription("TI", "VAISALA HMT331", "SN:S2220318", 0.1);
                metSensors[4] = new MeteoSensorDescription("HI", "VAISALA HMT331", "SN:S2220318", 1.5);
            }
            else
            {
                metSensors[0] = new MeteoSensorDescription("TD", "KRONEIS EVA700", "SN:700.092-12590592", 0.1);
                metSensors[1] = new MeteoSensorDescription("HR", "KRONEIS EVA700", "SN:700.092-12590592", 2.0);
                metSensors[2] = new MeteoSensorDescription("PR", "KRONEIS EVA700", "SN:700.092-12590592", 0.3);
            }
            // GP/TM.281 (PPP-derived antenna coordinates for use in P3 data, 21.08.2018)
            PositionX = 4087027.3000;
            PositionY = 1196557.4300;
            PositionZ = 4732637.1000;
            PositionH = 291.8;  // calculated  http://www.oc.nps.edu/oc2902w/coord/llhxyz.htm
        }
        
        string GenerateVersionHeaderInfo(RinexType version)
        {
            string returnString = "";
            switch (version)
            {
                case RinexType.Version3:
                    returnString = "     3.03           METEOROLOGICAL DATA                     RINEX VERSION / TYPE"; // A 22 of RINEX Version 3.03 Update 1 (Example)
                    break;
                case RinexType.Version2:
                    returnString = "     2.11           METEOROLOGICAL DATA                     RINEX VERSION / TYPE";
                    break;
                case RinexType.Bipm:
                    returnString = "METEOROLOGICAL DATA                                         DATA TYPE";
                    break;
                case RinexType.Cctf:
                    returnString = "METEOROLOGICAL DATA  CCTF V1.0                              DATA TYPE";
                    break;
                case RinexType.Unknown:
                    returnString = "< UNKNOWN TYPE >                                            DATA TYPE";
                    break;
            }
            return returnString;
        }

        string GenerateProgramVersionAndAgencyInfo(RinexType version)
        {
            // program, agency, date of generation
            string fileCreationDate = "";
            switch (version)
            {
                case RinexType.Version3:
                    fileCreationDate = DateTime.UtcNow.ToString("yyyyMMdd HHmmss") + " UTC";
                    break;
                case RinexType.Version2:
                case RinexType.Bipm:
                case RinexType.Cctf:
                    fileCreationDate = DateTime.UtcNow.ToString("dd-MMM-yy HH:mm", CultureInfo.InvariantCulture).ToUpper();
                    break;
                case RinexType.Unknown:
                    break;
            }

            return string.Format("{0}{1}{2}PGM / RUN BY / DATE",
                RinexTools.Consolidate(ProgramName),
                RinexTools.Consolidate(AgencyName),
                RinexTools.Consolidate(fileCreationDate)
                );
        }

        string GenerateLabNameInfo(RinexType version)
        {
            string returnString = "";
            switch (version)
            {
                case RinexType.Version3:
                case RinexType.Version2:
                    returnString = RinexTools.Consolidate(AgencyName, 60) + "LAB NAME";
                    break;
                case RinexType.Bipm:
                case RinexType.Cctf:
                    returnString = RinexTools.Consolidate(bipmStationCode, 60) + "LAB NAME";
                    break;
                case RinexType.Unknown:
                    returnString = RinexTools.Consolidate("< UNDEFINED >", 60) + "LAB NAME";
                    break;
            }
            return returnString;

        }

        #endregion

        #region Private fields
        RinexType version;
        string bipmStationCode;
        MeteoSensorDescription[] metSensors;
        #endregion

    }

    /*********************************************************************************/
    #region Private helper class
    class MeteoSensorDescription
    {
        const string unknownObservation = "??";

        string observationType;
        string model;
        string type;
        double accuracy;

        public MeteoSensorDescription(string observationType, string model, string type, double accuracy)
        {
            this.observationType = observationType.Trim().ToUpper();
            if (observationType.Length > 4) observationType = unknownObservation;
            this.model = RinexTools.Consolidate(model);
            this.type = RinexTools.Consolidate(type);
            this.accuracy = accuracy;
        }

        public string ToRinex(RinexType version)
        {
            switch (version)
            {
                case RinexType.Unknown:
                    return "";
                case RinexType.Version3:
                case RinexType.Version2:
                case RinexType.Bipm:
                    if (observationType.Length != 2) observationType = unknownObservation;
                    return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}      {2,7:0.0}    {3} SENSOR MOD/TYPE/ACC", model, type, accuracy, GetObservationType(version));
                case RinexType.Cctf:
                    if (observationType.Length > 4) observationType = unknownObservation;
                    return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}      {2,7:0.0}  {3} SENSOR MOD/TYPE/ACC", model, type, accuracy, GetObservationType(version));
            }
            return "";
        }

        public string GetObservationType(RinexType version)
        {
            switch (version)
            {
                case RinexType.Unknown:
                    return unknownObservation;
                case RinexType.Version3:
                case RinexType.Version2:
                case RinexType.Bipm:
                    if (observationType.Length != 2)
                        return unknownObservation;
                    return observationType;
                case RinexType.Cctf:
                    return RinexTools.Consolidate(observationType, 4);
            }
            return unknownObservation; // this can not happen
        }

    }
    #endregion
    /*********************************************************************************/

}
