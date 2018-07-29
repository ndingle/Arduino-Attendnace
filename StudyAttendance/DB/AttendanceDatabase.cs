using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace StudyAttendance
{
    public class AttendanceDatabase
    {

       
        MySqlDb db = new MySqlDb("localhost", "study", "root", "");


        /// <summary>
        /// Returns a list of all students in the database.
        /// </summary>
        /// <returns>
        /// A bloody big list if you're not careful.
        /// </returns>
        public List<Student> GetAllStudents(bool activeOnly = true)
        {

            List<Student> results = new List<Student>();
            string q = "SELECT * FROM students";

            if (activeOnly) q += " WHERE active=1";

            MySqlDataReader reader = db.Query(q);

            if (reader != null)
            {

                while (reader.Read())
                {
                    Student newStudent = new Student(reader.GetUInt32("id"),
                                                     reader.GetUInt32("uid"),
                                                     reader.GetUInt32("oasisid"),
                                                     reader.GetString("firstname"),
                                                     reader.GetString("lastname"),
                                                     reader.GetUInt16("finishyear"),
                                                     reader.GetBoolean("active"));
                    results.Add(newStudent);
                }

                reader.Close();

            }

            db.CloseConnection();
            return results;

        }


        /// <summary>
        /// Collects all the subjects in the database.
        /// </summary>
        /// <returns>A list of subjects</returns>
        public List<Subject> GetAllSubjects()
        {

            List<Subject> results = new List<Subject>();
            MySqlDataReader reader = db.Query("SELECT * FROM subjects ORDER BY position");

            if (reader != null)
            {

                while (reader.Read())
                {
                    Subject subject = new Subject(reader.GetInt32("id"),
                                                  reader.GetUInt32("position"),
                                                  reader.GetString("name"));
                    results.Add(subject);
                }

                //Close your stuff mate
                reader.Close();

            }

            db.CloseConnection();

            return results;

        }


        /// <summary>
        /// Provides the ability to search for students using their uid.
        /// </summary>
        /// <param name="uid">Should be the FOB uid used to search for the student.</param>
        /// <returns>
        /// True = they're alive and well. False = check their pulse.
        /// </returns>
        public Student FindStudentByUID(uint uid)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM students WHERE uid={uid}");
            Student result = null;

            if (reader != null && reader.HasRows)
            {
                reader.Read();
                result = new Student(reader.GetUInt32("id"),
                                    reader.GetUInt32("uid"),
                                    reader.GetUInt32("oasisid"),
                                    reader.GetString("firstname"),
                                    reader.GetString("lastname"),
                                    reader.GetUInt16("finishyear"),
                                    reader.GetBoolean("active"));

            }

            reader.Close();
            db.CloseConnection();
            return result;

        }


        /// <summary>
        /// Provides the ability to search for students using their id.
        /// </summary>
        /// <param name="id">The id of the student.</param>
        /// <returns>
        /// True = they're alive and well. False = check their pulse.
        /// </returns>
        public Student FindStudentByID(uint id)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM students WHERE id={id}");
            Student result = null;

            if (reader != null && reader.HasRows)
            {
                reader.Read();
                result = new Student(reader.GetUInt32("id"),
                                    reader.GetUInt32("uid"),
                                    reader.GetUInt32("oasisid"),
                                    reader.GetString("firstname"),
                                    reader.GetString("lastname"),
                                    reader.GetUInt16("finishyear"),
                                    reader.GetBoolean("active"));

            }

            reader.Close();
            db.CloseConnection();
            return result;

        }


        /// <summary>
        /// If there is an id, then the student's details gets edited to the details provided.
        /// </summary>
        /// <param name="student">Provides the id and student details to edit</param>
        /// <returns>True = it worked, False = no id or something worse.</returns>
        public bool EditStudent(Student student)
        {
            return db.NonQuery($"UPDATE students SET firstname='{student.firstname}', lastname='{student.lastname}', finishyear={student.finishyear}, uid={student.uid}, oasisid={student.oasisid} WHERE id={student.id}");
        }


        /// <summary>
        /// Checks to see if a student already has an attendance entry today.
        /// </summary>
        /// <param name="student">Uses the id to check for the student.</param>
        /// <returns>
        /// True = they're here already! False = not logged in... yet.
        /// </returns>
        public bool AttendanceExistsToday(Student student)
        {

            MySqlDataReader reader = db.Query($"SELECT id FROM attendance WHERE DATE(attendance.datetime) = DATE(NOW()) AND studentid={student.id}");
            bool result = (reader != null && reader.HasRows);
            reader.Close();
            db.CloseConnection();
            return result;

        }


        /// <summary>
        /// Adds an entry into the Attendance table. 
        /// </summary>
        /// <param name="student">Uses the id value to insert the attendance record.</param>
        /// <param name="subjectid">ID of the subject that the user selected.</param>
        /// <returns>
        /// True = you're good, False = no addy
        /// </returns>
        public bool AddAttendance(Student student, int subjectid)
        {
            //If we have one entry per day and it's not added, do that. Otherwise just add the entry
            return db.NonQuery($"INSERT INTO attendance(studentid, subjectid) VALUES({student.id}, {subjectid})");
        }


        /// <summary>
        /// Checks to see if the uid passed is associated with an existing student
        /// </summary>
        /// <param name="uid">UID to look for</param>
        /// <returns>
        /// True = It's already used, don't touch it. False = not there, go for gold.
        /// </returns>
        public bool DoesUIDExist(uint uid)
        {
            return (Convert.ToInt32(db.Scalar($"SELECT COUNT(id) FROM students WHERE uid={uid}")) > 0);
        }


        /// <summary>
        /// Checks to see if the oasis id passed is associated with an existing student
        /// </summary>
        /// <param name="oasisId">Oasis Id to look for</param>
        /// <returns>
        /// True = It's already there. False = not there, go for gold.
        /// </returns>
        public bool DoesOasisIDExist(uint oasisId)
        {
            return (Convert.ToInt32(db.Scalar($"SELECT COUNT(oasisid) FROM students WHERE oasisid={oasisId}")) > 0);
        }


        /// <summary>
        /// Adds a new student to the students table. Note: Allows duplicate students of the same name.
        /// </summary>
        /// <param name="student">The student to be added. MUST HAVE UNIQUE UID AND OASISID.</param>
        /// <returns>If it worked or not.</returns>
        public bool AddStudent(Student student)
        {
            return db.NonQuery($"INSERT INTO students(uid, oasisid, firstname, lastname, finishyear) VALUES({student.uid}, {student.oasisid}, '{student.firstname}', '{student.lastname}', {student.finishyear})");
        }


        /// <summary>
        /// Sets student's account to active or not.
        /// </summary>
        /// <param name="student">Student account to deactive.</param>
        /// <param name="value">Set the student acitve or not.</param>
        public bool SetActive(Student student, bool value)
        {
            return SetActive(student.id, value);
        }


        /// <summary>
        /// Sets student's account to active or not.
        /// </summary>
        /// <param name="id">ID of the student</param>
        /// <param name="value">Set the student acitve or not.</param>
        /// <returns></returns>
        public bool SetActive(uint id, bool value)
        {
            return db.NonQuery($"UPDATE students SET active={value} WHERE id={id}");
        }


    }
}
