using System;
using System.IO;
using System.Text;
using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public enum JMXConditionTypes
	{
		none,
		Where,
		OrderBy,
		GroupBy,
		Havind,
		Join
	}
	public class JMXCondition
	{
		public JMXCondition()
		{
			ConditionType = JMXConditionTypes.none;
			Definition = string.Empty;
		}

		public JMXCondition(JMXConditionTypes conditionType, string definition)
		{
			ConditionType = conditionType;
			Definition = definition;
		}

		public string Definition { get; set; }

		public JMXConditionTypes ConditionType { get; set; }


		public static bool operator ==(JMXCondition m1, JMXCondition m2)
		{
			return (m1.Definition == m2.Definition &&
				m1.ConditionType == m2.ConditionType);
		}

		public static bool operator !=(JMXCondition m1, JMXCondition m2)
		{
			return (m1.Definition != m2.Definition ||
				m1.ConditionType != m2.ConditionType);
		}

		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return new { Definition, ConditionType }.GetHashCode();
		}

		public bool IsEmpty() => string.IsNullOrEmpty(Definition);

		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			ToStringRaw(writer);
			return writer.ToString();
		}

		public void ToStringRaw(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("ConditionType", ConditionType.ToString());
			writer.WriteProperty("Definition", Definition);
			writer.WriteEndObject();
		}

		protected internal JMXCondition(JsonObject o)
		{
			ConditionType = o.GetEnum<JMXConditionTypes>("ConditionType");
			Definition = o["Definition"];
		}

		internal static JMXCondition ReadFrom(JsonObject o) 
			=> new JMXCondition(o);
	}
}
