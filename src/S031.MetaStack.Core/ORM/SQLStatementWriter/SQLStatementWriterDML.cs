using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace S031.MetaStack.Core.ORM
{
    public partial class SQLStatementWriter : StringWriter
    {
        #region DML
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
                                .Any(a => a.FieldName.Equals(m.FieldName, StringComparison.OrdinalIgnoreCase) && a.Required));

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
                    WriteConditionsStatement(fromSchema, cs);
            }
            return this;
        }
        
        public SQLStatementWriter WriteConditionsStatement(JMXSchema fromSchema,
            IEnumerable<JMXCondition> cs)
        {
            if (fromSchema == null)
                fromSchema = _schema;
            foreach (var join in cs.Where(c => c.ConditionType == JMXConditionTypes.Join))
                WriteLine(join.Definition);

            bool first = true;
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
            return this;
        }

        public SQLStatementWriter WriteSelectColumnStatement(JMXSchema fromSchema,
            JMXAttribute att, string sep)
        {
            if (fromSchema == null)
                fromSchema = _schema;
            if (att.Required)
                WriteLine($"\t{sep}{fromSchema.DbObjectName.ObjectName}.{att.FieldName} as {att.AttribName}");
            else if (att.DataType == MdbType.@object)
                WriteLine($"\t{sep}'***' {att.AttribName}");
            else
            {
                string emptyValue = att.DataType.Type().IsNumeric() ? "0" : "''";
                WriteLine($"\t{sep}COALESCE({fromSchema.DbObjectName.ObjectName}.{att.FieldName}, {emptyValue}) as {att.AttribName}");
            }
            return this;
        }

        public SQLStatementWriter WriteEqualsColumnStatement(JMXSchema fromSchema,
            JMXAttribute att, string variableName, string sep = "")
        {
            if (fromSchema == null)
                fromSchema = _schema;
            Write($"{sep}{fromSchema.DbObjectName.ObjectName}.{att.FieldName} = {variableName}");
            return this;
        }

        #endregion DML
    }
}
