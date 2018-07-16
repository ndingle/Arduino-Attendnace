using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        bool editMode = false;
        Student student = null;
       
        uint currentUid = 0;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        

        public AddStudent(AttendanceDatabase database, ArduinoSerialComms arduino, bool editStudent = false, Student student = null)
        {
            InitializeComponent();
            this.database = database;
            this.arduino = arduino;
            this.arduino.TagReceived += TagReceived;
            editMode = editStudent;

            //If we are editing, the setup the window
            if (editMode)
            {
                this.student = student;
                Title = "Edit student";
                txbTitle.Text = "Edit student";
                txtFirstname.Text = student.firstname;
                txtLastname.Text = student.lastname;
                txtOasisID.Text = student.oasisid.ToString();
                txtFinishYear.Text = student.finishyear.ToString();
                currentUid = student.uid;
                btnSaveNext.IsEnabled = false;

                //If we have a uid, then update the textblock
                if (currentUid != 0)
                {
                    txbUID.Text = $"Tag will be set to {BitConverter.ToString(BitConverter.GetBytes(currentUid))}";
                    txbUID.Foreground = Brushes.PaleGreen;
                }

            }

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
                    currentUid = 0;
                    txbUID.Foreground = Brushes.OrangeRed;
                }
                else
                {
                    currentUid = uid;
                    txbUID.Text = $"Tag will be set to {BitConverter.ToString(BitConverter.GetBytes(currentUid))}";
                    txbUID.Foreground = Brushes.PaleGreen;
                }

            }));
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            //Ensure they filled in the fields first
            if (txtFirstname.Text.Trim().Length == 0 ||
                txtLastname.Text.Trim().Length == 0)
                return;

            Student newStudent;

            if (editMode)
            {
                newStudent = new Student(student.id, currentUid, Convert.ToUInt32(txtOasisID.Text), txtFirstname.Text, txtLastname.Text, Convert.ToUInt16(txtFinishYear.Text), student.active);
                DialogResult = database.EditStudent(newStudent);
            }
            else
            {
                newStudent = new Student(0, currentUid, Convert.ToUInt32(txtOasisID.Text), txtFirstname.Text, txtLastname.Text, Convert.ToUInt16(txtFinishYear.Text), true);
                DialogResult = database.AddStudent(newStudent);
            }

            //Return if it worked or not and close
            Close();

        }

        private void btnSaveNext_Click(object sender, RoutedEventArgs e)
        {

            //Ensure they filled in the fields first
            if (txtFirstname.Text.Trim().Length == 0 ||
                txtLastname.Text.Trim().Length == 0)
                return;

            Student newStudent = new Student(0, currentUid, Convert.ToUInt32(txtOasisID.Text), txtFirstname.Text, txtLastname.Text, Convert.ToUInt16(txtFinishYear.Text), true);

            //Return if it worked or not and close
            bool result = database.AddStudent(newStudent);

            if (!result)
                MessageBox.Show("Unable to add student, database error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                txtFirstname.Clear();
                txtLastname.Clear();
                txtFinishYear.Clear();
                txtOasisID.Clear();
                currentUid = 0;
                txbUID.Text = "Tap FOB to set UID";
                txbUID.Foreground = Brushes.OrangeRed;
            }

        }


        private void txtOasisID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }


        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }


        private void txtFinishYear_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

    }
}
