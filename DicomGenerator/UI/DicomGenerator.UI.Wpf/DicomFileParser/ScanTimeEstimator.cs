using System;

namespace DicomGenerator.UI.Wpf.DicomFileParser
{
    public sealed class ScanTimeEstimator
    {
        private double _totalSeconds;
        private int _completed;

        public void Add(TimeSpan elapsed)
        {
            _totalSeconds += elapsed.TotalSeconds;
            _completed++;
        }

        public TimeSpan GetRemainingTime(int remaining)
        {
            if (_completed == 0)
                return TimeSpan.Zero;

            var averageSeconds = _totalSeconds / _completed;

            return TimeSpan.FromSeconds(averageSeconds * remaining);
        }

        public string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{time.Hours} ч {time.Minutes} мин {time.Seconds} сек";
            }
            if (time.TotalMinutes >= 1)
            {
                return $"{time.Minutes} мин {time.Seconds} сек";
            }
            return $"{time.TotalSeconds:F1} сек";
        }
    }
}
