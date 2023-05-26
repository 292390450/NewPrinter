// CloudPrinter.ViewModel.SettingItems.DevicesSettingViewModel

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Knd.Printer.Abstract;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;

namespace CloudPrinter.ViewModel.SettingItems;

[Export]
public class DevicesSettingViewModel : ViewModelBase
{
    private ObservableCollection<IBaseDevice> _devices;

    private IBaseDevice _selecteDevice;

    public ObservableCollection<IBaseDevice> Devices
    {
        get
        {
            return _devices;
        }
        set
        {
            _devices = value;
            NotifyPropertyChanged("Devices");
        }
    }

    public IBaseDevice SelecteDevice
    {
        get
        {
            return _selecteDevice;
        }
        set
        {
            _selecteDevice = value;
            value?.Selected?.Invoke();
            NotifyPropertyChanged("SelecteDevice");
        }
    }

    public override void InitaDataAsync()
    {
        if (Devices == null)
        {
            IEnumerable<IBaseDevice> devices = RunTimeHost.MEFContainer.GetExportedValues<IBaseDevice>();
            foreach (IBaseDevice device in devices)
            {
                device.Initial();
            }
            Devices = new ObservableCollection<IBaseDevice>();
            Devices.Clear();
            Devices.AddRange(devices);
        }
        SelecteDevice = Devices.FirstOrDefault();
        base.InitaDataAsync();
    }
}