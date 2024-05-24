using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Timers;

namespace DSR
{
    public partial class Page : ContentPage
    {
        public Page()
        {
            InitializeComponent();
            BindingContext = new PageViewModel();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var appShell = (AppShell)App.Current.MainPage;
            appShell.SetFlyoutVisibility(false);
            await Shell.Current.GoToAsync("///login", true);
        }
    }

    public class PageViewModel : INotifyPropertyChanged
    {
        private string _currentTime;
        private System.Timers.Timer _timer;

        public ObservableCollection<GymFeature> GymFeatures { get; private set; }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        public PageViewModel()
        {
            GymFeatures = new ObservableCollection<GymFeature>
            {
                new GymFeature { Title = "Cardio Zone", Description = "State-of-the-art treadmills and ellipticals." },
                new GymFeature { Title = "Weights Area", Description = "Wide range of weights and strength machines." },
                new GymFeature { Title = "Personal Training", Description = "Get personalized training from our experts." },
                new GymFeature { Title = "Yoga Classes", Description = "Relax and rejuvenate with our yoga sessions." }
            };

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateClock;
            _timer.Start();
        }

        private void UpdateClock(object sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("F");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
