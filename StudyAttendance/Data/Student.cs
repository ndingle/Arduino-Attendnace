namespace StudyAttendance
{
    public class Student
    {

        public int id;
        public ushort uid;
        public string firstname;
        public string lastname;


        public Student(int id, ushort uid, string firstname, string lastname)
        {
            this.id         = id;
            this.uid        = uid;
            this.firstname  = firstname;
            this.lastname   = lastname;
        }

    }
}
