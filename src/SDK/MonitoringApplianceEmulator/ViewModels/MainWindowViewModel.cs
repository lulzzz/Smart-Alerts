//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models;
    using Unity.Attributes;
    using Unity.Injection;

    /// <summary>
    /// The view model class for the <see cref="MainWindow"/> control.
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        private int numberOfAlertsFound;
        private SmartDetectorRunner smartDetectorRunner;
        private string userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class for design time only.
        /// </summary>
        public MainWindowViewModel()
        {
            this.UserName = "Lionel";
            this.NumberOfAlertssFound = 20;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="alertsRepository">The alerts repository model.</param>
        /// <param name="authenticationServices">The authentication services to use.</param>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        [InjectionConstructor]
        public MainWindowViewModel(AlertsRepository alertsRepository, AuthenticationServices authenticationServices, SmartDetectorRunner smartDetectorRunner)
        {
            this.NumberOfAlertssFound = 0;
            alertsRepository.Alerts.CollectionChanged +=
                (sender, args) => { this.NumberOfAlertssFound = args.NewItems.Count; };

            this.UserName = authenticationServices.AuthenticationResult.UserInfo.GivenName;
            this.SmartDetectorRunner = smartDetectorRunner;
        }

        /// <summary>
        /// Gets the number of alerts found in this run.
        /// </summary>
        public int NumberOfAlertssFound
        {
            get => this.numberOfAlertsFound;

            private set
            {
                this.numberOfAlertsFound = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public SmartDetectorRunner SmartDetectorRunner
        {
            get
            {
                return this.smartDetectorRunner;
            }

            private set
            {
                this.smartDetectorRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the name of the signed in user.
        /// </summary>
        public string UserName
        {
            get => this.userName;

            private set
            {
                this.userName = value;
                this.OnPropertyChanged();
            }
        }
    }
}
