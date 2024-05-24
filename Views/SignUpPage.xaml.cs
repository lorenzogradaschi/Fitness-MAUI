using DSR.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DSR
{
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
            var viewModel = new SignUpViewModel(Navigation);
            BindingContext = viewModel;
        }
    }

    public class SignUpViewModel : INotifyPropertyChanged
    {
        private User user = new User();
        private string _name, _surname, _emso, _email, _password, _confirmPassword, _placeOfBirth, _country;
        private int _age;
        private DateTime _dateOfBirth;
        private readonly INavigation _navigation;
        public Command SignUpCommand { get; private set; }
        public Command GoBackCommand { get; private set; }

        private string _validationMessage;
        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public SignUpViewModel(INavigation navigation)
        {
            _navigation = navigation;
            SignUpCommand = new Command(async () => await PerformSignUp(), CanSignUp);
            GoBackCommand = new Command(async () => await _navigation.PopAsync());
            LoadCities();
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string Surname
        {
            get => _surname;
            set
            {
                if (SetProperty(ref _surname, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string EMSO
        {
            get => _emso;
            set
            {
                if (SetProperty(ref _emso, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (SetProperty(ref _age, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string PlaceOfBirth
        {
            get => _placeOfBirth;
            set
            {
                if (SetProperty(ref _placeOfBirth, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (SetProperty(ref _country, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (SetProperty(ref _dateOfBirth, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public ObservableCollection<string> PlacesOfBirth { get; } = new ObservableCollection<string>();

        public string SelectedPlaceOfBirth
        {
            get => _placeOfBirth;
            set
            {
                if (SetProperty(ref _placeOfBirth, value))
                {
                    SignUpCommand.ChangeCanExecute();
                }
            }
        }

        public class StringIsNotNullOrEmptyConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var str = value as string;
                return !string.IsNullOrEmpty(str);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private bool CanSignUp()
        {
            ValidationMessage = "";
            bool isValid = true;

            // Check each field and append messages
            if (string.IsNullOrEmpty(Name) || !Regex.IsMatch(Name, @"^[a-zA-Z]+$"))
            {
                ValidationMessage += "Name is required and can only contain letters.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(Surname) || !Regex.IsMatch(Surname, @"^[a-zA-Z]+$"))
            {
                ValidationMessage += "Surname is required and can only contain letters.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(Email) || !Email.Contains("@"))
            {
                ValidationMessage += "Email is required and must contain '@'.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(Password) || !Regex.IsMatch(Password, @"^(?=.*[A-Z])(?=.*\W).{8,}$"))
            {
                ValidationMessage += "Password is required and must be at least 8 characters long, contain at least one uppercase letter and one special character.\n";
                isValid = false;
            }
            if (Password != ConfirmPassword)
            {
                ValidationMessage += "Passwords do not match.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(EMSO) || !Regex.IsMatch(EMSO, @"^\d{13}$"))
            {
                ValidationMessage += "EMSO is required and must be a 13-digit number.\n";
                isValid = false;
            }
            if (Age <= 0)
            {
                ValidationMessage += "Age must be greater than 0.\n";
                isValid = false;
            }
            if (DateOfBirth == default || DateOfBirth.Year < 1900 || DateOfBirth > DateTime.Now)
            {
                ValidationMessage += "Date of Birth must be between 1900 and today.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(PlaceOfBirth))
            {
                ValidationMessage += "Place of Birth is required.\n";
                isValid = false;
            }
            if (string.IsNullOrEmpty(Country))
            {
                ValidationMessage += "Country is required.\n";
                isValid = false;
            }

            return isValid;
        }

        private void LoadCities()
        {
            PlacesOfBirth.Add("Ljubljana");
            PlacesOfBirth.Add("Maribor");
            PlacesOfBirth.Add("Kranj");
            PlacesOfBirth.Add("Novo Mesto");
            PlacesOfBirth.Add("Murska Sobota");
            PlacesOfBirth.Add("Izola");
            PlacesOfBirth.Add("Koper");
            PlacesOfBirth.Add("Piran");
        }

        private async Task PerformSignUp()
        {
            if (!CanSignUp()) return;

            try
            {
                Debug.WriteLine($"Name: {Name}");
                Debug.WriteLine($"Surname: {Surname}");
                Debug.WriteLine($"Email: {Email}");
                Debug.WriteLine($"DateOfBirth: {DateOfBirth}");
                Debug.WriteLine($"Age: {Age}");
                Debug.WriteLine($"Country: {Country}");
                Debug.WriteLine($"EMSO: {EMSO}");
                Debug.WriteLine($"PlaceOfBirth: {PlaceOfBirth}");

                if (string.IsNullOrWhiteSpace(Password))
                {
                    throw new ArgumentException("Password cannot be null or empty");
                }

                user.Name = Name;
                user.Surname = Surname;
                user.Email = Email;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                user.DateOfBirth = DateOfBirth;
                user.Age = Age;
                user.Country = Country;
                user.EMSO = EMSO;
                user.PlaceOfBirth = PlaceOfBirth;

                // Insert the user into the database
                await DatabaseHelper.AddUserAsync(user);

                // Show success message and navigate back
                await App.Current.MainPage.DisplayAlert("Signup Success", "Account created successfully. Please log in.", "OK");
                // Navigate to the login page using the Shell route
                await Shell.Current.GoToAsync("///login", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Signup failed: {ex.Message}");
                Debug.WriteLine($"Exception Details: {ex}");
                await App.Current.MainPage.DisplayAlert("Signup Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke();
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
