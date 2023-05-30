using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Knd.Printer.FujiNetworkPrinter.Models
{
    public class FujiNetworkConfig
    {
        public bool IsOpen { get; set; }
        public int RefreshTime { get; set; }
        public int Port { get; set; }
        public string IpAddress { get; set; }
        public bool IsFuji8000 { get; set; }
    }
    struct MyData
    {
        public string msg;
        public string sgn;
    }
    public class FilmBoxModel:INotifyPropertyChanged
    {
        private string _size;

        public string Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }
        private int _count = 100;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }

        public string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                OnPropertyChanged();
            }
        }

      
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
