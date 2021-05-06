using Microsoft.Extensions.Logging;
using S031.MetaStack.ORM;
using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public sealed class JMXSqlBalance : JMXBalance
	{
		private enum AttribCompareDiff
		{
			none = 0,
			notFound = 2,
			name = 4,
			dataTtype = 8,
			size = 16,
			nullOptions = 32,
			remove = 64,
			constraint = 128,
			identity = 256
		}
		private enum DbObjectOnDiffActions
		{
			none = 0,
			add = 2,
			drop = 4,
			alter = 8,
		}

		private readonly IJMXRepo _schemaRepo;

		public JMXSqlBalance(JMXSqlFactory factory) : base(factory)
		{
			_schemaRepo = factory.SchemaFactory.CreateJMXRepo();
		}

		public ILogger Logger => Factory.Logger;

		#region Drop Schema
		public override void DropObjectSchema(string objectName)
		{
			DropObjectSchemaAsync(objectName).GetAwaiter().GetResult();
		}

		public override async Task DropObjectSchemaAsync(string objectName)
			=> await DropSchemaAsync(objectName).ConfigureAwait(false);

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
			using (SQLStatementWriter sb = new SQLStatementWriter(SqlServerHelper.TypeMapping, fromDbSchema))
			{
				await WriteDropStatementsAsync(sb, fromDbSchema);
				sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task WriteDropStatementsAsync(SQLStatementWriter sb, JMXSchema fromDbSchema)
		{
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(JMXFactory.DETAIL_FIELD_PREFIX)))
			{
				string[] names = att.FieldName.Split('_');
				var schemaFromDb = await GetObjectSchemaAsync(new JMXObjectName(names[1], names[2]).ToString());
				if (schemaFromDb != null)
					await WriteDropStatementsAsync(sb, schemaFromDb);

			}
			sb.WriteDropStatements(fromDbSchema);
		}

		#endregion Drop Schema

		#region GetObjectSchema
		public override JMXSchema GetObjectSchema(string objectName)
			=> GetObjectSchemaAsync(objectName).GetAwaiter().GetResult();

		public override async Task<JMXSchema> GetObjectSchemaAsync(string objectName)
		{
			var mdb = Factory.GetMdbContext();
			string s = await mdb.ExecuteAsync<string>(
				(await SqlServerHelper.IsSql17(mdb)) ? SqlServer.GetTableSchema : SqlServer.GetTableSchema_12,
				new MdbParameter("@table_name", objectName));
			if (s != null)
				return JMXSchema.Parse(s);
			return null;
		}
		#endregion GetObjectSchema

		#region Sync Schema
		public override JMXSchema SyncObjectSchema(string objectName)
			=> SyncObjectSchemaAsync(objectName).GetAwaiter().GetResult();
		
		public override async Task<JMXSchema> SyncObjectSchemaAsync(string objectName)
		{
			JMXObjectName name = objectName;
			return await SyncSchemaAsync(name.AreaName, name.ObjectName).ConfigureAwait(false);
		}
		
		/// <summary>
		/// required teting
		/// </summary>
		/// <param name="dbSchema"></param>
		/// <param name="objectName"></param>
		/// <returns></returns>
		private async Task<JMXSchema> SyncSchemaAsync(string dbSchema, string objectName)
		{
			MdbContext mdb = Factory.GetMdbContext();
			ILogger log = this.Logger;
			var schema = await _schemaRepo.GetSchemaAsync(objectName);
			
			//schema was syncronized
			//change this logic
			if (schema.SyncState == 1)
				return schema;

			foreach (var o in GetDependences(schema))
				await SyncSchemaAsync(o.AreaName, o.ObjectName);

			var schemaFromDb = await GetObjectSchemaAsync(schema.DbObjectName.ToString());
			bool createNew = (schemaFromDb == null);
			string[] sqlList;
			if (createNew)
				sqlList = CreateNewStatements(schema);
			else
			{
				// Compare with previos version of schema
				// error schema not found if db objects exists, but 
				// synced version schema don't exists
				//var prevSchema = await GetSchemaInternalAsync(mdb, dbSchema, objectName, 1);
				//!!! not tested
				var prevSchema = schemaFromDb;
				sqlList = await CompareSchemasAsync(schema, prevSchema);
			}

			await mdb.BeginTransactionAsync();
			try
			{
				foreach (var sql in sqlList)
				{
					log.LogDebug(sql);
					await mdb.ExecuteAsync(sql);
				}
				//if (!createNew)
				//	await schemaUpdate(mdb, schema);
				await mdb.ExecuteAsync($"update SysCat.SysSchemas set ObjectSchema = @ObjectSchema where id = {schema.ID}",
					new MdbParameter("@ObjectSchema", schema.ToString()));
				await mdb.ExecuteAsync(SqlServer.StateSysSchemas,
					new MdbParameter("@id", schema.ID));
				await mdb.CommitAsync();
			}
			catch
			{
				mdb.RollBack();
				throw;
			}
			return schema;
		}

		private static string[] CreateNewStatements(JMXSchema schema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(SqlServerHelper.TypeMapping, schema))
			{
				sb.WriteCreateNewTableStatements();
				sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		private async Task<string[]> CompareSchemasAsync(JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<string> sql = new List<string>();
			using (SQLStatementWriter sb = new SQLStatementWriter(SqlServerHelper.TypeMapping, schema))
			{
				await CompareSchemasStatementsAsync(sb, schema, fromDbSchema);
				string stmt = sb.ToString();
				if (stmt.Length > 1)
					sql.Add(sb.ToString());
			}
			return sql.ToArray();
		}

		/// <summary>
		/// !!! Add to GetTableSchema detail references as attributes for cascade delete FK
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="schema"></param>
		/// <param name="fromDbSchema"></param>
		/// <returns></returns>
		private async Task CompareSchemasStatementsAsync(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			MdbContext mdb = Factory.GetMdbContext();
			bool recreate = false;
			foreach (var att in fromDbSchema.Attributes.Where(a => a.FieldName.StartsWith(JMXFactory.DETAIL_FIELD_PREFIX)))
			{
				if (!schema.Attributes.Any(a => a.FieldName == att.FieldName))
				{
					string[] names = att.FieldName.Split('_');
					var schemaFromDb = await GetObjectSchemaAsync(new JMXObjectName(names[1], names[2]).ToString());
					if (schemaFromDb != null)
						await WriteDropStatementsAsync(sb, schemaFromDb);
				}

			}

			recreate = CompareAttributes(sb, schema, fromDbSchema);
			if (!recreate)
				recreate = ComparePK(sb, schema, fromDbSchema);

			if (!recreate)
			{
				CompareIndexes(sb, schema, fromDbSchema);
				CompareFK(sb, schema, fromDbSchema);
			}
			else
				await RecreateSchemaAsync(mdb, sb, schema, fromDbSchema);

			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
			{
				var schemaFromDb = await GetObjectSchemaAsync(att.ObjectSchema.DbObjectName.ToString());
				if (schemaFromDb != null)
					await CompareSchemasStatementsAsync(sb, att.ObjectSchema, schemaFromDb);
				else
					sb.WriteCreateNewTableStatements(att.ObjectSchema);

			}
		}

		private static bool CompareAttributes(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			bool recreate = false;
			int count = schema.Attributes.Count;
			List<(JMXAttribute att, JMXAttribute att2, AttribCompareDiff diff)> l =
				new List<(JMXAttribute, JMXAttribute, AttribCompareDiff)>();
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				var att2 = fromDbSchema.Attributes.FirstOrDefault(a =>
					a.FieldName.Equals(att.FieldName, StringComparison.OrdinalIgnoreCase));

				bool found = (att2 != null);
				AttribCompareDiff diff = AttribCompareDiff.none;
				if (found)
				{
					if (!att.ServerDataType.Equals(att2.ServerDataType, StringComparison.OrdinalIgnoreCase))
						diff = AttribCompareDiff.dataTtype;
					if (att.Required != att2.Required)
						diff |= AttribCompareDiff.nullOptions;
					if (att.Identity.IsIdentity != att2.Identity.IsIdentity ||
						att.Identity.Seed != att2.Identity.Seed ||
						att.Identity.Increment != att2.Identity.Increment)
						diff |= AttribCompareDiff.identity;

					if (att.DataType != MdbType.@object &&
						!att.AttribName.Equals(att2.FieldName, StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.name;

					//Server DataTypes is equals
					if ((diff & AttribCompareDiff.dataTtype) != AttribCompareDiff.dataTtype)
					{
						MdbTypeInfo ti = SqlServerHelper.TypeInfo[att.ServerDataType];
						if (!ti.FixedSize)
						{
							if (SqlServerHelper.TypeMapping.GetVariableLenghtDataTypes().Contains(att.ServerDataType) && att.DataSize.Size != att2.DataSize.Size)
								diff |= AttribCompareDiff.size;
							else if (ti.MdbType == MdbType.@decimal && (att.DataSize.Precision != att2.DataSize.Precision ||
								att.DataSize.Scale != att2.DataSize.Scale))
								diff |= AttribCompareDiff.size;
						}
					}
					if (!att.CheckConstraint
						.Definition
						.RemoveChar("[( )]".ToCharArray())
						.Equals(att2.CheckConstraint
							.Definition
							.RemoveChar("[( )]".ToCharArray()),
						StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.constraint;
					else if (!att.DefaultConstraint
						.Definition.RemoveChar("[( )]".ToCharArray())
						.Equals(att2.DefaultConstraint
							.Definition
							.RemoveChar("[( )]".ToCharArray()),
						StringComparison.OrdinalIgnoreCase))
						diff |= AttribCompareDiff.constraint;
				}
				//else if (att2 != null)
				//	diff |= AttribCompareDiff.name;
				else
					diff = AttribCompareDiff.notFound;
				l.Add((att, att2, diff));
			}
			foreach (var att2 in fromDbSchema.Attributes)
			{
				//var att = schema.Attributes.FirstOrDefault(a => a.ID == att2.ID);
				var att = schema.Attributes.FirstOrDefault(a =>
					a.FieldName.Equals(att2.FieldName, StringComparison.OrdinalIgnoreCase));
				if (att == null)
					l.Add((att, att2, AttribCompareDiff.remove));
			}

			foreach (var (att, att2, diff) in l)
			{
				if ((diff & AttribCompareDiff.remove) == AttribCompareDiff.remove)
				{
					sb.WriteDropColumnStatement(att2);
					continue;
				}
				if ((diff & AttribCompareDiff.constraint) == AttribCompareDiff.constraint)
				{
					sb.WriteDropConstraintStatement(att2, fromDbSchema);
					sb.WriteCreateConstraintStatement(att);
				}
				if ((diff & AttribCompareDiff.dataTtype) == AttribCompareDiff.dataTtype ||
					(diff & AttribCompareDiff.size) == AttribCompareDiff.size ||
					(diff & AttribCompareDiff.nullOptions) == AttribCompareDiff.nullOptions)
					sb.WriteAlterColumnStatement(att);
				else if ((diff & AttribCompareDiff.notFound) == AttribCompareDiff.notFound)
					sb.WriteAlterColumnStatement(att, true);
				else if ((diff & AttribCompareDiff.name) == AttribCompareDiff.name)
				{
					att.FieldName = att.AttribName;
					sb.WriteRenameColumnStatement(att2.FieldName, att.FieldName);
				}
				else if ((diff & AttribCompareDiff.identity) == AttribCompareDiff.identity)
					recreate = true;
			}
			return recreate;
		}

		private static bool ComparePK(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			///Add PK compare
			///writeDropPKStatement
			///writeCreatePKStatement
			///The constraint 'XPK1Requests' is being referenced by table 'PaymentStateHists', 
			///foreign key constraint 'FK_PAYMENTSTATEHISTS_REQUESTS'.
			///Could not drop constraint. See previous errors.
			if (schema.PrimaryKey == null && fromDbSchema.PrimaryKey != null)
				//writeDropPKStatement(sb, fromDbSchema);
				return true;
			else if (schema.PrimaryKey != null && fromDbSchema.PrimaryKey == null)
				sb.WriteCreatePKStatement();
			else if (schema.PrimaryKey != null &&
				schema.PrimaryKey.KeyName != fromDbSchema.PrimaryKey.KeyName ||
				schema.PrimaryKey.KeyMembers == fromDbSchema.PrimaryKey.KeyMembers)
			{
				//writeDropPKStatement(sb, fromDbSchema);
				//writeCreatePKStatement(sb, schema);
				return true;
			}
			return false;
		}

		private static void CompareIndexes(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<(JMXIndex i1, JMXIndex i2, DbObjectOnDiffActions action)> l =
				new List<(JMXIndex i1, JMXIndex i2, DbObjectOnDiffActions action)>();
			int count = schema.Indexes.Count;
			for (int i = 0; i < count; i++)
			{
				var i1 = schema.Indexes[i];
				var i2 = fromDbSchema.Indexes.FirstOrDefault(index => index.IndexName == i1.IndexName);
				if (i2 == null)
					l.Add((i1, i2, DbObjectOnDiffActions.add));
				else if (i1.ClusteredOption != i2.ClusteredOption)
					l.Add((i1, i2, DbObjectOnDiffActions.alter));
				else if (i1.IsUnique != i2.IsUnique)
					l.Add((i1, i2, DbObjectOnDiffActions.alter));
				else
				{
					foreach (var m in i1.KeyMembers)
					{
						var m2 = i2.KeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((i1, i2, DbObjectOnDiffActions.alter));
							break;
						}

					}
				}
			}
			count = fromDbSchema.Indexes.Count;
			for (int i = 0; i < count; i++)
			{
				var i2 = fromDbSchema.Indexes[i];
				var i1 = schema.Indexes.FirstOrDefault(index => index.IndexName == i2.IndexName);
				if (i1 == null)
					l.Add((i1, i2, DbObjectOnDiffActions.drop));
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.drop)
					sb.WriteDropIndexStatement(i2, fromDbSchema);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					sb.WriteCreateIndexStatement(i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					sb.WriteDropIndexStatement(i2, fromDbSchema);
					sb.WriteCreateIndexStatement(i1);
				}
			}

		}

		private static void CompareFK(SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			List<(JMXForeignKey i1, JMXForeignKey i2, DbObjectOnDiffActions action)> l =
				new List<(JMXForeignKey i1, JMXForeignKey i2, DbObjectOnDiffActions action)>();
			int count = schema.ForeignKeys.Count;
			for (int i = 0; i < count; i++)
			{
				var k1 = schema.ForeignKeys[i];
				var k2 = fromDbSchema.ForeignKeys.FirstOrDefault(fk => fk.KeyName == k1.KeyName);
				if (k2 == null)
					l.Add((k1, k2, DbObjectOnDiffActions.add));
				else if (k1.CheckOption != k2.CheckOption ||
					k1.DeleteRefAction != k2.DeleteRefAction ||
					k1.UpdateRefAction != k2.UpdateRefAction ||
					!k1.RefDbObjectName.ToString().Equals(k2.RefDbObjectName.ToString(), StringComparison.OrdinalIgnoreCase))
					l.Add((k1, k2, DbObjectOnDiffActions.alter));
				else
				{
					foreach (var m in k1.KeyMembers)
					{
						var m2 = k2.KeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((k1, k2, DbObjectOnDiffActions.alter));
							break;
						}

					}
					foreach (var m in k1.RefKeyMembers)
					{
						var m2 = k2.RefKeyMembers.FirstOrDefault(memeber => memeber.FieldName == m.FieldName && memeber.Position == m.Position);
						if (m != m2)
						{
							l.Add((k1, k2, DbObjectOnDiffActions.alter));
							break;
						}

					}
				}
			}

			count = fromDbSchema.ForeignKeys.Count;
			for (int i = 0; i < count; i++)
			{
				var k2 = fromDbSchema.ForeignKeys[i];
				var k1 = schema.ForeignKeys.FirstOrDefault(fk => fk.KeyName == k2.KeyName);
				if (k1 == null)
					l.Add((k1, k2, DbObjectOnDiffActions.drop));
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.drop)
					sb.WriteDropFKStatement(i2, fromDbSchema);
			}
			foreach (var (i1, i2, action) in l)
			{
				if (action == DbObjectOnDiffActions.add)
					sb.WriteCreateFKStatement(i1);
				else if (action == DbObjectOnDiffActions.alter)
				{
					sb.WriteDropFKStatement(i2, fromDbSchema);
					sb.WriteCreateFKStatement(i1);
				}
			}
		}

		private static async Task RecreateSchemaAsync(MdbContext mdb, SQLStatementWriter sb, JMXSchema schema, JMXSchema fromDbSchema)
		{
			int recCount = (await mdb.ExecuteAsync<int>($"select count(*) from {fromDbSchema.DbObjectName.ToString()}"));

			foreach (var fk in fromDbSchema.ParentRelations)
				sb.WriteDropParentRelationStatement(fk);

			foreach (var fk in fromDbSchema.ForeignKeys)
				sb.WriteDropFKStatement(fk, fromDbSchema);

			string tmpTableName = fromDbSchema.DbObjectName.ObjectName + "_" + DateTime.Now.Subtract(vbo.Date()).Seconds.ToString();
			if (recCount > 0)
				sb.WriteRenameTableStatement(tmpTableName, fromDbSchema);
			else
				sb.WriteDropTableStatement(null, fromDbSchema);

			sb.WriteCreateTableStatements(schema);

			if (recCount > 0)
			{
				sb.WriteInsertRowsStatement(tmpTableName, schema);
				sb.WriteDropTableStatement(tmpTableName, fromDbSchema);
			}

			sb.WriteCreatePKStatement(schema);
			foreach (var att in schema.Attributes)
				sb.WriteCreateConstraintStatement(att, schema);
			foreach (var index in schema.Indexes)
				sb.WriteCreateIndexStatement(index, schema);
			foreach (var fk in schema.ForeignKeys)
				sb.WriteCreateFKStatement(fk, schema);

			/// Need test
			foreach (var fk in fromDbSchema.ParentRelations)
				sb.WriteCreateParentRelationStatement(fk);
		}

		private static string GetDiffs(AttribCompareDiff diff)
		{
			List<string> l = new List<string>();
			if (((diff & AttribCompareDiff.notFound) == AttribCompareDiff.notFound))
				l.Add(AttribCompareDiff.notFound.ToString());
			if (((diff & AttribCompareDiff.name) == AttribCompareDiff.name))
				l.Add(AttribCompareDiff.name.ToString());
			if (((diff & AttribCompareDiff.dataTtype) == AttribCompareDiff.dataTtype))
				l.Add(AttribCompareDiff.dataTtype.ToString());
			if (((diff & AttribCompareDiff.size) == AttribCompareDiff.size))
				l.Add(AttribCompareDiff.size.ToString());
			if (((diff & AttribCompareDiff.nullOptions) == AttribCompareDiff.nullOptions))
				l.Add(AttribCompareDiff.nullOptions.ToString());
			if (((diff & AttribCompareDiff.identity) == AttribCompareDiff.identity))
				l.Add(AttribCompareDiff.identity.ToString());
			if (l.Count == 0)
				l.Add(AttribCompareDiff.none.ToString());
			return string.Join('\t', l.ToArray());
		}

		private static JMXObjectName[] GetDependences(JMXSchema schema) =>
			schema.ForeignKeys.Select(fk => fk.RefDbObjectName).ToArray();
		#endregion Sync Schema
	}
}
