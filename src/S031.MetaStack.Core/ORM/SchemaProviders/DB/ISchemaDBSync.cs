using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public interface ISchemaDBSync
	{
		IJMXSchemaProvider SchemaProvider { get; set; }
		string MapServerType(MdbType t);
		Task<bool> TestSysCatAsync(MdbContext mdb);
		Task<string> GetDefaultDbSchemaAsync(MdbContext mdb);
		Task CreateDbSchemaAsync(MdbContext mdb);
		Task DropDbSchemaAsync(MdbContext mdb);
		Task<JMXSchema> GetSchemaAsync(MdbContext mdb, string dbSchema, string objectName);
		Task<JMXSchema> NormalizeSchemaAsync(MdbContext mdb, JMXSchema schema);
		Task SaveSchemaAsync(MdbContext mdb, JMXSchema schema);
		Task<JMXSchema> SyncSchemaAsync(MdbContext mdb, string dbSchema, string objectName);
		Task DropSchemaAsync(MdbContext mdb, string dbSchema, string objectName);
	}
}
