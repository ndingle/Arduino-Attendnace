using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StudyAttendance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        AttendanceDatabase database = new AttendanceDatabase();
        ArduinoSerialComms ardunio;

        List<byte> dataBuffer = new List<byte>();

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            ardunio = new ArduinoSerialComms(9600);
            ardunio.DataReceived += DataReceivedOnPort;
            Hide();

        }

        void DataReceivedOnPort(byte[] data)
        {
            dataBuffer.AddRange(data);
            int newlineIndex = dataBuffer.IndexOf(ArduinoSerialComms.NEWLINE_BYTE);

            if (newlineIndex > -1)
                ProcessTag(dataBuffer.GetRange(0, newlineIndex - 1).ToArray());

            //Take away the stuff to the right of the newline char
            if (dataBuffer.Count > newlineIndex + 1)
                dataBuffer = dataBuffer.GetRange(newlineIndex + 1, dataBuffer.Count - (newlineIndex + 1));
            else
                dataBuffer.Clear();
        }


        void ProcessTag(byte[] tag)
        {
            //TODO: Make this good
            Debug.Write("Found tag: " + BitConverter.ToString(tag));

            //Find the student in the database
            ushort uid = BitConverter.ToUInt16(tag, 0);

            //Check for built in tags
            if (uid == 3392)
            {
                //Admin mode! Stupid threading, show the window
                Dispatcher.BeginInvoke(new ThreadStart(() => Show()));
                SendReply(ArduinoSerialComms.ADMIN_MSG_BYTE);
                return;
            }

            Student student = database.FindStudentByUID(uid);

            //Determine what was returned
            bool added = false;
            bool found = (student != null);
            Debug.WriteLine($" = {found}");
            if (found) added = database.AddAttendance(student, false);
            
            //Send a message back to arduino
            if (found && added)
                SendReply(ArduinoSerialComms.GOOD_MSG_BYTE);
            else if (found && !added)
                SendReply(ArduinoSerialComms.DUPLICATE_MSG_BYTE);
            else
                SendReply(ArduinoSerialComms.BAD_MSG_BYTE);

        }


        void SendReply(byte message)
        {
            ardunio.SendData(new byte[] { message, ArduinoSerialComms.NEWLINE_BYTE });
        }


    }
}
