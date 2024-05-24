using DSR.Models;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace DSR
{
    public partial class EditUserPage : ContentPage
    {
        public EditUserPage(User user)
        {
            InitializeComponent();
            BindingContext = new EditUserViewModel(user);
        }
    }

    public class EditUserViewModel : INotifyPropertyChanged
    {
        public User CurrentUser { get; set; }
        public ICommand SaveCommand { get; }

        public EditUserViewModel(User user)
        {
            CurrentUser = user;
            SaveCommand = new Command(async () =>
            {
                await DatabaseHelper.EditUserAsync(CurrentUser);
                await Shell.Current.GoToAsync("..");
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
