using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using CloudPrinter.View.SettingItems;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class SettingViewModel : ViewModelBase
    {
        private UserControl _systemSettingView;

        private UserControl _deviceSettingView;

        private UserControl _printSettingView;

        private UserControl _infoSettingView;

        private UserControl _cloudFilmSettingView;

        private UserControl _keyboardInputView;

        private TabItem _selectSettingItem;

        private string _visibility;

        public string CanVisibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                NotifyPropertyChanged("CanVisibility");
            }
        }

        public TabItem SelectSettingItem
        {
            get
            {
                return _selectSettingItem;
            }
            set
            {
                if (_selectSettingItem != null && _selectSettingItem != value && value.Content is ContentControl)
                {
                    (((FrameworkElement)((ContentControl)value.Content).Content).DataContext as ViewModelBase)?.InitaDataAsync();
                }
                _selectSettingItem = value;
                NotifyPropertyChanged("SelectSettingItem");
            }
        }

        public UserControl SystemSettingView
        {
            get
            {
                return _systemSettingView;
            }
            set
            {
                _systemSettingView = value;
                NotifyPropertyChanged("SystemSettingView");
            }
        }

        public UserControl DeviceSettingView
        {
            get
            {
                return _deviceSettingView;
            }
            set
            {
                _deviceSettingView = value;
                NotifyPropertyChanged("DeviceSettingView");
            }
        }

        public UserControl PrintSettingView
        {
            get
            {
                return _printSettingView;
            }
            set
            {
                _printSettingView = value;
                NotifyPropertyChanged("PrintSettingView");
            }
        }

        public UserControl InfoSettingView
        {
            get
            {
                return _infoSettingView;
            }
            set
            {
                _infoSettingView = value;
                NotifyPropertyChanged("InfoSettingView");
            }
        }

        public UserControl CloudFilmSettingView
        {
            get
            {
                return _cloudFilmSettingView;
            }
            set
            {
                _cloudFilmSettingView = value;
                NotifyPropertyChanged("CloudFilmSettingView");
            }
        }

        public UserControl KeyboardInputView
        {
            get
            {
                return _keyboardInputView;
            }
            set
            {
                _keyboardInputView = value;
                NotifyPropertyChanged("KeyboardInputView");
            }
        }

        public RelayCommand ReturnCommand { get; private set; }

        public RelayCommand ExitCommand { get; private set; }

        public RelayCommand ExitAndCloseSystemCommand { get; private set; }

        public override void InitCommand()
        {
            ReturnCommand = new RelayCommand(delegate
            {
                MainViewModel exportedValue = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>();
                exportedValue.CurrentControl = exportedValue?.WaitCardView;
            });
            ExitCommand = new RelayCommand(delegate
            {
                Application.Current.Shutdown(0);
            });
            base.InitCommand();
        }

        public override void InitaDataAsync()
        {
            SystemSettingView = RunTimeHost.MEFContainer.GetExportedValue<SystemSettingView>();
            DeviceSettingView = RunTimeHost.MEFContainer.GetExportedValue<DevicesSettingView>();
            PrintSettingView = RunTimeHost.MEFContainer.GetExportedValue<PrintSettingView>();
            InfoSettingView = RunTimeHost.MEFContainer.GetExportedValue<PatienInfoSettingView>();
            CloudFilmSettingView = RunTimeHost.MEFContainer.GetExportedValue<CloudFilmSettingView>();
            KeyboardInputView = RunTimeHost.MEFContainer.GetExportedValue<KeyboardInputView>();
            base.InitaDataAsync();
        }
    }
}
