using System;
using System.Collections.Generic;

namespace Eva2Rinex.BevMeteo
{
    public class VaisalaDataLog
    {
        #region Properties
        public string Title { get; }
        public int NumberOfEntries => dataLog.Count;
        #endregion

        #region Ctor
        public VaisalaDataLog(string title)
        {
            Title = title.Trim();
        }
        #endregion

        #region Public methods
        public void NewEntry(VaisalaDataPod pod)
        {
            if (pod.IsValid)
            {
                dataLog.Add(pod);
            }
        }

        public void NewEntry(string dataLine)
        {
            NewEntry(new VaisalaDataPod(dataLine));
        }

        public VaisalaDataPod GetPodForDate (DateTime date)
        {
            VaisalaDataPod invalidPod = new VaisalaDataPod("this creates an invalid pod");
            if (dataLog == null) return invalidPod;
            if (dataLog.Count < 1) return invalidPod;
            foreach (var pod in dataLog)
            {
                if (pod.MatchesDate(date)) return pod;
            }
            return invalidPod;
        }

        #endregion

        private List<VaisalaDataPod> dataLog = new List<VaisalaDataPod>();

    }
}
