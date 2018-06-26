using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;

namespace StudyAttendance
{
    public class ArduinoSerialComms
    {


        private SerialPort _port;

        public delegate void DataReceivedHandler(byte[] bytes);
        public event DataReceivedHandler DataReceived;


        public static byte NEWLINE_BYTE         = 10;
        public static byte GOOD_MSG_BYTE        = 1;
        public static byte BAD_MSG_BYTE         = 2;
        public static byte DUPLICATE_MSG_BYTE   = 3;
        public static byte ADMIN_MSG_BYTE       = 4;


        public ArduinoSerialComms(int baud)
        {
            Init(baud);
        }


        void Init(int baud)
        {

            string comPort = FindConnectedArduino();
            if (comPort == "")
                return;

            OpenPort(comPort, baud);

        }


        string FindConnectedArduino()
        {

            ManagementObjectSearcher manObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
            ManagementObjectCollection manObjReturn = manObjSearch.Get();

            foreach (ManagementObject manObj in manObjReturn)
            { 
                String portName = manObj["Name"].ToString();
                if (portName.Contains("Arduino")) return manObj["DeviceID"].ToString();
            }
            return "";

        }


        bool OpenPort(string comPort, int baud)
        {

            _port = new SerialPort(comPort, baud);
            try
            {
                _port.Open();
                _port.DataReceived += DataReceivedEvent;
                return true;
            }
            catch 
            {
                _port = null;
            }

            return false;
            
        }


        void DataReceivedEvent(object sender, SerialDataReceivedEventArgs e)
        {

            //Collect the data
            byte[] buffer = new byte[_port.BytesToRead];
            _port.Read(buffer, 0, _port.BytesToRead);

            //Pass the data onward
            DataReceived(buffer);

        }


        public bool IsOpen()
        {

            if (_port == null)
                return false;

            return _port.IsOpen;

        }


        public bool SendData(byte[] data)
        {

            //If we don't have a good port, don't send
            if (!IsOpen())
                return false;

            //Send the data down the port and all is good with the world
            _port.Write(data, 0, data.Length);
            return true;

        }


    }
}
