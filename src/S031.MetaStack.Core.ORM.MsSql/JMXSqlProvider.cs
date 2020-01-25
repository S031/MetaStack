using System;
using System.Threading.Tasks;
using System.Linq;
using S031.MetaStack.Data;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public class JMXSqlProvider : JMXProvider
	{
		public JMXSqlProvider(JMXSqlFactory factory) : base(factory)
		{
		}

		public override JMXObject Read(JMXObjectName objectName, long id)
			=> ReadAsync(objectName, id)
			.GetAwaiter()
			.GetResult();

		public override async Task<JMXObject> ReadAsync(JMXObjectName objectName, long id)
		{
			JMXObject o = new JMXObject(objectName, Factory);
			JMXSchema schema = o.Schema;
			if (!_statementsCache.TryGetValue(objectName, out string sql))
			{
				sql = GetReadStatement(o);
				_statementsCache.TryAdd(objectName, sql);
			}

			var mdb = Factory.GetMdbContext(ContextTypes.Work);
			var drs = await mdb.GetReadersAsync(sql);
			for (int i = 0; i < drs.Length; i++)
			{
				var dr = drs[i];
				dr.Dispose();
			}
			return o;
		}

		private string GetReadStatement(JMXObject o)
		{
			using (var writer = Factory.CreateSQLStatementWriter())
			{
				var schema = o.Schema;
				var sm = new SchemaManager(schema);
				var id = sm.GetIdentityAttribute();
				if (id == null)
					//!!! to resource
					throw new InvalidOperationException("For read operations your mast have one or more identifier columns, aka identity or unique key field");
				writer
					.WriteSelectStatement(schema, new JMXCondition(JMXConditionTypes.Where, ""))
					.WriteEqualsColumnStatement(schema, id, $"@{id.FieldName}", "");
				return writer.ToString();
			}

		}
	}

	class SchemaManager
	{
		private readonly JMXSchema _schema;
		private readonly JMXAttribute _identityAttribute;

		public SchemaManager(JMXSchema schema)
		{
			_schema = schema;
			if (_schema.PrimaryKey.KeyMembers.Count == 1)
			{
				var key = _schema.PrimaryKey.KeyMembers[0].FieldName;
				var att = _schema.Attributes.FirstOrDefault(a => a.FieldName == key);
				if (att == null)
					///!!! To Resources
					throw new InvalidOperationException($"Attribute with  FieldName that equals {key} not found");
				else if (att.DataType == MdbType.@int || att.DataType == MdbType.@long)
					_identityAttribute = att;
			}

			if (_identityAttribute == null)
				_identityAttribute = schema.Attributes.FirstOrDefault(a => a.Identity.IsIdentity);

			if (_identityAttribute == null)
			{
				foreach (var ak in schema.Indexes.Where(idx => idx.IsUnique && idx.KeyMembers.Count == 1))
				{
					var key = ak.KeyMembers[0].FieldName;
					var att = _schema.Attributes.FirstOrDefault(a => a.FieldName == key);
					if (att == null)
						///!!! To Resources
						throw new InvalidOperationException($"Attribute with  FieldName that equals {key} not found");
					else if (att.DataType == MdbType.@int || att.DataType == MdbType.@long)
						_identityAttribute = att;

				}
			}
		}
		public JMXAttribute GetIdentityAttribute()
			=> _identityAttribute;
	}
}