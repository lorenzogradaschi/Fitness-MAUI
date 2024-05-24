using DSR.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;


namespace DSR
{

    public partial class AdminPanelPage : ContentPage
    {
        public AdminPanelPage()
        {
            InitializeComponent();
            BindingContext = new AdminPanelViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminPanelViewModel viewModel)
            {
                viewModel.LoadUsersCommand.Execute(null);
            }
        }
    }


    public class AdminPanelViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<User> Users { get; private set; }
        public ICommand LoadUsersCommand { get; private set; }
        public ICommand AddNewUserCommand { get; private set; }
        public ICommand ToggleEditCommand { get; private set; }
        public ICommand SaveChangesCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }

        public AdminPanelViewModel()
        {
            Users = new ObservableCollection<User>();
            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            AddNewUserCommand = new Command(async () => await AddNewUserAsync());
            ToggleEditCommand = new Command<User>(ToggleEdit);
            SaveChangesCommand = new Command<User>(async user => await SaveChanges(user));
            DeleteUserCommand = new Command<User>(async user => await DeleteUserAsync(user));
        }

        private async Task LoadUsersAsync()
        {
            var userList = await DatabaseHelper.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in userList)
            {
                Users.Add(user);
            }
        }

        private void ToggleEdit(User user)
        {
            if (user != null)
            {
                user.IsEditing = !user.IsEditing;
                OnPropertyChanged(nameof(Users));
            }
        }

        private async Task SaveChanges(User user)
        {
            if (user == null) return;
            await DatabaseHelper.EditUserAsync(user);
            user.IsEditing = false;
            await LoadUsersAsync();
        }

        private async Task AddNewUserAsync()
        {
            await Shell.Current.GoToAsync("///signup");
        }

        private async Task DeleteUserAsync(User user)
        {
            bool confirm = await App.Current.MainPage.DisplayAlert("Confirm Delete", $"Delete user {user.Name}?", "Yes", "No");
            if (confirm)
            {
                await DatabaseHelper.DeleteUserAsync(user);
                Users.Remove(user);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

