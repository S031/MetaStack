using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public enum JMXConstraintTypes
	{
		none,
		checkConstraint,
		defaultConstraint
	}
	public class JMXConstraint
	{
		public JMXConstraint()
		{
			ConstraintType = JMXConstraintTypes.none;
			ConstraintName = string.Empty;
			Definition = string.Empty;
			CheckOption = true;
		}

		public JMXConstraint(JMXConstraintTypes constraintType, string definition) : this(constraintType, "", definition)
		{
		}

		public JMXConstraint(JMXConstraintTypes constraintType, string constraintName, string definition)
		{
			ConstraintType = constraintType;
			ConstraintName = constraintName;
			Definition = definition;
			CheckOption = true;
		}

		public string ConstraintName { get; set; }

		public string Definition { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public JMXConstraintTypes ConstraintType { get; set; }

		public bool CheckOption { get; set; }

		public static bool operator ==(JMXConstraint m1, JMXConstraint m2)
		{
			return (m1.ConstraintName == m2.ConstraintName &&
				m1.Definition == m2.Definition &&
				m1.ConstraintType == m2.ConstraintType);
		}

		public static bool operator !=(JMXConstraint m1, JMXConstraint m2)
		{
			return (m1.ConstraintName != m2.ConstraintName ||
				m1.Definition != m2.Definition ||
				m1.ConstraintType != m2.ConstraintType);
		}

		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return new { ConstraintName, Definition, ConstraintType }.GetHashCode();
		}

		public bool IsEmpty() => string.IsNullOrEmpty(Definition);
	}
}
