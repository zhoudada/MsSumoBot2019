//#define MOCK

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
using Windows.UI.Core;
using System.Threading;

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

        private readonly Dictionary<Windows.System.VirtualKey, CommandType> _keyboardMap =
            new Dictionary<Windows.System.VirtualKey, CommandType>
            {
                { Windows.System.VirtualKey.Up, CommandType.Forward },
                { Windows.System.VirtualKey.S, CommandType.Stop },
                { Windows.System.VirtualKey.Down, CommandType.Backward },
                { Windows.System.VirtualKey.Left, CommandType.Left },
                { Windows.System.VirtualKey.Right, CommandType.Right },
                { Windows.System.VirtualKey.Q, CommandType.LeftForward },
                { Windows.System.VirtualKey.E, CommandType.RightForward },
                { Windows.System.VirtualKey.Z, CommandType.LeftBackward },
                { Windows.System.VirtualKey.C, CommandType.RightBackward },
                { Windows.System.VirtualKey.U, CommandType.HighSpeed },
                { Windows.System.VirtualKey.I, CommandType.MediumSpeed },
                { Windows.System.VirtualKey.O, CommandType.LowSpeed },
        };

        private readonly Dictionary<CommandType, Char> _commandToMessage = new Dictionary<CommandType, char>
        {
            { CommandType.Forward, 'f' },
            { CommandType.Stop, 's' },
            { CommandType.Backward, 'b' },
            { CommandType.Left, 'l' },
            { CommandType.Right, 'r' },
            { CommandType.LeftForward, 'q' },
            { CommandType.RightForward, 'e' },
            { CommandType.LeftBackward, 'z' },
            { CommandType.RightBackward, 'c' },
            { CommandType.HighSpeed, 'u' },
            { CommandType.MediumSpeed, 'i' },
            { CommandType.LowSpeed, 'o' },
        };

        private bool _isConnected;
        private BluetoothLEDevice _selectedDevice = null;
        private DeviceWatcher _watcher = null;
        private GattCharacteristic _characteristc = null;
        private readonly CommandManager _commandManager = new CommandManager();

        public void OnSuspended()
        {
#if MOCK
#else
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
#endif
            Window.Current.CoreWindow.KeyDown -= Key_Down;
            Window.Current.CoreWindow.KeyUp -= Key_Up;
            _commandManager.SendCommand -= SendCommand;
        }

        public void OnResumed()
        {
            Initialize();
        }

        private void Initialize()
        {
#if MOCK
#else
            ManualResetEvent deviceFoundSignal = new ManualResetEvent(false);
            string deviceId = string.Empty;

            _watcher = DeviceInformation.CreateWatcher(_aqsAllBLEDevices, _requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);
            _watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) =>
            {
                Debug.WriteLine(devInfo.Name);
                if (devInfo.Name == "HC-08" && devInfo.Id == "BluetoothLE#BluetoothLE9c:b6:d0:97:8e:0c-88:3f:4a:d8:34:5d")
                {
                    deviceId = devInfo.Id;
                    Debug.WriteLine($"Found device. ID: {deviceId}. Name: {devInfo.Name}.");
                    deviceFoundSignal.Set();
                }
            };
            _watcher.Updated += (_, __) => { }; // We need handler for this event, even an empty!
            _watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) => { sender.Stop(); };
            _watcher.Stopped += (DeviceWatcher sender, object arg) => { };
            _watcher.Start();

            deviceFoundSignal.WaitOne();

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
#endif
            Window.Current.CoreWindow.KeyDown += Key_Down;
            Window.Current.CoreWindow.KeyUp += Key_Up;
            _commandManager.SendCommand += SendCommand;
        }

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        private async Task Write(Char c)
        {
#if MOCK
            Debug.WriteLine($"Write message: {c}");
#else
            if (_isConnected)
            {
                IBuffer buffer = Utilities.FormatData(c.ToString(), DataFormat.UTF8);
                await _characteristc.WriteValueAsync(buffer);
            }
#endif
        }

        private async void Forward_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.Forward);
        }

        private async void Forward_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.Forward);
        }

        private async void Stop_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.Stop);
        }

        private async void Stop_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.Stop);
        }

        private async void Back_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.Backward);
        }

        private async void Back_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.Backward);
        }

        private async void LF_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.LeftForward);
        }

        private async void LF_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.LeftForward);
        }

        private async void RF_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.RightForward);
        }

        private async void RF_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.RightForward);
        }

        private async void LB_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.LeftBackward);
        }

        private async void LB_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.LeftBackward);
        }

        private async void RB_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.RightBackward);
        }

        private async void RB_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.RightBackward);
        }

        private async void Left_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.Left);
        }

        private async void Left_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.Left);
        }

        private async void Right_Pressed(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandEnter(CommandType.Right);
        }

        private async void Right_Released(object sender, PointerRoutedEventArgs e)
        {
            await _commandManager.OnDirectionCommandExit(CommandType.Right);
        }

        private async void High_Clicked(object sender, RoutedEventArgs e)
        {
            await _commandManager.OnSpeedControlCommand(CommandType.HighSpeed);
        }

        private async void Medium_Clicked(object sender, RoutedEventArgs e)
        {
            await _commandManager.OnSpeedControlCommand(CommandType.MediumSpeed);
        }

        private async void Low_Clicked(object sender, RoutedEventArgs e)
        {
            await _commandManager.OnSpeedControlCommand(CommandType.LowSpeed);
        }

        private async void Key_Down(CoreWindow sender, KeyEventArgs e)
        {
            CommandType command;
            if (!_keyboardMap.TryGetValue(e.VirtualKey, out command))
            {
                return;
            }

            await _commandManager.OnGeneralCommandEnter(command);
        }

        private async void Key_Up(CoreWindow sender, KeyEventArgs e)
        {
            CommandType command;
            if (!_keyboardMap.TryGetValue(e.VirtualKey, out command))
            {
                return;
            }

            await _commandManager.OnGeneralCommandExit(command);
        }

        private async Task SendCommand(CommandType command)
        {
            await Write(_commandToMessage[command]);
        }
    }
}
