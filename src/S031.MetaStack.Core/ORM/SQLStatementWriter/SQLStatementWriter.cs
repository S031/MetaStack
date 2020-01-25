using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;
using System.IO;
using System.Linq;

namespace S031.MetaStack.Core.ORM
{
	public partial class SQLStatementWriter : StringWriter
	{
		const string detail_field_prefix = "$1_";
		private readonly JMXSchema _schema;
		private readonly IJMXTypeMapping _typeMapping;
		private readonly bool _schemaSupport = true;


		public SQLStatementWriter(IJMXTypeMapping typeMapping)
		{
			_typeMapping = typeMapping;
			_schemaSupport = true;
		}
		public SQLStatementWriter(IJMXTypeMapping typeMapping, bool schemaSupport)
		{
			_typeMapping = typeMapping;
			_schemaSupport = schemaSupport;
		}

		public SQLStatementWriter(IJMXTypeMapping typeMapping, JMXSchema schema, bool schemaSupport = true)
		{
			_typeMapping = typeMapping;
			_schema = schema;
			_schemaSupport = schemaSupport;
		}

		public new SQLStatementWriter Write(string source)
		{
			base.Write(source);
			return this;
		}

		#region DDL
		public SQLStatementWriter WriteCreateNewTableStatements(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			WriteCreateTableStatements();
			WriteCreatePKStatement();
			foreach (var att in fromSchema.Attributes)
				WriteCreateConstraintStatement(att);
			foreach (var index in fromSchema.Indexes)
				WriteCreateIndexStatement(index);
			foreach (var fk in fromSchema.ForeignKeys)
				WriteCreateFKStatement(fk);
			WriteCreateDetailTableStatements();
			return this;
		}

		public SQLStatementWriter WriteCreateTableStatements(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write($"create table {fromSchema.DbObjectName.ToString()}");
			Write("(\n");
			int count = fromSchema.Attributes.Count;
			for (int i = 0; i < count; i++)
			{
				var att = fromSchema.Attributes[i];
				Write($"[{att.FieldName}]\t{att.ServerDataType}");

				MdbTypeInfo ti = _typeMapping.GetServerTypeMap()[att.ServerDataType];
				if (att.DataType == MdbType.@decimal && !ti.FixedSize && !att.DataSize.IsEmpty())
					Write($"({att.DataSize.Precision},{att.DataSize.Scale})");
				else if (_typeMapping.GetVariableLenghtDataTypes().Contains(att.ServerDataType))
					if (att.DataSize.Size == -1)
						Write("(max)");
					else
						Write($"({att.DataSize.Size})");

				if (att.Identity.IsIdentity)
					Write($"\tidentity({att.Identity.Seed},{att.Identity.Increment})");
				else
					Write($"\t{att.NullOption}");

				if (i != count - 1)
					Write(",\n");

				att.ID = i + 1;
			}
			Write(")\n");
			return this;
		}

		public SQLStatementWriter WriteCreateDetailTableStatements(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			foreach (var att in fromSchema.Attributes.Where(a => a.DataType == MdbType.@object))
				WriteCreateNewTableStatements(att.ObjectSchema);
			return this;
		}

		public SQLStatementWriter WriteCreatePKStatement(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			var pk = fromSchema.PrimaryKey;
			if (pk != null)
			{
				Write($"alter table {fromSchema.DbObjectName} add constraint [{pk.KeyName}] primary key (");
				int count = pk.KeyMembers.Count;
				for (int i = 0; i < count; i++)
				{
					var m = pk.KeyMembers[i];
					Write($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
					if (i != count - 1)
						Write(", ");
					else
						Write(")\n");

				}
			}
			return this;
		}

		public SQLStatementWriter WriteCreateIndexStatement(JMXIndex index, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			//CREATE UNIQUE NONCLUSTERED INDEX [AK1_SysSchemas] ON [SysCat].[SysSchemas] ([SysAreaID] ASC, [ObjectName] ASC)
			Write("create " + (index.IsUnique ? "unique " : "") + (index.ClusteredOption == 1 ? "clustered " : "nonclustered ") +
				$"index [{index.IndexName}] " +
				$"on {fromSchema.DbObjectName.ToString()} (");
			int count = index.KeyMembers.Count;
			for (int i = 0; i < count; i++)
			{
				var m = index.KeyMembers[i];
				Write($"[{m.FieldName}] " + (m.IsDescending ? "DESC" : "ASC"));
				if (i != count - 1)
					Write(", ");
				else
					Write(")\n");
			}
			return this;
		}

		public SQLStatementWriter WriteCreateFKStatement(JMXForeignKey fk, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			//Можно сделать сначала с NOCHECK затем CHECK
			//	ALTER TABLE[SysCat].[SysSchemas] WITH CHECK ADD CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]([SysAreaID]) REFERENCES[SysCat].[SysAreas] ([ID]) 
			//	ALTER TABLE[SysCat].[SysSchemas] CHECK CONSTRAINT[FK_SYSSCHEMAS_SYSAREAS]
			if (fk.CheckOption)
				// Enable for new added rows
				Write($"alter table {fromSchema.DbObjectName} with check  add constraint [{fk.KeyName}] foreign key (");
			else
				Write($"alter table {fromSchema.DbObjectName} with nocheck  add constraint [{fk.KeyName}] foreign key (");

			Write(string.Join(", ", fk.KeyMembers.Select(m => "[" + m.FieldName + "]").ToArray()));
			Write(")");
			Write($"references {fk.RefDbObjectName} (");
			Write(string.Join(", ", fk.RefKeyMembers.Select(m => "[" + m.FieldName + "]").ToArray()));
			Write(")\n");
			if (fk.CheckOption)
				// check existing rows
				Write($"alter table {fromSchema.DbObjectName} check constraint [{fk.KeyName}]\n");
			return this;
		}

		public SQLStatementWriter WriteCreateConstraintStatement(JMXAttribute att, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			if (!att.CheckConstraint.IsEmpty())
				Write($"alter table {fromSchema.DbObjectName} add constraint {att.CheckConstraint.ConstraintName} check({att.CheckConstraint.Definition})\n");
			if (!att.DefaultConstraint.IsEmpty())
				Write($"alter table {fromSchema.DbObjectName} add constraint {att.DefaultConstraint.ConstraintName} default({att.DefaultConstraint.Definition}) for [{att.FieldName}]\n");
			return this;
		}

		/// <summary>
		/// Remove comment && update
		/// </summary>
		/// <param name="fk"></param>
		/// <returns></returns>
		public SQLStatementWriter WriteCreateParentRelationStatement(JMXForeignKey fk)
		{
			//bool withCheck = (bool)fk["CheckOption"];
			//string check = withCheck ? "check" : "nocheck";
			//// Enable for new added rows
			//Write("alter table [{0}].[{1}] with {2} add constraint [{3}] foreign key (".ToFormat(
			//	(string)fk["ParentObject"]["AreaName"],
			//	(string)fk["ParentObject"]["ObjectName"],
			//	check, (string)fk["KeyName"]));

			//Write(string.Join(", ", (fk["KeyMembers"] as JsonArray).Select(m => "[" + (string)m["FieldName"] + "]").ToArray()));
			//Write(")\n");
			//Write($"references [{fk["RefObject"]["AreaName"]}].[{fk["RefObject"]["ObjectName"]}] (");
			//Write(string.Join(", ", (fk["RefKeyMembers"] as JsonArray).Select(m => "[" + (string)m["FieldName"] + "]").ToArray()));
			//Write(")\n");
			//if (withCheck)
			//	// check existing rows
			//	Write($"alter table [{fk["ParentObject"]["AreaName"]}].[{fk["ParentObject"]["ObjectName"]}] " +
			//		$"check constraint [{(string)fk["KeyName"]}]\n");
			return this;
		}

		public SQLStatementWriter WriteRenameTableStatement(string newName, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			//EXEC sp_rename 'Sales.SalesTerritory', 'SalesTerr'; 
			Write($"EXEC sp_rename '{fromSchema.DbObjectName}', '{newName}'\n");
			return this;
		}

		public SQLStatementWriter WriteInsertRowsStatement(string tmpTableName, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			/// [dbo].[Customers]
			//		([Name])

			//select
			//	Name
			//From Bank.dbo.Clients where ClType = 0;
			Write($"insert into {fromSchema.DbObjectName} (\n");
			int count = fromSchema.Attributes.Count;
			for (int i = 0; i < count; i++)
			{
				var att = fromSchema.Attributes[i];
				if (!att.Identity.IsIdentity)
				{
					Write($"[{att.FieldName}]");
					if (i != count - 1)
						Write(",\n");
				}
				att.ID = i + 1;
			}
			Write(")\nselect\n");
			for (int i = 0; i < count; i++)
			{
				var att = fromSchema.Attributes[i];
				if (!att.Identity.IsIdentity)
				{
					Write($"[{att.FieldName}]");
					if (i != count - 1)
						Write(",\n");
				}
				att.ID = i + 1;
			}
			Write($"from [{fromSchema.DbObjectName.AreaName}].[{tmpTableName}]");
			return this;
		}

		/// <summary>
		/// ALTER TABLE only allows columns to be added that can contain nulls, 
		/// or have a DEFAULT definition specified, 
		/// or the column being added is an identity or timestamp column, 
		/// or alternatively if none of the previous conditions are satisfied the table must be empty
		/// to allow addition of this column. 
		/// </summary>
		public SQLStatementWriter WriteAlterColumnStatement(JMXAttribute att, bool addNew = false, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			string action = addNew ? "add" : "alter column";
			Write($"alter table [{fromSchema.DbObjectName.AreaName}].[{fromSchema.DbObjectName.ObjectName}] {action} [{att.FieldName}]\t{att.ServerDataType}");

			MdbTypeInfo ti = _typeMapping.GetServerTypeMap()[att.ServerDataType];
			if (att.DataType == MdbType.@decimal && !ti.FixedSize && !att.DataSize.IsEmpty())
				Write($"({att.DataSize.Precision},{att.DataSize.Scale})");
			else if (_typeMapping.GetVariableLenghtDataTypes().Contains(att.ServerDataType))
				if (att.DataSize.Size == -1)
					Write("(max)");
				else
					Write($"({att.DataSize.Size})");
			Write($"\t{att.NullOption}\n");
			return this;
		}

		public SQLStatementWriter WriteRenameColumnStatement(string oldName, string newName, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write($"exec sp_rename '{fromSchema.DbObjectName}.{oldName}', '{newName}', 'COLUMN'\n");
			return this;
		}

		public SQLStatementWriter WriteDropColumnStatement(JMXAttribute att, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write($"alter table [{fromSchema.DbObjectName.AreaName}].[{fromSchema.DbObjectName.ObjectName}] drop column [{att.FieldName}]\n");
			return this;
		}

		public SQLStatementWriter WriteDropIndexStatement(JMXIndex index, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write($"drop index {index.IndexName} on {fromSchema.DbObjectName}\n");
			return this;
		}

		public SQLStatementWriter WriteDropPKStatement(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write($"alter table {fromSchema.DbObjectName} drop constraint {fromSchema.PrimaryKey.KeyName}\n");
			return this;
		}

		public SQLStatementWriter WriteDropConstraintStatement(JMXAttribute att, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			if (!att.DefaultConstraint.IsEmpty())
				Write($"alter table {fromSchema.DbObjectName} drop constraint {att.DefaultConstraint.ConstraintName}\n");
			if (!att.CheckConstraint.IsEmpty())
				Write($"alter table {fromSchema.DbObjectName} drop constraint {att.CheckConstraint.ConstraintName}\n");
			return this;
		}

		/// <summary>
		/// !!! Required testing after fromSchema.ParentRelations are added
		/// </summary>
		/// <param name="fromSchema"></param>
		/// <returns></returns>
		public SQLStatementWriter WriteDropStatements(JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			foreach (var fk in fromSchema.ParentRelations)
			{
				//!!! test this
				string sch = fk.RefDbObjectName.AreaName;
				string tbl = fk.RefDbObjectName.ObjectName;
				if (!fromSchema.Attributes.Any(at =>
					at.FieldName == $"{detail_field_prefix}{sch}_{tbl}"))
					WriteDropParentRelationStatement(fk);
			}
			foreach (var fk in fromSchema.ForeignKeys)
				WriteDropFKStatement(fk);

			WriteDropTableStatement();
			return this;
		}

		public SQLStatementWriter WriteDropParentRelationStatement(JMXForeignKey fk)
		{
			Write("alter table [{0}].[{1}] drop constraint [{2}]\n".ToFormat(
				fk.RefDbObjectName.AreaName,
				fk.RefDbObjectName.ObjectName,
				fk.KeyName));
			return this;
		}

		public SQLStatementWriter WriteDropFKStatement(JMXForeignKey fk, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write("alter table [{0}].[{1}] drop constraint [{2}]\n".ToFormat(
				fromSchema.DbObjectName.AreaName,
				fromSchema.DbObjectName.ObjectName,
				fk.KeyName));
			return this;
		}

		public SQLStatementWriter WriteDropTableStatement(string tmpTableName = null, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			Write("drop table [{0}].[{1}]\n".ToFormat(
				fromSchema.DbObjectName.AreaName,
				tmpTableName.IsEmpty() ? fromSchema.DbObjectName.ObjectName : tmpTableName));
			return this;
		}
		#endregion DDL
	}
}
