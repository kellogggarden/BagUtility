using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace KGP.Services
{
    public class SqlService : IDisposable
    {
        public string ConnectionSting { get; set; }
        public SqlService(string DBConnectionString)
        {
            ConnectionSting = DBConnectionString;
        }

        public DataSet ExecuteSPDataSet(string procedureName, List<DBParameter> parameters, int commandTimeout)
        {
            DataSet data = null;
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;
            using (SqlConnection connection = new SqlConnection(ConnectionSting))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.CommandType = CommandType.StoredProcedure;

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        sqlParameter = new SqlParameter();
                        parameter = parameters[i];

                        sqlParameter.ParameterName = parameter.Name;
                        sqlParameter.Size = parameter.Size;
                        sqlParameter.Direction = parameter.Direction;
                        sqlParameter.SqlDbType = parameter.Type;
                        sqlParameter.Value = parameter.Value;

                        command.Parameters.Add(sqlParameter);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.TableMappings.Add("Table", "Query");
                        data = new DataSet();
                        adapter.Fill(data);
                    }
                }
            }
            return data;
        }

        public SqlDataReader ExecuteSPReader(string procedureName, List<DBParameter> parameters, int commandTimeout)
        {
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;
            SqlDataReader read = null;
            SqlConnection connection = new SqlConnection(ConnectionSting);
            connection.Open();
            using (SqlCommand command = new SqlCommand(procedureName, connection))
            {
                command.CommandTimeout = commandTimeout;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameters.Count; i++)
                {
                    sqlParameter = new SqlParameter();
                    parameter = parameters[i];

                    sqlParameter.ParameterName = parameter.Name;
                    sqlParameter.Size = parameter.Size;
                    sqlParameter.Direction = parameter.Direction;
                    sqlParameter.SqlDbType = parameter.Type;
                    sqlParameter.Value = parameter.Value;

                    command.Parameters.Add(sqlParameter);
                }

                try
                {
                    read = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    throw;
                }
            }
            return read;
        }

        public int ExecuteSP(string procedureName, List<DBParameter> parameters, int commandTimeout)
        {
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;
            int intReturn = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionSting))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.CommandType = CommandType.StoredProcedure;

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        sqlParameter = new SqlParameter();
                        parameter = parameters[i];
                        sqlParameter.ParameterName = parameter.Name;
                        sqlParameter.Size = parameter.Size;
                        sqlParameter.Direction = parameter.Direction;
                        sqlParameter.SqlDbType = parameter.Type;
                        sqlParameter.Value = parameter.Value;
                        command.Parameters.Add(sqlParameter);
                    }
                    intReturn = command.ExecuteNonQuery();
                }

            }

            return intReturn;
        }

        public bool ExecuteSPList(string connectionString, List<DBCommand> commandList, int commandTimeout)
        {
            bool result = false;
            DBCommand comm = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    for (int i = 0; i < commandList.Count; i++)
                    {
                        comm = (DBCommand)commandList[i];

                        if (comm.DBParameters != null)
                        {
                            ExecuteSP(comm.ProcedureName, comm.DBParameters, commandTimeout, connection, trans);
                        }
                    }
                    trans.Commit();
                }
                result = true;
            }
            return result;
        }

        public bool ExecuteSPList(List<DBCommand> commandList, int commandTimeout)
        {
            return ExecuteSPList(ConnectionSting, commandList, commandTimeout);
        }

        public bool ExecuteSPList(List<DBCommand> commandList, int commandTimeout, SqlConnection connection, SqlTransaction transaction)
        {
            for (int i = 0; i < commandList.Count; i++)
            {
                var comm = (DBCommand)commandList[i];

                if (comm.DBParameters != null)
                {
                    ExecuteSP(comm.ProcedureName, comm.DBParameters, commandTimeout, connection, transaction);
                }
            }

            return true;
        }


        public int ExecuteSP(string procedureName, List<DBParameter> parameters, int commandTimeout, SqlConnection connection, SqlTransaction transaction)
        {
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;
            int intReturn = 0;
            using (SqlCommand command = new SqlCommand(procedureName, connection))
            {
                command.CommandTimeout = commandTimeout;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameters.Count; i++)
                {
                    sqlParameter = new SqlParameter();
                    parameter = parameters[i];

                    sqlParameter.ParameterName = parameter.Name;
                    sqlParameter.Size = parameter.Size;
                    sqlParameter.Direction = parameter.Direction;
                    sqlParameter.SqlDbType = parameter.Type;
                    sqlParameter.Value = parameter.Value;

                    command.Parameters.Add(sqlParameter);
                }

                if (transaction != null) command.Transaction = transaction;
                intReturn = command.ExecuteNonQuery();
            }

            return intReturn;
        }

        public long ExecuteSPWithIdentity(string procedureName, List<DBParameter> parameters, int commandTimeout, SqlConnection connection, SqlTransaction transaction)
        {
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;
            var outParm = new SqlParameter("@ID", SqlDbType.Int);
            using (SqlCommand command = new SqlCommand(procedureName, connection))
            {
                command.CommandTimeout = commandTimeout;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameters.Count; i++)
                {
                    sqlParameter = new SqlParameter();
                    parameter = parameters[i];

                    sqlParameter.ParameterName = parameter.Name;
                    sqlParameter.Size = parameter.Size;
                    sqlParameter.Direction = parameter.Direction;
                    sqlParameter.SqlDbType = parameter.Type;
                    sqlParameter.Value = parameter.Value;

                    command.Parameters.Add(sqlParameter);
                }

                //var outParm = new SqlParameter("@ID", SqlDbType.Int);
                outParm.Direction = ParameterDirection.Output;

                command.Parameters.Add(outParm);

                if (transaction != null) command.Transaction = transaction;
                command.ExecuteNonQuery();
            }

            return Convert.ToInt32(outParm.Value);
        }


        public long ExecuteSPWithIdentity(string procedureName, List<DBParameter> parameters, int commandTimeout)
        {
            DBParameter parameter = null;
            SqlParameter sqlParameter = null;

            var outParm = new SqlParameter("@ID", SqlDbType.Int);
            using (SqlConnection connection = new SqlConnection(ConnectionSting))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    command.CommandType = CommandType.StoredProcedure;

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        sqlParameter = new SqlParameter();
                        parameter = parameters[i];

                        sqlParameter.ParameterName = parameter.Name;
                        sqlParameter.Size = parameter.Size;
                        sqlParameter.Direction = parameter.Direction;
                        sqlParameter.SqlDbType = parameter.Type;
                        sqlParameter.Value = parameter.Value;

                        command.Parameters.Add(sqlParameter);
                    }
                    outParm.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outParm);
                    command.ExecuteNonQuery();
                }

            }

            return Convert.ToInt32(outParm.Value);
        }



        public int ExecuteSQL(string sql, int commandTimeout)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionSting))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandTimeout = commandTimeout;
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        public DataTable ListToDataTable<T>(List<T> list)
        {
            DataTable dt = new DataTable();

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
            }
            foreach (T t in list)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    if (!IsNullableType(info.PropertyType))
                        row[info.Name] = info.GetValue(t, null);
                    else
                        row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        private bool IsNullableType(Type type)
        {
            return (type == typeof(string) ||
                    type.IsArray ||
                    (type.IsGenericType &&
                     type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        private Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }

        public void Dispose()
        {

        }

    }

    public class DBCommand
    {
        public string ProcedureName { get; set; }
        public List<DBParameter> DBParameters { get; set; }
    }

    public class DBParameter
    {
        public DBParameter()
        {

        }
        public DBParameter(string name, object value, int size, SqlDbType type, ParameterDirection direction)
        {
            Name = name;
            Value = value;
            Size = size;
            Type = type;
            Direction = direction;
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public int Size { get; set; }

        public SqlDbType Type { get; set; }
        public ParameterDirection Direction { get; set; }

    }
}
