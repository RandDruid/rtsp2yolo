using System;
using System.ComponentModel;
using System.Net;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using RtspClientSharp;
using Rtsp2YoloPlayer.GUI.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Rtsp2YoloPlayer.GUI.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";

        private string _status = string.Empty;
        private readonly IMainWindowModel _mainWindowModel;
        private bool _startButtonEnabled = true;
        private bool _stopButtonEnabled;

        public string DeviceAddress { get; set; } = "rtsp://10.70.11.33/ch01.h264";

        public string Login { get; set; } = "yolo";
        public string Password { get; set; } = "yoloyolo";

        public IVideoSource VideoSource => _mainWindowModel.VideoSource;

        public RelayCommand StartClickCommand { get; }
        public RelayCommand StopClickCommand { get; }
        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public ObservableCollection<BboxItem> BboxItems { get; } = new ObservableCollection<BboxItem>();
        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; private set; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IMainWindowModel mainWindowModel)
        {
            _mainWindowModel = mainWindowModel ?? throw new ArgumentNullException(nameof(mainWindowModel));
            _mainWindowModel.ObjectsOut += _mainWindowModel_ObjectsOut;
            _mainWindowModel.FrameSizeChanged += _mainWindowModel_FrameSizeChanged;
            
            StartClickCommand = new RelayCommand(OnStartButtonClick, () => _startButtonEnabled);
            StopClickCommand = new RelayCommand(OnStopButtonClick, () => _stopButtonEnabled);
            ClosingCommand = new RelayCommand<CancelEventArgs>(OnClosing);
        }

        private void _mainWindowModel_FrameSizeChanged(object sender, Tuple<double, double> e)
        {
            CanvasWidth = e.Item1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanvasWidth"));
            CanvasHeight = e.Item2;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanvasHeight"));
        }

        private void _mainWindowModel_ObjectsOut(object sender, List<BboxItem> e)
        {
            List<BboxItem> toDelete = new List<BboxItem>(BboxItems);
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (BboxItem newItem in e)
                {
                    if (BboxItems.FirstOrDefault(b => b.Id == newItem.Id) is BboxItem oldItem)
                    {
                        newItem.CopyTo(oldItem);
                        toDelete.Remove(oldItem);
                    }
                    else
                    {
                        BboxItems.Add(newItem);
                    }
                }
                foreach (BboxItem oldItem in toDelete) BboxItems.Remove(oldItem);
            });
            toDelete.Clear();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStartButtonClick()
        {
            string address = DeviceAddress;

            if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                address = RtspPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
            {
                MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(Login, Password);

            var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) : 
                new ConnectionParameters(deviceUri, credential);

            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            _mainWindowModel.Start(connectionParameters);
            _mainWindowModel.StatusChanged += MainWindowModelOnStatusChanged;

            _startButtonEnabled = false;
            StartClickCommand.RaiseCanExecuteChanged();
            _stopButtonEnabled = true;
            StopClickCommand.RaiseCanExecuteChanged();
        }

        private void OnStopButtonClick()
        {
            _mainWindowModel.Stop();
            _mainWindowModel.StatusChanged -= MainWindowModelOnStatusChanged;

            _stopButtonEnabled = false;
            StopClickCommand.RaiseCanExecuteChanged();
            _startButtonEnabled = true;
            StartClickCommand.RaiseCanExecuteChanged();
            Status = string.Empty;
        }

        private void MainWindowModelOnStatusChanged(object sender, string s)
        {
            Application.Current.Dispatcher.Invoke(() => Status = s);
        }

        private void OnClosing(CancelEventArgs args)
        {
            _mainWindowModel.Stop();
        }
    }
}