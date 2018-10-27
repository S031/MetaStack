using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;

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

		[JsonConverter(typeof(StringEnumConverter))]
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
			StringBuilder sb = new StringBuilder(1024);
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				writer.WriteProperty("ConditionType", ConditionType.ToString());
				writer.WriteProperty("Definition", Definition);
				writer.WriteEndObject();
				return sb.ToString();
			}
		}
	}
}
