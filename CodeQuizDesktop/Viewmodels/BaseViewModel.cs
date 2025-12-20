using CodeQuizDesktop.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeQuizDesktop.Viewmodels
{
    public partial class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // UI service for loading indicators - lazily resolved from DI container
        // Can be set directly for testing purposes
        private IUIService? _uiService;
        private bool _uiServiceResolved;
        protected IUIService? UIService
        {
            get
            {
                if (!_uiServiceResolved && _uiService == null)
                {
                    try
                    {
                        _uiService = MauiProgram.GetService<IUIService>();
                    }
                    catch
                    {
                        // In test environments, the service provider may not be available
                    }
                    _uiServiceResolved = true;
                }
                return _uiService;
            }
            set
            {
                _uiService = value;
                _uiServiceResolved = true;
            }
        }

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
        /// Helper method to execute an async operation with busy state management and UI loading indicator.
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> operation, string? busyMessage = null)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                if (UIService != null)
                    await UIService.ShowLoadingAsync(busyMessage);
                await operation();
            }
            finally
            {
                if (UIService != null)
                    await UIService.HideLoadingAsync();
                IsBusy = false;
                BusyMessage = null;
            }
        }

        /// <summary>
        /// Helper method to execute an async operation with busy state management, UI loading indicator, and return a value.
        /// </summary>
        protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? busyMessage = null)
        {
            if (IsBusy)
                return default;

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                if (UIService != null)
                    await UIService.ShowLoadingAsync(busyMessage);
                return await operation();
            }
            finally
            {
                if (UIService != null)
                    await UIService.HideLoadingAsync();
                IsBusy = false;
                BusyMessage = null;
            }
        }
    }
}
