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
	public class JMXAttribute: JsonSerializible
	{
		private JMXConstraint _checkConstraint;
		private JMXConstraint _defaultConstraint;

		public JMXAttribute()
            :base(null)
		{
            _checkConstraint = new JMXConstraint(JMXConstraintTypes.checkConstraint);
            _defaultConstraint = new JMXConstraint(JMXConstraintTypes.defaultConstraint);
		}

		public JMXAttribute(string attrtibName) : this()
		{
			AttribName = attrtibName;
		}

		#region DBServerSpecificAttributes
		public int ID { get; set; }
        public Guid UID { get; set; } = Guid.Empty;
		public string AttribName { get; set; }
		public int Position { get; set; }
		public string ServerDataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; } = true;
		public string NullOption { get => (IsNullable ? "NULL" : "NOT NULL"); }
		public bool Required { get => !IsNullable; set => IsNullable = !value; }
		public string CollationName { get; set; } = string.Empty;
        public bool IsPK { get; set; }
		public bool IsFK { get; set; }
		public JMXDataSize DataSize { get; set; } = new JMXDataSize();
        public JMXIdentity Identity { get; set; } = new JMXIdentity();
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
		public string Name { get; set; } = string.Empty;
        public string Caption { get => Name; set => Name = value; }

		public MdbType DataType { get; set; } = MdbType.@null;
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
        public bool Visible { get; set; } = true;
		public bool ReadOnly { get; set; }
        public bool Enabled { get; set; } = true;
		#endregion AppServerSpecificAttributes

		public string AttribPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PresentationType { get; set; }
        public int DisplayWidth { get; set; } = 10;
		public string Mask { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool Sorted { get; set; } = true;
		public string SuperForm { get; set; } = string.Empty;
        public string SuperObject { get; set; } = string.Empty;
        public string SuperMethod { get; set; } = string.Empty;
        public string SuperFilter { get; set; } = string.Empty;
        public List<string> ListItems { get; } = new List<string>();
		public List<object> ListData { get; } = new List<object>();
		public string FieldName { get; set; } = string.Empty;
        public string ConstName { get; set; } = string.Empty;
        public string Agregate { get; set; } = string.Empty;
        public object DefaultValue { get; set; }


		/// <summary>
		/// For DataType == "Object"
		/// </summary>
		public string ObjectName { get; set; }
		public bool IsArray { get; set; }
		public JMXSchema ObjectSchema { get; set; }
        public override string ToString()
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }
        protected internal JMXAttribute(JsonObject o)
            :base(o)
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

        public override void ToJson(JsonWriter writer)
        {
            writer.WriteStartObject();
            ToJsonRaw(writer);
            writer.WriteEndObject();
        }

        protected override void ToJsonRaw(JsonWriter writer)
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
                Identity.ToJson(writer);
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

		internal static JMXAttribute ReadFrom(JsonObject o)
			=> new JMXAttribute(o);
    }
}
