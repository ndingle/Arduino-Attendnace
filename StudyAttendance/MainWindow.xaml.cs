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

        List<Student> students = new List<Student>();


        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //Setup the student list and sort it 
            LoadStudents();

            //Setup the connection to the Arduino
            ardunio = new ArduinoSerialComms(9600);
            ardunio.TagReceived += TagReceived;

            //UNDONE: This
            ShowAddStudent();

        }


        void LoadStudents()
        {
            //Load from the database, sort them by last name and then refresh the list
            students = database.GetAllStudents();
            students.Sort(Compare);
            lstStudents.ItemsSource = students;
        }


        public int Compare(Student x, Student y)
        {
            return string.Compare(x.lastname, y.lastname);
        }


        void TagReceived(uint uid)
        {
            Login(uid);
        }


        void Login(uint uid)
        {

            // Are they in the database?
            Student student = database.FindStudentByUID(uid);

            //If we found them, then add them (if they haven't already signed in)
            bool found = (student != null);
            bool added = false;
            if (found) added = database.AddAttendance(student, true);

            if (found && added)
                ShowPopup($"Welcome {student.ToString()}", Brushes.PaleGreen);
            else if (found && !added)
                ShowPopup($"You're already here {student.ToString()}", Brushes.PaleGreen);
            else if (!found)
                ShowPopup($"FOB not registered.", Brushes.OrangeRed);

        }


        //void ProcessTag(byte[] tag)
        //{



        //    Debug.Write("Found tag: " + BitConverter.ToString(tag));

        //    //Find the student in the database
        //    uint uid = BitConverter.ToUInt32(tag, 0);

        //    //Check for built in tags
        //    //if (uid == 3392)
        //    //{
        //    //    //Admin mode! Stupid threading, show the window
        //    //    //Dispatcher.BeginInvoke(new ThreadStart(() => Show()));
        //    //    SendReply(ArduinoSerialComms.ADMIN_MSG_BYTE);
        //    //    return;
        //    //}


        //    //Determine what was returned
        //    bool added = false;
        //    bool found = (student != null);
        //    Debug.WriteLine($" = {found}");
        //    if (found) added = database.AddAttendance(student, true);

        //    //Send a message back to arduino
        //    if (found && added)
        //    {
        //        SendReply(ArduinoSerialComms.GOOD_MSG_BYTE);
        //        ShowPopup($"Welcome {student.ToString()}", Brushes.PaleGreen);
        //    }
        //    else if (found && !added)
        //    {
        //        SendReply(ArduinoSerialComms.DUPLICATE_MSG_BYTE);
        //        ShowPopup($"You're already here {student.ToString()}", Brushes.PaleGreen);
        //    }
        //    else
        //    {
        //        SendReply(ArduinoSerialComms.BAD_MSG_BYTE);
        //        ShowPopup($"Can't recognise FOB.", Brushes.OrangeRed);
        //    }

        //}


        void SendReply(byte message)
        {
            ardunio.SendData(new byte[] { message, ArduinoSerialComms.END_OF_PACKET_CHAR });
        }


        void ShowPopup(string message, Brush colour)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PopupWindow popup = new PopupWindow(message, colour, 2500);
                popup.Owner = this;
                popup.ShowDialog();
            }));
            
        }


        void ShowAddStudent()
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {

                //Remove the tag received event for this form
                ardunio.TagReceived -= TagReceived;

                AddStudent addStudent = new AddStudent(database, ardunio);
                addStudent.Owner = this;
                bool? result = addStudent.ShowDialog();

                //Reattach the arduino event
                ardunio.TagReceived += TagReceived;

                //Check if we got a result for the adding
                if (result.Value)
                {
                    ShowPopup("Student added!", Brushes.ForestGreen);
                    LoadStudents();
                }


            }));
        }


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {

            //I don't think they could ever not have something, but better safe than sorry
            if (lstStudents.SelectedItem == null)
                return;

            //Log them in then
            Student selectedStudent = (Student)lstStudents.SelectedItem;
            Login(selectedStudent.uid);

        }

    }
}
