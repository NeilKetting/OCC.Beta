using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class TrainingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<HseqTrainingRecord> _trainingRecords;

        public TrainingViewModel()
        {
            // Mock Data based on Excel headers
            _trainingRecords = new ObservableCollection<HseqTrainingRecord>
            {
                new HseqTrainingRecord
                {
                    EmployeeName = "Neil Ketting",
                    Role = "Site Manager",
                    TrainingTopic = "First Aid Level 1",
                    DateCompleted = new DateTime(2025, 05, 10),
                    ValidUntil = new DateTime(2028, 05, 10),
                    Trainer = "Red Cross",
                },
                new HseqTrainingRecord
                {
                    EmployeeName = "John Doe",
                    Role = "Supervisor",
                    TrainingTopic = "Working at Heights",
                    DateCompleted = new DateTime(2024, 01, 15),
                    ValidUntil = new DateTime(2026, 01, 15), // Expiring soon scenario
                    Trainer = "HeightSafety Co",
                },
                new HseqTrainingRecord
                {
                    EmployeeName = "Jane Smith",
                    Role = "Laborer",
                    TrainingTopic = "Site Induction",
                    DateCompleted = new DateTime(2023, 11, 20),
                    ValidUntil = new DateTime(2024, 11, 20), // Expired scenario
                    Trainer = "Internal",
                }
            };
        }
    }
}
