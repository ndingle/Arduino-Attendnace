using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToExcel;

namespace StudyAttendance
{
    /// <summary>
    /// Used to read excel files into student lists, ready for database access
    /// </summary>
    class ExcelReader
    {


        private string _filepath = "";


        /// <summary>
        /// Constructor! Pass it a file and magical things will happen.
        /// </summary>
        /// <param name="filepath"></param>
        public ExcelReader(string filepath)
        {
            SetFilepath(filepath);
        }


        /// <summary>
        /// Changes the provided filepath
        /// </summary>
        /// <param name="filepath"></param>
        public void SetFilepath(string filepath)
        {
            if (filepath.Trim().Length > 0) _filepath = filepath;
        }


        /// <summary>
        /// Reads students from the Excel file stored in Filepath.
        /// </summary>
        /// <returns>An array of students. Null array if filepath is empty or an error ocurrs.</returns>
        public Student[] ReadStudents()
        {

            //If we don't have a file path, return the empty list
            if (_filepath == "")
                return null;

            //Read the excel file
            var excel = new ExcelQueryFactory(_filepath);
            try
            {
                var students = from c in excel.Worksheet<Student>(excel.GetWorksheetNames().First().ToString())
                               select c;
                return students.ToArray();
            }
            catch
            {
                return null;
            }

        }


    }
}
