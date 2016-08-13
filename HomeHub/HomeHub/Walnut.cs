using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace HomeHub
{
    class Walnut
    {
        private ulong _address;

        public Walnut(BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            _address = eventArgs.BluetoothAddress;
        }

        public ulong Address
        {
            get
            {
                return _address;
            }
        }

        public string AddressToString()
        {
            string result = "";
            ulong temp = _address;
            byte[] addr = new byte[6];

            for (int i = 0; i < 6; i++)
            {
                addr[i] = (byte)(temp & 0xFF);
                temp = temp >> 8;
            }

            for (int i = 5; i > 0; i--)
            {
                result += string.Format("{0:x2}", addr[i]);
                result += ":";
            }

            result += string.Format("{0:x2}", addr[0]);

            return result;
        }

    }
}
