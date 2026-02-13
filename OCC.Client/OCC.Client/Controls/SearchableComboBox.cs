using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace OCC.Client.Controls
{
    /// <summary>
    /// A custom control that combines AutoCompleteBox with a Dropdown toggle and "Add New" functionality.
    /// </summary>
    [TemplatePart("PART_ToggleButton", typeof(Button))]
    public class SearchableComboBox : AutoCompleteBox
    {
        public static readonly StyledProperty<ICommand?> AddNewCommandProperty =
            AvaloniaProperty.Register<SearchableComboBox, ICommand?>(nameof(AddNewCommand));

        public static readonly StyledProperty<string> AddNewLabelProperty =
            AvaloniaProperty.Register<SearchableComboBox, string>(nameof(AddNewLabel), "Add new");

        public static readonly StyledProperty<bool> IsDropdownButtonVisibleProperty =
            AvaloniaProperty.Register<SearchableComboBox, bool>(nameof(IsDropdownButtonVisible), true);

        public static readonly StyledProperty<string?> WatermarkProperty =
            AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(Watermark));

        public ICommand? AddNewCommand
        {
            get => GetValue(AddNewCommandProperty);
            set => SetValue(AddNewCommandProperty, value);
        }

        public string AddNewLabel
        {
            get => GetValue(AddNewLabelProperty);
            set => SetValue(AddNewLabelProperty, value);
        }

        public bool IsDropdownButtonVisible
        {
            get => GetValue(IsDropdownButtonVisibleProperty);
            set => SetValue(IsDropdownButtonVisibleProperty, value);
        }

        public string? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var toggleButton = e.NameScope.Find<Button>("PART_ToggleButton");
            if (toggleButton != null)
            {
                toggleButton.Click += ToggleButton_Click;
            }
        }

        private void ToggleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
            }
            else
            {
                Focus();
                // Logic to show all items when toggle is clicked
                PopulateComplete();
                IsDropDownOpen = true;
            }
        }

        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Allow opening dropdown with Alt+Down or similar if we want to mimic ComboBox precisely
            if (e.Key == Avalonia.Input.Key.Down && e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt))
            {
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
            }
        }
    }
}
