using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteBalance : JMXBalance
	{
		private readonly IJMXRepo _schemaRepo;

		public JMXSQLiteBalance(JMXSQLiteFactory factory) : base(factory)
		{
			_schemaRepo = factory.SchemaFactory.CreateJMXRepo();
		}

		public ILogger Logger => Factory.Logger;

		#region DropObjectSchema
		public override void DropObjectSchema(string objectName)
			=> DropObjectSchemaAsync(objectName).GetAwaiter().GetResult();

		public override async Task DropObjectSchemaAsync(string objectName)
			=> await DropSchemaAsync(objectName).ConfigureAwait(false);

		private void DropSchema(string objectName)
			=> DropSchemaAsync(objectName).GetAwaiter().GetResult();

		private async Task DropSchemaAsync(string objectName)
		{
			MdbContext mdb = Factory.GetMdbContext();
			var schema = await _schemaRepo.GetSchemaAsync(objectName);
			var schemaFromDb = await GetObjectSchemaAsync(schema.DbObjectName.ToString());

			string[] sqlList;
			if (schemaFromDb == null)
				sqlList = Array.Empty<string>();
			else
				sqlList = await DropSchemaStatementsAsync(schemaFromDb);

			if (sqlList.Length > 0)
			{
				await mdb.BeginTransactionAsync();
				try
				{
					foreach (var sql in sqlList)
					{
						Logger.LogDebug(sql);
						await mdb.ExecuteAsync(sql);
					}
					await mdb.CommitAsync();
				}
				catch
				{
					mdb.RollBack();
					throw;
				}
			}
		}

		private async Task<string[]> DropSchemaStatementsAsync(JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(SQLiteHelper.TypeMapping, fromDbSchema))
			{
				await WriteDropStatementsAsync(sb, fromDbSchema);
				sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task WriteDropStatementsAsync(SQLStatementWriter sb, JMXSchema fromDbSchema)
		{
			MdbContext mdb = Factory.GetMdbContext();
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(JMXFactory.DETAIL_FIELD_PREFIX)))
			{
				string[] names = att.FieldName.Split('_');
				var schemaFromDb = await GetObjectSchemaAsync(new JMXObjectName(names[1], names[2]).ToString());
				if (schemaFromDb != null)
					await WriteDropStatementsAsync(sb, schemaFromDb);

			}
			sb.WriteDropStatements(fromDbSchema);
		}

		#endregion DropObjectSchema

		#region GetObjectSchema
		public override JMXSchema GetObjectSchema(string objectName)
			=> GetObjectSchemaAsync(objectName)
			.GetAwaiter()
			.GetResult();

		public override async Task<JMXSchema> GetObjectSchemaAsync(string objectName)
		{
			///!!! костыль с именем таблицы
			JMXSchema schema = new JMXSchema(objectName.Left(objectName.Length - 1))
			{
				DbObjectType = DbObjectTypes.Table,
				DbObjectName = objectName,
				SchemaRepo = _schemaRepo
			};

			var drs = await Factory
				.GetMdbContext()
				.GetReadersAsync(SQLite.GetTableSchema,
					new MdbParameter("@table_name", objectName));

			var dr = drs[0];
			string sql = string.Empty;
			if (dr.Read())
				sql = (string)dr["sql"];
			else
				return null;
			var s = sql.Split(new string[] { "CONSTRAINT ", " PRIMARY KEY" }, StringSplitOptions.RemoveEmptyEntries)
				.First(str => !str.Contains(' '));
			if (!s.IsEmpty())
				schema.PrimaryKey = new JMXPrimaryKey(s);

			dr = drs[1];
			for (; dr.Read();)
			{
				JMXAttribute att = new JMXAttribute((string)dr["name"]);
				att.FieldName = att.AttribName;
				att.IsFK = dr["pk"].Equals(1);
				att.Required = dr["notnull"].Equals(1);
				if (att.IsFK)
					schema.PrimaryKey.AddKeyMember(att.FieldName);
				SetAttrType(att, (string)dr["type"]);
				schema.Attributes.Add(att);
			}
			//Костыль!!! доделать
			return null;
		}

		private static void SetAttrType(JMXAttribute att, string source)
		{
			string s = source.Between('(', ')');
			if (!s.IsEmpty())
				if (s.Contains(','))
				{
					var a = s.Split(',');
					att.DataSize = new JMXDataSize(a[0].ToIntOrDefault(), a[1].ToIntOrDefault());
				}
				else
					att.DataSize = new JMXDataSize(s.ToIntOrDefault());
			var serverType = source.GetToken(0, " ")
				.RemoveChar("[]".ToCharArray());
			att.ServerDataType = serverType;
			if (SQLiteHelper.TypeInfo.ContainsKey(serverType))
				att.DataType = SQLiteHelper.TypeInfo[serverType].MdbType;
			else
				att.DataType = MdbType.@string;
		}
		#endregion GetObjectSchema

		#region SyncObjectSchema
		public override JMXSchema SyncObjectSchema(string objectName)
		{
			throw new NotImplementedException();
		}

		public override Task<JMXSchema> SyncObjectSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}
		#endregion SyncObjectSchema


	}
}
