using Eva2Rinex.Properties;
using Eva2Rinex.BevMeteo;
using System;
using System.Globalization;
using System.IO;

namespace Eva2Rinex
{
    class Eva2Rinex
    {
        static void Main(string[] args)
        {

            // check command line parameters on provided filename 
            if (args.Length < 1)
                ConsoleUI.ErrorExit("No filename given", 1);

            // check settings file
            Settings settings = new Settings();
            if (settings.Verbatim)
                ConsoleUI.BeVerbatim();
            else
                ConsoleUI.BeSilent();

            // query settings on Rinex output file type
            // actually RinexType.Cctf is the current standard
            RinexType rinexType = RinexType.Unknown;
            string rinexTypeFromSettings = settings.RinexType.ToUpper().Trim();
            if (rinexTypeFromSettings == "BIPM") rinexType = RinexType.Bipm;
            if (rinexTypeFromSettings == "CCTF") rinexType = RinexType.Cctf;
            if (rinexTypeFromSettings == "VERSION2") rinexType = RinexType.Version2;
            if (rinexTypeFromSettings == "VERSION3") rinexType = RinexType.Version3;

            #region File name handling
            string baseFileName = Path.GetFileNameWithoutExtension(args[0]);
            string evaInputFileName = Path.ChangeExtension(baseFileName, ".TXT");
            string vaisalaInputFileName = Path.ChangeExtension($"Vaisala_Data_{baseFileName}", ".TXT");
            string evaInputPath = Path.Combine(settings.InputDirectory, evaInputFileName);
            string vaisalaInputPath = Path.Combine(settings.VaisalaInputDirectory, vaisalaInputFileName);
            DateTime dateToProcess = DateTime.UtcNow; // just to initialize the type
            // estimate the date (the day) from the base file name and construct the output file name 
            try
            {
                dateToProcess = DateTime.ParseExact(baseFileName, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                ConsoleUI.ErrorExit("Filename syntax invalid", 2);
            }
            string rinexOutputFileName = RinexTools.RinexFileName(dateToProcess, rinexType);
            string rinexOutputPath = Path.Combine(settings.OutputDirectory, rinexOutputFileName);
            #endregion

            // load outdoor data file
            EvaDataLog evaDataLog = new EvaDataLog($"file: {evaInputPath}");
            ConsoleUI.ReadingFile(evaInputFileName);
            using (StreamReader hFile = File.OpenText(evaInputPath))
            {
                string line;
                while ((line = hFile.ReadLine()) != null)
                {
                    evaDataLog.NewEntry(line);
                }
                hFile.Close();
            }
            ConsoleUI.Done();

            // load indoor data file (if CCTF mode)
            VaisalaDataLog vaisalaDataLog = new VaisalaDataLog($"file: {vaisalaInputPath}");
            if (rinexType == RinexType.Cctf)
            {
                ConsoleUI.ReadingFile(vaisalaInputPath);
                using (StreamReader hFile = File.OpenText(vaisalaInputPath))
                {
                    string line;
                    while ((line = hFile.ReadLine()) != null)
                    {
                        vaisalaDataLog.NewEntry(line);
                    }
                    hFile.Close();
                }
                ConsoleUI.Done();
            }
            
            // extract relevant data in a new object
            SensorDataLog sensorDataLog = new SensorDataLog($"outdoor: {evaDataLog.Title}, indoor: {vaisalaDataLog.Title}");
            foreach (var edp in evaDataLog.GetData())
            {
                if (rinexType == RinexType.Cctf)
                {
                    VaisalaDataPod vdp = vaisalaDataLog.GetPodForDate(edp.TimeStamp);
                    sensorDataLog.NewEntry(edp.TimeStamp, edp.Temperature1, edp.RelativeHumidity, edp.AbsolutePressure, vdp.Temperature, vdp.RelativeHumidity);
                }
                else
                {
                    sensorDataLog.NewEntry(edp.TimeStamp, edp.Temperature1, edp.RelativeHumidity, edp.AbsolutePressure);
                }
                
            }

            // prepare meta data
            SensorMetaData sensorMetaData = new SensorMetaData(rinexType);
            sensorMetaData.ProgramName = ConsoleUI.Title + " V" + ConsoleUI.Version;
            sensorMetaData.AddComment("External sensor located close to GNSS antenna");
            sensorMetaData.AddComment($"Input file name: {evaInputFileName}");
            sensorMetaData.AddComment($"Internal (indoor) sensor data file: {vaisalaInputFileName}");

            // finaly write the output file
            ConsoleUI.WritingFile(rinexOutputFileName);
            using (StreamWriter outFile = new StreamWriter(rinexOutputPath))
            {
                outFile.Write(sensorMetaData.ToRinex());
                outFile.Write(sensorDataLog.ToRinex(dateToProcess));
                outFile.Close();
            }
            ConsoleUI.Done();


        }
    }
}
