using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using System.Collections.Generic;
using LiveChartsCore.Defaults;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class HealthSafetyDashboardViewModel : ViewModelBase
    {
        // 1. Audit Scores
        public ObservableCollection<ISeries> AuditScoreSeries { get; set; } = new();
        public ObservableCollection<Axis> AuditXAxes { get; set; } = new();

        // 2. Training Stats
        public ObservableCollection<ISeries> TrainingSeries { get; set; } = new();
        public ObservableCollection<Axis> TrainingXAxes { get; set; } = new();

        // 3. Safe Hours
        public ObservableCollection<ISeries> SafeHoursSeries { get; set; } = new();
        public ObservableCollection<Axis> SafeHoursXAxes { get; set; } = new();

        // 4. Non-Conformance
        public ObservableCollection<ISeries> NonConformanceSeries { get; set; } = new();

        public HealthSafetyDashboardViewModel()
        {
            LoadMockData();
        }

        private void LoadMockData()
        {
            // --- 1. Audit Scores ---
            AuditScoreSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double>
                {
                    Name = "Audit Score",
                    Values = new ObservableCollection<double> { 98, 94, 98, 99, 95, 96 },
                    Fill = new SolidColorPaint(SKColors.Teal),
                    MaxBarWidth = 35
                },
                new ColumnSeries<double>
                {
                    Name = "Target",
                    Values = new ObservableCollection<double> { 95, 95, 95, 95, 95, 95 },
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 35
                }
            };

            AuditXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = new[] { "Edenglen", "Mayville", "Roshnee", "Syabuswa", "The Reeds", "Winelands" },
                    LabelsRotation = 0
                }
            };

            // --- 2. Training Stats ---
            TrainingSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<int>
                {
                    Name = "Employees",
                    Values = new ObservableCollection<int> { 3, 7, 17, 10, 10, 10 },
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 35
                }
            };

            TrainingXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = new[] { "Constr. Regs", "Demo Sup.", "Evacuation", "First Aid", "Scaf. Erector", "Scaf. Insp." },
                    LabelsRotation = 15
                }
            };

            // --- 3. Safe Hours ---
            SafeHoursSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<int>
                {
                    Name = "Safe Hours",
                    Values = new ObservableCollection<int> { 8400, 10200, 12698, 13081, 11500 },
                    Fill = new SolidColorPaint(SKColors.ForestGreen),
                    MaxBarWidth = 35
                },
                new ColumnSeries<int>
                {
                    Name = "Incidents",
                    Values = new ObservableCollection<int> { 100, 200, 0, 150, 50 }, // Scaled for visibility vs 10k hours or need secondary axis? 
                    // Wait, 100 vs 10000 is invisible. The Excel chart has specific "count" vs "hours" problem. 
                    // Looking at user image 1: Safe Hours ~13000. Incidents/Near Misses are just lines at bottom.
                    // The user's image shows them as small bars. I will stick to small values.
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 35
                },
                new ColumnSeries<int>
                {
                    Name = "Near Misses",
                    Values = new ObservableCollection<int> { 200, 100, 150, 50, 0 },
                    Fill = new SolidColorPaint(SKColors.OrangeRed),
                    MaxBarWidth = 35
                }
            };

            SafeHoursXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" }
                }
            };

            // --- 4. Non-Conformance (Pie) ---
            NonConformanceSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<double> { Values = new double[] { 50 }, Name = "Housekeeping", Fill = new SolidColorPaint(SKColors.YellowGreen) },
                new PieSeries<double> { Values = new double[] { 22 }, Name = "PPE", Fill = new SolidColorPaint(SKColors.OrangeRed) },
                new PieSeries<double> { Values = new double[] { 15 }, Name = "Docs", Fill = new SolidColorPaint(SKColors.Gray) },
                new PieSeries<double> { Values = new double[] { 8 }, Name = "Site Access", Fill = new SolidColorPaint(SKColors.SteelBlue) },
                new PieSeries<double> { Values = new double[] { 5 }, Name = "Electrical", Fill = new SolidColorPaint(SKColors.Gold) }
            };

            LoadDemoData();
        }

        // --- Demo Data ---
        public ObservableCollection<ISeries> LineSeries { get; set; } = new();
        public ObservableCollection<ISeries> StackedSeries { get; set; } = new();
        public ObservableCollection<ISeries> RadarSeries { get; set; } = new();
        public ObservableCollection<Axis> RadarAxes { get; set; } = new();
        public ObservableCollection<ISeries> ScatterSeries { get; set; } = new();

        private void LoadDemoData()
        {
            // 5. Line Chart (Spline)
            LineSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Name = "Trend A",
                    Values = new ObservableCollection<double> { 2, 5, 4, 6, 8, 3, 5 },
                    Fill = null,
                    GeometrySize = 10,
                    LineSmoothness = 1 // 1 = Curved
                },
                new LineSeries<double>
                {
                    Name = "Trend B",
                    Values = new ObservableCollection<double> { 6, 3, 2, 4, 2, 5, 8 },
                    Fill = null,
                    GeometrySize = 10,
                    LineSmoothness = 1
                }
            };

            // 6. Stacked Column
            StackedSeries = new ObservableCollection<ISeries>
            {
                new StackedColumnSeries<int> { Values = new[] { 3, 5, 2, 8 }, Name = "Layer 1" },
                new StackedColumnSeries<int> { Values = new[] { 4, 2, 5, 3 }, Name = "Layer 2" },
                new StackedColumnSeries<int> { Values = new[] { 2, 6, 3, 4 }, Name = "Layer 3" }
            };

            // 7. Radar / Polar
            RadarSeries = new ObservableCollection<ISeries>
            {
                new PolarLineSeries<double>
                {
                    Values = new ObservableCollection<double> { 15, 10, 16, 12, 8, 14 },
                    Name = "Audit A",
                    IsClosed = true
                }
            };
            RadarAxes = new ObservableCollection<Axis>
            {
                new Axis { Labels = new[] { "Safety", "Quality", "Env", "Health", "Social", "Risk" } }
            };

            // 8. Scatter
            ScatterSeries = new ObservableCollection<ISeries>
            {
                new ScatterSeries<ObservablePoint>
                {
                    Values = new ObservableCollection<ObservablePoint>
                    {
                        new ObservablePoint(2.2, 5.4), new ObservablePoint(4.5, 2.5),
                        new ObservablePoint(4.2, 6.4), new ObservablePoint(6.4, 8.5),
                        new ObservablePoint(2.6, 9.4), new ObservablePoint(7.1, 8.4),
                        new ObservablePoint(5.2, 4.4), new ObservablePoint(3.3, 1.3)
                    },
                    Name = "Risk Events"
                }
            };

            // 9. Financial (Candlesticks)
            FinancialSeries = new ObservableCollection<ISeries>
            {
                new CandlesticksSeries<FinancialPoint>
                {
                    Name = "Project Costs",
                    Values = new ObservableCollection<FinancialPoint>
                    {
                        new FinancialPoint(new System.DateTime(2025, 1, 1), 500, 600, 350, 450),
                        new FinancialPoint(new System.DateTime(2025, 1, 2), 450, 550, 400, 520),
                        new FinancialPoint(new System.DateTime(2025, 1, 3), 520, 680, 500, 620),
                        new FinancialPoint(new System.DateTime(2025, 1, 4), 620, 640, 580, 600),
                        new FinancialPoint(new System.DateTime(2025, 1, 5), 600, 620, 480, 500)
                    }
                }
            };
            FinancialXAxes = new ObservableCollection<Axis>
            {
                new Axis { Labeler = value => new System.DateTime((long)value).ToString("MM/dd") }
            };

            // 10. Gauge (Pie-based)
            GaugeSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<double> { Values = new double[] { 75 }, Name = "Completion", InnerRadius = 60, Fill = new SolidColorPaint(SKColors.Green) },
                new PieSeries<double> { Values = new double[] { 25 }, Name = "Remaining", InnerRadius = 60, Fill = new SolidColorPaint(SKColors.LightGray) }
            };

            // 11. Heatmap (Matrix equivalent)
            HeatSeries = new ObservableCollection<ISeries>
            {
                new HeatSeries<WeightedPoint>
                {
                    Values = new ObservableCollection<WeightedPoint>
                    {
                        // X, Y, Weight
                        new WeightedPoint(0, 0, 10), new WeightedPoint(0, 1, 50), new WeightedPoint(0, 2, 90),
                        new WeightedPoint(1, 0, 40), new WeightedPoint(1, 1, 80), new WeightedPoint(1, 2, 20),
                        new WeightedPoint(2, 0, 60), new WeightedPoint(2, 1, 10), new WeightedPoint(2, 2, 70)
                    },
                    Name = "Risk Matrix"
                }
            };
        }

        public ObservableCollection<ISeries> FinancialSeries { get; set; } = new();
        public ObservableCollection<Axis> FinancialXAxes { get; set; } = new();
        public ObservableCollection<ISeries> GaugeSeries { get; set; } = new();
        public ObservableCollection<ISeries> HeatSeries { get; set; } = new();
    }
}
