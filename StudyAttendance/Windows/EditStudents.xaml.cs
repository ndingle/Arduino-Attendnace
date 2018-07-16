using Microsoft.Win32;
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
    /// Interaction logic for EditStudents.xaml
    /// </summary>
    public partial class EditStudents : Window
    {

        AttendanceDatabase database;
        ArduinoSerialComms arduino;

        List<Student> students;


        public EditStudents(AttendanceDatabase database, ArduinoSerialComms arduino)
        {
            InitializeComponent();
            this.database = database;
            this.arduino = arduino;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshStudents();
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Remove the tag event handler
            AddStudent addStudent = new AddStudent(database, arduino);
            addStudent.ShowDialog();
            //TODO: Add the tag event handler
        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

        }


        private void chkShowActive_Checked(object sender, RoutedEventArgs e)
        {
            RefreshStudents();
        }


        private void chkShowActive_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshStudents();
        }


        private void btnBulk_Click(object sender, RoutedEventArgs e)
        {
            BulkAddStudents();
        }


        private void btnDeactivate_Click(object sender, RoutedEventArgs e)
        {
            var student = (Student)((Button)sender).DataContext;
            ToggleStudentActivation(student);
        }



        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var student = (Student)((Button)sender).DataContext;
            //Active the add window but in edit mode
            AddStudent addStudent = new AddStudent(database, arduino, true, student);
            addStudent.ShowDialog();
            RefreshStudents();
        }


        private void btnRemoveUID_Click(object sender, RoutedEventArgs e)
        {

            if (lsvStudents.SelectedItem == null)
                return;

            if (MessageBox.Show($"Do you want to remove {lsvStudents.SelectedItems.Count} UIDs?", "UID Removal", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            foreach (var obj in lsvStudents.SelectedItems)
            {
                var student = (Student)obj;
                student.uid = 0;
                database.EditStudent(student);
            }

            RefreshStudents();

        }


        private void btnFOBs_Click(object sender, RoutedEventArgs e)
        {
            if (lsvStudents.SelectedItem == null)
                return;

            foreach (var obj in lsvStudents.SelectedItems)
            {
                var student = (Student)obj;
                AddFOB(student);
            }

            RefreshStudents();

        }


        /// <summary>
        /// Prompts the user to add a FOB for the given user
        /// </summary>
        /// <param name="student">The student to add the new FOB</param>
        void AddFOB(Student student)
        {

            PopupWindow popup = new PopupWindow($"Scan FOB to add to {student.ToString()}", Brushes.Azure, 0, arduino);
            popup.Owner = this;
            popup.ShowDialog();
            uint uid = popup.UIDResult;

            //No result = get out early
            if (uid == 0)
                return;

            //Check if it exists, otherwise add it to the student
            if (!database.DoesUIDExist(uid))
            {
                student.uid = uid;
                database.EditStudent(student);
            }

        }


        /// <summary>
        /// Actives or deactives student accounts.
        /// </summary>
        /// <param name="student">The student to toggle activation for.</param>
        void ToggleStudentActivation(Student student)
        {
            database.SetActive(student.id, !student.active);
            RefreshStudents();
        }


        /// <summary>
        /// Go through the students provided, ensure they aren't already a 
        /// part of the database and add those who aren't.
        /// </summary>
        /// <param name="students"></param>
        void ProcessStudents(Student[] students)
        {

            //Track students who've been processed and added into the database
            int processed = 0;
            int added = 0;

            foreach (var student in students)
            {

                processed++;

                //If the oasis id is blank or already in the database, we skip.
                if (student.oasisid == 0) continue;
                if (database.DoesOasisIDExist(student.oasisid)) continue;
                if (database.AddStudent(student)) added++;

            }

            MessageBox.Show($"{students.Length} student(s) found.\n{processed} student(s) processed.\n{added} student(s) added.");
            RefreshStudents();

        }


        /// <summary>
        /// Provides the user with an open file dialog and reads through the file 
        /// to find all students. If it succeeds then we use ProcessStudents
        /// </summary>
        void BulkAddStudents()
        {

            OpenFileDialog openFile = new OpenFileDialog()
            {
                Title = "Select Excel file to open...",
                Filter = "Excel files|*.xlsx;*.xls"
            };

            //Show the dialog, if closed we exit early
            if (openFile.ShowDialog().Value == false)
                return;

            //Use the excel reader to get students
            ExcelReader reader = new ExcelReader(openFile.FileName);
            var students = reader.ReadStudents();

            if (students == null)
            {
                MessageBox.Show("Unable to read students from provided Excel file.", "File error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ProcessStudents(students);

        }


        /// <summary>
        /// Update the list of students
        /// </summary>
        void RefreshStudents()
        {
            //This prevents early null errors when the window isn't fully loaded
            if (database == null) return;

            //Load the students and sort them out
            students = database.GetAllStudents(!chkShowActive.IsChecked.Value);
            students.Sort(Student.Compare);
            lsvStudents.ItemsSource = students;
        }

    }
}
