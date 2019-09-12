using BLEConsole;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace SumoBotTerminalDotNetCore
{
    class Program
    {
        // "Magic" string for all BLE devices
        static string _aqsAllBLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        static string[] _requestedBLEProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };


        static void Main(string[] args)
        {
            BluetoothLEDevice selectedDevice = null;
            bool deviceFound = false;
            string deviceId = string.Empty;
            DeviceWatcher watcher = null;

            try
            {
                watcher = DeviceInformation.CreateWatcher(_aqsAllBLEDevices, _requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);
                watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) =>
                {
                    Console.WriteLine(devInfo.Name);
                    if (devInfo.Name == "HC-08")
                    {
                        deviceId = devInfo.Id;
                        Console.WriteLine($"Found device. ID: {deviceId}. Name: {devInfo.Name}.");
                        deviceFound = true;
                    }
                    //if (_deviceList.FirstOrDefault(d => d.Id.Equals(devInfo.Id) || d.Name.Equals(devInfo.Name)) == null) _deviceList.Add(devInfo);
                };
                watcher.Updated += (_, __) => { }; // We need handler for this event, even an empty!
                watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) => { sender.Stop(); };
                watcher.Stopped += (DeviceWatcher sender, object arg) => { };
                watcher.Start();

                while (!deviceFound)
                {
                }

                selectedDevice = BluetoothLEDevice.FromIdAsync(deviceId).GetAwaiter().GetResult();
                GattDeviceServicesResult result = selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached).GetAwaiter().GetResult();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    for (int i = 0; i < result.Services.Count; i++)
                    {
                        var serviceToDisplay = new BluetoothLEAttributeDisplay(result.Services[i]);
                        Console.WriteLine($"#{i:00}: {serviceToDisplay.Name}");
                        if (serviceToDisplay.Name.Contains("Key"))
                        {
                            var characteristicResult = serviceToDisplay.service.GetCharacteristicsAsync().GetAwaiter().GetResult();
                            IBuffer buffer = Utilities.FormatData("s", DataFormat.UTF8);
                            characteristicResult.Characteristics[0].WriteValueAsync(buffer).GetAwaiter();
                        }
                    }
                }
            }
            finally
            {
                if (selectedDevice != null)
                {
                    selectedDevice.Dispose();
                    Console.WriteLine("Close device");
                }

                if (watcher != null)
                {
                    watcher.Stop();
                }

                Console.ReadLine();
            }
        }
    }
}
