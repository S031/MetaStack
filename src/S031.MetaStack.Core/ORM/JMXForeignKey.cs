using Newtonsoft.Json;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if NETCOREAPP2_0
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
		List<JMXKeyMember> _keyMembers;
		List<JMXKeyMember> _refKeyMembers;

		public JMXForeignKey(string keyName)
		{
			UID = Guid.NewGuid();
			KeyName = keyName;
			CheckOption = true;
			DeleteRefAction = string.Empty;
			UpdateRefAction = string.Empty;
			_keyMembers = new List<JMXKeyMember>();
			_refKeyMembers = new List<JMXKeyMember>();
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
		public List<JMXKeyMember> KeyMembers { get => _keyMembers; }
		public List<JMXKeyMember> RefKeyMembers { get => _refKeyMembers; }
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
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;
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
				writer.WriteEnd();

				writer.WritePropertyName("RefKeyMembers");
				writer.WriteStartArray();
				foreach (var item in _refKeyMembers)
				{
					writer.WriteStartObject();
					writer.WriteProperty("FieldName", item.FieldName);
					writer.WriteProperty("Position", item.Position);
					writer.WriteEndObject();
				}
				writer.WriteEnd();
				writer.WriteEndObject();
				return sb.ToString();
			}
		}
	}
}
