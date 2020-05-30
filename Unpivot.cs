using System;
using System.Data;
using System.Collections;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace ExampleNamespace
{
	//A custom SQL Server function written to be able to dynamically unpivot tables.
	//Useful for sql tables with a large amount of columns.
	public class UserDefinedFunctions
    {
        [Microsoft.SqlServer.Server.SqlFunction(
            DataAccess = DataAccessKind.Read,
            FillRowMethodName = "Unpivot_FillRows",
            TableDefinition = "ID int, Key varchar(32), Value nvarchar(MAX)"
            )]
        public static IEnumerable Unpivot(string source, string idName)
        {
            using (SqlConnection sqlConnection = new SqlConnection("context connection=true"))
            {
                sqlConnection.Open();
                using (SqlCommand select = new SqlCommand(String.Format("SELECT * FROM {0}", @source), sqlConnection))
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("ID", typeof(SqlInt32));
                    dataTable.Columns.Add("Key", typeof(SqlString));
                    dataTable.Columns.Add("Value", typeof(SqlString));
                    using (SqlDataReader sqlDataReader = select.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            SqlInt32 id = sqlDataReader.GetSqlInt32(sqlDataReader.GetOrdinal(idName));
                            for (int i = 0; i < sqlDataReader.FieldCount - 1; i++)
                            {
                             	DataRow dataRow = dataTable.NewRow();
                             	dataRow[0] = id;
                             	dataRow[1] = sqlDataReader.GetName(i);
                             	dataRow[2] = sqlDataReader.GetValue(i).ToString();
                             	dataTable.Rows.Add(dataRow);
                            }

                        }
                    }
                    return dataTable.Rows;
                }
            }
        }
        public static void PivotResponses_FillRows(object obj, out SqlInt32 ID, out SqlString Key, out SqlString Value)
        {
            DataRow dataRow = (DataRow)obj;
            ID = (SqlInt32)dataRow[0];
            Key = (SqlString)dataRow[1];
            Value = (SqlString)dataRow[2];
        }
    }

}
