using System;
using System.Data;
using System.Collections;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace ExampleNamespace
{
	//A custom SQL Server function written to be able to easily pivot data in a specific format.
    public class UserDefinedFunctions
    {
        [Microsoft.SqlServer.Server.SqlFunction(
            DataAccess = DataAccessKind.Read,
            FillRowMethodName = "PivotResponses_FillRows",
            TableDefinition = "variable1 int, variable2 nvarchar(64), variable3 nvarchar(MAX)"
            )]
        public static IEnumerable PivotResponses(string source)
        {
            using (SqlConnection sqlConnection = new SqlConnection("context connection=true"))
            {
                sqlConnection.Open();
                using (SqlCommand select = new SqlCommand(String.Format("SELECT * FROM {0}", @source), sqlConnection))
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("variable1", typeof(SqlInt32));
                    dataTable.Columns.Add("variable2", typeof(SqlString));
                    dataTable.Columns.Add("variable3", typeof(SqlString));
                    using (SqlDataReader sqlDataReader = select.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            SqlInt32 variable1 = sqlDataReader.GetSqlInt32(sqlDataReader.GetOrdinal("variable1"));
                            for (int i = 0; i < sqlDataReader.FieldCount - 1; i++)
                            {
                                if (sqlDataReader.GetName(i).StartsWith("v_", StringComparison.OrdinalIgnoreCase))
                                {
                                    DataRow dataRow = dataTable.NewRow();
                                    dataRow[0] = variable1;
                                    dataRow[1] = sqlDataReader.GetName(i);
                                    dataRow[2] = sqlDataReader.GetValue(i).ToString();
                                    dataTable.Rows.Add(dataRow);
                                }
                            }

                        }
                    }
                    return dataTable.Rows;
                }
            }
        }
        public static void PivotResponses_FillRows(object obj, out SqlInt32 variable1, out SqlString variable2, out SqlString variable3)
        {
            DataRow dataRow = (DataRow)obj;
            variable1 = (SqlInt32)dataRow[0];
            variable2 = (SqlString)dataRow[1];
            variable3 = (SqlString)dataRow[2];
        }
    }

}
