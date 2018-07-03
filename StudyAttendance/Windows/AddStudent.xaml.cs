using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StudyAttendance
{
    /// <summary>
    /// Interaction logic for AddStudent.xaml
    /// </summary>
    public partial class AddStudent : Window
    {

        AttendanceDatabase database;
        ArduinoSerialComms arduino;
       
        bool hasUID = false;
        uint currentUid = 0;


        public AddStudent(AttendanceDatabase database, ArduinoSerialComms arduino)
        {
            InitializeComponent();
            this.database = database;
            this.arduino = arduino;
            this.arduino.TagReceived += TagReceived;
        }


        void TagReceived(uint uid)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Student student = database.FindStudentByUID(uid);

                //Depending on the result, we either store the info or clear it
                if (student != null)
                {
                    txbUID.Text = $"Tag is already associated with {student.ToString()}";
                    hasUID = false;
                    currentUid = 0;
                    txbUID.Foreground = Brushes.OrangeRed;
                }
                else
                {
                    currentUid = uid;
                    hasUID = true;
                    txbUID.Text = $"Tag will be set to {BitConverter.ToString(BitConverter.GetBytes(currentUid))}";
                    txbUID.Foreground = Brushes.PaleGreen;
                }

            }));
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            //Ensure they filled in the fields first
            if (txtFirstname.Text.Trim().Length == 0 ||
                txtLastname.Text.Trim().Length == 0 ||
                !hasUID)
                return;

            Student newStudent = new Student(0, currentUid, txtFirstname.Text, txtLastname.Text);

            //Return if it worked or not and close
            DialogResult = database.AddStudent(newStudent);
            Close();

        }

        private void btnSaveNext_Click(object sender, RoutedEventArgs e)
        {

            //Ensure they filled in the fields first
            if (txtFirstname.Text.Trim().Length == 0 ||
                txtLastname.Text.Trim().Length == 0 ||
                !hasUID)
                return;

            Student newStudent = new Student(0, currentUid, txtFirstname.Text, txtLastname.Text);

            //Return if it worked or not and close
            bool result = database.AddStudent(newStudent);

            if (!result)
                MessageBox.Show("Unable to add student, database error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                txtFirstname.Clear();
                txtLastname.Clear();
                hasUID = false;
                currentUid = 0;
                txbUID.Text = "Tap FOB to set UID";
                txbUID.Foreground = Brushes.OrangeRed;
            }

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!hasUID)
                DialogResult = null;
        }

    }
}
