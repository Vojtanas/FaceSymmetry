/* 
Application for evaluation of facial symmetry using Microsoft Kinect v2.
Copyright(C) 2017  Sedlák Vojtěch (Vojta.sedlak@gmail.com)

This file is part of FaceSymmetry. 

FaceSymmetry is free software: you can redistribute it and/or modify 
it under the terms of the GNU General Public License as published by 
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version. 

FaceSymmetry is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU General Public License for more details. 

You should have received a copy of the GNU General Public License 
along with Application for evaluation of facial symmetry using Microsoft Kinect v2.. If not, see <http://www.gnu.org/licenses/>.
 */ 

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;

namespace Common
{
    public static class DBAdapter
    {
        private static MySqlConnection _connection;
        private static string _server;
        private const string _database = "facesymmetry";
        private const string _uid = "faceSymmetryApplication";
        private const string _password = "Jsidw4784sD875F7Pls01254SD68VcWx01";

        static DBAdapter()
        {
            Initialize();
        }

        private static void Initialize()
        {
            _server = Settings.Server;
            string connectionString;
            connectionString = "SERVER=" + _server + ";" + "DATABASE=" +
            _database + ";" + "UID=" + _uid + ";" + "PASSWORD=" + _password + ";";

            _connection = new MySqlConnection(connectionString);

        }

        public static void SetServer(string server)
        {
            _server = server;
            string connectionString;
            connectionString = "SERVER=" + _server + ";" + "DATABASE=" +
            _database + ";" + "UID=" + _uid + ";" + "PASSWORD=" + _password + ";";

            _connection = new MySqlConnection(connectionString);
        }

        public static bool TestConnection()
        {

            bool test = OpenConnection();
            CloseConnection();
            return test;
        }

        private static bool OpenConnection()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                StringBuilder message = new StringBuilder();
                switch (ex.Number)
                {
                    case 0:
                        message.Append("Cannot connect to server.  Contact administrator\n\n");
                        break;

                    case 1045:
                        message.Append("Invalid username/password, please try again\n\n");
                        break;
                }

                message.Append(string.Format("{0}", ex.Message));

                throw new DataBaseException(message.ToString(), ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, ex.InnerException);
            }


        }

        private static bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static bool Insert(string table, string[] column, string[] value, MySqlTransaction transaction = null)
        {

            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            int x = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Length != 0)
                {
                    if (x > 0)
                    {
                        columns.Append(",");
                        values.Append(",");
                    }
                    columns.Append(column[i]);
                    values.Append("'" + value[i] + "'");
                    x++;
                }
            }

            string query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, columns, values);

            if (transaction != null || OpenConnection() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection, transaction);
                    cmd.CommandTimeout = int.MaxValue;
                    cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (transaction == null)
                        CloseConnection();
                }
            }
            else
            {
                return false;
            }
        }

        public static void Update(string table, string[] column, string[] value, string where, MySqlTransaction transaction = null)
        {
            if (column.Length != value.Length)
                throw new ArgumentException("Column and values does not have same length");

            StringBuilder set = new StringBuilder();

            for (int i = 0; i < column.Length; i++)
            {
                if (i > 0)
                    set.Append(",");

                set.Append(column[i].ToString());
                set.Append("='");
                set.Append(value[i]);
                set.Append("'");
            }

            string query = string.Format("UPDATE {0} SET {1} WHERE {2}", table, set, where);


            if (transaction != null || OpenConnection() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection, transaction);
                    cmd.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (transaction == null)
                        CloseConnection();
                }
            }
        }

        public static void Delete(string table, string where, MySqlTransaction transaction = null)
        {
            string query = string.Format(@"DELETE FROM {0} WHERE {1}", table, where);

            if (transaction != null || OpenConnection() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection, transaction);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (transaction == null)
                        CloseConnection();
                }
            }
        }

        public static List<string>[] SelectL(string table, string[] what = null, string where = null, string join = null)
        {
            string select;
            if (what == null)
                select = "*";
            else
                select = string.Join(",", what);

            if (where == null)
                where = "";
            else
                where = "WHERE " + where;

            if (join == null)
                join = "";

            string query = string.Format("SELECT {0} FROM {1} {3} {2}", select, table, where, join);

            List<string>[] list;

            if (OpenConnection() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();


                    list = new List<string>[dataReader.FieldCount];
                    for (int i = 0; i < list.Length; i++)
                        list[i] = new List<string>();



                    while (dataReader.Read())
                    {
                        for (int i = 0; i < list.Length; i++)
                            list[i].Add(dataReader[dataReader.GetName(i)] + "");
                    }

                    dataReader.Close();

                    return list;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    CloseConnection();

                }
            }
            else
            {
                return null;
            }

        }

        public static DataSet Select(string table, string[] what = null, string where = null, string join = null)
        {
            string select;
            if (what == null)
                select = "*";
            else
                select = string.Join(",", what);

            if (where == null)
                where = "";
            else
                where = "WHERE " + where;

            if (join == null)
                join = "";
            else
                join = "join " + join;

            string query = string.Format("SELECT {0} FROM {1} {2} {3}", select, table, join, where);
            try
            {
                if (OpenConnection() == true)
                {

                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    return ds;
                }
                else
                {
                    return null;
                }

            }

            catch (Exception)
            {
                throw;

            }
            finally
            {
                CloseConnection();
            }
        }


        public static void Transaction(Action<MySqlTransaction> action)
        {
            _connection.Open();

            using (MySqlTransaction transaction = _connection.BeginTransaction())
            {

                try
                {
                    action(transaction);

                    transaction.Commit();
                }

                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    _connection.Close();
                }
            }
        }


        public static ExerciseCollection GetExercisesFromDB()
        {
            DataSet ds = Select("exercise");

            if (ds == null)
            {
                throw new DataBaseException("Adapter returned null data set");
            }
            else
            {
                return ExerciseFromDBtoCollection(ds);
            }

        }

        public static ExerciseCollection ExerciseFromDBtoCollection(DataSet ds)
        {
            ExerciseCollection collection = new ExerciseCollection();

            var exerciseTable = ds.Tables[0];

            foreach (DataRow exercise in exerciseTable.Rows)
            {
                collection.Add(new ObservableExercise(
                    exercise.ItemArray[0].ToString(),
                    exercise.ItemArray[1].ToString(),
                    exercise.ItemArray[2].ToString(),
                    exercise.ItemArray[3].ToString()));
            }


            return collection;
        }

        public static int LastInsertID(MySqlTransaction transaction = null)
        {
            string query = string.Format("SELECT LAST_INSERT_ID()");

            if (transaction != null || OpenConnection() == true)
            {

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection, transaction);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    int id = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0]);

                    return id;
                }

                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (transaction == null)
                        CloseConnection();
                }
            }
            else
            {
                return -1;

            }
        }
    }
}

