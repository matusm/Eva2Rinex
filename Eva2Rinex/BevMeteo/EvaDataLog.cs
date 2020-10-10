using System;
using System.Collections.Generic;

namespace Eva2Rinex
{
    public class EvaDataLog
    {

        #region Properties
        public string Title { get; }
        public int NumberOfEntries => dataLog.Count;
        #endregion

        #region Ctor
        public EvaDataLog(string title)
        {
            Title = title.Trim();
            isSorted = false;
        }
        #endregion

        #region Public methods
        public void NewEntry(EvaDataPod pod)
        {
            if (pod.IsValid)
            {
                dataLog.Add(pod);
                isSorted = false;
            }
        }

        public void NewEntry(string dataLine)
        {
            NewEntry(new EvaDataPod(dataLine));
        }


        /// <summary>
        /// Returns the sorted list of all measurement samples between the start and end time.
        /// </summary>
        /// <param name="fromDate">The start time.</param>
        /// <param name="tillDate">The end time.</param>
        /// <returns>Sorted list of all measurement samples between the start and end time, or <c>null</c>.</returns>
        public List<EvaDataPod> GetData(DateTime fromDate, DateTime tillDate)
        {
            SortData();
            return dataLog.FindAll(
                delegate (EvaDataPod edp)
                {
                    return (edp.TimeStamp >= fromDate && edp.TimeStamp <= tillDate);
                }
                );
        }

        /// <summary>
        /// Returns the sorted list of all measurement samples for a specific day.
        /// </summary>
        /// <param name="forDate">The date.</param>
        /// <returns>Sorted list of all measurement samples on a specific day, or <c>null</c>.</returns>
        public List<EvaDataPod> GetData(DateTime forDate)
        {
            SortData();
            return dataLog.FindAll(
                delegate (EvaDataPod edp)
                {
                    return (edp.TimeStamp.Date == forDate.Date);
                }
                );
        }

        /// <summary>
        /// Returns the sorted list of all measurement samples.
        /// </summary>
        /// <returns>Sorted list of all measurement samples.</returns>
        public List<EvaDataPod> GetData()
        {
            SortData();
            return dataLog;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Sorts the internal collection of <c>dataLog</c> objects according to the time stamp.
        /// But only if list is not already sorted.
        /// </summary>
        void SortData()
        {
            if (dataLog.Count <= 1) return;  // if collection is too short, return
            if (isSorted) return;   // if collection is already sorted, return
            dataLog.Sort();
            isSorted = true;
        }
        #endregion

        #region Private fields
        private List<EvaDataPod> dataLog = new List<EvaDataPod>();
        private bool isSorted;
        #endregion
    }
}
