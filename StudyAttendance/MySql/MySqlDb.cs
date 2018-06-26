﻿using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace StudyAttendance
{
    class MySqlDb
    {

        private string _server      = "";
        private string _database    = "";
        private string _uid         = "";
        private string _password    = "";

        private MySqlConnection _connection;


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


        void Initialise()
        {
            _connection = new MySqlConnection(GetConnectionString());
        }


        string GetConnectionString()
        {
            return $"SERVER={_server};DATABASE={_database};UID={_uid};PASSWORD={_password};SSLMODE=0;";
        }


        public bool OpenConnection()
        {

            try
            {
                _connection.Open();
            }
            //TODO: Add error handling
            catch (MySqlException ex)
            {
                return false;
            }

            return (_connection.State == System.Data.ConnectionState.Open);

        }


        public bool CloseConnection()
        {

            try
            {
                _connection.Close();
            }
            //TODO: Add error handling
            catch (MySqlException ex)
            {
                return false;
            }

            return true;

        }


        public void NonQuery(string query)
        {

            if (!OpenConnection())
                return;

            MySqlCommand cmd = new MySqlCommand(query, _connection);
            cmd.ExecuteNonQuery();

            CloseConnection();

        }


        public MySqlDataReader Query(string query)
        {

            if (!OpenConnection())
                return null;

            MySqlCommand cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return reader;

        }


        public int Scalar(string query)
        {

            if (!OpenConnection())
                return -1;

            MySqlCommand cmd = new MySqlCommand(query, _connection);
            int result = -1;

            try
            {
                object value = cmd.ExecuteScalar();
                if (value != null) result = (int)value;
            }
            catch { }

            CloseConnection();
            return result;

        }



    }
}
