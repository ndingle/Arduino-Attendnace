﻿using System;
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
using System.Windows.Media.Animation;
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

        Storyboard showStudentsColumn;
        Storyboard hideStudentsColumn;
        Storyboard showSubjectsColumn;
        Storyboard hideSubjectsColumn;


        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //Grab the animations
            showStudentsColumn = FindResource("ShowStudents") as Storyboard;
            hideStudentsColumn = FindResource("HideStudents") as Storyboard;
            showSubjectsColumn = FindResource("ShowSubjects") as Storyboard;
            hideSubjectsColumn = FindResource("HideSubjects") as Storyboard;

            //Setup the student list and sort it 
            LoadStudents();

            //Setup the connection to the Arduino
            ardunio = new ArduinoSerialComms(9600);
            ardunio.TagReceived += TagReceived;

            //UNDONE: Debug code
            //ShowAdminWindow();

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


        /// <summary>
        /// Loads the students from the database and sorts it into the listview
        /// </summary>
        void LoadStudents()
        {
            students = database.GetAllStudents();
            students.Sort(Student.Compare);
            lstStudents.ItemsSource = students;
        }


        /// <summary>
        /// The receiver event from the arduino
        /// </summary>
        /// <param name="uid">The UID received</param>
        void TagReceived(uint uid)
        {
            Login(uid);
        }


        /// <summary>
        /// Starts the subject grid animations
        /// </summary>
        void ShowSubjectSelection()
        {
            hideStudentsColumn.Begin();
            showSubjectsColumn.Begin();
        }


        /// <summary>
        /// Starts the student grid animations
        /// </summary>
        void showStudentSelection()
        {
            showStudentsColumn.Begin();
            hideSubjectsColumn.Begin();
        }


        /// <summary>
        /// The login function that will attempt to log in a user
        /// </summary>
        /// <param name="uid">The UID that will attempt to login</param>
        void Login2(uint uid)
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


        /// <summary>
        /// Opens the popup window for 2.5 seconds
        /// </summary>
        /// <param name="message">The text to be displayed</param>
        /// <param name="colour">Colour to be used on the window</param>
        void ShowPopup(string message, Brush colour)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PopupWindow popup = new PopupWindow(message, colour, 2500);
                popup.Owner = this;
                popup.ShowDialog();
            }));
            
        }


        /// <summary>
        /// Opens the admin window
        /// </summary>
        void ShowAdminWindow()
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {

                //Remove the tag received event for this form
                ardunio.TagReceived -= TagReceived;

                EditStudents adminWindow = new EditStudents(database, ardunio);
                adminWindow.Owner = this;
                bool? result = adminWindow.ShowDialog();

                //Reattach the arduino event
                ardunio.TagReceived += TagReceived;

            }));

        }

    }
}
