using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace StudyAttendance
{
    public class AttendanceDatabase
    {

        MySqlDb db = new MySqlDb("localhost", "study", "root", "");


        public List<Student> GetAllStudents()
        {

            List<Student> results = new List<Student>();

            MySqlDataReader reader = db.Query("SELECT * FROM students");

            while (reader.Read())
            {
                Student newStudent = new Student(reader.GetInt32("id"),
                                                 reader.GetUInt16("uid"),
                                                 reader.GetString("firstname"),
                                                 reader.GetString("lastname"));
                results.Add(newStudent);
            }

            reader.Close();
            db.CloseConnection();

            return results;

        }


        public Student FindStudentByUID(ushort uid)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM students WHERE uid={uid}");
            Student result = null;

            if (reader != null && reader.HasRows)
            {
                reader.Read();
                result = new Student(reader.GetInt32("id"),
                                         reader.GetUInt16("uid"),
                                         reader.GetString("firstname"),
                                         reader.GetString("lastname"));
                
            }

            reader.Close();
            db.CloseConnection();
            return result;

        }


        public bool AttendanceExistsToday(int studentid)
        {

            MySqlDataReader reader = db.Query($"SELECT * FROM attendance WHERE DATE(attendance.datetime) = DATE(NOW()) AND studentid={studentid}");
            bool result = (reader != null && reader.HasRows);
            reader.Close();
            db.CloseConnection();
            return result;

        }


        public bool AddAttendance(Student student, bool oneEntryPerDay = true)
        {
            //If we have one entry per day and it's not added, do that. Otherwise just add the entry
            if ((oneEntryPerDay && AttendanceExistsToday(student.id) == false) || !oneEntryPerDay)
            {
                db.NonQuery($"INSERT INTO attendance(studentid) VALUES({student.id})");
                return true;
            }
            else
                return false;
        }


    }
}
