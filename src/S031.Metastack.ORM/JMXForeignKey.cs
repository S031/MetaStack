using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace S031.MetaStack.ORM
{
    public sealed class JMXForeignKey : JsonSerializible
    {
        private string _objectName;
        private string _areaName;
        private string _dbObjectName;

        public JMXForeignKey(string keyName)
            : base(null)
        {
            KeyName = keyName;
        }

        public int ID { get; set; }
        public string KeyName { get; set; }
        public Guid UID { get; set; } = Guid.Empty;
        public bool CheckOption { get; set; } = true;
        public string DeleteRefAction { get; set; } = string.Empty;
        public string UpdateRefAction { get; set; } = string.Empty;
        public JMXObjectName RefObjectName
        {
            get => new JMXObjectName(_areaName, _objectName);
            set
            {
                _areaName = value.AreaName;
                _objectName = value.ObjectName;
            }
        }
        public JMXObjectName RefDbObjectName
        {
            get => new JMXObjectName(_areaName, _dbObjectName);
            set
            {
                if (!value.AreaName.IsEmpty())
                    _areaName = value.AreaName;
                _dbObjectName = value.ObjectName;
            }
        }
        public List<JMXKeyMember> KeyMembers { get; private set; } = new List<JMXKeyMember>();
        public List<JMXKeyMember> RefKeyMembers { get; private set; } = new List<JMXKeyMember>();
        public List<JMXKeyMember> MigrateKeyMembers { get; private set; } = new List<JMXKeyMember>();
        public void AddKeyMember(params JMXKeyMember[] members)
        {
            foreach (var m in members)
                KeyMembers.Add(m);
        }
        public void AddKeyMember(params string[] attribNames)
        {
            int position = KeyMembers.Count + 1;
            foreach (var n in attribNames)
            {
                KeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
                position++;
            }
        }
        public void AddRefKeyMember(params JMXKeyMember[] members)
        {
            foreach (var m in members)
                RefKeyMembers.Add(m);
        }
        public void AddRefKeyMember(params string[] attribNames)
        {
            int position = RefKeyMembers.Count + 1;
            foreach (var n in attribNames)
            {
                RefKeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
                position++;
            }
        }
        public void AddMigrateKeyMember(params JMXKeyMember[] members)
        {
            foreach (var m in members)
                MigrateKeyMembers.Add(m);
        }
        public void AddMigrateKeyMember(params string[] attribNames)
        {
            int position = MigrateKeyMembers.Count + 1;
            foreach (var n in attribNames)
            {
                MigrateKeyMembers.Add(new JMXKeyMember() { FieldName = n, Position = position });
                position++;
            }
        }

        protected override void ToJsonRaw(JsonWriter writer)
        {
            writer.WriteProperty("ID", ID);
            writer.WriteProperty("UID", UID);
            writer.WriteProperty("KeyName", KeyName);
            writer.WriteProperty("CheckOption", CheckOption);
            writer.WriteProperty("DeleteRefAction", DeleteRefAction);
            writer.WriteProperty("UpdateRefAction", UpdateRefAction);

            writer.WritePropertyName("RefObjectName");
            writer.WriteStartObject();
            writer.WriteProperty("AreaName", _areaName);
            writer.WriteProperty("ObjectName", _objectName);
            writer.WriteEndObject();

            writer.WritePropertyName("RefDbObjectName");
            writer.WriteStartObject();
            writer.WriteProperty("AreaName", _areaName);
            writer.WriteProperty("ObjectName", _dbObjectName);
            writer.WriteEndObject();

            writer.WritePropertyName("KeyMembers");
            writer.WriteStartArray();
            foreach (var item in KeyMembers)
            {
                writer.WriteStartObject();
                writer.WriteProperty("FieldName", item.FieldName);
                writer.WriteProperty("Position", item.Position);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("RefKeyMembers");
            writer.WriteStartArray();
            foreach (var item in RefKeyMembers)
            {
                writer.WriteStartObject();
                writer.WriteProperty("FieldName", item.FieldName);
                writer.WriteProperty("Position", item.Position);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("MigrateKeyMembers");
            writer.WriteStartArray();
            foreach (var item in MigrateKeyMembers)
            {
                writer.WriteStartObject();
                writer.WriteProperty("FieldName", item.FieldName);
                writer.WriteProperty("Position", item.Position);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        internal JMXForeignKey(JsonObject o) : base(o)
        {
        }
        internal static JMXForeignKey ReadFrom(JsonObject o)
            => new JMXForeignKey(o);
		public override void FromJson(JsonValue source)
		{
            var o = source as JsonObject;
            KeyName = o.GetStringOrDefault("KeyName");
            CheckOption = o.GetBoolOrDefault("CheckOption", true);
            DeleteRefAction = o.GetStringOrDefault("DeleteRefAction");
            UpdateRefAction = o.GetStringOrDefault("DeleteRefAction");
            ID = o.GetIntOrDefault("ID");
            UID = o.GetGuidOrDefault("UID", () => Guid.NewGuid());

            if (o.TryGetValue("RefObjectName", out JsonObject j))
                RefObjectName = new JMXObjectName(j.GetStringOrDefault("AreaName"), j.GetStringOrDefault("ObjectName"));

            if (o.TryGetValue("RefDbObjectName", out j))
                RefDbObjectName = new JMXObjectName(j.GetStringOrDefault("AreaName"), j.GetStringOrDefault("ObjectName"));

            KeyMembers = new List<JMXKeyMember>();
            if (o.TryGetValue("KeyMembers", out JsonArray a))
                foreach (JsonObject m in a)
                    KeyMembers.Add(JMXKeyMember.ReadFrom(m));

            RefKeyMembers = new List<JMXKeyMember>();
            if (o.TryGetValue("RefKeyMembers", out a))
                foreach (JsonObject m in a)
                    RefKeyMembers.Add(JMXKeyMember.ReadFrom(m));

            MigrateKeyMembers = new List<JMXKeyMember>();
            if (o.TryGetValue("MigrateKeyMembers", out a))
                foreach (JsonObject m in a)
                    MigrateKeyMembers.Add(JMXKeyMember.ReadFrom(m));
        }
    }
}

