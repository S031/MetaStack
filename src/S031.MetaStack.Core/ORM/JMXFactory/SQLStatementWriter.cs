using Newtonsoft.Json.Linq;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Core.ORM
{
	internal class SQLStatementWriter
	{
		private readonly StringBuilder _sb = new StringBuilder();
		private JMXSchema _schema;
		private JMXRepo _repo;
		public SQLStatementWriter(JMXRepo repo, JMXSchema schema)
		{
			_repo = repo;
			_schema = schema;
		}
		private void WriteCreateNewTableStatements(StringBuilder sb, JMXSchema schema)
		{
			WriteCreateTableStatements(sb, schema);
			WriteCreatePKStatement(sb, schema);
			foreach (var att in schema.Attributes)
				WriteCreateConstraintStatement(sb, schema, att);
			foreach (var index in schema.Indexes)
				WriteCreateIndexStatement(sb, schema, index);
			foreach (var fk in schema.ForeignKeys)
				WriteCreateFKStatement(sb, schema, fk);
			WriteCreateDetailTableStatements(sb, schema);
		}

		private static void WriteCreateTableStatements(StringBuilder sb, JMXSchema schema)
		{
			sb.Append($"create table {schema.DbObjectName.ToString()}");
			sb.Append("(\n");
			int count = schema.Attributes.Count;
			for (int i = 0; i < count; i++)
			{
				var att = schema.Attributes[i];
				sb.Append($"[{att.FieldName}]\t{att.ServerDataType}");

				MdbTypeInfo ti = _typeInfo[att.ServerDataType];
				if (att.DataType == MdbType.@decimal && !ti.FixedSize && !att.DataSize.IsEmpty())
					sb.Append($"({att.DataSize.Precision},{att.DataSize.Scale})");
				else if (variable_lenght_data_types.IndexOf(att.ServerDataType) > -1)
					if (att.DataSize.Size == -1)
						sb.Append("(max)");
					else
						sb.Append($"({att.DataSize.Size})");

				if (att.Identity.IsIdentity)
					sb.Append($"\tidentity({att.Identity.Seed},{att.Identity.Increment})");
				else
					sb.Append($"\t{att.NullOption}");

				if (i != count - 1)
					sb.Append(",\n");

				att.ID = i + 1;
			}
			sb.Append(")\n");
		}

		private static void WriteCreateDetailTableStatements(StringBuilder sb, JMXSchema schema)
		{
			foreach (var att in schema.Attributes.Where(a => a.DataType == MdbType.@object))
				WriteCreateNewTableStatements(sb, att.ObjectSchema);
		}

		private static void WriteCreatePKStatement(StringBuilder sb, JMXSchema schema)
		{
			var pk = schema.PrimaryKey;
			if (pk != null)
			{
				sb.Append($"alter table {schema.DbObjectName} add constraint [{pk.KeyName}] primary key (");
				int count = pk.KeyMembers.Count;
				for (int i = 0; i < count; i++)
				{
					var m = pk.KeyMembers[i];
					sb.Append($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
					if (i != count - 1)
						sb.Append(", ");
					else
						sb.Append(")\n");

				}
			}
		}

		private static void WriteCreateIndexStatement(StringBuilder sb, JMXSchema schema, JMXIndex index)
		{
			//CREATE UNIQUE NONCLUSTERED INDEX [AK1_SysSchemas] ON [SysCat].[SysSchemas] ([SysAreaID] ASC, [ObjectName] ASC)
			sb.Append("create " + (index.IsUnique ? "unique " : "") + (index.ClusteredOption == 1 ? "clustered " : "nonclustered ") +
				$"index [{index.IndexName}] " +
				$"on {schema.DbObjectName.ToString()} (");
			int count = index.KeyMembers.Count;
			for (int i = 0; i < count; i++)
			{
				var m = index.KeyMembers[i];
				sb.Append($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
				if (i != count - 1)
					sb.Append(", ");
				else
					sb.Append(")\n");
			}
		}

		private static void WriteCreateFKStatement(StringBuilder sb, JMXSchema schema, JMXForeignKey fk)
		{
			//Можно сделать сначала с NOCHECK затем CHECK
			//	ALTER TABLE[SysCat].[SysSchemas] WITH CHECK ADD CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]([SysAreaID]) REFERENCES[SysCat].[SysAreas] ([ID]) 
			//	ALTER TABLE[SysCat].[SysSchemas] CHECK CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]
			if (fk.CheckOption)
				// Enable for new added rows
				sb.Append($"alter table {schema.DbObjectName} with check  add constraint [{fk.KeyName}] foreign key (");
			else
				sb.Append($"alter table {schema.DbObjectName} with nocheck  add constraint [{fk.KeyName}] foreign key (");

			sb.Append(string.Join(", ", fk.KeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
			sb.Append(")");
			sb.Append($"references {fk.RefDbObjectName} (");
			sb.Append(string.Join(", ", fk.RefKeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
			sb.Append(")\n");
			if (fk.CheckOption)
				// check existing rows
				sb.Append($"alter table {schema.DbObjectName} check constraint [{fk.KeyName}]\n");
		}

		private static void WriteCreateConstraintStatement(StringBuilder sb, JMXSchema schema, JMXAttribute att)
		{
			if (!att.CheckConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} add constraint {att.CheckConstraint.ConstraintName} check({att.CheckConstraint.Definition})\n");
			if (!att.DefaultConstraint.IsEmpty())
				sb.Append($"alter table {schema.DbObjectName} add constraint {att.DefaultConstraint.ConstraintName} default({att.DefaultConstraint.Definition}) for [{att.FieldName}]\n");
		}

		private static void WriteCreateParentRelationStatement(StringBuilder sb, JMXSchema schema, JToken fk)
		{
			bool withCheck = (bool)fk["CheckOption"];
			string check = withCheck ? "check" : "nocheck";
			// Enable for new added rows
			sb.AppendFormat("alter table [{0}].[{1}] with {2} add constraint [{3}] foreign key (",
			(string)fk["ParentObject"]["AreaName"],
			(string)fk["ParentObject"]["ObjectName"],
			check, (string)fk["KeyName"]);


			sb.Append(string.Join(", ", fk["KeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			sb.Append(")");
			sb.Append($"references [{fk["RefObject"]["AreaName"]}].[{fk["RefObject"]["ObjectName"]}] (");
			sb.Append(string.Join(", ", fk["RefKeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			sb.Append(")\n");
			if (withCheck)
				// check existing rows
				sb.Append($"alter table [{fk["ParentObject"]["AreaName"]}].[{fk["ParentObject"]["ObjectName"]}] " +
					$"check constraint [{(string)fk["KeyName"]}]\n");

		}

	}
}
