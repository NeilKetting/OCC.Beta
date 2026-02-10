using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OCC.Client.Services.Infrastructure
{
    public class ConnectionSettings : INotifyPropertyChanged
    {
        private static ConnectionSettings? _instance;
        public static ConnectionSettings Instance => _instance ??= new ConnectionSettings();

        private bool _useApi = true;
        public bool UseApi
        {
            get => _useApi;
            set
            {
                if (_useApi != value)
                {
                    _useApi = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _apiBaseUrl = "http://102.221.36.149:8081/";
        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set
            {
                if (_apiBaseUrl != value)
                {
                    _apiBaseUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _googleApiKey = "AIzaSyCnU8_bP_FTksXDd4qgTspUZuRp_zb-Bsk";
        public string GoogleApiKey
        {
            get => _googleApiKey;
            set
            {
                if (_googleApiKey != value)
                {
                    _googleApiKey = value;
                    OnPropertyChanged();
                }
            }
        }

        private ConnectionSettings() 
        { 
#if DEBUG
            _selectedEnvironment = AppEnvironment.Local;
            _apiBaseUrl = "http://localhost:5237/";
#else
            _selectedEnvironment = AppEnvironment.Live;
            _apiBaseUrl = "http://102.221.36.149:8081/";
#endif
        }

        public enum AppEnvironment
        {
            Live,
            Local
        }

        private AppEnvironment _selectedEnvironment;
        public AppEnvironment SelectedEnvironment
        {
            get => _selectedEnvironment;
            set
            {
                if (_selectedEnvironment != value)
                {
                    _selectedEnvironment = value;
                    switch (_selectedEnvironment)
                    {
                        case AppEnvironment.Live:
                            ApiBaseUrl = "http://102.221.36.149:8081/";
                            break;
                        case AppEnvironment.Local:
                            ApiBaseUrl = "http://localhost:5237/";
                            break;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UseLocalDb)); // Backward compatibility/Binding update trigger
                }
            }
        }

        // Backward compatibility helper if needed, or remove if fully refactored
        public bool UseLocalDb
        {
            get => _selectedEnvironment == AppEnvironment.Local;
            set => SelectedEnvironment = value ? AppEnvironment.Local : AppEnvironment.Live;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
