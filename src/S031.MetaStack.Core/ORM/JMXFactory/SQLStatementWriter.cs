﻿using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Core.ORM
{
	public class SQLStatementWriter : StringWriter
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
		public SQLStatementWriter WriteSelectStatement(JMXSchema fromSchema,
			params JMXCondition[] conditions)
		{
			if (fromSchema == null)
				fromSchema = _schema;

			string target = _schemaSupport ?
				fromSchema.DbObjectName.ToString() :
				fromSchema.DbObjectName.ObjectName;

			if (fromSchema.DbObjectType == DbObjectTypes.Procedure ||
				fromSchema.DbObjectType == DbObjectTypes.Action)
				Write(target);

			else if (fromSchema.DbObjectType == DbObjectTypes.View ||
				fromSchema.DbObjectType == DbObjectTypes.Function ||
				fromSchema.DbObjectType == DbObjectTypes.Table)
			{
				WriteLine("SELECT");
				bool first = true;
				foreach (var att in fromSchema.Attributes)
					if (first)
					{
						WriteSelectColumnStatement(fromSchema, att, "");
						first = false;
					}
					else
						WriteSelectColumnStatement(fromSchema, att, ",");

				foreach (var fk in fromSchema.ForeignKeys)
				{
					foreach (var m in fk.MigrateKeyMembers)
					{
						//Migrated attribute may be not null
						WriteLine($"\t,{fk.KeyName}.{m.FieldName} [{fk.KeyName}.{m.FieldName}]");
					}
				}
				WriteLine($"FROM {target} AS {fromSchema.DbObjectName.ObjectName}");
				foreach (var fk in fromSchema.ForeignKeys)
				{
					if (fk.MigrateKeyMembers.Count > 0)
					{
						bool required = fk.KeyMembers
							.Any(m => fromSchema.Attributes
								.Any(a => a.FieldName.Equals(m.FieldName, StringComparison.CurrentCultureIgnoreCase) && a.Required));

						string fkObj = _schemaSupport ?
							fk.RefDbObjectName.ToString() :
							fk.RefDbObjectName.ObjectName;

						if (required)
							Write($"INNER JOIN ");
						else 
							Write("LEFT JOIN ");
						Write($"{fkObj} AS {fk.KeyName} ON ");
						for (int i = 0; i < fk.KeyMembers.Count; i++)
							WriteLine($"{fromSchema.DbObjectName.ObjectName}.{fk.KeyMembers[i].FieldName} == {fk.KeyName}.{fk.RefKeyMembers[i].FieldName}");
					}
				}
				var cs = fromSchema.Conditions.Concat(conditions);
				if (cs.Count() > 0)
				{
					foreach (var join in cs.Where(c => c.ConditionType == JMXConditionTypes.Join))
						WriteLine(join.Definition);

					first = true;
					foreach (var filter in cs.Where(c => c.ConditionType == JMXConditionTypes.Where))
						if (first)
						{
							WriteLine($"WHERE ({filter.Definition})");
							first = false;
						}
						else
							WriteLine($"AND ({filter.Definition})");

					if (cs.Any(c => c.ConditionType == JMXConditionTypes.OrderBy))
					{
						var sort = cs.Last(c => c.ConditionType == JMXConditionTypes.OrderBy);
						WriteLine($"ORDER BY {sort.Definition}");
					}

					if (cs.Any(c => c.ConditionType == JMXConditionTypes.GroupBy))
					{
						var group = cs.LastOrDefault(c => c.ConditionType == JMXConditionTypes.GroupBy);
						WriteLine($"GROUP BY {group.Definition}");
					}
					first = true;
					foreach (var filter in cs.Where(c => c.ConditionType == JMXConditionTypes.Havind))
						if (first)
						{
							WriteLine($"HAVING ({filter.Definition})");
							first = false;
						}
						else
							WriteLine($"AND ({filter.Definition})");
				}
			}
			return this;
		}

		public SQLStatementWriter WriteSelectColumnStatement(JMXSchema fromSchema,
			JMXAttribute att, string sep)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			if (att.Required)
				WriteLine($"\t{sep}{fromSchema.DbObjectName.ObjectName}.{att.FieldName} {att.AttribName}");
			else if (att.DataType == MdbType.@object)
				WriteLine($"\t{sep}'***' {att.AttribName}");
			else
			{
				string emptyValue = att.DataType.Type().IsNumeric() ? "0" : "''";
				WriteLine($"\t{sep}COALESCE({fromSchema.DbObjectName.ObjectName}.{att.FieldName}, {emptyValue}) as {att.AttribName}");
			}
			return this;
		}

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

			Write(string.Join(", ", fk.KeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
			Write(")");
			Write($"references {fk.RefDbObjectName} (");
			Write(string.Join(", ", fk.RefKeyMembers.Select(m => '[' + m.FieldName + ']').ToArray()));
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

		public SQLStatementWriter WriteCreateParentRelationStatement(JToken fk)
		{
			bool withCheck = (bool)fk["CheckOption"];
			string check = withCheck ? "check" : "nocheck";
			// Enable for new added rows
			Write("alter table [{0}].[{1}] with {2} add constraint [{3}] foreign key (".ToFormat(
				(string)fk["ParentObject"]["AreaName"],
				(string)fk["ParentObject"]["ObjectName"],
				check, (string)fk["KeyName"]));

			Write(string.Join(", ", fk["KeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			Write(")\n");
			Write($"references [{fk["RefObject"]["AreaName"]}].[{fk["RefObject"]["ObjectName"]}] (");
			Write(string.Join(", ", fk["RefKeyMembers"].Select(m => '[' + (string)m["FieldName"] + ']').ToArray()));
			Write(")\n");
			if (withCheck)
				// check existing rows
				Write($"alter table [{fk["ParentObject"]["AreaName"]}].[{fk["ParentObject"]["ObjectName"]}] " +
					$"check constraint [{(string)fk["KeyName"]}]\n");
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
			int count =	fromSchema.Attributes.Count;
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

		public SQLStatementWriter WriteDropStatements(string parentRelationsSchema, JMXSchema fromSchema = null)
		{
			if (fromSchema == null)
				fromSchema = _schema;
			if (!parentRelationsSchema.IsEmpty())
			{
				JArray a = JArray.Parse(parentRelationsSchema);
				foreach (var o in a)
				{
					string sch = (string)o["ParentObject"]["AreaName"];
					string tbl = (string)o["ParentObject"]["ObjectName"];
					if (!fromSchema.Attributes.Any(at =>
						at.FieldName == $"{detail_field_prefix}{sch}_{tbl}"))
						WriteDropParentRelationStatement(o);
				}
			}
			foreach (var fk in fromSchema.ForeignKeys)
				WriteDropFKStatement(fk);
			WriteDropTableStatement();
			return this;
		}

		public SQLStatementWriter WriteDropParentRelationStatement(JToken o)
		{
			Write("alter table [{0}].[{1}] drop constraint [{2}]\n".ToFormat(
				(string)o["ParentObject"]["AreaName"],
				(string)o["ParentObject"]["ObjectName"],
				(string)o["KeyName"]));
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
	}
}
