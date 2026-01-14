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
            var versionString = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.3.5";

            ReleaseNotes = new ObservableCollection<ReleaseNoteItem>
            {
                new ReleaseNoteItem
                {
                    Version = $"{versionString} (Current)",
                    Date = DateTime.Today.ToString("d MMMM yyyy"),
                    Description = "Health & Safety Module & Dark Mode",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Health & Safety Module - Added Dashboard, Performance Monitoring (Safe Hours), Incidents and Audits views.",
                        "NEW: HSEQ Data - Implemented tracking for Safe Hours, Near Misses, and Audit Scores.",
                        "NEW: Dark Mode - Introduced system-wide Dark Mode theme with toggle in Preferences.",
                        "NEW: Task Detail Panel - Slide-in panel for task details (Double-click to open/pin, Hold to preview).",
                        "FIXED: Project Navigation - Resolved issue where 'Projects' tab was not active on load.",
                        "FIXED: Restock Review - Fixed navigation and logic for generated orders.",
                        "FIXED: PO Item Population - Resolved issue where existing items appeared at bottom of grid."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.5",
                    Date = "14 January 2026",
                    Description = "Low Stock Tracking & Codebase Health",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Low Stock Tracking - Toggle tracking for individual inventory items.",
                        "NEW: Conditional Visibility - Reorder Point is now hidden when tracking is disabled.",
                        "IMPROVED: Code Quality - Achieved zero-warning build across all projects (Client, API, Shared).",
                        "FIXED: Database Stability - Configured explicit decimal precision for all currency and rate fields.",
                        "FIXED: Orders Module Navigation - Resolved binding issues and missing menu properties.",
                        "FIXED: Order Printing - Added missing Email and Print commands to Create Order view.",
                        "FIXED: Order Filtering - Resolved issue where typing in SKU/Product fields didn't filter the dropdown."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.4",
                    Date = "12 January 2026",
                    Description = "Site Manager UI & Documentation",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Site Manager Selection - Searchable dropdown for assigning managers to projects.",
                        "IMPROVED: Site Manager UI - Added visual indicators (avatars) for assignment status.",
                        "IMPROVED: Documentation - Added comprehensive XML comments to IOrderManager and OrderManager.",
                        "FIXED: Bug Reporting - Improved overlay detection for more accurate bug reports."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.3",
                    Date = "09 January 2026",
                    Description = "Developer Tools & Notifications",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Developer Tools - Added restricted 'Developer' menu for simulations.",
                        "NEW: Toast Notification System - Real-time alerts for system-wide broadcasts.",
                        "NEW: Birthday Simulation - Added capability to test and trigger birthday greetings.",
                        "FIXED: Notification Actions - Restored 'Approve', 'Deny', and 'GoTo' buttons."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.2",
                    Date = "08 January 2026",
                    Description = "Overtime Approvals & Logistics",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Overtime Approval Flow - UI for reviewing and acting on OT requests.",
                        "IMPROVED: PDF Layout - Refined printing formats for purchase orders.",
                        "FIXED: Window State - App now starts maximized by default for better visibility."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.1",
                    Date = "07 January 2026",
                    Description = "Order Printing & Item Management",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Order Printing - Generate and preview PDF versions of orders.",
                        "NEW: Item List Feature - Manage item master records separately from inventory.",
                        "FIXED: Memory Management - Resolved potential leak in SignalR connection handling."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.3.0",
                    Date = "06 January 2026",
                    Description = "Procurement Beta",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Procurement Module - Initial rollout of Order creation and supplier tracking.",
                        "NEW: Real-time Updates - Integrated SignalR for instant UI synchronization of arrivals.",
                        "NEW: Auto-Update Mechanism - Pre-login splash screen now handles silent updates."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.1.11",
                    Date = "04 January 2026",
                    Description = "System Security & Presence",
                    Changes = new ObservableCollection<string>
                    {
                        "NEW: Safe Deletion Logic - Prevents accidental deletion of active Teams or Employees.",
                        "NEW: User Presence - Real-time Online/Away status indicators.",
                        "NEW: Session Security - 5-minute auto-logout for inactive users.",
                        "FIXED: Midnight Vanish - Active attendance sessions no longer disappear at midnight."
                    }
                },
                new ReleaseNoteItem
                {
                    Version = "v1.0.1",
                    Date = "01 January 2026",
                    Description = "Major UX Improvements",
                    Changes = new ObservableCollection<string>
                    {
                        "Added Search functionality to Roll Call and Attendance History views.",
                        "Implemented Searchable AutoComplete boxes for Employee selection.",
                        "Standardized alphabetical sorting across all management lists."
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
                        "Employee & Project Management",
                        "User Roles and Access Control",
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
