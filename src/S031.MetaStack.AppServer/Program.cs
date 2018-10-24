using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Security;
using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace S031.MetaStack.AppServer
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			using (var host = new HostBuilder()
				.UseConsoleLifetime()
				.UseApplicationContext(configuration)
				.Build())
			{
				//await host.RunAsync(ApplicationContext.CancellationToken);
				TestConnection();
				await host.RunAsync();
			}
		}
		private static void TestConnection()
		{
			var sp = ApplicationContext.GetServices();
			var config = sp.GetService<IConfiguration>();
			var connectionName = config["appSettings:defaultConnection"];
			var cn = config.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			var log = sp.GetService<ILogger>();
			using (MdbContext mdb = new MdbContext(cn))
			using (JMXFactory f = JMXFactory.Create(mdb, log))
			{
				//show(mdb.Connection);
			}
		}

		/*
		static void show(DbConnection conn)
		{

				// Get the Meta Data for Supported Schema Collections  
				DataTable metaDataTable = conn.GetSchema("MetaDataCollections");

				Console.WriteLine("Meta Data for Supported Schema Collections:");
				ShowDataTable(metaDataTable, 25);
				Console.WriteLine();

				//// Get the schema information of Databases in your instance  
				//DataTable databasesSchemaTable = conn.GetSchema("Databases");

				//Console.WriteLine("Schema Information of Databases:");
				//ShowDataTable(databasesSchemaTable, 25);
				//Console.WriteLine();

				// First, get schema information of all the tables in current database;  
				DataTable allTablesSchemaTable = conn.GetSchema("Tables");

				Console.WriteLine("Schema Information of All Tables:");
				ShowDataTable(allTablesSchemaTable, 20);
				Console.WriteLine();

				// You can specify the Catalog, Schema, Table Name, Table Type to get   
				// the specified table(s).  
				// You can use four restrictions for Table, so you should create a 4 members array.  
				String[] tableRestrictions = new String[4];

				// For the array, 0-member represents Catalog; 1-member represents Schema;   
				// 2-member represents Table Name; 3-member represents Table Type.   
				// Now we specify the Table Name of the table what we want to get schema information.  
				tableRestrictions[2] = "Course";

				DataTable courseTableSchemaTable = conn.GetSchema("Tables", tableRestrictions);

				Console.WriteLine("Schema Information of Course Tables:");
				ShowDataTable(courseTableSchemaTable, 20);
				Console.WriteLine();

				// First, get schema information of all the columns in current database.  
				DataTable allColumnsSchemaTable = conn.GetSchema("Columns");

				Console.WriteLine("Schema Information of All Columns:");
				ShowColumns(allColumnsSchemaTable);
				Console.WriteLine();

				// You can specify the Catalog, Schema, Table Name, Column Name to get the specified column(s).  
				// You can use four restrictions for Column, so you should create a 4 members array.  
				String[] columnRestrictions = new String[4];

				// For the array, 0-member represents Catalog; 1-member represents Schema;   
				// 2-member represents Table Name; 3-member represents Column Name.   
				// Now we specify the Table_Name and Column_Name of the columns what we want to get schema information.  
				columnRestrictions[2] = "Course";
				columnRestrictions[3] = "DepartmentID";

				DataTable departmentIDSchemaTable = conn.GetSchema("Columns", columnRestrictions);

				Console.WriteLine("Schema Information of DepartmentID Column in Course Table:");
				ShowColumns(departmentIDSchemaTable);
				Console.WriteLine();

				// First, get schema information of all the IndexColumns in current database  
				DataTable allIndexColumnsSchemaTable = conn.GetSchema("IndexColumns");

				Console.WriteLine("Schema Information of All IndexColumns:");
				ShowIndexColumns(allIndexColumnsSchemaTable);
				Console.WriteLine();

				// You can specify the Catalog, Schema, Table Name, Constraint Name, Column Name to   
				// get the specified column(s).  
				// You can use five restrictions for Column, so you should create a 5 members array.  
				String[] indexColumnsRestrictions = new String[5];

				// For the array, 0-member represents Catalog; 1-member represents Schema;   
				// 2-member represents Table Name; 3-member represents Constraint Name;4-member represents Column Name.   
				// Now we specify the Table_Name and Column_Name of the columns what we want to get schema information.  
				indexColumnsRestrictions[2] = "Course";
				indexColumnsRestrictions[4] = "CourseID";

				DataTable courseIdIndexSchemaTable = conn.GetSchema("IndexColumns", indexColumnsRestrictions);

				Console.WriteLine("Index Schema Information of CourseID Column in Course Table:");
				ShowIndexColumns(courseIdIndexSchemaTable);
				Console.WriteLine();
		}

		private static void ShowDataTable(DataTable table, Int32 length)
		{
			foreach (DataColumn col in table.Columns)
			{
				Console.Write("{0,-" + length + "}", col.ColumnName);
			}
			Console.WriteLine();

			foreach (DataRow row in table.Rows)
			{
				foreach (DataColumn col in table.Columns)
				{
					if (col.DataType.Equals(typeof(DateTime)))
						Console.Write("{0,-" + length + ":d}", row[col]);
					else if (col.DataType.Equals(typeof(Decimal)))
						Console.Write("{0,-" + length + ":C}", row[col]);
					else
						Console.Write("{0,-" + length + "}", row[col]);
				}
				Console.WriteLine();
			}
		}

		private static void ShowDataTable(DataTable table)
		{
			ShowDataTable(table, 14);
		}

		private static void ShowColumns(DataTable columnsTable)
		{
			Console.WriteLine("{0,-15}{1,-15}{2,-15}{3,-15}{4,-15}", "TableCatalog", "TABLE_SCHEMA",
				"TABLE_NAME", "COLUMN_NAME", "DATA_TYPE");
			//var selectedRows = from info in columnsTable.AsQueryable()
			//				   select new
			//				   {
			//					   TableCatalog = info["TABLE_CATALOG"],
			//					   TableSchema = info["TABLE_SCHEMA"],
			//					   TableName = info["TABLE_NAME"],
			//					   ColumnName = info["COLUMN_NAME"],
			//					   DataType = info["DATA_TYPE"]
			//				   };

			foreach (DataRow row in columnsTable.Rows)
			{
				Console.WriteLine("{0,-15}{1,-15}{2,-15}{3,-15}{4,-15}", row["TABLE_CATALOG"],
					row["TABLE_SCHEMA"], row["TABLE_NAME"], row["COLUMN_NAME"], row["DATA_TYPE"]);
			}
		}

		private static void ShowIndexColumns(DataTable indexColumnsTable)
		{
			Console.WriteLine("{0,-14}{1,-11}{2,-14}{3,-18}{4,-16}{5,-8}", "table_schema", "table_name", "column_name", "constraint_schema", "constraint_name", "KeyType");
			foreach (DataRow row in indexColumnsTable.Rows)
			{
				Console.WriteLine("{0,-14}{1,-11}{2,-14}{3,-18}{4,-16}{5,-8}", row["table_schema"],
					row["table_name"], row["column_name"], row["constraint_schema"], row["constraint_name"], "");
			}
		}
		*/
	}
}
