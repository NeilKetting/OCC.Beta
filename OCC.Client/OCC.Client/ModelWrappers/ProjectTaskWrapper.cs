using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OCC.Client.ModelWrappers
{
    /// <summary>
    /// A wrapper class for the ProjectTask model that provides strict MVVM separation.
    /// This class implements INotifyPropertyChanged via ObservableObject and encapsulates
    /// presentation logic (like duration formatting, color coding) away from the clean data model.
    /// It ensures that the underlying Model is kept pure while the View binds to these reactive properties.
    /// </summary>
    public partial class ProjectTaskWrapper : ObservableObject
    {
        private ProjectTask _model;
        private bool _isUpdatingDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectTaskWrapper"/> class.
        /// </summary>
        /// <param name="model">The original ProjectTask model to wrap.</param>
        public ProjectTaskWrapper(ProjectTask model)
        {
            _model = model;
            Initialize();
        }

        /// <summary>
        /// Gets the underlying ProjectTask model.
        /// Use this when saving data back to the repository.
        /// </summary>
        public ProjectTask Model => _model;

        /// <summary>
        /// Gets the Task ID.
        /// </summary>
        public Guid Id => _model.Id;

        /// <summary>
        /// Gets a formatted display ID (e.g., T-1234).
        /// </summary>
        public string DisplayId => $"T-{_model.Id.ToString().Substring(_model.Id.ToString().Length - 4)}";

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isComplete;

        [ObservableProperty]
        private string _status = "To Do";

        [ObservableProperty]
        private int _percentComplete;

        [ObservableProperty]
        private bool _isOnHold;

        [ObservableProperty]
        private string _priority = "Medium";

        [ObservableProperty]
        private DateTime? _startDate;

        [ObservableProperty]
        private DateTime? _finishDate;

        [ObservableProperty]
        private DateTime? _actualStartDate;

        [ObservableProperty]
        private DateTime? _actualCompleteDate;

        [ObservableProperty]
        private double? _plannedHours;

        [ObservableProperty]
        private double? _actualHours;

        [ObservableProperty]
        private string _plannedDurationText = string.Empty;

        [ObservableProperty]
        private string _actualDurationText = string.Empty;

        [ObservableProperty]
        private string _statusColor = "#CBD5E1";

        [ObservableProperty]
        private double _progressPercent;

        /// <summary>
        /// Initializes the wrapper properties from the model.
        /// </summary>
        /// <summary>
        /// Initializes the wrapper properties from the model.
        /// </summary>
        private void Initialize()
        {
            // Initialize simple properties from model
            Name = _model.Name;
            Description = _model.Description;
            IsOnHold = _model.IsOnHold;
            Priority = _model.Priority;
            
            // Initialize status-related properties
            // Status is set first as it drives defaults for color and progress
            Status = _model.Status;
            
            // Overwrite specific values if model data differs from defaults derived from Status
            PercentComplete = _model.PercentComplete;
            ProgressPercent = _model.PercentComplete;
            IsComplete = _model.IsComplete;

            // Initialize Dates
            StartDate = _model.StartDate == DateTime.MinValue ? null : _model.StartDate;
            FinishDate = _model.FinishDate == DateTime.MinValue ? null : _model.FinishDate;
            ActualStartDate = _model.ActualStartDate;
            ActualCompleteDate = _model.ActualCompleteDate;
            
            // Initialize Duration
            // Prioritize model's stored duration. If missing, attempt to calculate from dates.
            var modelPlannedHours = _model.PlanedDurationHours?.TotalHours;
            if (modelPlannedHours.HasValue)
            {
                PlannedHours = modelPlannedHours;
            }
            else if (StartDate.HasValue && FinishDate.HasValue)
            {
                 PlannedHours = CalculatePlannedHours(StartDate.Value, FinishDate.Value);
            }

            ActualHours = _model.ActualDuration?.TotalHours;
            
            // Ensure derived text properties are consistent with the values set above
            if (string.IsNullOrEmpty(PlannedDurationText)) UpdatePlannedDurationText();
            if (string.IsNullOrEmpty(ActualDurationText)) UpdateActualDurationText();
            
            // Ensure status color is consistent with Status/IsOnHold state
            if (string.IsNullOrEmpty(StatusColor)) UpdateStatusColor();
        }

        // --- Synchronization Methods (Push specific changes back to Model) ---

        /// <summary>
        /// Commits all current wrapper property values back to the underlying Model.
        /// Call this before saving the Model to the repository.
        /// </summary>
        public void CommitToModel()
        {
            _model.Name = Name;
            _model.Description = Description;
            
            // Note: IsComplete is calculated on the model, so we don't set it directly.
            // It is derived from Status and PercentComplete.
            
            _model.Status = Status;
            _model.PercentComplete = PercentComplete;
            _model.IsOnHold = IsOnHold;
            _model.Priority = Priority;
            
            var safeMinDate = new DateTime(1753, 1, 1);
            _model.StartDate = StartDate ?? safeMinDate; 
            _model.FinishDate = FinishDate ?? safeMinDate;
            _model.ActualStartDate = ActualStartDate;
            _model.ActualCompleteDate = ActualCompleteDate;

            if (PlannedHours.HasValue)
                _model.PlanedDurationHours = TimeSpan.FromHours(PlannedHours.Value);
            else
                _model.PlanedDurationHours = null;

            if (ActualHours.HasValue)
                _model.ActualDuration = TimeSpan.FromHours(ActualHours.Value);
            else
                _model.ActualDuration = null;
        }

        // --- Property Change Handlers ---

        partial void OnNameChanged(string value) => _model.Name = value;
        partial void OnDescriptionChanged(string value) => _model.Description = value;
        
        partial void OnIsCompleteChanged(bool value)
        {
            // We cannot set _model.IsComplete directly as it is read-only.
            // Instead, we update the state that triggers completion.
            if (value)
            {
                if (ActualCompleteDate == null) ActualCompleteDate = DateTime.Now; 
                PercentComplete = 100;
                Status = "Completed"; 
            }
            else
            {
                if (Status == "Completed" || Status == "Done") 
                    Status = "In Progress";
                
                if (PercentComplete == 100) 
                {
                    PercentComplete = 50; 
                    ProgressPercent = 50;
                }
                ActualCompleteDate = null;
            }
            
            // Sync to model immediately so CommitToModel is redundant but safe
            _model.ActualCompleteDate = ActualCompleteDate;
            _model.PercentComplete = PercentComplete;
            _model.Status = Status;
        }

        partial void OnStatusChanged(string value)
        {
            _model.Status = value;
            UpdateStatusColor();
            
            // Auto-update progress based on status
             switch(value)
            {
                case "Not Started": 
                    ProgressPercent = 0; 
                    break;
                case "Started": 
                    ProgressPercent = 25; 
                    break;
                case "Halfway": 
                    ProgressPercent = 50; 
                    break;
                case "Almost Done": 
                    ProgressPercent = 75; 
                    break;
                case "Done": 
                case "Completed":
                    ProgressPercent = 100; 
                    IsComplete = true; 
                    break;
            }
            if (value != "Done" && value != "Completed" && IsComplete) IsComplete = false;
        }

        partial void OnProgressPercentChanged(double value)
        {
            _percentComplete = (int)value;
            _model.PercentComplete = (int)value;
        }

        partial void OnIsOnHoldChanged(bool value)
        {
            _model.IsOnHold = value;
            UpdateStatusColor();
        }

        partial void OnPriorityChanged(string value) => _model.Priority = value;

        partial void OnStartDateChanged(DateTime? value)
        {
            _model.StartDate = value ?? DateTime.MinValue;
            if (value.HasValue && FinishDate.HasValue)
            {
                 PlannedHours = CalculatePlannedHours(value.Value, FinishDate.Value);
            }
            UpdatePlannedDurationText();
        }

        partial void OnFinishDateChanged(DateTime? value)
        {
            _model.FinishDate = value ?? DateTime.MinValue;
            if (StartDate.HasValue && value.HasValue)
            {
                 PlannedHours = CalculatePlannedHours(StartDate.Value, value.Value);
            }
            UpdatePlannedDurationText();
        }

        partial void OnActualStartDateChanged(DateTime? value) 
        {
            _model.ActualStartDate = value;
            UpdateActualDurationText();
        }

        partial void OnActualCompleteDateChanged(DateTime? value) 
        {
            _model.ActualCompleteDate = value;
            UpdateActualDurationText();
        }

        partial void OnPlannedHoursChanged(double? value)
        {
            if (_isUpdatingDuration) return;
            
            if (value.HasValue)
            {
                _isUpdatingDuration = true;
                double days = value.Value / 8.0;
                PlannedDurationText = $"{days:0.#} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
        }

        partial void OnPlannedDurationTextChanged(string value)
        {
            if (_isUpdatingDuration) return;
            FormatDuration(value, isPlanned: true);
        }

        partial void OnActualDurationTextChanged(string value)
        {
            if (_isUpdatingDuration) return;
            FormatDuration(value, isPlanned: false);
        }

        // --- Logic ---

        private void UpdateStatusColor()
        {
            if (IsOnHold)
            {
                StatusColor = "#22C55E"; // Green
                return;
            }

            switch (Status)
            {
                case "Not Started": StatusColor = "#CBD5E1"; break; // Gray
                default: StatusColor = "#EF4444"; break; // Red for active/done
            }
        }

        private double CalculatePlannedHours(DateTime start, DateTime end)
        {
             var days = (end.Date - start.Date).TotalDays + 1;
             return Math.Round(days * 8, 1);
        }

        private void UpdatePlannedDurationText()
        {
            if (StartDate.HasValue && FinishDate.HasValue)
            {
                var days = (FinishDate.Value.Date - StartDate.Value.Date).TotalDays + 1;
                _isUpdatingDuration = true;
                PlannedDurationText = $"{days:0.#} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
            else
            {
                _isUpdatingDuration = true;
                PlannedDurationText = "None";
                _isUpdatingDuration = false;
            }
        }

        private void UpdateActualDurationText()
        {
            if (ActualStartDate.HasValue && ActualCompleteDate.HasValue)
            {
                var days = (ActualCompleteDate.Value.Date - ActualStartDate.Value.Date).TotalDays + 1;
                _isUpdatingDuration = true;
                ActualDurationText = $"{days:0.#} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
            else
            {
                _isUpdatingDuration = true;
                ActualDurationText = "None";
                _isUpdatingDuration = false;
            }
        }

        private void FormatDuration(string value, bool isPlanned)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "None") return;

            var numericPart = "";
            bool foundDecimal = false;
            foreach (char c in value)
            {
                if (char.IsDigit(c)) numericPart += c;
                else if (c == '.' && !foundDecimal) { numericPart += c; foundDecimal = true; }
                else if (numericPart.Length > 0) break;
            }

            if (double.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double days))
            {
                _isUpdatingDuration = true;
                var formatted = $"{days:0.#} {(days == 1 ? "day" : "days")}";
                
                if (isPlanned)
                {
                    PlannedDurationText = formatted;
                    PlannedHours = days * 8.0;
                }
                else
                {
                    ActualDurationText = formatted;
                    // We don't auto-set ActualHours based on days usually, but we could.
                    // Let's assume ActualHours is manually entered or we can sync it.
                    // For consistency with planned:
                    ActualHours = days * 8.0; 
                }
                
                _isUpdatingDuration = false;
            }
        }
    }
}
