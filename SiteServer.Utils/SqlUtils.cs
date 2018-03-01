﻿using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using Oracle.ManagedDataAccess.Client;
using SiteServer.Plugin;

namespace SiteServer.Utils
{
    public static class SqlUtils
    {
        public const string Asterisk = "*";

        public static string GetConnectionString(DatabaseType databaseType, string server, bool isDefaultPort, int port, string userName, string password, string database)
        {
            var connectionString = string.Empty;

            if (databaseType == DatabaseType.MySql)
            {
                connectionString = $"Server={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString += $"Port={port};";
                }
                connectionString += $"Uid={userName};Pwd={password};";
                if (!string.IsNullOrEmpty(database))
                {
                    connectionString += $"Database={database};";
                }
            }
            else if (databaseType == DatabaseType.SqlServer)
            {
                connectionString = $"Server={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString += $"Port={port};";
                }
                connectionString += $"Uid={userName};Pwd={password};";
                if (!string.IsNullOrEmpty(database))
                {
                    connectionString += $"Database={database};";
                }
            }
            else if (databaseType == DatabaseType.PostgreSql)
            {
                connectionString = $"Host={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString += $"Port={port};";
                }
                connectionString += $"Username={userName};Password={password};";
                if (!string.IsNullOrEmpty(database))
                {
                    connectionString += $"Database={database};";
                }
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                port = !isDefaultPort && port > 0 ? port : 1521;
                database = string.IsNullOrEmpty(database)
                    ? string.Empty
                    : $"(CONNECT_DATA=(SERVICE_NAME={database}))";
                connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={server})(PORT={port})){database});User ID={userName};Password={password};";
            }

            return connectionString;
        }

        public static IDbConnection GetIDbConnection(DatabaseType databaseType, string connectionString)
        {
            IDbConnection conn = null;

            if (databaseType == DatabaseType.MySql)
            {
                conn = new MySqlConnection(connectionString);
            }
            else if (databaseType == DatabaseType.SqlServer)
            {
                conn = new SqlConnection(connectionString);
            }
            else if (databaseType == DatabaseType.PostgreSql)
            {
                conn = new NpgsqlConnection(connectionString);
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                conn = new OracleConnection(connectionString);
            }

            return conn;
        }

        public static IDbCommand GetIDbCommand()
        {
            IDbCommand command = null;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                command = new MySqlCommand();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                command = new SqlCommand();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                command = new NpgsqlCommand();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                command = new OracleCommand();
            }

            return command;
        }

        public static IDbDataAdapter GetIDbDataAdapter(string text, string connectionString)
        {
            IDbDataAdapter adapter = null;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                adapter = new MySqlDataAdapter(text, connectionString);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                adapter = new SqlDataAdapter(text, connectionString);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                adapter = new NpgsqlDataAdapter(text, connectionString);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                adapter = new OracleDataAdapter(text, connectionString);
            }

            return adapter;
        }

        public static IDbDataAdapter GetIDbDataAdapter()
        {
            IDbDataAdapter adapter = null;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                adapter = new MySqlDataAdapter();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                adapter = new SqlDataAdapter();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                adapter = new NpgsqlDataAdapter();
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                adapter = new OracleDataAdapter();
            }

            return adapter;
        }

        public static void FillDataAdapterWithDataTable(IDbDataAdapter adapter, DataTable table)
        {
            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                ((MySqlDataAdapter)adapter).Fill(table);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                ((SqlDataAdapter)adapter).Fill(table);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                ((NpgsqlDataAdapter)adapter).Fill(table);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                ((OracleDataAdapter)adapter).Fill(table);
            }
        }

        public static IDbDataParameter GetIDbDataParameter(string parameterName, DataType dataType, int size)
        {
            IDbDataParameter parameter = null;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                parameter = new MySqlParameter(parameterName, ToMySqlDbType(dataType), size);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                parameter = new SqlParameter(parameterName, ToSqlServerDbType(dataType), size);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                parameter = new NpgsqlParameter(parameterName, ToNpgsqlDbType(dataType), size);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                parameter = new OracleParameter(parameterName, ToOracleDbType(dataType), size);
            }

            return parameter;
        }

        public static IDbDataParameter GetIDbDataParameter(string parameterName, DataType dataType)
        {
            IDbDataParameter parameter = null;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                parameter = new MySqlParameter(parameterName, ToMySqlDbType(dataType));
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                parameter = new SqlParameter(parameterName, ToSqlServerDbType(dataType));
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                parameter = new NpgsqlParameter(parameterName, ToNpgsqlDbType(dataType));
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                parameter = new OracleParameter(parameterName, ToOracleDbType(dataType));
            }

            return parameter;
        }

        public static string GetInStr(string columnName, string inStr)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"INSTR({columnName}, '{inStr}') > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"CHARINDEX('{inStr}', {columnName}) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"POSITION('{inStr}' IN {columnName}) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"INSTR({columnName}, '{inStr}') > 0";
            }

            return retval;
        }

        public static string GetInStrReverse(string inStr, string columnName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"INSTR('{inStr}', {columnName}) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"CHARINDEX({columnName}, '{inStr}') > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"POSITION({columnName} IN '{inStr}') > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"INSTR('{inStr}', {columnName}) > 0";
            }

            return retval;
        }

        public static string GetNotInStr(string columnName, string inStr)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"INSTR({columnName}, '{inStr}') = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"CHARINDEX('{inStr}', {columnName}) = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"POSITION('{inStr}' IN {columnName}) = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"INSTR({columnName}, '{inStr}') = 0";
            }

            return retval;
        }

        public static string GetNotNullAndEmpty(string columnName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"LENGTH(IFNULL({columnName},'')) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATALENGTH({columnName}) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"LENGTH(COALESCE({columnName}, '')) > 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"LENGTH(COALESCE({columnName}, '')) > 0";
            }

            return retval;
        }

        public static string GetNullOrEmpty(string columnName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"LENGTH(IFNULL({columnName},'')) = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATALENGTH({columnName}) = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"LENGTH(COALESCE({columnName}, '')) = 0";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"LENGTH(COALESCE({columnName}, '')) = 0";
            }

            return retval;
        }

        public static string ToTopSqlString(string tableName, string columns, string whereString, string orderString, int topN)
        {
            string retval = $"SELECT {columns} FROM {tableName} {whereString} {orderString}";
            if (topN <= 0) return retval;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"SELECT {columns} FROM {tableName} {whereString} {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"SELECT TOP {topN} {columns} FROM {tableName} {whereString} {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"SELECT {columns} FROM {tableName} {whereString} {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $@"SELECT {columns} FROM {tableName} {whereString} {orderString} FETCH FIRST {topN} ROWS ONLY";
            }

            return retval;
        }

        public static string ToTopSqlString(string sqlString, string orderString, int topN)
        {
            string retval = $"SELECT * FROM ({sqlString}) {orderString}";
            if (topN <= 0) return retval;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"SELECT * FROM ({sqlString}) {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"SELECT TOP {topN} * FROM ({sqlString}) {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"SELECT * FROM ({sqlString}) {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $@"SELECT * FROM ({sqlString}) {orderString} FETCH FIRST {topN} ROWS ONLY";
            }

            return retval;
        }

        public static string GetPageSqlString(string sqlString, string orderString, int itemsPerPage, int currentPageIndex, int pageCount, int recordsInLastPage)
        {
            var retval = string.Empty;

            var recsToRetrieve = itemsPerPage;
            if (currentPageIndex == pageCount - 1)
            {
                recsToRetrieve = recordsInLastPage;
            }

            orderString = orderString.ToUpper();
            var orderStringReverse = orderString.Replace(" DESC", " DESC2");
            orderStringReverse = orderStringReverse.Replace(" ASC", " DESC");
            orderStringReverse = orderStringReverse.Replace(" DESC2", " ASC");

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) AS t0 {orderString} LIMIT {itemsPerPage * (currentPageIndex + 1)}
    ) AS t1 {orderStringReverse} LIMIT {recsToRetrieve}
) AS t2 {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $@"
SELECT * FROM (
    SELECT TOP {recsToRetrieve} * FROM (
        SELECT TOP {itemsPerPage * (currentPageIndex + 1)} * FROM ({sqlString}) AS t0 {orderString}
    ) AS t1 {orderStringReverse}
) AS t2 {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) AS t0 {orderString} LIMIT {itemsPerPage * (currentPageIndex + 1)}
    ) AS t1 {orderStringReverse} LIMIT {recsToRetrieve}
) AS t2 {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) {orderString} FETCH FIRST {itemsPerPage * (currentPageIndex + 1)} ROWS ONLY
    ) {orderStringReverse} FETCH FIRST {recsToRetrieve} ROWS ONLY
) {orderString}";
            }

            return retval;
        }

        public static string GetDistinctTopSqlString(string tableName, string columns, string whereString, string orderString, int topN)
        {
            var retval = $"SELECT DISTINCT {columns} FROM {tableName} {whereString} {orderString}";
            if (topN <= 0) return retval;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"SELECT DISTINCT {columns} FROM {tableName} {whereString} {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"SELECT DISTINCT TOP {topN} {columns} FROM {tableName} {whereString} {orderString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"SELECT DISTINCT {columns} FROM {tableName} {whereString} {orderString} LIMIT {topN}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"SELECT DISTINCT {columns} FROM {tableName} {whereString} {orderString} FETCH FIRST {topN} ROWS ONLY";
            }

            return retval;
        }

        public static string ToInTopSqlString(string tableName, string columns, string whereString, string orderString, int topN)
        {
            var builder = new StringBuilder();
            if (WebConfigUtils.DatabaseType != DatabaseType.Oracle)
            {
                foreach (var column in TranslateUtils.StringCollectionToStringList(columns))
                {
                    builder.Append($"T.{column}, ");
                }
                builder.Length = builder.Length - 2;
                return
                    $"SELECT {builder} FROM ({ToTopSqlString(tableName, columns, whereString, orderString, topN)}) AS T";
            }

            foreach (var column in TranslateUtils.StringCollectionToStringList(columns))
            {
                builder.Append($"{column}, ");
            }
            builder.Length = builder.Length - 2;
            return
                $"SELECT {builder} FROM ({ToTopSqlString(tableName, columns, whereString, orderString, topN)})";
        }

        public static string GetColumnSqlString(DataType dataType, string attributeName, int length)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = ToMySqlColumnString(dataType, attributeName, length);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = ToSqlServerColumnString(dataType, attributeName, length);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = ToPostgreColumnString(dataType, attributeName, length);
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = ToOracleColumnString(dataType, attributeName, length);
            }

            return retval;
        }

        public static string GetAddColumnsSqlString(string tableName, string columnsSqlString)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"ALTER TABLE `{tableName}` ADD ({columnsSqlString})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"ALTER TABLE [{tableName}] ADD {columnsSqlString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"ALTER TABLE {tableName} ADD {columnsSqlString}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"ALTER TABLE {tableName} ADD {columnsSqlString}";
            }

            return retval;
        }

        public static string GetDropTableSqlString(string tableName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DROP TABLE `{tableName}`";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DROP TABLE [{tableName}]";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"DROP TABLE {tableName}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"DROP TABLE {tableName}";
            }

            return retval;
        }

        public static string GetAutoIncrementDataType()
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = "INT AUTO_INCREMENT";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = "int IDENTITY (1, 1)";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = "SERIAL";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = "NUMBER GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1)";
            }

            return retval;
        }

        public static DataType ToDataType(DatabaseType databaseType, string dataTypeStr)
        {
            if (string.IsNullOrEmpty(dataTypeStr)) return DataType.VarChar;

            var dataType = DataType.VarChar;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                dataTypeStr = dataTypeStr.ToLower().Trim();
                switch (dataTypeStr)
                {
                    case "bit":
                        dataType = DataType.Boolean;
                        break;
                    case "datetime":
                        dataType = DataType.DateTime;
                        break;
                    case "decimal":
                        dataType = DataType.Decimal;
                        break;
                    case "int":
                        dataType = DataType.Integer;
                        break;
                    case "longtext":
                        dataType = DataType.Text;
                        break;
                    case "nvarchar":
                        dataType = DataType.VarChar;
                        break;
                    case "text":
                        dataType = DataType.Text;
                        break;
                    case "varchar":
                        dataType = DataType.VarChar;
                        break;
                }
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                dataTypeStr = dataTypeStr.ToLower().Trim();
                switch (dataTypeStr)
                {
                    case "bit":
                        dataType = DataType.Boolean;
                        break;
                    case "datetime":
                        dataType = DataType.DateTime;
                        break;
                    case "decimal":
                        dataType = DataType.Decimal;
                        break;
                    case "int":
                        dataType = DataType.Integer;
                        break;
                    case "ntext":
                        dataType = DataType.Text;
                        break;
                    case "nvarchar":
                        dataType = DataType.VarChar;
                        break;
                    case "text":
                        dataType = DataType.Text;
                        break;
                    case "varchar":
                        dataType = DataType.VarChar;
                        break;
                }
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                dataTypeStr = dataTypeStr.ToLower().Trim();
                switch (dataTypeStr)
                {
                    case "varchar":
                        dataType = DataType.VarChar;
                        break;
                    case "bool":
                        dataType = DataType.Boolean;
                        break;
                    case "timestamptz":
                        dataType = DataType.DateTime;
                        break;
                    case "numeric":
                        dataType = DataType.Decimal;
                        break;
                    case "int4":
                        dataType = DataType.Integer;
                        break;
                    case "text":
                        dataType = DataType.Text;
                        break;
                }
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                dataTypeStr = dataTypeStr.ToUpper().Trim();
                if (dataTypeStr.StartsWith("TIMESTAMP("))
                {
                    dataType = DataType.DateTime;
                }
                else if (dataTypeStr == "NUMBER")
                {
                    dataType = DataType.Integer;
                }
                else if (dataTypeStr == "NCLOB")
                {
                    dataType = DataType.Text;
                }
                else if (dataTypeStr == "NVARCHAR2")
                {
                    dataType = DataType.VarChar;
                }
                else if (dataTypeStr == "CLOB")
                {
                    dataType = DataType.Text;
                }
                else if (dataTypeStr == "VARCHAR2")
                {
                    dataType = DataType.VarChar;
                }
            }

            return dataType;
        }

        public static SqlDbType ToSqlServerDbType(DataType type)
        {
            if (type == DataType.Boolean)
            {
                return SqlDbType.Bit;
            }
            if (type == DataType.DateTime)
            {
                return SqlDbType.DateTime;
            }
            if (type == DataType.Decimal)
            {
                return SqlDbType.Decimal;
            }
            if (type == DataType.Integer)
            {
                return SqlDbType.Int;
            }
            if (type == DataType.Text)
            {
                return SqlDbType.NText;
            }
            if (type == DataType.VarChar)
            {
                return SqlDbType.NVarChar;
            }
            return SqlDbType.VarChar;
        }

        public static MySqlDbType ToMySqlDbType(DataType type)
        {
            if (type == DataType.Boolean)
            {
                return MySqlDbType.Bit;
            }
            if (type == DataType.DateTime)
            {
                return MySqlDbType.DateTime;
            }
            if (type == DataType.Decimal)
            {
                return MySqlDbType.Decimal;
            }
            if (type == DataType.Integer)
            {
                return MySqlDbType.Int32;
            }
            if (type == DataType.Text)
            {
                return MySqlDbType.LongText;
            }
            if (type == DataType.VarChar)
            {
                return MySqlDbType.VarString;
            }

            return MySqlDbType.VarString;
        }

        public static NpgsqlDbType ToNpgsqlDbType(DataType type)
        {
            if (type == DataType.Boolean)
            {
                return NpgsqlDbType.Boolean;
            }
            if (type == DataType.DateTime)
            {
                return NpgsqlDbType.TimestampTZ;
            }
            if (type == DataType.Decimal)
            {
                return NpgsqlDbType.Numeric;
            }
            if (type == DataType.Integer)
            {
                return NpgsqlDbType.Integer;
            }
            if (type == DataType.Text)
            {
                return NpgsqlDbType.Text;
            }
            return NpgsqlDbType.Varchar;
        }

        public static OracleDbType ToOracleDbType(DataType type)
        {
            if (type == DataType.Boolean)
            {
                return OracleDbType.Int32;
            }
            if (type == DataType.DateTime)
            {
                return OracleDbType.TimeStampTZ;
            }
            if (type == DataType.Decimal)
            {
                return OracleDbType.Decimal;
            }
            if (type == DataType.Integer)
            {
                return OracleDbType.Int32;
            }
            if (type == DataType.Text)
            {
                return OracleDbType.NClob;
            }
            return OracleDbType.NVarchar2;
        }

        public static string ToMySqlColumnString(DataType type, string attributeName, int length)
        {
            if (type == DataType.Boolean)
            {
                return $"`{attributeName}` tinyint(1)";
            }
            if (type == DataType.DateTime)
            {
                return $"`{attributeName}` datetime";
            }
            if (type == DataType.Decimal)
            {
                return $"`{attributeName}` decimal(18, 2)";
            }
            if (type == DataType.Integer)
            {
                return $"`{attributeName}` int";
            }
            if (type == DataType.Text)
            {
                return $"`{attributeName}` longtext";
            }
            return $"`{attributeName}` varchar({length})";
        }

        public static string ToSqlServerColumnString(DataType type, string attributeName, int length)
        {
            if (type == DataType.Boolean)
            {
                return $"[{attributeName}] [bit]";
            }
            if (type == DataType.DateTime)
            {
                return $"[{attributeName}] [datetime]";
            }
            if (type == DataType.Decimal)
            {
                return $"[{attributeName}] [decimal] (18, 2)";
            }
            if (type == DataType.Integer)
            {
                return $"[{attributeName}] [int]";
            }
            if (type == DataType.Text)
            {
                return $"[{attributeName}] [ntext]";
            }
            return $"[{attributeName}] [nvarchar] ({length})";
        }

        public static string ToPostgreColumnString(DataType type, string attributeName, int length)
        {
            if (type == DataType.Boolean)
            {
                return $"{attributeName} bool";
            }
            if (type == DataType.DateTime)
            {
                return $"{attributeName} timestamptz";
            }
            if (type == DataType.Decimal)
            {
                return $"{attributeName} numeric(18, 2)";
            }
            if (type == DataType.Integer)
            {
                return $"{attributeName} int4";
            }
            if (type == DataType.Text)
            {
                return $"{attributeName} text";
            }
            return $"{attributeName} varchar({length})";
        }

        public static string ToOracleColumnString(DataType type, string attributeName, int length)
        {
            if (type == DataType.Boolean)
            {
                return $"{attributeName} number(1)";
            }
            if (type == DataType.DateTime)
            {
                return $"{attributeName} timestamp(6) with time zone";
            }
            if (type == DataType.Decimal)
            {
                return $"{attributeName} number(38, 2)";
            }
            if (type == DataType.Integer)
            {
                return $"{attributeName} number";
            }
            if (type == DataType.Text)
            {
                return $"{attributeName} nclob";
            }
            return $"{attributeName} nvarchar2({length})";
        }

        public static string GetDateDiffLessThanYears(string fieldName, string years)
        {
            return GetDateDiffLessThan(fieldName, years, "YEAR");
        }

        public static string GetDateDiffLessThanMonths(string fieldName, string months)
        {
            return GetDateDiffLessThan(fieldName, months, "MONTH");
        }

        public static string GetDateDiffLessThanDays(string fieldName, string days)
        {
            return GetDateDiffLessThan(fieldName, days, "DAY");
        }

        public static string GetDateDiffLessThanHours(string fieldName, string hours)
        {
            return GetDateDiffLessThan(fieldName, hours, "HOUR");
        }

        public static string GetDateDiffLessThanMinutes(string fieldName, string minutes)
        {
            return GetDateDiffLessThan(fieldName, minutes, "MINUTE");
        }

        private static int GetSecondsByUnit(string unit)
        {
            var seconds = 1;
            if (unit == "MINUTE")
            {
                seconds = 60;
            }
            else if (unit == "HOUR")
            {
                seconds = 3600;
            }
            else if (unit == "DAY")
            {
                seconds = 86400;
            }
            else if (unit == "MONTH")
            {
                seconds = 2592000;
            }
            else if (unit == "YEAR")
            {
                seconds = 31536000;
            }
            return seconds;
        }

        private static string GetDateDiffLessThan(string fieldName, string fieldValue, string unit)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"TIMESTAMPDIFF({unit}, {fieldName}, now()) < {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEDIFF({unit}, {fieldName}, getdate()) < {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"EXTRACT(EPOCH FROM current_timestamp - {fieldName})/{GetSecondsByUnit(unit)} < {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT({unit} FROM CURRENT_TIMESTAMP - {fieldName}) < {fieldValue}";
            }
            
            return retval;
        }

        public static string GetDateDiffGreatThanYears(string fieldName, string years)
        {
            return GetDateDiffGreatThan(fieldName, years, "YEAR");
        }

        public static string GetDateDiffGreatThanMonths(string fieldName, string months)
        {
            return GetDateDiffGreatThan(fieldName, months, "MONTH");
        }

        public static string GetDateDiffGreatThanDays(string fieldName, string days)
        {
            return GetDateDiffGreatThan(fieldName, days, "DAY");
        }

        public static string GetDateDiffGreatThanHours(string fieldName, string hours)
        {
            return GetDateDiffGreatThan(fieldName, hours, "HOUR");
        }

        public static string GetDateDiffGreatThanMinutes(string fieldName, string minutes)
        {
            return GetDateDiffGreatThan(fieldName, minutes, "MINUTE");
        }

        private static string GetDateDiffGreatThan(string fieldName, string fieldValue, string unit)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"TIMESTAMPDIFF({unit}, {fieldName}, now()) > {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEDIFF({unit}, {fieldName}, getdate()) > {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"EXTRACT(EPOCH FROM current_timestamp - {fieldName})/{GetSecondsByUnit(unit)} > {fieldValue}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT({unit} FROM CURRENT_TIMESTAMP - {fieldName}) > {fieldValue}";
            }

            return retval;
        }

        public static string GetDatePartYear(string fieldName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DATE_FORMAT({fieldName}, '%Y')";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEPART([YEAR], {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"date_part('year', {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT(year from {fieldName})";
            }

            return retval;
        }

        public static string GetDatePartMonth(string fieldName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DATE_FORMAT({fieldName}, '%c')";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEPART([MONTH], {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"date_part('month', {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT(month from {fieldName})";
            }            

            return retval;
        }

        public static string GetDatePartDay(string fieldName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DATE_FORMAT({fieldName}, '%e')";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEPART([DAY], {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"date_part('day', {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT(day from {fieldName})";
            }

            return retval;
        }

        public static string GetDatePartHour(string fieldName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DATE_FORMAT({fieldName}, '%k')";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEPART([HOUR], {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"date_part('hour', {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"EXTRACT(hour from {fieldName})";
            }

            return retval;
        }

        public static string GetDatePartDayOfYear(string fieldName)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"DATE_FORMAT({fieldName}, '%j')";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"DATEPART([DAYOFYEAR], {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"date_part('doy', {fieldName})";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"TO_CHAR({fieldName}, 'DDD')";
            }

            return retval;
        }

        public static string GetComparableNow()
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = "now()";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = "getdate()";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = "current_timestamp";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = "sysdate";
            }

            return retval;
        }

        public static string GetComparableDate(DateTime dateTime)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"'{dateTime:yyyy-MM-dd}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"'{dateTime:yyyy-MM-dd}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"'{dateTime:yyyy-MM-dd}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"to_date('{dateTime:yyyy-MM-dd}', 'yyyy-mm-dd')";
            }

            return retval;
        }

        public static string GetComparableDateTime(DateTime dateTime)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"to_date('{dateTime:yyyy-MM-dd HH:mm:ss}', 'yyyy-mm-dd hh24:mi:ss')";
            }

            return retval;
        }

        public static string ToPlusSqlString(string fieldName, int plusNum = 1)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"{fieldName} = IFNULL({fieldName}, 0) + {plusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"{fieldName} = ISNULL({fieldName}, 0) + {plusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"{fieldName} = COALESCE({fieldName}, 0) + {plusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"{fieldName} = COALESCE({fieldName}, 0) + {plusNum}";
            }

            return retval;
        }

        public static string ToMinusSqlString(string fieldName, int minusNum = 1)
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = $"{fieldName} = IFNULL({fieldName}, 0) - {minusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = $"{fieldName} = ISNULL({fieldName}, 0) - {minusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = $"{fieldName} = COALESCE({fieldName}, 0) - {minusNum}";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = $"{fieldName} = COALESCE({fieldName}, 0) - {minusNum}";
            }

            return retval;
        }

        public static string GetOrderByRandom()
        {
            var retval = string.Empty;

            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            {
                retval = "ORDER BY RAND()";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.SqlServer)
            {
                retval = "ORDER BY NEWID() DESC";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.PostgreSql)
            {
                retval = "ORDER BY random()";
            }
            else if (WebConfigUtils.DatabaseType == DatabaseType.Oracle)
            {
                retval = "ORDER BY dbms_random.value()";
            }

            return retval;
        }

        public static int GetMaxLengthForNVarChar()
        {
            return 4000;
        }

        public static string ToSqlString(string inputString)
        {
            return !string.IsNullOrEmpty(inputString) ? inputString.Replace("'", "''") : string.Empty;
        }

        public static string ToSqlString(string inputString, int maxLength)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;

            if (maxLength > 0 && inputString.Length > maxLength)
            {
                inputString = inputString.Substring(0, maxLength);
            }
            return inputString.Replace("'", "''");
        }

        /// <summary>
        /// 验证此字符串是否合作作为字段名称
        /// </summary>
        public static bool IsAttributeNameCompliant(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName) || attributeName.IndexOf(" ", StringComparison.Ordinal) != -1) return false;
            if (-1 != attributeName.IndexOfAny(PathUtils.InvalidPathChars))
            {
                return false;
            }
            foreach (var t in attributeName)
            {
                if (StringUtils.IsTwoBytesChar(t))
                {
                    return false;
                }
            }
            return true;
        }

        public static string ReadNextSqlString(StreamReader reader)
        {
            try
            {
                var sb = new StringBuilder();

                while (true)
                {
                    var lineOfText = reader.ReadLine();

                    if (lineOfText == null)
                    {
                        return sb.Length > 0 ? sb.ToString() : null;
                    }

                    if (lineOfText.StartsWith("--")) continue;
                    lineOfText = lineOfText.Replace(")ENGINE=INNODB", ") ENGINE=InnoDB DEFAULT CHARSET=utf8");

                    if (lineOfText.TrimEnd().ToUpper() == "GO")
                    {
                        break;
                    }

                    sb.Append(lineOfText + Environment.NewLine);
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string ReadNextStatementFromStream(StringReader reader)
        {
            try
            {
                var sb = new StringBuilder();

                while (true)
                {
                    var lineOfText = reader.ReadLine();
                    if (lineOfText == null)
                    {
                        return sb.Length > 0 ? sb.ToString() : null;
                    }

                    if (lineOfText.TrimEnd().ToUpper() == "GO")
                    {
                        break;
                    }

                    sb.Append(lineOfText + Environment.NewLine);
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static object Eval(object dataItem, string name)
        {
            object o = null;
            try
            {
                o = DataBinder.Eval(dataItem, name);
            }
            catch
            {
                // ignored
            }
            if (o == DBNull.Value)
            {
                o = null;
            }
            return o;
        }

        public static int EvalInt(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        public static decimal EvalDecimal(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o == null ? 0 : Convert.ToDecimal(o);
        }

        public static string EvalString(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o?.ToString() ?? string.Empty;
        }

        public static DateTime EvalDateTime(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            if (o == null)
            {
                return DateUtils.SqlMinValue;
            }
            return (DateTime)o;
        }

        public static bool EvalBool(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o != null && TranslateUtils.ToBool(o.ToString());
        }

        public static string GetDatabaseNameFormConnectionString(DatabaseType databaseType, string connectionString)
        {
            if (databaseType == DatabaseType.Oracle)
            {
                var index1 = connectionString.IndexOf("SERVICE_NAME=", StringComparison.Ordinal);
                var index2 = connectionString.IndexOf(")));", StringComparison.Ordinal);
                return connectionString.Substring(index1 + 13, index2 - index1 - 13);
            }
            return GetValueFromConnectionString(databaseType, connectionString, "Database");
        }

        public static string GetValueFromConnectionString(DatabaseType databaseType, string connectionString, string attribute)
        {
            var retval = string.Empty;
            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(attribute))
            {
                var pairs = connectionString.Split(';');
                foreach (var pair in pairs)
                {
                    if (pair.IndexOf("=", StringComparison.Ordinal) != -1)
                    {
                        if (StringUtils.EqualsIgnoreCase(attribute, pair.Trim().Split('=')[0]))
                        {
                            retval = pair.Trim().Split('=')[1];
                            break;
                        }
                    }
                }
            }
            return retval;
        }

        
    }
}
