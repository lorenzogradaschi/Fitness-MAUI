using DSR;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace DSR.Models
{
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public string Email { get; set; }

        public string Password {  get; set; }

        public string ConfirmedPassword { get; set; }

        public string PasswordHash { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }

        public string EMSO { get; set; }
        public string PlaceOfBirth { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
