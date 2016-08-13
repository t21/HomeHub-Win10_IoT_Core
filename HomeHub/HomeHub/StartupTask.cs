using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Advertisement;
using System.Diagnostics;
using Windows.System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace HomeHub
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        BluetoothLEAdvertisementWatcher watcher;
        private Guid WALNUT_UUID = new Guid("00001100-0f58-2ba7-72c3-4d8d58fa16de");
        private IList<Walnut> walnutList = new List<Walnut>();
        private ThreadPoolTimer timer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(30));

            watcher = new BluetoothLEAdvertisementWatcher();
            //watcher.AdvertisementFilter.Advertisement.LocalName = "W";
            //watcher.AdvertisementFilter.Advertisement.ServiceUuids += WALNUT_UUID;
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -90;
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -93;
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromSeconds(60);
            watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(1000);
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
            watcher.Received += OnAdvertisementReceived;
            watcher.Stopped += Watcher_Stopped;
            Debug.WriteLine("Init ready ...");
            Debug.WriteLine("Starting scan ...");
            watcher.Start();
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            if (eventArgs.AdvertisementType.Equals(BluetoothLEAdvertisementType.ConnectableUndirected))
            {
                bool walnutFound = false;
                foreach (Guid uuid in eventArgs.Advertisement.ServiceUuids)
                {
                    if (uuid.Equals(WALNUT_UUID))
                    {
                        walnutFound = true;
                        break;
                    }
                }

                if (walnutFound)
                {
                    Walnut walnut = new Walnut(eventArgs);

                    bool walnutFoundInList = false;

                    foreach (Walnut w in walnutList)
                    {
                        if (w.Address.Equals(walnut.Address))
                        {
                            Debug.WriteLine("Device found in list");
                            walnutFoundInList = true;
                            // Uppdatera data
                            break;
                        }
                    }

                    if (!walnutFoundInList)
                    {
                        Debug.WriteLine("New device");
                        walnutList.Add(walnut);
                    }

                    Debug.Write("Received: ");
                    Debug.WriteLine(eventArgs.Timestamp.ToString());

                    Debug.Write(eventArgs.Advertisement.LocalName);
                    Debug.Write(" ");
                    Debug.Write(walnut.AddressToString());
                    Debug.Write(" ");
                    Debug.WriteLine(eventArgs.RawSignalStrengthInDBm + "dBm");

                    foreach (BluetoothLEManufacturerData manufData in eventArgs.Advertisement.ManufacturerData)
                    {
                        Debug.WriteLine(manufData.ToString());
                        Debug.WriteLine(manufData.CompanyId);
                        Debug.WriteLine(manufData.Data);
                        Debug.WriteLine(manufData.Data.Capacity);
                        Debug.WriteLine(manufData.Data.Length);
                    }

                    Debug.WriteLine("");
                }
            }
            else if (eventArgs.AdvertisementType.Equals(BluetoothLEAdvertisementType.ScanResponse))
            {
                foreach (Walnut w in walnutList)
                {
                    if (w.Address.Equals(eventArgs.BluetoothAddress))
                    {
                        // Uppdatera data
                        foreach (BluetoothLEManufacturerData manufData in eventArgs.Advertisement.ManufacturerData)
                        {
                            Debug.WriteLine(w.AddressToString());
                            //Debug.WriteLine(manufData.ToString());
                            //Debug.WriteLine(manufData.CompanyId);
                            //Debug.WriteLine(manufData.Data);
                            byte[] bArray = manufData.Data.ToArray();
                            //Debug.WriteLine(manufData.Data.Capacity);
                            //Debug.WriteLine(manufData.Data.Length);
                            for (int i = 0; i < manufData.Data.Length; i++)
                            {
                                Debug.Write(string.Format("{0:x2}", bArray[i]) + " ");
                            }
                            Debug.WriteLine("");
                        }

                        Debug.WriteLine("");

                        break;
                    }
                }
            }



            //var address = eventArgs.BluetoothAddress;

            //BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);

            //var cnt = device.GattServices.Count;
        }

        private void Watcher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            Debug.WriteLine("Stopped.");
            Debug.WriteLine(args.Error.ToString());
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            //Debug.WriteLine("Stopping scan ...");
            //watcher.Stop();
            timer.Cancel();
        }

    }
}
