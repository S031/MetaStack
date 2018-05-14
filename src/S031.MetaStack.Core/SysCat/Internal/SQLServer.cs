using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.SysCat
{
	/// <summary>
	/// SQL Servber
	/// </summary>
	internal static class SQLCatSQLServer
	{ 
		public static void register()
		{ 
			Dictionary<string, string> statements = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
			statements["TestSchema"] = @"Select Name From sys.schemas Where name = 'SysCat'";

			statements["CreateSchemaObjects"] = @"
			Create Schema SysCat
			--go
			create table SysCat.SysAreas (
				ID					int				identity(1, 1),
				SchemaName			varchar(60)		not null,
				SchemaOwner			varchar(30)		not null,
				SchemaVersion		varchar(30)		not null,
				SchemaConfig		varchar(max)	null,
					constraint CKC_SCHEMACONFIG_SYSAREAS check(SchemaConfig is null or isjson(SchemaConfig) = 1),
				IsDefault			bit				not null,
				UpdateTime			datetime		not null,
				DateBegin			datetime		not null, 
				DateEnd				datetime		null 
				constraint PK_SYSAREAS primary key (ID)
			)
			create unique nonclustered index AK1_SysAreas on SysCat.SysAreas(SchemaName ASC)
			--go
			create procedure SysCat.Add_SysAreas
				@SchemaName			varchar(60),
				@SchemaOwner		varchar(30),
				@SchemaVersion		varchar(30),
				@SchemaConfig		varchar(max)=null,
				@IsDefault			bit=0,
				@DateBegin			datetime=null,
				@DateEnd			datetime=null 
			as
			begin
				declare @id			int

				if @DateBegin is null
					set @DateBegin = GETDATE()

				if @IsDefault = 0 and not exists (Select 1 From SysCat.SysAreas)
					set @IsDefault = 1

				insert into [SysCat].[SysAreas]
					([SchemaName]
					,[SchemaOwner]
					,[SchemaVersion]
					,[SchemaConfig]
					,[IsDefault]
					,[UpdateTime]
					,[DateBegin]
					,[DateEnd])
				output inserted.ID
				values (@SchemaName, @SchemaOwner, @SchemaVersion, @SchemaConfig, 
					@IsDefault, GETDATE(), @DateBegin, @DateEnd)				
			end
			--go
			/************SysSchemas**************/
			create table SysCat.SysSchemas(
				ID					int					identity(1, 1),
				SysAreaID			int					not null,
				ObjectType			varchar(30)			not null,
				ObjectName			varchar(60)			not null,
				ObjectSchema		varchar(max)		not null
					constraint CKC_OBJECTSCHEMA_SYSSCHEMAS check(isjson(ObjectSchema) = (1)),
				UpdateTime			datetime			not null,
				Version				varchar(30)			not null,
				SyncState			bit					not null,
				DateBegin			datetime			not null, 
				DateEnd				datetime			null 
				constraint PK_SYSSCHEMAS primary key(ID)
			)
			create unique nonclustered index AK1_SysSchemas on SysCat.SysSchemas(SysAreaID ASC, ObjectName ASC)
			alter table SysCat.SysSchemas
				add constraint FK_SYSSCHEMAS_SYSAREAS foreign key(SysAreaID)
					references SysCat.SysAreas(ID)
			--go
			create procedure SysCat.Add_SysSchemas
				@SysAreaID		int = null,
				@ObjectType		varchar(30) = 'table', --may be table, action, function, view,
				@ObjectName		varchar(60),
				@ObjectSchema	varchar(max),
				@Version		varchar(30) = '0.0.1',
				@DateBegin		datetime = null,
				@DateEnd		datetime = null
			as
			begin
				declare @id			int

				if @DateBegin is null
					set @DateBegin = GETDATE()

				if @SysAreaID is null
					select Top 1 @SysAreaID = ID from SysCat.SysAreas where IsDefault = 1

				insert into [SysCat].[SysSchemas]
					([SysAreaID]
					,[ObjectType]
					,[ObjectName]
					,[ObjectSchema]
					,[UpdateTime]
					,[Version]
					,[SyncState]
					,[DateBegin]
					,[DateEnd])
				output inserted.ID
				values
					(@SysAreaID
					,@ObjectType
					,@ObjectName
					,@ObjectSchema
					,GETDATE()
					,@Version
					,0
					,@DateBegin
					,@DateEnd)
			end
			--go
			create view SysCat.V_SysSchemas as
				select S.[ID]
					,S.[SysAreaID]
					,A.[SchemaName]
					,A.[SchemaOwner]
					,A.[SchemaVersion]
					,A.[IsDefault]
					,S.[ObjectType]
					,S.[ObjectName]
					,S.[ObjectSchema]
					,S.[UpdateTime]
					,S.[Version]
					,S.[SyncState]
					,S.[DateBegin]
					,S.[DateEnd]
				from [SysCat].[SysAreas] A
				inner join [SysCat].[SysSchemas] S On A.ID = S.[SysAreaID]
			";

			statements["DropSchema"] = @"
			begin
				drop view if exists SysCat.V_SysSchemas
				drop procedure if exists SysCat.Add_SysSchemas
				drop table if exists [SysCat].[SysSchemas]
				drop procedure if exists SysCat.Add_SysAreas
				drop table if exists [SysCat].[SysAreas]
				drop schema [SysCat]
			end";

			statements["GetCurrentSchema"] = @"Select schema_name()";

			statements["AddSysAreas"] = @"SysCat.Add_SysAreas";

			statements["AddSysSchemas"] = @"SysCat.Add_SysSchemas";

			statements["GetDefaultSchema"] = @"
				if exists (Select Name From sys.schemas Where name = 'SysCat')
					select Top 1 IsNull(S.SchemaName, sys.schemas.Name) as Name From sys.schemas
					left join SysCat.SysAreas S On S.IsDefault = 1
					where sys.schemas.name = schema_name()
				else
					select schema_name() as Name";
			SQLCat.SetCatalog("System.Data.SqlClient", statements);
		}
	}
}
