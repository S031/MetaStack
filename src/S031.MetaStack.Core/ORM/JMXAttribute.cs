using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
		private string _attribName;
		private JMXSchema _schema;
		private JMXDataSize _dataSize;
		private JMXIdentity _identity;
		private readonly List<string> _listItems;
		private readonly List<object> _listData;
		private JMXConstraint _checkConstraint;
		private JMXConstraint _defaultConstraint;
		private MdbType _dataType;
		public JMXAttribute()
		{
			_identity = new JMXIdentity();
			_listItems = new List<string>();
			_listData = new List<object>();
			_dataType = MdbType.@null;
			_dataSize = new JMXDataSize();
			UID = Guid.NewGuid();
			ServerDataType = string.Empty;
			IsNullable = true;
			_checkConstraint = new JMXConstraint() { ConstraintType = JMXConstraintTypes.checkConstraint };
			_defaultConstraint = new JMXConstraint() { ConstraintType = JMXConstraintTypes.defaultConstraint };

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
			_attribName = attrtibName;
		}

		#region DBServerSpecificAttributes
		public int ID { get; set; }
		public Guid UID { get; set; }
		public string AttribName { get => _attribName; set => _attribName = value; }
		public int Position { get; set; }
		public string ServerDataType { get; set; }
		public bool IsNullable { get; set; }
		public string NullOption { get => (IsNullable ? "NULL" : "NOT NULL"); }
		public bool Required { get => !IsNullable; set => IsNullable = !value; }
		public string CollationName { get; set; }
		public bool IsPK { get; set; }
		public bool IsFK { get; set; }
		public JMXDataSize DataSize { get => _dataSize; set => _dataSize = value; }
		public JMXIdentity Identity { get => _identity; set => _identity = value; }
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

		public MdbType DataType
		{
			get => _dataType;//.ToString();
			set => _dataType = value;// MdbTypeMap.GetTypeInfo(MdbTypeMap.GetType(value)).MdbType;
		}
		public int Width
		{
			get => _dataType.GetTypeInfo().FixedSize ? _dataSize.Precision : _dataSize.Size;
			set
			{
				if (_dataType.GetTypeInfo().FixedSize)
					_dataSize.Precision = value;
				else
					_dataSize.Size = value;
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
		public List<string> ListItems { get => _listItems; }
		public List<object> ListData { get => _listData; }
		public string FieldName { get; set; }
		public string ConstName { get; set; }
		public string Agregate { get; set; }
		public object DefaultValue { get; set; }


		/// <summary>
		/// For DataType == "Object"
		/// </summary>
		public string ObjectName { get; set; }
		public bool IsArray { get; set; }
		public JMXSchema ObjectSchema { get => _schema; set => _schema = value; }
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
				CheckConstraint.ToStringRaw(writer);
			}

			if (!DefaultConstraint.IsEmpty())
			{
				writer.WritePropertyName("DefaultConstraint");
				DefaultConstraint.ToStringRaw(writer);
			}

			writer.WritePropertyName("DataSize");
			DataSize.ToStringRaw(writer);

			writer.WritePropertyName("Identity");
			Identity.ToStringRaw(writer);

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

			writer.WritePropertyName("ListItems");
			writer.WriteStartArray();
			foreach (var item in _listItems)
				writer.WriteValue(item);
			writer.WriteEndArray();
			writer.WritePropertyName("ListData");
			writer.WriteStartArray();
			foreach (var item in _listData)
				writer.WriteValue(new JsonValue(item));
			writer.WriteEndArray();

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
			ToStringRaw(writer);
			return writer.ToString();
		}
	}
}
