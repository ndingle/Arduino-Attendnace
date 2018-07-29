using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace StudyAttendance
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        //Main variables for database and arduino communication
        AttendanceDatabase database = new AttendanceDatabase();
        ArduinoSerialComms arduino;
        Timer arduinoTimer;

        //Main variables, the list of students and the current student to log in
        List<Student> students = new List<Student>();
        Student loginStudent = null;

        //TODO: Consider making this nicer
        //Variables used for the animation of the grid columns
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
            LoadSubjects();

            //Setup the connection to the Arduino
            arduino = new ArduinoSerialComms(9600);
            arduino.TagReceived += TagReceived;

            //Setup the timer
            arduinoTimer = new Timer(1000);
            arduinoTimer.Elapsed += CheckArduinoStatus;
            arduinoTimer.AutoReset = true;
            arduinoTimer.Start();

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
            Login(selectedStudent);

        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            //Reset the subject selection
            loginStudent = null;
            ShowStudentSelection();
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
        /// Collects subjects from the database and loads them into the grid
        /// </summary>
        void LoadSubjects()
        {

            List<Subject> subjects = database.GetAllSubjects();
            int column = 0;
            int row = 1;

            foreach (Subject subject in subjects)
            {
                
                var textBlock = new TextBlock()
                {
                    Text = subject.ToString(),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 48
                };

                var btn = new Button()
                {
                    Content = textBlock,
                    Tag = subject
                };

                btn.Click += SubjectButton_Click;

                //Add button to the grid
                subjectGrid.Children.Add(btn);
                Grid.SetColumn(btn, column);
                Grid.SetRow(btn, row);

                column++;

                //If we got past column count, then we add a new row and reset counters
                if (column == subjectGrid.ColumnDefinitions.Count)
                {
                    var rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(1, GridUnitType.Star);
                    subjectGrid.RowDefinitions.Add(rowDefinition);
                    row++;
                    column = 0;
                }

            }

        }


        /// <summary>
        /// Event handler used by the generated subject buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SubjectButton_Click(object sender, RoutedEventArgs e)
        {

            //Don't we all want to wrte something ugly!!! I Do.
            int subjectid = ((sender as Button).Tag as Subject).id;

            if (database.AddAttendance(loginStudent, subjectid))
                ShowPopup($"Welcome {loginStudent.ToString()}", Brushes.PaleGreen);
            else
                ShowPopup("Something happened...", Brushes.Aqua);

            ShowStudentSelection();
            loginStudent = null;

        }


        /// <summary>
        /// The receiver event from the arduino
        /// </summary>
        /// <param name="uid">The UID received</param>
        void TagReceived(uint uid)
        {
            Student student = database.FindStudentByUID(uid);

            //Run on the main thread since this is an async and its causing me a bloody headache with random catches...
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Login(student);
            }));
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
        void ShowStudentSelection()
        {
            showStudentsColumn.Begin();
            hideSubjectsColumn.Begin();
        }


        /// <summary>
        /// The login function that will attempt to log in a user
        /// </summary>
        /// <param name="student">The student we will attempt to login</param>
        void Login(Student student)
        {

            //If we found them, then add them (if they haven't already signed in)
            bool found = (student != null);

            bool loggedIn = database.AttendanceExistsToday(student);

            if (found && loggedIn)
                ShowPopup($"You're already here {student.ToString()}", Brushes.PaleGreen);
            else if (!found)
                ShowPopup($"FOB not registered.", Brushes.OrangeRed);
            else
            {
                loginStudent = student;
                ShowSubjectSelection();
            }

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
                arduino.TagReceived -= TagReceived;

                EditStudents adminWindow = new EditStudents(database, arduino);
                adminWindow.Owner = this;
                bool? result = adminWindow.ShowDialog();

                //Reattach the arduino event
                arduino.TagReceived += TagReceived;

            }));

        }


        /// <summary>
        /// Used by the timer class to ensure the ardunio stays
        /// connected and will attempt to reestablish a connection.
        /// </summary>
        void CheckArduinoStatus(Object source, ElapsedEventArgs e)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                
                //Check if arduino is still connected
                if (!arduino.IsOpen())
                    arduino.Connect();

            }));

        }

    }
}
