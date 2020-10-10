using System;
using System.Collections.Generic;
using System.Text;

namespace Eva2Rinex
{
    /// <summary>
    ///  A container class to hold a list of air parameter measurement result.
    ///  Can output data in Rinex format also.
    /// </summary>
    public class SensorDataLog
    {
        #region Properties
        public string Title { get; }
        public int NumberOfEntries => sensorDataList.Count;
        #endregion

        #region Ctor
        public SensorDataLog(string title)
        {
            Title = title.Trim(); // this sets the title for the collection
            isSorted = false;     // the (still empty) list is marked as unsorted.
        }
        #endregion

        #region Public methods
        public void NewEntry(DateTime timeStamp, double airTemperature, double relativeHumidity, double airPressure)
        {
            sensorDataList.Add(new SensorDataPod(timeStamp, airTemperature, relativeHumidity, airPressure));
            isSorted = false;   // the expanded list is marked as unsorted.
        }

        public void NewEntry(DateTime timeStamp, double airTemperature, double relativeHumidity, double airPressure, double internalTemperature, double internalHumidity)
        {
            sensorDataList.Add(new SensorDataPod(timeStamp, airTemperature, relativeHumidity, airPressure, internalTemperature, internalHumidity));
            isSorted = false;   // the expanded list is marked as unsorted.
        }

        public List<SensorDataPod> GetData()
        {
            SortData(); 
            return sensorDataList;
        }

        public List<SensorDataPod> GetData(DateTime fromDate, DateTime tillDate)
        {
            SortData();
            return sensorDataList.FindAll(
                delegate (SensorDataPod sdp)
                {
                    return (sdp.TimeStamp >= fromDate && sdp.TimeStamp <= tillDate);
                }
                );
        }

        public List<SensorDataPod> GetData(DateTime forDate)
        {
            SortData();
            return sensorDataList.FindAll(
                delegate (SensorDataPod sdp)
                {
                    return (sdp.TimeStamp.Date == forDate.Date);
                }
                );
        }

        public string ToRinex()
        {
            SortData();
            return ToRinex(sensorDataList);
        }

        public string ToOpenSenseMapJson()
        {
            SortData();
            return ToOpenSenseMapJson(sensorDataList);
        }

        public string ToRinex(DateTime fromDate, DateTime tillDate)
        {
            return ToRinex(GetData(fromDate, tillDate));
        }

        public string ToRinex(DateTime forDate)
        {
            return ToRinex(GetData(forDate));
        }

        public override string ToString()
        {
            return string.Format("[SensorDataLog: \"{0}\", entries={1}]", Title, NumberOfEntries);
        }

        #endregion

        #region Private methods
        private void SortData()
        {
            if (sensorDataList.Count <= 1) return;  // if collection is too short, return
            if (isSorted) return;   // if collection is already sorted, return
            sensorDataList.Sort();
            isSorted = true;
        }

        private string ToRinex(List<SensorDataPod> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SensorDataPod d in list)
                sb.AppendLine(d.ToRinexString());
            return sb.ToString();
        }

        private string ToOpenSenseMapJson(List<SensorDataPod> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (SensorDataPod d in list)
            {
                sb.Append(d.ToOpenSenseMapJsonString());
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            return sb.ToString();
        }
        #endregion

        #region Private fields
        private List<SensorDataPod> sensorDataList = new List<SensorDataPod>();
        private bool isSorted;
        #endregion
    }
}
