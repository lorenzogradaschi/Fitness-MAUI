using DSR.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DSR
{
    public partial class FitnessPanelPage : ContentPage
    {
        public FitnessPanelPage()
        {
            InitializeComponent();
            BindingContext = new FitnessPanelViewModel();
        }
    }

    public class FitnessPanelViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Fitness> FitnessCenters { get; set; } = new ObservableCollection<Fitness>();
        public ICommand ToggleEditCommand { get; private set; }


        public ICommand AddFitnessCommand { get; private set; }
        public ICommand EditFitnessCommand { get; private set; } // Toggle edit
        public ICommand SaveChangesCommand { get; private set; } // Save changes
        public ICommand DeleteFitnessCommand { get; private set; } // Delete fitness center

        private string _newName;
        public string NewName
        {
            get => _newName;
            set
            {
                if (_newName != value)
                {
                    _newName = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"NewName set to: {_newName}");
                }
            }
        }

        private string _newLocation;
        public string NewLocation
        {
            get => _newLocation;
            set
            {
                if (_newLocation != value)
                {
                    _newLocation = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"NewLocation set to: {_newLocation}");
                }
            }
        }

        private string _newSubscribers;
        public string NewSubscribers
        {
            get => _newSubscribers;
            set
            {
                if (_newSubscribers != value)
                {
                    _newSubscribers = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"NewSubscribers set to: {_newSubscribers}");
                }
            }
        }

        private string _newAverageVisitors;
        public string NewAverageVisitors
        {
            get => _newAverageVisitors;
            set
            {
                if (_newAverageVisitors != value)
                {
                    _newAverageVisitors = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"NewAverageVisitors set to: {_newAverageVisitors}");
                }
            }
        }

        public bool IsFormVisible { get; set; } = false;

        public ICommand ShowAddFitnessFormCommand { get; private set; }
        public ICommand ConfirmAddFitnessCommand { get; private set; }

        public FitnessPanelViewModel()
        {
            LoadFitnessCentersAsync();
            AddFitnessCommand = new Command(async () => await AddFitnessAsync(), CanExecuteAdd);
            EditFitnessCommand = new Command<Fitness>(ToggleEdit);
            SaveChangesCommand = new Command<Fitness>(async fitness => await SaveChanges(fitness));
            DeleteFitnessCommand = new Command<Fitness>(async fitness => await DeleteFitnessAsync(fitness));
            ToggleEditCommand = new Command<Fitness>(ToggleEdit);


            ShowAddFitnessFormCommand = new Command(() =>
            {
                IsFormVisible = !IsFormVisible;
                OnPropertyChanged(nameof(IsFormVisible));
                Debug.WriteLine($"IsFormVisible changed to: {IsFormVisible}");
            });

            ConfirmAddFitnessCommand = new Command(async () => await AddFitnessAsync(), CanExecuteAdd);
        }

        private bool CanExecuteAdd()
        {
            bool canExecute = true;
            Debug.WriteLine($"Can execute add: {canExecute}");
            return canExecute;
        }

        private void ToggleEdit(Fitness fitness)
        {
            if (fitness != null)
            {
                fitness.IsEditing = !fitness.IsEditing;
                OnPropertyChanged(nameof(FitnessCenters)); // Make sure this updates the relevant UI parts
            }
            RefreshUI(); // Refresh UI after loading all centers
        }


        private async Task AddFitnessAsync()
        {
            try
            {
                Debug.WriteLine("Attempting to add new fitness center...");
                var newFitness = new Fitness
                {
                    Name = NewName,
                    Location = NewLocation,
                    NumberOfSubscribers = int.TryParse(NewSubscribers, out int subs) ? subs : 0,
                    AverageNumberOfPeople = int.TryParse(NewAverageVisitors, out int visitors) ? visitors : 0
                };

                await DatabaseHelper.AddFitnessAsync(newFitness);
                IsFormVisible = false;
                await LoadFitnessCentersAsync();
                Debug.WriteLine("New fitness center added and list refreshed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in AddFitnessAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to add fitness center due to an error: " + ex.Message, "OK");
            }
        }


        private async Task SaveChanges(Fitness fitness)
        {
            if (fitness != null)
            {
                await DatabaseHelper.EditFitnessAsync(fitness);
                ///fitness._isEditing = false;
                await LoadFitnessCentersAsync();
            }
        }

        public void RefreshUI()
        {
            OnPropertyChanged(nameof(FitnessCenters));
            foreach (var fitness in FitnessCenters)
            {
                OnPropertyChanged(nameof(fitness.Name));
                OnPropertyChanged(nameof(fitness.Location));
                OnPropertyChanged(nameof(fitness.NumberOfSubscribers));
                OnPropertyChanged(nameof(fitness.AverageNumberOfPeople));
                OnPropertyChanged(nameof(fitness.IsEditing));
            }
        }


        private async Task DeleteFitnessAsync(Fitness fitness)
        {
            if (fitness != null)
            {
                await DatabaseHelper.DeleteFitnessAsync(fitness);
                FitnessCenters.Remove(fitness);
            }
        }

        private async Task LoadFitnessCentersAsync()
        {
            var fitnessList = await DatabaseHelper.GetAllFitnessAsync();
            FitnessCenters.Clear();
            foreach (var fitness in fitnessList)
            {
                FitnessCenters.Add(fitness);
            }
            RefreshUI(); // Refresh UI after loading all centers
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ensure the input value is a boolean.
            if (value is bool isVisible)
            {
                // Return Visible when true, Collapsed when false.
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed; // Default to collapsed if the input is not valid.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                // Convert visibility back to boolean: Visible is true, everything else is false.
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }




    public class NotBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

}
