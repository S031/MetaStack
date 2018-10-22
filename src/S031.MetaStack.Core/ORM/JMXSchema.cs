using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if NETCOREAPP
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace S031.MetaStack.Core.ORM
#else

namespace S031.MetaStack.WinForms.ORM
#endif
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

    public class JMXSchema
    {
        private readonly string _objectName;
        private string _areaName;
        private string _dbObjectName;
        private readonly List<JMXAttribute> _attributes;
        private readonly List<JMXIndex> _indexes;
        private readonly List<JMXForeignKey> _fkeys;

        public JMXSchema(string objectName) : this(new JMXObjectName(objectName))
        {
        }
        [JsonConstructor]
        public JMXSchema(JMXObjectName objectName)
        {
            _objectName = objectName.ObjectName;
            _areaName = objectName.AreaName;
            _dbObjectName = objectName.ObjectName;
            _attributes = new List<JMXAttribute>();
            _indexes = new List<JMXIndex>();
            _fkeys = new List<JMXForeignKey>();
            UID = Guid.NewGuid();
            DirectAccess = DirectAccess.ForAll;
            DbObjectType = DbObjectTypes.Table;
        }
        /// <summary>
        /// Assigned from schema repository manager
        /// </summary>
        public IJMXRepo SchemaRepo { get; set; }
        public int ID { get; set; }
        public Guid UID { get; set; }
        public string Name { get; set; }
        public int SyncState { get; set; }
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
        [JsonConverter(typeof(StringEnumConverter))]
        public DirectAccess DirectAccess { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DbObjectTypes DbObjectType { get; set; }
        public List<JMXAttribute> Attributes { get => _attributes; }
        public JMXPrimaryKey PrimaryKey { get; set; }
        public List<JMXIndex> Indexes { get => _indexes; }
        public List<JMXForeignKey> ForeignKeys { get => _fkeys; }
        protected void ToStringRaw(JsonWriter writer)
        {
            writer.WriteProperty("ID", ID);
            writer.WriteProperty("UID", UID);
            writer.WriteProperty("Name", Name);
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

            if (PrimaryKey != null)
            {
                writer.WritePropertyName("PrimaryKey");
                writer.WriteRawValue(PrimaryKey.ToString());
            }

            writer.WritePropertyName("Attributes");
            writer.WriteStartArray();
            foreach (var item in _attributes)
                writer.WriteRawValue(item.ToString());
            writer.WriteEnd();

            writer.WritePropertyName("Indexes");
            writer.WriteStartArray();
            foreach (var item in _indexes)
                writer.WriteRawValue(item.ToString());
            writer.WriteEnd();

            writer.WritePropertyName("ForeignKeys");
            writer.WriteStartArray();
            foreach (var item in _fkeys)
                writer.WriteRawValue(item.ToString());
            writer.WriteEnd();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1024);
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                writer.WriteStartObject();
                ToStringRaw(writer);
                writer.WriteEndObject();
                return sb.ToString();
            }
        }
        public static JMXSchema Parse(string schemaJson)
        {
            schemaJson.NullTest(nameof(schemaJson));
            return JsonConvert.DeserializeObject<JMXSchema>(schemaJson);
        }
#if NETCOREAPP
        public static JMXSchema ParseXml(string schemaXML)
        {
            schemaXML.NullTest(nameof(schemaXML));
            var xDoc = XDocument.Parse(schemaXML);

            var root = xDoc.Element("ObjectSchema");
            JMXSchema schema = new JMXSchema(root.Attribute("ObjectName").Value);

            var elem = root.selectElement("DbObjectName");
            if (elem == null)
                //Missing required element 'DbObjectName'
                throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchema.ParseXml.1", "DbObjectName"));

            schema.DbObjectName = new JMXObjectName(elem.elementValue("AreaName"),
                elem.elementValue("ObjectName"));

            elem = root.selectElement("PrimaryKey");
            if (elem != null)
            {
                schema.PrimaryKey = new JMXPrimaryKey(elem.Attribute("KeyName").Value);
                schema.PrimaryKey.AddKeyMember(elem
                    .selectElement("KeyMembers")
                    .Elements()
                    .Select(e => new JMXKeyMember()
                    {
                        FieldName = e.elementValue("FieldName"),
                        IsDescending = e.elementValue("IsDescending").ToBoolOrDefault()
                    }).ToArray());

            }
            elem = root.selectElement("Attributes");
            if (elem == null)
                throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchema.ParseXml.1", "Attributes"));
            schema.Attributes.AddRange(elem.Elements()
                .Select(e => new JMXAttribute(e.elementValue("AttribName"))
                {
                    ID = e.elementValue("ID").ToIntOrDefault(),
                    FieldName = e.elementValue("FieldName"),
                    Position = e.elementValue("Position").ToIntOrDefault(),
                    ServerDataType = e.elementValue("ServerDataType"),
                    DataType = Data.MdbTypeMap.GetTypeInfo(Data.MdbTypeMap.GetType(e.elementValue("DataType"))).MdbType,
                    IsNullable = e.elementValue("IsNullable").ToBoolOrDefault(),
                    DataSize = new JMXDataSize(
                        e.selectElement("DataSize").elementValue("Size").ToIntOrDefault(),
                        e.selectElement("DataSize").elementValue("Scale").ToIntOrDefault(),
                        e.selectElement("DataSize").elementValue("Precision").ToIntOrDefault())
                }));

            elem = root.selectElement("Indexes");
            if (elem != null)
                foreach (var e in elem.Elements())
                {
					JMXIndex idx = new JMXIndex
					{
						ID = e.elementValue("ID").ToIntOrDefault(),
						IndexName = e.elementValue("IndexName"),
						IsUnique = e.elementValue("IsUnique").ToBoolOrDefault()
					};
					idx.KeyMembers.AddRange(e.Element("KeyMembers").Elements()
                        .Select(e1 => new JMXKeyMember()
                        {
                            FieldName = e1.elementValue("FieldName"),
                            IsDescending = e1.elementValue("IsDescending").ToBoolOrDefault(),
                            Position = e1.elementValue("Position").ToIntOrDefault(),
                            IsIncluded = e1.elementValue("IsIncluded").ToBoolOrDefault(),
                        }));
                    schema.Indexes.Add(idx);
                }

            elem = root.selectElement("ForeignKeys");
            if (elem != null)
                foreach (var e in elem.Elements())
                {
					JMXForeignKey key = new JMXForeignKey(e.elementValue("KeyName"))
					{
						ID = e.elementValue("ID").ToIntOrDefault(),
						RefDbObjectName = new JMXObjectName(e.Element("RefDbObjectName").elementValue("AreaName"),
						e.Element("RefDbObjectName").elementValue("ObjectName")),
						CheckOption = e.elementValue("CheckOption").ToBoolOrDefault(),
						DeleteRefAction = e.elementValue("DeleteRefAction"),
						UpdateRefAction = e.elementValue("UpdateRefAction")
					};
					key.KeyMembers.AddRange(e.Element("KeyMembers").Elements()
                        .Select(e1 => new JMXKeyMember()
                        {
                            FieldName = e1.elementValue("FieldName"),
                            Position = e1.elementValue("Position").ToIntOrDefault(),
                        }));
                    key.RefKeyMembers.AddRange(e.Element("RefKeyMembers").Elements()
                        .Select(e1 => new JMXKeyMember()
                        {
                            FieldName = e1.elementValue("FieldName"),
                            Position = e1.elementValue("Position").ToIntOrDefault(),
                        }));
                    schema.ForeignKeys.Add(key);
                }
            return schema;
        }
#endif
    }
}
