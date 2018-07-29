using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyAttendance
{
    public class Subject
    {

        public int id { get; set; }
        public uint position { get; set; }
        public string name { get; set; }


        public Subject() { }

        public Subject(int id, uint position, string name)
        {
            this.id = id;
            this.position = position;
            this.name = name;
        }


        public override string ToString()
        {
            return name;
        }

    }
}
