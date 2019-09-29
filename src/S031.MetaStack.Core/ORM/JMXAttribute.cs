using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using S031.MetaStack.Common;
using System.Linq;
#if NETCOREAPP
using S031.MetaStack.Core.Data;
namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Data;
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public class JMXAttribute
	{
		private JMXConstraint _checkConstraint;
		private JMXConstraint _defaultConstraint;

		public JMXAttribute()
		{
			Identity = new JMXIdentity();
			ListItems = new List<string>();
			ListData = new List<object>();
			DataType = MdbType.@null;
			DataSize = new JMXDataSize();
			UID = Guid.NewGuid();
			ServerDataType = string.Empty;
			IsNullable = true;
            _checkConstraint = new JMXConstraint(JMXConstraintTypes.checkConstraint);
            _defaultConstraint = new JMXConstraint(JMXConstraintTypes.defaultConstraint);

			DisplayWidth = 10;
			Visible = true;
			Enabled = true;
			Sorted = true;

			CollationName = string.Empty;
			Name = string.Empty;
			AttribPath = string.Empty;
			Description = string.Empty;
			Mask = string.Empty;
			Format = string.Empty;
			SuperForm = string.Empty;
			SuperObject = string.Empty;
			SuperMethod = string.Empty;
			SuperFilter = string.Empty;
			FieldName = string.Empty;
			ConstName = string.Empty;
			Agregate = string.Empty;
		}
		public JMXAttribute(string attrtibName) : this()
		{
			AttribName = attrtibName;
		}

		#region DBServerSpecificAttributes
		public int ID { get; set; }
		public Guid UID { get; set; }
		public string AttribName { get; set; }
		public int Position { get; set; }
		public string ServerDataType { get; set; }
		public bool IsNullable { get; set; }
		public string NullOption { get => (IsNullable ? "NULL" : "NOT NULL"); }
		public bool Required { get => !IsNullable; set => IsNullable = !value; }
		public string CollationName { get; set; }
		public bool IsPK { get; set; }
		public bool IsFK { get; set; }
		public JMXDataSize DataSize { get; set; }
		public JMXIdentity Identity { get; set; }
		public JMXConstraint CheckConstraint
		{
			get => _checkConstraint;
			set
			{
				_checkConstraint = value;
				_checkConstraint.ConstraintType = JMXConstraintTypes.checkConstraint;
			}
		}
		public JMXConstraint DefaultConstraint
		{
			get => _defaultConstraint;
			set
			{
				_defaultConstraint = value;
				_defaultConstraint.ConstraintType = JMXConstraintTypes.defaultConstraint;
			}
		}
		#endregion DBServerSpecificAttributes

		#region AppServerSpecificAttributes
		public string Name { get; set; }
		public string Caption { get => Name; set => Name = value; }

		public MdbType DataType { get; set; }
		public int Width
		{
			get => DataType.GetTypeInfo().FixedSize ? DataSize.Precision : DataSize.Size;
			set
			{
				if (DataType.GetTypeInfo().FixedSize)
					DataSize = new JMXDataSize(DataSize.Size, DataSize.Scale, value);
				else
					DataSize = new JMXDataSize(value, DataSize.Scale, DataSize.Precision);
			}
		}
		public bool Locate { get => (Visible || !ReadOnly); }
		public bool Visible { get; set; }
		public bool ReadOnly { get; set; }
		public bool Enabled { get; set; }
		#endregion AppServerSpecificAttributes

		public string AttribPath { get; set; }
		public string Description { get; set; }
		public string PresentationType { get; set; }
		public int DisplayWidth { get; set; }
		public string Mask { get; set; }
		public string Format { get; set; }
		public bool Sorted { get; set; }
		public string SuperForm { get; set; }
		public string SuperObject { get; set; }
		public string SuperMethod { get; set; }
		public string SuperFilter { get; set; }
		public List<string> ListItems { get; } = new List<string>();
		public List<object> ListData { get; } = new List<object>();
		public string FieldName { get; set; }
		public string ConstName { get; set; }
		public string Agregate { get; set; }
		public object DefaultValue { get; set; }


		/// <summary>
		/// For DataType == "Object"
		/// </summary>
		public string ObjectName { get; set; }
		public bool IsArray { get; set; }
		public JMXSchema ObjectSchema { get; set; }
		public virtual void ToStringRaw(JsonWriter writer)
		{
			writer.WriteProperty("ID", ID);
			writer.WriteProperty("UID", UID);
			writer.WriteProperty("AttribName", AttribName);
			writer.WriteProperty("Position", Position);
			writer.WriteProperty("ServerDataType", ServerDataType);
			writer.WriteProperty("IsNullable", IsNullable);
			writer.WriteProperty("NullOption", NullOption);
			writer.WriteProperty("Required", Required);
			writer.WriteProperty("CollationName", CollationName);
			writer.WriteProperty("IsPK", IsPK);
			writer.WriteProperty("IsFK", IsFK);

			if (!CheckConstraint.IsEmpty())
			{
				writer.WritePropertyName("CheckConstraint");
				CheckConstraint.ToJson(writer);
			}

			if (!DefaultConstraint.IsEmpty())
			{
				writer.WritePropertyName("DefaultConstraint");
				DefaultConstraint.ToJson(writer);
			}

			writer.WritePropertyName("DataSize");
			DataSize.ToJson(writer);

			if (Identity.IsIdentity)
			{
				writer.WritePropertyName("Identity");
				writer.WriteStartObject();
				Identity.ToStringRaw(writer);
				writer.WriteEndObject();
			}

			writer.WriteProperty("Name", Name);
			writer.WriteProperty("Caption", Caption);
			writer.WriteProperty("DataType", DataType.ToString());
			writer.WriteProperty("Width", Width);
			writer.WriteProperty("Locate", Locate);
			writer.WriteProperty("Visible", Visible);
			writer.WriteProperty("ReadOnly", ReadOnly);
			writer.WriteProperty("Enabled", Enabled);
			writer.WriteProperty("Description", Description);
			writer.WriteProperty("PresentationType", PresentationType);
			writer.WriteProperty("DisplayWidth", DisplayWidth);
			writer.WriteProperty("Mask", Mask);
			writer.WriteProperty("Format", Format);
			writer.WriteProperty("Sorted", Sorted);
			writer.WriteProperty("SuperForm", SuperForm);
			writer.WriteProperty("SuperObject", SuperObject);
			writer.WriteProperty("SuperMethod", SuperMethod);
			writer.WriteProperty("SuperFilter", SuperFilter);

			if (ListItems.Count > 0)
			{
				writer.WritePropertyName("ListItems");
				writer.WriteStartArray();
				foreach (var item in ListItems)
					writer.WriteValue(item);
				writer.WriteEndArray();
			}

			if (ListData.Count > 0)
			{
				writer.WritePropertyName("ListData");
				writer.WriteStartArray();
				foreach (var item in ListData)
					writer.WriteValue(new JsonValue(item));
				writer.WriteEndArray();
			}

			writer.WriteProperty("FieldName", FieldName);
			writer.WriteProperty("ConstName", ConstName);
			writer.WriteProperty("Agregate", Agregate);
			writer.WriteProperty("AttribPath", AttribPath);
			writer.WriteProperty("DefaultValue", DefaultValue);

			writer.WriteProperty("ObjectName", ObjectName);
			writer.WriteProperty("IsArray", IsArray);
			if (ObjectName != string.Empty && ObjectSchema != null)
			{

				writer.WritePropertyName("ObjectSchema");
				writer.WriteRaw(ObjectSchema.ToString());
			}
		}
		public override string ToString()
		{
			JsonWriter writer = new JsonWriter(Formatting.None);
			writer.WriteStartObject();
			ToStringRaw(writer);
			writer.WriteEndObject();
			return writer.ToString();
		}
		protected internal JMXAttribute(JsonObject o)
		{
			ID = o.GetIntOrDefault("ID");
			UID = o.GetGuidOrDefault("UID", () => Guid.NewGuid());
			AttribName = o.GetStringOrDefault("AttribName");
			Position = o.GetIntOrDefault("Position");
			ServerDataType = o.GetStringOrDefault("ServerDataType");
			IsNullable = o.GetBoolOrDefault("IsNullable", true);
			Required = o.GetBoolOrDefault("Required");
			CollationName = o.GetStringOrDefault("CollationName");
			IsPK = o.GetBoolOrDefault("IsPK");
			IsFK = o.GetBoolOrDefault("IsFK");
			Name = o.GetStringOrDefault("Name");
			if (o.TryGetValue("Caption", out JsonValue caption))
				Caption = o.GetStringOrDefault("Caption");
			DataType = o.GetEnum<MdbType>("DataType");
			Width = o.GetIntOrDefault("Width");
			Visible = o.GetBoolOrDefault("Visible", true);
			ReadOnly = o.GetBoolOrDefault("ReadOnly");
			Enabled = o.GetBoolOrDefault("Enabled", true);
			Description = o.GetStringOrDefault("Description");
			PresentationType = o.GetStringOrDefault("PresentationType");
			DisplayWidth = o.GetIntOrDefault("DisplayWidth", 10);
			Mask = o.GetStringOrDefault("Mask");
			Format = o.GetStringOrDefault("Format");
			Sorted = o.GetBoolOrDefault("Sorted", true);
			SuperForm = o.GetStringOrDefault("SuperForm");
			SuperObject = o.GetStringOrDefault("SuperObject");
			SuperMethod = o.GetStringOrDefault("SuperMethod");
			SuperFilter = o.GetStringOrDefault("SuperFilter");
			FieldName = o.GetStringOrDefault("FieldName");
			ConstName = o.GetStringOrDefault("ConstName");
			Agregate = o.GetStringOrDefault("Agregate");
			AttribPath = o.GetStringOrDefault("AttribPath");

			if (o.TryGetValue("DefaultValue", out JsonValue value)
				&& !string.IsNullOrEmpty(value))
				DefaultValue = ((string)value).ToObjectOf(MdbTypeMap.GetType(DataType));

			if (o.TryGetValue("ObjectName", out value)
				&& !string.IsNullOrEmpty(value))
			{
				ObjectName = value;
				if (o.TryGetValue("ObjectSchema", out JsonValue schema)
					&& !string.IsNullOrEmpty(schema))
					ObjectSchema = JMXSchema.Parse(schema);
				IsArray = o.GetBoolOrDefault("IsArray");
			}

			if (o.TryGetValue("ListItems", out JsonArray a))
				ListItems.AddRange(a.Select(v => (string)v));

			if (o.TryGetValue("ListData", out a))
				ListData.AddRange(a.Select(v=>v.GetValue()));


            if (o.TryGetValue("CheckConstraint", out JsonObject j))
                CheckConstraint = JMXConstraint.ReadFrom(j);
            else
                CheckConstraint = new JMXConstraint(JMXConstraintTypes.checkConstraint);

            if (o.TryGetValue("DefaultConstraint", out j))
                DefaultConstraint = JMXConstraint.ReadFrom(j);
            else
                DefaultConstraint = new JMXConstraint(JMXConstraintTypes.defaultConstraint);

			if (o.TryGetValue("DataSize", out j))
				DataSize = JMXDataSize.ReadFrom(j);
			else
				DataSize = new JMXDataSize();

			if (o.TryGetValue("Identity", out j))
				Identity = JMXIdentity.ReadFrom(j);
			else
				Identity = new JMXIdentity();
		}
		internal static JMXAttribute ReadFrom(JsonObject o)
			=> new JMXAttribute(o);
	}
}
