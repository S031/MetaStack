using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXForeignKey
	{
		private string _objectName;
		private string _areaName;
		private string _dbObjectName;
		readonly List<JMXKeyMember> _keyMembers;
		readonly List<JMXKeyMember> _refKeyMembers;
		readonly List<JMXKeyMember> _migrateKeyMembers;

		public JMXForeignKey(string keyName)
		{
			UID = Guid.NewGuid();
			KeyName = keyName;
			CheckOption = true;
			DeleteRefAction = string.Empty;
			UpdateRefAction = string.Empty;
			_keyMembers = new List<JMXKeyMember>();
			_refKeyMembers = new List<JMXKeyMember>();
			_migrateKeyMembers = new List<JMXKeyMember>();
		}

		public int ID { get; set; }
		public Guid UID { get; set; }
		public string KeyName { get; set; }
		public bool CheckOption { get; set; }
		public string DeleteRefAction { get; set; }
		public string UpdateRefAction { get; set; }
		public JMXObjectName RefObjectName
		{
			get => new JMXObjectName(_areaName, _objectName);
			set
			{
				_areaName = value.AreaName;
				_objectName = value.ObjectName;
			}
		}
		public JMXObjectName RefDbObjectName
		{
			get => new JMXObjectName(_areaName, _dbObjectName);
			set
			{
				if (!value.AreaName.IsEmpty())
					_areaName = value.AreaName;
				_dbObjectName = value.ObjectName;
			}
		}
		public List<JMXKeyMember> KeyMembers => _keyMembers;
		public List<JMXKeyMember> RefKeyMembers => _refKeyMembers;
		public List<JMXKeyMember> MigrateKeyMembers => _migrateKeyMembers;
		public void AddKeyMember(params JMXKeyMember[] members)
		{
			foreach (var m in members)
				_keyMembers.Add(m);
		}
		public void AddKeyMember(params string[] attribNames)
		{
			int position = _keyMembers.Count + 1;
			foreach (var n in attribNames)
			{
				_keyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
				position++;
			}
		}
		public void AddRefKeyMember(params JMXKeyMember[] members)
		{
			foreach (var m in members)
				_refKeyMembers.Add(m);
		}
		public void AddRefKeyMember(params string[] attribNames)
		{
			int position = _refKeyMembers.Count + 1;
			foreach (var n in attribNames)
			{
				_refKeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
				position++;
			}
		}
		public void AddMigrateKeyMember(params JMXKeyMember[] members)
		{
			foreach (var m in members)
				_migrateKeyMembers.Add(m);
		}
		public void AddMigrateKeyMember(params string[] attribNames)
		{
			int position = _migrateKeyMembers.Count + 1;
			foreach (var n in attribNames)
			{
				_migrateKeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
				position++;
			}
		}
		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("ID", ID);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("KeyName", KeyName);
			writer.WriteProperty("CheckOption", CheckOption);
			writer.WriteProperty("DeleteRefAction", DeleteRefAction);
			writer.WriteProperty("UpdateRefAction", UpdateRefAction);

			writer.WritePropertyName("RefObjectName");
			writer.WriteStartObject();
			writer.WriteProperty("AreaName", _areaName);
			writer.WriteProperty("ObjectName", _objectName);
			writer.WriteEndObject();

			writer.WritePropertyName("RefDbObjectName");
			writer.WriteStartObject();
			writer.WriteProperty("AreaName", _areaName);
			writer.WriteProperty("ObjectName", _dbObjectName);
			writer.WriteEndObject();

			writer.WritePropertyName("KeyMembers");
			writer.WriteStartArray();
			foreach (var item in _keyMembers)
			{
				writer.WriteStartObject();
				writer.WriteProperty("FieldName", item.FieldName);
				writer.WriteProperty("Position", item.Position);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();

			writer.WritePropertyName("RefKeyMembers");
			writer.WriteStartArray();
			foreach (var item in _refKeyMembers)
			{
				writer.WriteStartObject();
				writer.WriteProperty("FieldName", item.FieldName);
				writer.WriteProperty("Position", item.Position);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();

			writer.WritePropertyName("MigrateKeyMembers");
			writer.WriteStartArray();
			foreach (var item in _migrateKeyMembers)
			{
				writer.WriteStartObject();
				writer.WriteProperty("FieldName", item.FieldName);
				writer.WriteProperty("Position", item.Position);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		internal static JMXForeignKey ReadFrom(JsonObject o)
		{
			var fk = new JMXForeignKey(o["KeyName"])
			{
				CheckOption = o.GetBoolOrDefault("CheckOption"),
				DeleteRefAction = o.GetStringOrDefault("DeleteRefAction"),
				UpdateRefAction = o.GetStringOrDefault("DeleteRefAction"),
				ID = o.GetIntOrDefault("ID"),
				UID = o.GetGuidOrDefault("UID")
			};

			if (o.TryGetValue("RefObjectName", out JsonObject j))
				fk.RefObjectName = new JMXObjectName(j.GetStringOrDefault("AreaName"), j.GetStringOrDefault("ObjectName"));

			if (o.TryGetValue("RefDbObjectName", out j))
				fk.RefDbObjectName = new JMXObjectName(j.GetStringOrDefault("AreaName"), j.GetStringOrDefault("ObjectName"));

			if (o.TryGetValue("KeyMembers", out JsonArray a))
			{
				foreach (JsonObject m in a)
				{
					fk.KeyMembers.Add(new JMXKeyMember()
					{
						FieldName = m["FieldName"],
						Position = m.GetIntOrDefault("Position"),
						IsIncluded = m.GetBoolOrDefault("IsIncluded"),
						IsDescending = m.GetBoolOrDefault("IsDescending")
					});
				}
			}

			if (o.TryGetValue("RefKeyMembers", out a))
			{
				foreach (JsonObject m in a)
				{
					fk.RefKeyMembers.Add(new JMXKeyMember()
					{
						FieldName = m["FieldName"],
						Position = m.GetIntOrDefault("Position"),
						IsIncluded = m.GetBoolOrDefault("IsIncluded"),
						IsDescending = m.GetBoolOrDefault("IsDescending")
					});
				}
			}

			if (o.TryGetValue("MigrateKeyMembers", out a))
			{
				foreach (JsonObject m in a)
				{
					fk.MigrateKeyMembers.Add(new JMXKeyMember()
					{
						FieldName = m["FieldName"],
						Position = m.GetIntOrDefault("Position"),
						IsIncluded = m.GetBoolOrDefault("IsIncluded"),
						IsDescending = m.GetBoolOrDefault("IsDescending")
					});
				}
			}
			return fk;
		}
	}
}

