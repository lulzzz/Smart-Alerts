//-----------------------------------------------------------------------
// <copyright file="TimePickerControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels
{
    using System;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="TimePickerControlViewModel"/> control.
    /// </summary>
    public class TimePickerControlViewModel : ObservableObject
    {
        private string title;

        private DateTime selectedDate;

        private DateTime selectedTime;

        private DateTime minDate;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePickerControlViewModel"/> class for design time only.
        /// </summary>
        public TimePickerControlViewModel()
        {
            this.Title = "Start time:";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePickerControlViewModel"/> class.
        /// </summary>
        /// <param name="title">The control title</param>
        [InjectionConstructor]
        public TimePickerControlViewModel(string title)
        {
            this.Title = title;
            this.SelectedDate = DateTime.UtcNow;
            this.MinDate = this.SelectedDate.AddMonths(-3);
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets or sets the control title.
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the minimum date to display.
        /// </summary>
        public DateTime MinDate
        {
            get
            {
                return this.minDate;
            }

            set
            {
                this.minDate = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        public DateTime SelectedDate
        {
            get
            {
                return this.selectedDate;
            }

            set
            {
                this.selectedDate = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected time.
        /// </summary>
        public DateTime SelectedTime
        {
            get
            {
                return this.selectedTime;
            }

            set
            {
                this.selectedTime = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets the full selected date time.
        /// </summary>
        /// <returns>The full selected date time</returns>
        public DateTime GetSelectedDateTime()
        {
            var selectedDate = new DateTime(
                this.SelectedDate.Year,
                this.SelectedDate.Month,
                this.SelectedDate.Day,
                this.SelectedTime.Hour,
                this.SelectedTime.Minute,
                second: 0);

            return DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
        }
    }
}