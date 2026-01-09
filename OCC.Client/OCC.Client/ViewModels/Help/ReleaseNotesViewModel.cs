using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;

namespace OCC.Client.ViewModels.Help
{
    public partial class ReleaseNotesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ReleaseNoteItem> _releaseNotes = new();

        public event EventHandler? CloseRequested;

        public ReleaseNotesViewModel()
        {
            LoadNotes();
        }

        private void LoadNotes()
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            var versionString = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.1.9";

            ReleaseNotes = new ObservableCollection<ReleaseNoteItem>
            {
                new ReleaseNoteItem
                {
                    Version = $"{versionString} (Current)",
                    Date = DateTime.Today.ToString("d MMMM yyyy"),
                    Description = "Overnight Shift Fixes & Team Management Polish",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Session Timeout & Auto-Logout - Adds security by logging out inactive users (5m).",
                        "NEW: Pre-Login Auto-Update - App updates now install cleanly via Splash Screen before login.",
                        "NEW: 'Leave Early' Reason Dialog - Prompts for a reason/notes when clocking out >15 mins early.",
                        "NEW: Time Correction - Added 'Edit' button to fix Timesheet entries manually.",
                        "NEW: User Presence - See who is Online vs Away (Orange) in real-time.",
                        "IMPROVED: Daily Timesheet View - Split 'Pending' vs 'Actioned' for better workflow.",
                        "NEW: Implemented Safe Deletion Logic (Prevents accidental deletion of active Teams/Employees).",
                        "FIXED: 'Save Changes' not persisting to database properly.",
                        "FIXED: Fixed various UI layout issues (DataGrid Overflow).",
                        "FIXED: Fixed Null Reference crash on Login.",
                        "FIXED: Active users no longer disappear from 'Live View' after midnight (Midnight Vanish Bug).",
                        "FIXED: Real-time Wage & Hour calculations now work correctly for overnight shifts.",
                        "FIXED: Clock-Out logic improved to ensure valid Start/End times for overnight records.",
                        "IMPROVED: Team Management now supports adding members to a BRAND NEW team before saving.",
                        "IMPROVED: Team Member selection is now a Searchable Dropdown (like Banking).",
                        "UI: Refined Team Detail popup with compact rows and a larger layout."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.0.1",
                    Date = DateTime.Today.AddDays(-1).ToString("d MMMM yyyy"),
                    Description = "Major UX Improvements and Real-time Features",
                    Changes = new ObservableCollection<string>
                    {
                        "Added Search functionality to Roll Call, Clock Out, and Attendance History views.",
                        "Replaced standard dropdowns with searchable 'AutoComplete' boxes for easier Employee selection.",
                        "Implemented Live Updates for Active Attendance sessions.",
                        "Standardized alphabetical sorting (by Name) across all Management and Staff lists.",
                        "Fixed various DataGrid stying issues and improved overall UI responsiveness."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.0.0",
                    Date = "01 January 2026",
                    Description = "Initial Beta Release",
                    Changes = new ObservableCollection<string>
                    {
                        "Core Time & Attendance Module",
                        "Employee Management System",
                        "User Management and Role-Based Access Control",
                        "Project & Team Management Modules",
                        "Basic Reporting and Export functionality"
                    }
                }
            };
        }

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ReleaseNoteItem
    {
        public string Version { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObservableCollection<string> Changes { get; set; } = new();
    }
}
