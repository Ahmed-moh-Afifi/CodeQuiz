using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Viewmodels
{
    public partial class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;
        /// <summary>
        /// Gets or sets whether the ViewModel is busy performing an operation.
        /// Use this property to show/hide loading indicators in views.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        /// <summary>
        /// Gets the inverse of IsBusy for convenience in bindings.
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        private string? _busyMessage;
        /// <summary>
        /// Gets or sets the message to display while busy.
        /// </summary>
        public string? BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper method to execute an async operation with busy state management.
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> operation, string? busyMessage = null)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                await operation();
            }
            finally
            {
                IsBusy = false;
                BusyMessage = null;
            }
        }

        /// <summary>
        /// Helper method to execute an async operation with busy state management and return a value.
        /// </summary>
        protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? busyMessage = null)
        {
            if (IsBusy)
                return default;

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                return await operation();
            }
            finally
            {
                IsBusy = false;
                BusyMessage = null;
            }
        }
    }
}
