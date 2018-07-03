using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace StudyAttendance
{
    public class AttendanceDatabase
    {

        
        const uint UID_MIN = 200;                //Constants used for minimum UID value generation
        const uint UID_MAX = 4294967294;         //Constants used for maximum UID value generation

        MySqlDb db = new MySqlDb("localhost", "study", "root", "");


        /// <summary>
        /// Returns a list of all students in the database.
        /// </summary>
        /// <returns>
        /// A bloody big list if you're not careful.
        /// </returns>
        public List<Student> GetAllStudents()
        {

            List<Student> results = new List<Student>();

            MySqlDataReader reader = db.Query("SELECT * FROM students");

            while (reader.Read())
            {
                Student newStudent = new Student(reader.GetInt32("id"),
                                                 reader.GetUInt32("uid"),
                                                 reader.GetString("firstname"),
                                                 reader.GetString("lastname"));
                results.Add(newStudent);
            }

            reader.Close();
            db.CloseConnection();

            return results;

        }


        /// <summary>
        /// Provides the ability to search for students using their uid.
        /// </summary>
        /// <param name="uid">Should be the FOB uid used to search for the student.</param>
        /// <returns>
        /// True = they're alive and well.
        /// False = check their pulse.
        /// </returns>
        public Student FindStudentByUID(uint uid)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM students WHERE uid={uid}");
            Student result = null;

            if (reader != null && reader.HasRows)
            {
                reader.Read();
                result = new Student(reader.GetInt32("id"),
                                         reader.GetUInt32("uid"),
                                         reader.GetString("firstname"),
                                         reader.GetString("lastname"));
                
            }

            reader.Close();
            db.CloseConnection();
            return result;

        }


        /// <summary>
        /// Checks to see if a student already has an attendance entry today.
        /// </summary>
        /// <param name="student">Uses the id to check for the student.</param>
        /// <returns>
        /// True = they're here already!
        /// False = not in... yet.
        /// </returns>
        public bool AttendanceExistsToday(Student student)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM attendance WHERE DATE(attendance.datetime) = DATE(NOW()) AND studentid={student.id}");
            bool result = (reader != null && reader.HasRows);
            reader.Close();
            db.CloseConnection();
            return result;

        }


        /// <summary>
        /// Adds an entry into the Attendance table. 
        /// </summary>
        /// <param name="student">Uses the id value to insert the attendance record.</param>
        /// <param name="oneEntryPerDay">Limits the number of attendance entries for this student to 1 per day.</param>
        /// <returns>
        /// True = you're good
        /// False = no addy
        /// </returns>
        public bool AddAttendance(Student student, bool oneEntryPerDay = true)
        {
            //If we have one entry per day and it's not added, do that. Otherwise just add the entry
            if ((oneEntryPerDay && AttendanceExistsToday(student) == false) || !oneEntryPerDay)
            {
                db.NonQuery($"INSERT INTO attendance(studentid) VALUES({student.id})");
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Checks to see if the uid passed is associated with an existing student
        /// </summary>
        /// <param name="uid">The student's... uid.</param>
        /// <returns>
        /// True = it's there don't touch it
        /// False = not there, go for gold
        /// </returns>
        public bool DoesUIDExist(uint uid)
        {
            return (Convert.ToInt32(db.Scalar($"SELECT COUNT(id) FROM students WHERE uid={uid}")) > 0);
        }



        /// <summary>
        /// This function is used to produce a new UID used for adding students
        /// 
        /// Function works by randomising between 100 - 65525 because:
        ///     0-99        = reserved for status bytes, probably won't cause problems, but why risk it
        ///     100-65525   = potential tag UIDs
        ///     65526-65535 = reserved for admin tags
        /// </summary>
        /// <returns>
        /// 0 = unable to produce random uid
        /// otherwise = you're good to go
        /// </returns>
        [Obsolete("This function produces a random UID, which isn't required by the system anymore.")]
        public uint GetRandomUID()
        {

            Random random = new Random();
            int attempts = 0;
            int maxAttempts = 100;          //TODO: Consider if this should be a constant
            uint uid = 0;

            //Create a random uid until we get a unique one
            do
            {
                //Thanks https://stackoverflow.com/questions/17080112/generate-random-uint
                uint thirtyBits = (uint)random.Next(1 << 30);
                uint twoBits = (uint)random.Next(1 << 2);
                uid = (thirtyBits << 2) | twoBits;
                attempts++;
            } while (DoesUIDExist(uid) && attempts < maxAttempts);

            //Error handling at its finest, 0 means bad right?
            if (attempts >= maxAttempts)
                return 0;

        
            return uid;

        }


        /// <summary>
        /// Adds a new student to the students table. Note: Allows duplicate students of the same name.
        /// </summary>
        /// <param name="student">The student to be added. MUST HAVE UNIQUE UID.</param>
        /// <returns>If it worked or not.</returns>
        public bool AddStudent(Student student)
        {
            return db.NonQuery($"INSERT INTO students(firstname, lastname, uid) VALUES('{student.firstname}', '{student.lastname}', {student.uid})");
        }


    }
}
