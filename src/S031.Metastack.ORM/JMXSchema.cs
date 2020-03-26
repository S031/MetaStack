using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;

namespace S031.MetaStack.ORM
{
	public enum DirectAccess
    {
        None = 0,
        ForInsert = 2,
        ForUpdate = 4,
        ForDelete = 8,
        ForAll = ForInsert | ForUpdate | ForDelete
    }
    public enum DbObjectTypes
    {
        Table = 1,
        Procedure = 2,
        Function = 3,
        View = 4,
		Action = 5
    }

	public class JMXSchema: JsonSerializible
    {
        private readonly string _objectName;
        private string _areaName;
        private string _dbObjectName;
		private bool _readOnly;

		public JMXSchema(string objectName) : this(new JMXObjectName(objectName))
        {
        }
        public JMXSchema(JMXObjectName objectName)
            :base(null)
        {
            _objectName = objectName.ObjectName;
            _areaName = objectName.AreaName;
            _dbObjectName = objectName.ObjectName;
        }
		/// <summary>
		/// Assigned from schema repository manager
		/// </summary>
		public IJMXRepo SchemaRepo { get; set; }
        public int ID { get; set; }
		public Guid UID { get; set; } = Guid.Empty;
        public string Name { get; set; }
        public int SyncState { get; set; }
		public bool AuditEnabled { get; set; } = true;
        public bool ReadOnly
		{
			get => DbObjectType == DbObjectTypes.Table ? _readOnly : true;
			set
			{
				if (DbObjectType == DbObjectTypes.Table)
					_readOnly = value;
			}
		}
        public JMXObjectName ObjectName
        {
            get => new JMXObjectName(_areaName, _objectName);
            //set
            //{
            //	_areaName = value.AreaName;
            //	_objectName = value.ObjectName;
            //}
        }
		public JMXObjectName DbObjectName
        {
            get => new JMXObjectName(_areaName, _dbObjectName);
            set
            {
                if (!value.AreaName.IsEmpty())
                    _areaName = value.AreaName;
                _dbObjectName = value.ObjectName;
            }
        }
		/// <summary>
		/// Костыль!!! Связать с ParentID
		/// </summary>
        public JMXObjectName OwnerObject { get; set; }
		public DirectAccess DirectAccess { get; set; } = DirectAccess.ForAll;
		public DbObjectTypes DbObjectType { get; set; } = DbObjectTypes.Table;
		public List<JMXAttribute> Attributes { get; } = new List<JMXAttribute>();
		public List<JMXParameter> Parameters { get; } = new List<JMXParameter>();
		public JMXPrimaryKey PrimaryKey { get; set; }
		public List<JMXIndex> Indexes { get; } = new List<JMXIndex>();
		public List<JMXForeignKey> ForeignKeys { get; } = new List<JMXForeignKey>();
		public List<JMXForeignKey> ParentRelations { get; } = new List<JMXForeignKey>();
		public List<JMXCondition> Conditions { get; } = new List<JMXCondition>();
        public override string ToString()
            => this.ToString(Formatting.None);

        public override string ToString(Formatting formatting)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }

		internal JMXSchema(JsonObject j) : base(j)
		{
			JMXObjectName name;
			JsonObject o;
			if (j.TryGetValue("ObjectName", out JsonValue n))
				if (n.JsonType == JsonType.String)
					name = new JMXObjectName((string)n);
				else
				{
					o = (n as JsonObject);
					name = new JMXObjectName(o.GetStringOrDefault("AreaName"), o.GetStringOrDefault("ObjectName"));
				}
			else
				//!!! from resources
				throw new InvalidOperationException("Sorce json must have ObjectName token");

			_objectName = name.ObjectName;
			_areaName = name.AreaName;
			_dbObjectName = name.ObjectName;

			AuditEnabled = j.GetBoolOrDefault("AuditEnabled");
			DbObjectType = j.GetEnum<DbObjectTypes>("DbObjectType", DbObjectTypes.Table);
			Name = j.GetStringOrDefault("Name");
			ID = j.GetIntOrDefault("ID");
			UID = j.GetGuidOrDefault("UID", () => Guid.Empty);
			DirectAccess = j.GetEnum<DirectAccess>("DirectAccess", DirectAccess.ForAll);
			ReadOnly = j.GetBoolOrDefault("ReadOnly");


			if (j.TryGetValue("DbObjectName", out o))
				DbObjectName = new JMXObjectName(o.GetStringOrDefault("AreaName"), o.GetStringOrDefault("ObjectName"));

			if (j.TryGetValue("OwnerObject", out o))
				OwnerObject = new JMXObjectName(o.GetStringOrDefault("AreaName"), o.GetStringOrDefault("ObjectName"));

			if (j.TryGetValue("PrimaryKey", out o))
				PrimaryKey = JMXPrimaryKey.ReadFrom(o);

			if (j.TryGetValue("ForeignKeys", out JsonArray a))
				foreach (JsonObject obj in a)
					ForeignKeys.Add(JMXForeignKey.ReadFrom(obj));
			
            if (j.TryGetValue("ParentRelations", out a))
				foreach (JsonObject obj in a)
					ParentRelations.Add(JMXForeignKey.ReadFrom(obj));

			if (j.TryGetValue("Indexes", out a))
				foreach (JsonObject obj in a)
					Indexes.Add(JMXIndex.ReadFrom(obj));

			if (j.TryGetValue("Conditions", out a))
				foreach (JsonObject obj in a)
					Conditions.Add(JMXCondition.ReadFrom(obj));

			if (j.TryGetValue("Attributes", out a))
				foreach (JsonObject obj in a)
					Attributes.Add(JMXAttribute.ReadFrom(obj));

			if (j.TryGetValue("Parameters", out a))
				foreach (JsonObject obj in a)
					Parameters.Add(JMXParameter.ReadFrom(obj));


		}

        public static JMXSchema Parse(string schemaJson)
		{
			schemaJson.NullTest(nameof(schemaJson));
            JsonObject j = (JsonObject)new JsonReader(schemaJson).Read();

            return new JMXSchema(j);
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
            writer.WriteProperty("Name", Name);
            writer.WriteProperty("AuditEnabled", AuditEnabled);
            writer.WriteProperty("ReadOnly", ReadOnly);
            writer.WriteProperty("DirectAccess", DirectAccess.ToString());
            writer.WriteProperty("DbObjectType", DbObjectType.ToString());

            //writer.WriteProperty("ObjectName", ObjectName);
            writer.WritePropertyName("ObjectName");
            writer.WriteStartObject();
            writer.WriteProperty("AreaName", ObjectName.AreaName);
            writer.WriteProperty("ObjectName", ObjectName.ObjectName);
            writer.WriteEndObject();

            writer.WritePropertyName("DbObjectName");
            writer.WriteStartObject();
            writer.WriteProperty("AreaName", DbObjectName.AreaName);
            writer.WriteProperty("ObjectName", DbObjectName.ObjectName);
            writer.WriteEndObject();

            if (!OwnerObject.IsEmpty())
            {
                writer.WritePropertyName("OwnerObject");
                writer.WriteStartObject();
                writer.WriteProperty("AreaName", OwnerObject.AreaName);
                writer.WriteProperty("ObjectName", OwnerObject.ObjectName);
                writer.WriteEndObject();
            }

            if (PrimaryKey != null)
            {
                writer.WritePropertyName("PrimaryKey");
                PrimaryKey.ToJson(writer);
            }

            writer.WritePropertyName("Attributes");
            writer.WriteStartArray();
            foreach (var item in Attributes)
                item.ToJson(writer);
            writer.WriteEndArray();

            writer.WritePropertyName("Parameters");
            writer.WriteStartArray();
            foreach (var item in Parameters)
                item.ToJson(writer);
            writer.WriteEndArray();

            writer.WritePropertyName("Indexes");
            writer.WriteStartArray();
            foreach (var item in Indexes)
                item.ToJson(writer);
            writer.WriteEndArray();

            writer.WritePropertyName("ForeignKeys");
            writer.WriteStartArray();
            foreach (var item in ForeignKeys)
                item.ToJson(writer);
            writer.WriteEndArray();
            
            writer.WritePropertyName("ParentRelations");
            writer.WriteStartArray();
            foreach (var item in ParentRelations)
                item.ToJson(writer);
            writer.WriteEndArray();

            writer.WritePropertyName("Conditions");
            writer.WriteStartArray();
            foreach (var item in Conditions)
                item.ToJson(writer);
            writer.WriteEndArray();
        }
    }
}
