using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;

namespace StudyAttendance
{

    public enum ArduinoSerialCommsStatus
    {
        //Good messages
        OK = 0,
        DUPLICATE_LOGIN,

        //Bad messages
        UNKNOWN_UID,
        EXISTS_IN_DATABASE
    }


    public class ArduinoSerialComms
    {


        private SerialPort _port;
        private List<byte> _dataBuffer;

        public static byte END_OF_PACKET_CHAR = 10;

        public delegate void TagReceivedHandler(uint uid);
        public event TagReceivedHandler TagReceived;


        /// <summary>
        /// Constructor function, that takes the baud rate.
        /// </summary>
        /// <param name="baud">The baud rate to connect at.</param>
        public ArduinoSerialComms(int baud)
        {
            Init(baud);
        }


        /// <summary>
        /// Used as the initial connect from the constructor.
        /// </summary>
        /// <param name="baud">The baud rate to connect at.</param>
        void Init(int baud)
        {

            _dataBuffer = new List<byte>();
            string comPort = FindConnectedArduino();
            if (comPort == "")
                return;

            OpenPort(comPort, baud);

        }


        /// <summary>
        /// Finds a connected Arduino and returns the COM port. 
        /// WARNING: Will only return the first Arduino, perhaps I need to add a skip paramter... nah.
        /// </summary>
        /// <returns>Ze COM port an Arduino is connected to.</returns>
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


        /// <summary>
        /// Attempt to open the given PORT at the given baud rate. 
        /// </summary>
        /// <param name="comPort">Ze COM Port.</param>
        /// <param name="baud">Ze Baud rate.</param>
        /// <returns>True if succeeded, false if failed.</returns>
        bool OpenPort(string comPort, int baud)
        {

            _port = new SerialPort(comPort, baud);
            try
            {
                _port.Open();
                if (_port.BytesToRead > 0) _port.ReadLine();
                _port.DataReceived += DataReceivedEvent;
                return true;
            }
            catch 
            {
                _port = null;
            }

            return false;
            
        }


        /// <summary>
        /// Event that is used for the basic reading of bytes and uid information from an Arduino
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataReceivedEvent(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                //Collect the data
                int newlineIndex = -1;

                //TODO: Add a timeout
                while (newlineIndex == -1)
                {

                    byte[] buffer = new byte[_port.BytesToRead];
                    _port.Read(buffer, 0, _port.BytesToRead);
                    _dataBuffer.AddRange(buffer);

                    newlineIndex = _dataBuffer.IndexOf(END_OF_PACKET_CHAR);

                }

                uint value = 0;

                //If we have a newline character, process the tag
                if (newlineIndex > -1)
                {
                    value = ConvertTag(_dataBuffer.GetRange(0, newlineIndex).ToArray());

                    //Pass the data onward
                    TagReceived(value);
                }

                //Take away the stuff to the right of the newline char
                if (_dataBuffer.Count > newlineIndex + 1)
                    _dataBuffer = _dataBuffer.GetRange(newlineIndex + 1, _dataBuffer.Count - (newlineIndex + 1));
                else
                    _dataBuffer.Clear();

            }
            //TODO: Look at the error handling of this
            catch { }

        }


        /// <summary>
        /// This function converts raw data from byte[] to its uint equivalent
        /// </summary>
        /// <param name="data">The array of byte[] to convert</param>
        /// <returns>The uint version of awesomeness</returns>
        uint ConvertTag(byte[] data)
        {
            
            //Depending on the system setup, we need to reverse the array first
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            return BitConverter.ToUInt32(data, 0);

        }


        /// <summary>
        /// Price check on connected Arduino.
        /// </summary>
        /// <returns>True if port is not null and connected. False for other things.</returns>
        public bool IsOpen()
        {

            if (_port == null)
                return false;

            return _port.IsOpen;

        }


        /// <summary>
        /// Sending data down the pipe.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
