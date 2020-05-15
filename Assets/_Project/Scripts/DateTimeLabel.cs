using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace LockScreen
{
    public class DateTimeLabel 
        : MonoBehaviour
    {
        [SerializeField] 
        private Text _timeLabel;
        [SerializeField] 
        private Text _dateLabel;

        private void Update()
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            var now = DateTime.Now;
            
            if (_timeLabel != null)
            {
                _timeLabel.text = $"{now.Hour}:{now.Minute}";
            }
            if (_dateLabel != null)
            {
                _dateLabel.text = $"{now.DayOfWeek}, {now.Day} " +
                                  $"{now.ToString("MMMM", CultureInfo.InvariantCulture)}";
            }
        }
    }
}