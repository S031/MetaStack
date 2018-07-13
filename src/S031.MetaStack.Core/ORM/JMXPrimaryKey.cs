using Newtonsoft.Json;
using S031.MetaStack.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXPrimaryKey
	{
		private List<JMXKeyMember> _keyMembers;
		public JMXPrimaryKey()
		{
			_keyMembers = new List<JMXKeyMember>();
		}
		public JMXPrimaryKey(string keyName, params string[] keyMemberNames):this()
		{
			KeyName = keyName;
			AddKeyMember(keyMemberNames);
		}
		public int ID { get; set; }
		public int Handle { get => ID; set => ID = value; }
		public string KeyName { get; set; }
		public List<JMXKeyMember> KeyMembers { get => _keyMembers; }
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
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				writer.WriteProperty("ID", ID);
				writer.WriteProperty("Handle", Handle);
				writer.WriteProperty("KeyName", KeyName);

				writer.WritePropertyName("KeyMembers");
				writer.WriteStartArray();
				foreach (var item in _keyMembers)
				{
					writer.WriteStartObject();
					writer.WriteProperty("FieldName", item.FieldName);
					writer.WriteProperty("Position", item.Position);
					writer.WriteProperty("IsDescending", item.IsDescending);
					writer.WriteEndObject();
				}
				writer.WriteEnd();
				writer.WriteEndObject();
				return sb.ToString();
			}
		}
	}
}
