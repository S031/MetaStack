﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace S031.MetaStack.Core.ORM {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlServer {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlServer() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("S031.MetaStack.Core.ORM.JMXFactory.DB.SqlServer.SqlServer", typeof(SqlServer).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.Add_SysAreas.
        /// </summary>
        internal static string AddSysAreas {
            get {
                return ResourceManager.GetString("AddSysAreas", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.Add_SysSchemas.
        /// </summary>
        internal static string AddSysSchemas {
            get {
                return ResourceManager.GetString("AddSysSchemas", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create Schema SysCat
        ///--go
        ///create table SysCat.SysAreas (
        ///	ID					int				identity(1, 1),
        ///	SchemaName			varchar(60)		not null,
        ///	SchemaOwner			varchar(30)		not null,
        ///	SchemaVersion		varchar(30)		not null,
        ///	SchemaConfig		varchar(max)	null
        ///		/*constraint CKC_SCHEMACONFIG_SYSAREAS check(SchemaConfig is null or isjson(SchemaConfig) = 1)*/,
        ///	IsDefault			bit				not null,
        ///	UpdateTime			datetime		not null
        ///		 CONSTRAINT [DF_UpdateTime_SysAreas]  DEFAULT (getdate()),
        ///	DateBegin			datetime		not null, 
        ///	DateE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateSchemaObjects {
            get {
                return ResourceManager.GetString("CreateSchemaObjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create Schema SysCat
        ///--go
        ///create table SysCat.SysAreas (
        ///	ID					int				identity(1, 1),
        ///	SchemaName			varchar(60)		not null,
        ///	SchemaOwner			varchar(30)		not null,
        ///	SchemaVersion		varchar(30)		not null,
        ///	SchemaConfig		varchar(max)	null,
        ///	IsDefault			bit				not null,
        ///	UpdateTime			datetime		not null
        ///		 CONSTRAINT [DF_UpdateTime_SysAreas]  DEFAULT (getdate()),
        ///	DateBegin			datetime		not null, 
        ///	DateEnd				datetime		null 
        ///	constraint PK_SYSAREAS primary key (ID)
        ///)
        ///create unique nonclustered index AK1_SysAreas o [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateSchemaObjects_12 {
            get {
                return ResourceManager.GetString("CreateSchemaObjects_12", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.Del_SysSchemas.
        /// </summary>
        internal static string DelSysSchemas {
            get {
                return ResourceManager.GetString("DelSysSchemas", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to begin
        ///	drop procedure if exists SysCat.Get_TableSchema
        ///	drop procedure if exists SysCat.Get_TableSchema_xml
        ///	drop procedure if exists SysCat.Get_TableSchema_ansi
        ///	drop procedure if exists SysCat.Get_ParentRelations
        ///	drop procedure if exists SysCat.Get_ParentRelations_ansi
        ///	drop view if exists SysCat.V_SysSchemas
        ///	drop procedure if exists SysCat.Add_SysSchemas
        ///	drop procedure if exists SysCat.Del_SysSchemas
        ///	drop procedure if exists SysCat.State_SysSchemas
        ///	drop table if exists [SysCat].[SysSchemas [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DropSchema {
            get {
                return ResourceManager.GetString("DropSchema", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT c.column_id AS ID,
        ///	c.name AS FieldName
        ///FROM sys.objects o WITH (NOWAIT)
        ///JOIN sys.schemas s WITH (NOWAIT) ON o.[schema_id] = s.[schema_id] AND s.name = @SchemaName AND o.name = @TableName
        ///	AND o.[type] = &apos;U&apos; AND o.is_ms_shipped = 0
        ///JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = o.object_id
        ///ORDER BY c.column_id.
        /// </summary>
        internal static string GetColumnsList {
            get {
                return ResourceManager.GetString("GetColumnsList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select schema_name().
        /// </summary>
        internal static string GetCurrentSchema {
            get {
                return ResourceManager.GetString("GetCurrentSchema", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to if exists (Select Name From sys.schemas Where name = &apos;SysCat&apos;)
        ///	select Top 1 IsNull(S.SchemaName, sys.schemas.Name) as Name From sys.schemas
        ///	left join SysCat.SysAreas S On S.IsDefault = 1
        ///	where sys.schemas.name = schema_name()
        ///else
        ///	select schema_name() as Name.
        /// </summary>
        internal static string GetDefaultSchema {
            get {
                return ResourceManager.GetString("GetDefaultSchema", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.Get_ParentRelations.
        /// </summary>
        internal static string GetParentRelations {
            get {
                return ResourceManager.GetString("GetParentRelations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.Get_TableSchema.
        /// </summary>
        internal static string GetTableSchema {
            get {
                return ResourceManager.GetString("GetTableSchema", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT SERVERPROPERTY(&apos;ProductVersion&apos;) AS ProductVersion.
        /// </summary>
        internal static string SQLVersion {
            get {
                return ResourceManager.GetString("SQLVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SysCat.State_SysSchemas.
        /// </summary>
        internal static string StateSysSchemas {
            get {
                return ResourceManager.GetString("StateSysSchemas", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select Name From sys.schemas Where name = &apos;SysCat&apos;.
        /// </summary>
        internal static string TestSchema {
            get {
                return ResourceManager.GetString("TestSchema", resourceCulture);
            }
        }
    }
}
