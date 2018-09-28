Create Schema SysCat
--go
create table SysCat.SysAreas (
	ID					int				identity(1, 1),
	SchemaName			varchar(60)		not null,
	SchemaOwner			varchar(30)		not null,
	SchemaVersion		varchar(30)		not null,
	SchemaConfig		varchar(max)	null
		/*constraint CKC_SCHEMACONFIG_SYSAREAS check(SchemaConfig is null or isjson(SchemaConfig) = 1)*/,
	IsDefault			bit				not null,
	UpdateTime			datetime		not null
		 CONSTRAINT [DF_UpdateTime_SysAreas]  DEFAULT (getdate()),
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
	UID					as cast(json_value(ObjectSchema, '$.UID') as uniqueidentifier) persisted not null,
	SysAreaID			int					not null,
	ObjectType			as cast(json_value(ObjectSchema, '$.DbObjectType') as varchar(30)) persisted not null,
	ObjectName			as cast(json_value(ObjectSchema, '$.ObjectName.ObjectName') as varchar(60)) persisted not null,
	ObjectSchema		varchar(max)		not null
		/*constraint CKC_OBJECTSCHEMA_SYSSCHEMAS check(isjson(ObjectSchema) = (1))*/,
	DbObjectName		as cast(json_value(ObjectSchema, '$.DbObjectName.ObjectName') as varchar(60)) persisted not null,
	UpdateTime			datetime			not null
		 CONSTRAINT [DF_UpdateTime_SysSchemas]  DEFAULT (getdate()),
	Version				varchar(30)			not null,
	SyncState			int					not null,
	DateBegin			datetime			not null, 
	DateEnd				datetime			null,
	PreviosID			int					null
	constraint PK_SYSSCHEMAS primary key(ID)
)
create nonclustered index IE1_SysSchemas on SysCat.SysSchemas(SysAreaID ASC, ObjectName ASC)
create nonclustered index IE2_SysSchemas on SysCat.SysSchemas(SysAreaID ASC, DbObjectName ASC)
create nonclustered index IE3_SysSchemas on SysCat.SysSchemas(PreviosID ASC)
create nonclustered index IE4_SysSchemas on SysCat.SysSchemas(UID ASC)
alter table SysCat.SysSchemas
	add constraint FK_SYSSCHEMAS_SYSAREAS foreign key(SysAreaID)
		references SysCat.SysAreas(ID)
alter table SysCat.SysSchemas
	add constraint FK_SYSSCHEMAS_SYSSCHEMAS foreign key(PreviosID)
		references SysCat.SysSchemas(ID)
--go
create procedure [SysCat].[Add_SysSchemas]
	@ObjectSchema	varchar(max),
	@Version		varchar(30) = '0.0.1',
	@DateBegin		datetime = null,
	@DateEnd		datetime = null
as
begin
	declare @id						int
	declare @SysAreaID				int
	declare @syncState				int
	declare @uid					uniqueidentifier=cast(json_value(@ObjectSchema, '$.UID') as uniqueidentifier)
	declare @SysAreaSchemaName		varchar(30) = json_value(@ObjectSchema, '$.ObjectName.AreaName')

	if @DateBegin is null
		set @DateBegin = GETDATE()

	if @SysAreaSchemaName is null
		select Top 1 @SysAreaID = ID from SysCat.SysAreas where IsDefault = 1
	else
		select Top 1 @SysAreaID = ID from SysCat.SysAreas where SchemaName = @SysAreaSchemaName

	select Top 1 @id = ID, @syncState = SyncState from [SysCat].[SysSchemas] 
	where [UID] = @uid
	order by ID desc
	
	if (@id is null or @syncState <> 0)
	begin
		insert into [SysCat].[SysSchemas]
			([SysAreaID]
			,[ObjectSchema]
			,[Version]
			,[SyncState]
			,[DateBegin]
			,[DateEnd]
			,[PreviosID])
		output inserted.ID
		values
			(@SysAreaID
			,@ObjectSchema
			,@Version
			,0
			,@DateBegin
			,@DateEnd
			,@id)
	end
	else
		update [SysCat].SysSchemas set
			[ObjectSchema] = @ObjectSchema,
			[Version] = @Version,
			[UpdateTime] = GETDATE()
		output @id
		where [id] = @id
end
--go
create procedure [SysCat].[Del_SysSchemas]
	@uid	uniqueidentifier
as
begin
	declare @id		int
	declare @prevId	int

	select Top 1 
		@id = ID,
		@prevId = PreviosID
	from [SysCat].[SysSchemas]
	where [UID] = @uid
	order by ID desc

	--begin transaction
	while not @id is null
	begin
		delete from [SysCat].[SysSchemas] where ID = @id
		if not @prevId is null
			select @id = ID, @prevId = PreviosID from [SysCat].[SysSchemas] where ID = @prevId
		else
			set @id = null
	end	
	--commit
end
--go
create procedure SysCat.State_SysSchemas
	@id			int
as
begin
	declare @prevID		int=null
	declare @syncState	int=1

	select @prevID = PreviosID from [SysCat].[SysSchemas] where ID = @id
	begin transaction
		update [SysCat].[SysSchemas]
			set SyncState = @syncState
		where ID = @id
		if not @prevId is null
			update [SysCat].[SysSchemas]
				set SyncState = -1
			where ID = @prevID
	commit
end
--go
create view SysCat.V_SysSchemas as
	select S.[ID]
		,S.[UID]
		,S.[SysAreaID]
		,A.[SchemaName]
		,A.[SchemaOwner]
		,A.[SchemaVersion]
		,A.[IsDefault]
		,S.[ObjectType]
		,S.[ObjectName]
		,S.[ObjectSchema]
		,S.[DbObjectName]
		,S.[UpdateTime]
		,S.[Version]
		,S.[SyncState]
		,S.[DateBegin]
		,S.[DateEnd]
	from [SysCat].[SysAreas] A
	inner join [SysCat].[SysSchemas] S On A.ID = S.[SysAreaID]
--go
CREATE PROCEDURE  [SysCat].[Get_TableSchema]
	@table_name sysname
AS
BEGIN
DECLARE 
	@object_name	sysname,
	@object_id		int,
	@schema_name	sysname,
	@result			nvarchar(max)

SELECT @object_name = '[' + s.name + '].[' + o.name + ']'
	,@object_id = o.[object_id]
	,@table_name = o.name
	,@schema_name = s.name
FROM sys.objects o WITH (NOWAIT)
JOIN sys.schemas s WITH (NOWAIT) ON o.[schema_id] = s.[schema_id]
WHERE s.name + '.' + o.name = @table_name
	AND o.[type] = 'U'
	AND o.is_ms_shipped = 0

IF NOT @object_id IS NULL
BEGIN
	SET @result = (SELECT 
		CASE WHEN Exists (select v.object_id
			FROM sys.views v
			INNER JOIN sys.schemas s on v.schema_id = s.schema_id
			WHERE v.name = 'v_SysSchemas' and s.name = 'SysCat')
		THEN IsNull((select Top 1 ObjectName from SysCat.V_SysSchemas 
			WHERE SchemaName = @schema_name and DbObjectName = @table_name ORDER BY ID DESC), left(@table_name, len(@table_name)-1))
		ELSE
			left(@table_name, len(@table_name)-1)
		END As ObjectName,
		@schema_name AS [DBObjectName.AreaName],
		@table_name AS [DBObjectName.ObjectName],
		(SELECT c.column_id AS ID,
			c.name AS AttribName,
			c.name AS FieldName,
			c.column_id AS Position,
			CASE WHEN c.is_computed = 1 THEN cc.[definition] ELSE tp.name END AS [ServerDataType],
			CASE tp.name 
				WHEN 'bit' THEN 'bool'
				WHEN 'tinyint' THEN 'byte'
				WHEN 'smallint' THEN 'short'
				WHEN 'int' THEN 'int'
				WHEN 'bigint' THEN 'long'
				WHEN 'float' THEN 'float'
				WHEN 'real' THEN 'float'
				WHEN 'decimal' THEN 'decimal'
				WHEN 'money' THEN 'decimal'
				WHEN 'numeric' THEN 'decimal'
				WHEN 'smallmoney' THEN 'decimal'
				WHEN 'date' THEN 'dateTime'
				WHEN 'datetime' THEN 'dateTime'
				WHEN 'datetime2' THEN 'dateTime'
				WHEN 'smalldatetime' THEN 'dateTime'
				WHEN 'time' THEN 'dateTime'
				WHEN 'varchar' THEN 'string'
				WHEN 'nvarchar' THEN 'string'
				WHEN 'text' THEN 'string'
				WHEN 'ntext' THEN 'string'
				WHEN 'char' THEN 'string'
				WHEN 'nchar' THEN 'string'
				WHEN 'xml' THEN ''
				WHEN 'binary' THEN 'byteArray'
				WHEN 'varbinary' THEN 'byteArray'
				WHEN 'image' THEN 'byteArray'
				WHEN 'rowversion' THEN 'byteArray'
				WHEN 'timestamp' THEN 'byteArray'
				WHEN 'sql_variant' THEN 'byteArray'
				WHEN 'uniqueidentifier' THEN 'guid'
				ELSE 'string'END AS [DataType],
			CASE WHEN c.is_computed = 1 THEN 0 ELSE c.max_length END AS [DataSize.Size],
			c.scale AS [DataSize.Scale],
			c.[precision] AS [DataSize.Precision],
			c.collation_name AS [CollationName],
			c.is_nullable AS IsNullable,
			CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END AS NullOption,
			IsNull(dc.name, '') AS [DefaultConstraint.ConstraintName],
			IsNull(dc.[definition], '') AS [DefaultConstraint.Definition],
			IsNull(chc.name, '') AS [CheckConstraint.ConstraintName],
			IsNull(chc.[definition], '') AS [CheckConstraint.Definition],
			ic.is_identity AS [Identity.IsIdentity],
			ic.seed_value AS [Identity.Seed],
			ic.increment_value AS [Identity.Increment]
		FROM sys.columns c WITH (NOWAIT)
		JOIN sys.types tp WITH (NOWAIT) ON c.user_type_id = tp.user_type_id
		LEFT JOIN sys.computed_columns cc WITH (NOWAIT) ON c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
		LEFT JOIN sys.default_constraints dc WITH (NOWAIT) ON c.default_object_id != 0 AND c.[object_id] = dc.parent_object_id AND c.column_id = dc.parent_column_id
		LEFT JOIN sys.check_constraints chc WITH (NOWAIT) ON c.[object_id] = chc.parent_object_id AND c.column_id = chc.parent_column_id
		LEFT JOIN sys.identity_columns ic WITH (NOWAIT) ON c.is_identity = 1 AND c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
		WHERE c.[object_id] = @object_id
		ORDER BY c.column_id
		FOR JSON PATH) AS Attributes,

		k.name AS [PrimaryKey.KeyName],
		(SELECT c.name AS [FieldName]
,
			ic.is_descending_key AS [IsDescending]
		FROM sys.index_columns ic WITH (NOWAIT) 
		JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
		WHERE ic.[object_id] = k.parent_object_id AND ic.index_id = k.unique_index_id AND 
		k.[type] = 'PK' AND ic.is_included_column = 0
		FOR JSON PATH) AS [PrimaryKey.KeyMembers],


		(SELECT 
			fk.name AS [KeyName],
			CAST((CASE WHEN fk.is_not_trusted = 1 
				THEN 0 
				ELSE 1
				END) AS bit) AS CheckOption,
			CASE 
				WHEN fk.delete_referential_action = 1 THEN ' ON DELETE CASCADE' 
				WHEN fk.delete_referential_action = 2 THEN ' ON DELETE SET NULL'
				WHEN fk.delete_referential_action = 3 THEN ' ON DELETE SET DEFAULT' 
				ELSE '' 
				END AS DeleteRefAction,
			CASE 
				WHEN fk.update_referential_action = 1 THEN ' ON UPDATE CASCADE'
				WHEN fk.update_referential_action = 2 THEN ' ON UPDATE SET NULL'
				WHEN fk.update_referential_action = 3 THEN ' ON UPDATE SET DEFAULT'  
				ELSE '' 
				END AS UpdateRefAction,
			SCHEMA_NAME(ro.[schema_id]) AS [RefDbObjectName.AreaName],
			ro.name AS [RefDbObjectName.ObjectName],
			(SELECT 
				c.name AS [FieldName],
				k.constraint_column_id As Position
			FROM sys.foreign_key_columns k WITH (NOWAIT)
			JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.parent_object_id AND c.column_id = k.parent_column_id
			WHERE k.constraint_object_id = fk.object_id and k.parent_object_id = @object_id FOR JSON PATH) AS [KeyMembers],
			(SELECT 
				rc.name AS [FieldName],
				k.constraint_column_id As Position
			FROM sys.foreign_key_columns k WITH (NOWAIT)
			JOIN sys.columns rc WITH (NOWAIT) ON rc.[object_id] = k.referenced_object_id AND rc.column_id = k.referenced_column_id 
			WHERE k.constraint_object_id = fk.object_id and k.parent_object_id = @object_id FOR JSON PATH) AS [RefKeyMembers]

		FROM sys.foreign_keys fk WITH (NOWAIT)
		JOIN sys.objects ro WITH (NOWAIT) ON ro.[object_id] = fk.referenced_object_id
		WHERE fk.parent_object_id = @object_id FOR JSON PATH) AS [ForeignKeys],
		(SELECT 
			i.name AS [IndexName],
			i.is_unique AS IsUnique,
			0 AS [ClusteredOption],
			(SELECT 
				c.name AS [FieldName],
				ic.index_column_id As Position,
				ic.is_descending_key AS [IsDescending],
				ic.is_included_column AS [IsIncluded]
			FROM sys.index_columns ic WITH (NOWAIT)
			JOIN sys.columns c WITH (NOWAIT) ON ic.[object_id] = c.[object_id] AND ic.column_id = c.column_id
			WHERE ic.[object_id] = i.[object_id] and ic.[index_id] = i.[index_id] FOR JSON PATH) AS [KeyMembers]
		FROM sys.indexes i WITH (NOWAIT)
		WHERE i.[object_id] = @object_id
			AND i.is_primary_key = 0
			AND i.[type] = 2  FOR JSON PATH) AS [Indexes]
	FROM sys.objects o WITH (NOWAIT)
	LEFT JOIN sys.key_constraints k WITH (NOWAIT) on k.parent_object_id = @object_id 
		AND k.[type] = 'PK'
	WHERE o.object_id = @object_id
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
	SELECT @result
END
ELSE
	SELECT ''
END
--go
CREATE PROCEDURE SysCat.Get_ParentRelations
	@table_name		sysname
AS
BEGIN
	DECLARE 
		@object_name	sysname,
		@object_id		int,
		@schema_name	sysname,
		@json			nvarchar(max)

	SELECT @object_name = '[' + s.name + '].[' + o.name + ']'
		,@object_id = o.[object_id]
		,@table_name = o.name
		,@schema_name = s.name
	FROM sys.objects o WITH (NOWAIT)
	JOIN sys.schemas s WITH (NOWAIT) ON o.[schema_id] = s.[schema_id]
	WHERE s.name + '.' + o.name = @table_name
		AND o.[type] = 'U'
		AND o.is_ms_shipped = 0

	IF NOT @object_id IS NULL
		BEGIN
			SET @json = IsNull((SELECT 
				fk.name AS [KeyName],
				CAST((CASE WHEN fk.is_not_trusted = 1 
					THEN 0 
					ELSE 1
					END) AS bit) AS CheckOption,
				CASE 
					WHEN fk.delete_referential_action = 1 THEN ' ON DELETE CASCADE' 
					WHEN fk.delete_referential_action = 2 THEN ' ON DELETE SET NULL'
					WHEN fk.delete_referential_action = 3 THEN ' ON DELETE SET DEFAULT' 
					ELSE '' 
					END AS DeleteRefAction,
				CASE 
					WHEN fk.update_referential_action = 1 THEN ' ON UPDATE CASCADE'
					WHEN fk.update_referential_action = 2 THEN ' ON UPDATE SET NULL'
					WHEN fk.update_referential_action = 3 THEN ' ON UPDATE SET DEFAULT'  
					ELSE '' 
					END AS UpdateRefAction,
				SCHEMA_NAME(po.[schema_id]) AS [ParentObject.AreaName],
				po.name AS [ParentObject.ObjectName],
				SCHEMA_NAME(ro.[schema_id]) AS [RefDbObjectName.AreaName],
				ro.name AS [RefDbObjectName.ObjectName],
				(SELECT 
					c.name AS [FieldName],
					k.constraint_column_id As Position
				FROM sys.foreign_key_columns k WITH (NOWAIT)
				JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.parent_object_id AND c.column_id = k.parent_column_id
				WHERE k.constraint_object_id = fk.object_id and k.parent_object_id = po.object_id FOR JSON PATH) AS [KeyMembers],
				(SELECT 
					rc.name AS [FieldName],
					k.constraint_column_id As Position
				FROM sys.foreign_key_columns k WITH (NOWAIT)
				JOIN sys.columns rc WITH (NOWAIT) ON rc.[object_id] = k.referenced_object_id AND rc.column_id = k.referenced_column_id 
				WHERE k.constraint_object_id = fk.object_id and k.parent_object_id = po.object_id FOR JSON PATH) AS [RefKeyMembers]

			FROM sys.foreign_keys fk WITH (NOWAIT)
			JOIN sys.objects ro WITH (NOWAIT) ON ro.[object_id] = fk.referenced_object_id
			JOIN sys.objects po WITH (NOWAIT) ON po.[object_id] = fk.parent_object_id
			WHERE fk.referenced_object_id = @object_id FOR JSON PATH), '')

			SELECT @json
		END
	ELSE
		SELECT ''
END

