using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public struct JMXKeyMember
	{
        public string FieldName { get; set; }
        public int Position { get; set; }
        public bool IsDescending { get; set; }
        public bool IsIncluded { get; set; }
		public static bool operator ==(JMXKeyMember m1, JMXKeyMember m2)
		{
			return (m1.FieldName == m2.FieldName &&
				m1.Position == m2.Position &&
				m1.IsDescending == m2.IsDescending &&
				m1.IsIncluded == m2.IsIncluded);
		}
		public static bool operator !=(JMXKeyMember m1, JMXKeyMember m2)
		{
			return (m1.FieldName != m2.FieldName ||
				m1.Position != m2.Position ||
				m1.IsDescending != m2.IsDescending ||
				m1.IsIncluded != m2.IsIncluded);
		}
		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}
		public override int GetHashCode()
		{
			return new { FieldName, Position, IsDescending, IsIncluded }.GetHashCode();
		}
		public bool IsEmpty() => string.IsNullOrEmpty(FieldName);

		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}

		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("FieldName", FieldName);
			writer.WriteProperty("Position", Position);
			writer.WriteProperty("IsDescending", IsDescending);
			writer.WriteProperty("IsIncluded", IsIncluded);
			writer.WriteEndObject();
		}
	}
}
