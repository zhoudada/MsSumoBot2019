using BLEConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SumoBotController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // "Magic" string for all BLE devices
        private const string _aqsAllBLEDevices =
            "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

        private readonly string[] _requestedBLEProperties =
        {
            "System.Devices.Aep.DeviceAddress",
            "System.Devices.Aep.Bluetooth.Le.IsConnectable"
        };

        private bool _isConnected;
        private BluetoothLEDevice _selectedDevice = null;
        private DeviceWatcher _watcher = null;
        private GattCharacteristic _characteristc = null;

        public void OnSuspended()
        {
            if (_watcher != null && (_watcher.Status == DeviceWatcherStatus.Started ||
                _watcher.Status == DeviceWatcherStatus.EnumerationCompleted))
            {
                _watcher.Stop();
            }

            if (_selectedDevice != null)
            {
                _selectedDevice.Dispose();
                _selectedDevice = null;
                Debug.WriteLine("Close device");
            }

            _characteristc = null;

            _isConnected = false;
        }

        public void OnResumed()
        {
            Initialize();
        }

        private void Initialize()
        {
            bool deviceFound = false;
            string deviceId = string.Empty;

            _watcher = DeviceInformation.CreateWatcher(_aqsAllBLEDevices, _requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);
            _watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) =>
            {
                Debug.WriteLine(devInfo.Name);
                if (devInfo.Name == "HC-08")
                {
                    deviceId = devInfo.Id;
                    Debug.WriteLine($"Found device. ID: {deviceId}. Name: {devInfo.Name}.");
                    deviceFound = true;
                }
                //if (_deviceList.FirstOrDefault(d => d.Id.Equals(devInfo.Id) || d.Name.Equals(devInfo.Name)) == null) _deviceList.Add(devInfo);
            };
            _watcher.Updated += (_, __) => { }; // We need handler for this event, even an empty!
            _watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) => { sender.Stop(); };
            _watcher.Stopped += (DeviceWatcher sender, object arg) => { };
            _watcher.Start();

            while (!deviceFound)
            {
            }

            _selectedDevice = BluetoothLEDevice.FromIdAsync(deviceId).GetAwaiter().GetResult();
            GattDeviceServicesResult result = _selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached).GetAwaiter().GetResult();
            if (result.Status == GattCommunicationStatus.Success)
            {
                for (int i = 0; i < result.Services.Count; i++)
                {
                    var serviceToDisplay = new BluetoothLEAttributeDisplay(result.Services[i]);
                    Debug.WriteLine($"#{i:00}: {serviceToDisplay.Name}");
                    if (serviceToDisplay.Name == "SimpleKeyService")
                    {
                        var characteristicResult = serviceToDisplay.service.GetCharacteristicsAsync().GetAwaiter().GetResult();

                        foreach (GattCharacteristic characteristic in characteristicResult.Characteristics)
                        {
                            var characteristicToDisplay = new BluetoothLEAttributeDisplay(characteristic);
                            if (characteristicToDisplay.Name == "SimpleKeyState")
                            {
                                _characteristc = characteristic;
                                Debug.WriteLine("Characteristic found.");
                                _isConnected = true;
                            }
                        }
                    }
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        private async Task Write(Char c)
        {
            if (_isConnected)
            {
                IBuffer buffer = Utilities.FormatData(c.ToString(), DataFormat.UTF8);
                await _characteristc.WriteValueAsync(buffer);
            }
        }

        private async void Forward_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('f');
        }

        private async void Forward_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void Stop_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void Stop_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void Back_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('b');
        }

        private async void Back_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void LF_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('q');
        }

        private async void LF_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void RF_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('e');
        }

        private async void RF_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void LB_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('z');
        }

        private async void LB_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void RB_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('b');
        }

        private async void RB_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void Left_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('l');
        }

        private async void Left_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void Right_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await Write('r');
        }

        private async void Right_Released(object sender, PointerRoutedEventArgs e)
        {
            await Write('s');
        }

        private async void High_Clicked(object sender, RoutedEventArgs e)
        {
            await Write('u');
        }

        private async void Medium_Clicked(object sender, RoutedEventArgs e)
        {
            await Write('i');
        }

        private async void Low_Clicked(object sender, RoutedEventArgs e)
        {
            await Write('o');
        }
    }
}
