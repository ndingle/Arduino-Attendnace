namespace StudyAttendance
{
    public class Student
    {

        public int id;
        public uint uid;
        public string firstname;
        public string lastname;


        public Student(int id, uint uid, string firstname, string lastname)
        {
            this.id         = id;
            this.uid        = uid;
            this.firstname  = firstname;
            this.lastname   = lastname;
        }


        public override string ToString()
        {
            return $"{firstname} {lastname}";
        }


    }
}
