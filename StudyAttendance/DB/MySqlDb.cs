using MySql.Data.MySqlClient;

namespace StudyAttendance
{

    /// <summary>
    /// A class used by other classes for raw database communication. 
    /// </summary>
    class MySqlDb
    {

        private string _server      = "";
        private string _database    = "";
        private string _uid         = "";
        private string _password    = "";

        private MySqlConnection _connection;


        /// <summary>
        /// Constructor... mate. Send me the server, database, username and password and I'll break in.
        /// </summary>
        /// <param name="server">Where is the database?</param>
        /// <param name="database">What's the database called?</param>
        /// <param name="uid">What's your username?</param>
        /// <param name="password">What's the key?</param>
        public MySqlDb(string server,
                       string database,
                       string uid,
                       string password)
        {
            _server     = server;
            _database   = database;
            _uid        = uid;
            _password   = password;

            Initialise();
        }


        /// <summary>
        /// Creates the connection to the database, used only by the constructor
        /// </summary>
        void Initialise()
        {
            _connection = new MySqlConnection(GetConnectionString());
        }


        /// <summary>
        /// Generates the connection string, really I made this function for abstraction.
        /// </summary>
        /// <returns></returns>
        string GetConnectionString()
        {
            return $"SERVER={_server};DATABASE={_database};UID={_uid};PASSWORD={_password};SSLMODE=0;";
        }


        /// <summary>
        /// Opens the connection to the database.
        /// </summary>
        /// <returns>True, connection was established. False, no connection made.</returns>
        public bool OpenConnection()
        {

            try
            {
                _connection.Open();
            }
            //TODO: Add error handling
            catch
            {
                return false;
            }

            return (_connection.State == System.Data.ConnectionState.Open);

        }


        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <returns>True, the connection was closed. False, connection couldn't be closed (is that good?!).</returns>
        public bool CloseConnection()
        {

            try
            {
                _connection.Close();
            }
            //TODO: Add error handling
            catch 
            {
                return false;
            }

            return true;

        }


        /// <summary>
        /// Performs a query with no expected return from the database. No error returns yet.
        /// </summary>
        /// <param name="query">The query string to be executed</param>
        /// <returns>True, if the query was executed correctly. False, something failed - either connection didn't open or the query was wrong.</returns>
        public bool NonQuery(string query)
        {

            if (!OpenConnection())
                return false;

            bool result = true;
            MySqlCommand cmd = new MySqlCommand(query, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                result = false;
            }

            CloseConnection();
            return result;

        }


        /// <summary>
        /// Performs a query with an expected return from the database.
        /// </summary>
        /// <param name="query">The query string to be executed</param>
        /// <returns>Data reader to read the values from.</returns>
        public MySqlDataReader Query(string query)
        {

            if (!OpenConnection())
                return null;

            MySqlCommand cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return reader;

        }


        /// <summary>
        /// Performs a query with a single expected value from the database.
        /// </summary>
        /// <param name="query">Query string to be executed.</param>
        /// <returns>Object that you'll need to cast to the expected data type. Null if an error occurred, CHECK YOUR NULLS.</returns>
        public object Scalar(string query)
        {

            if (!OpenConnection())
                return -1;

            MySqlCommand cmd = new MySqlCommand(query, _connection);
            object value = null;

            try
            {
                value = cmd.ExecuteScalar();
            }
            catch
            {
                return null;
            }

            CloseConnection();
            return value;

        }



    }
}
