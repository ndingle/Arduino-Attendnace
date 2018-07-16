using LinqToExcel.Attributes;

namespace StudyAttendance
{
    public class Student
    {

        
        public int id { get; set; }
        public uint uid { get; set; }
        [ExcelColumn("oasis id")]
        public uint oasisid { get; set; }
        [ExcelColumn("firstname")]
        public string firstname { get; set; }
        [ExcelColumn("lastname")]
        public string lastname { get; set; }
        [ExcelColumn("finish year")]
        public ushort finishyear { get; set; }
        public bool active { get; set; }


        public string uidcolumn
        {
            get
            {
                if (uid == 0)
                    return "Not set";
                else
                    return "Set";
            }
            
        }


        /// <summary>
        /// Parameterless constructor for LinqToExcel...
        /// </summary>
        public Student() { }


        public Student(int id, 
                       uint uid, 
                       uint oasisid, 
                       string firstname, 
                       string lastname, 
                       ushort finishyear,
                       bool active)
        {
            this.id         = id;
            this.uid        = uid;
            this.oasisid    = oasisid;
            this.firstname  = firstname;
            this.lastname   = lastname;
            this.finishyear = finishyear;
            this.active     = active;
        }


        public override string ToString()
        {
            return $"{firstname} {lastname}";
        }


        public static int Compare(Student x, Student y)
        {
            return string.Compare(x.lastname, y.lastname);
        }


    }
}
