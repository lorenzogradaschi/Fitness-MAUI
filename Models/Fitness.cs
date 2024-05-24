using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DSR.Models
{
    public class Fitness : INotifyPropertyChanged
    {
        private string _name;
        private string _location;
        private int _numberOfSubscribers;
        private int _averageNumberOfPeople;
      //  private bool _isEditing;

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing)); // Pass the name of the property
                }
            }
        }



        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public int NumberOfSubscribers
        {
            get => _numberOfSubscribers;
            set => SetProperty(ref _numberOfSubscribers, value);
        }

        public int AverageNumberOfPeople
        {
            get => _averageNumberOfPeople;
            set => SetProperty(ref _averageNumberOfPeople, value);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return;
            storage = value;
            OnPropertyChanged(propertyName);
        }
    }
}
