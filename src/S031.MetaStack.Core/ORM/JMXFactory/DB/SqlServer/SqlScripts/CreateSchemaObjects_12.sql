Create Schema SysCat
--go
create table SysCat.SysAreas (
	ID					int				identity(1, 1),
	SchemaName			varchar(60)		not null,
	SchemaOwner			varchar(30)		not null,
	SchemaVersion		varchar(30)		not null,
	SchemaConfig		varchar(max)	null,
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
	UID					uniqueidentifier	not null,
	SysAreaID			int					not null,
	ObjectType			varchar(30)			not null,
	ObjectName			varchar(60)			not null,
	ObjectSchema		varchar(max)		not null,
	DbObjectName		varchar(60)			not null,
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
	@uid					uniqueidentifier,
	@SysAreaSchemaName		varchar(30),
	@ObjectType				varchar(30),
	@ObjectName				varchar(60),
	@DbObjectName			varchar(60),
	@ObjectSchema			varchar(max),
	@Version				varchar(30) = '0.0.1',
	@DateBegin				datetime = null,
	@DateEnd				datetime = null
as
begin
	declare @id						int
	declare @SysAreaID				int
	declare @syncState				int

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
			,[UID]
			,[ObjectType]
			,[ObjectName]
			,[DbObjectName]
			,[ObjectSchema]
			,[Version]
			,[SyncState]
			,[DateBegin]
			,[DateEnd]
			,[PreviosID])
		output inserted.ID
		values
			(@SysAreaID
			,@uid
			,@ObjectType
			,@ObjectName
			,@DbObjectName
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
CREATE PROCEDURE [SysCat].[Get_ParentRelations]
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
			(SELECT char(10) +'[' + 
				STUFF((SELECT char(10) + '{ "KeyName": "' + [ForeignKey].name + 
				'", "CheckOption": ' + CASE WHEN [ForeignKey].is_not_trusted = 1 THEN '0' ELSE '1'	END + 
				', "DeleteRefAction": ' + CASE 
						WHEN [ForeignKey].delete_referential_action = 1 THEN '" ON DELETE CASCADE"' 
						WHEN [ForeignKey].delete_referential_action = 2 THEN '" ON DELETE SET NULL"'
						WHEN [ForeignKey].delete_referential_action = 3 THEN '" ON DELETE SET DEFAULT"' 
						ELSE '""' END +
				', "UpdateRefAction": ' + CASE 
						WHEN [ForeignKey].update_referential_action = 1 THEN '" ON UPDATE CASCADE"' 
						WHEN [ForeignKey].update_referential_action = 2 THEN '" ON UPDATE SET NULL"'
						WHEN [ForeignKey].update_referential_action = 3 THEN '" ON UPDATE SET DEFAULT"' 
						ELSE '""' END +
					STUFF((SELECT char(10) + ', "ParentObject": { "AreaName": "' + 
						SCHEMA_NAME([ParentObject].[schema_id]) + 
						'", "ObjectName": "' + [ParentObject].name + '" }'
						FROM sys.objects [ParentObject] WITH (NOWAIT) WHERE [ParentObject].[object_id] = [ForeignKey].parent_object_id
						FOR XML PATH('')), 1, 1, '') +
					STUFF((SELECT char(10) + ', "RefDbObjectName": { "AreaName": "' + 
						SCHEMA_NAME([RefDbObjectName].[schema_id]) + 
						'", "ObjectName": "' + [RefDbObjectName].name + '" }'
						FROM sys.objects [RefDbObjectName] WITH (NOWAIT) WHERE [RefDbObjectName].[object_id] = [ForeignKey].referenced_object_id
						FOR XML PATH('')), 1, 1, '') +
						
					', "KeyMembers": { ' +
					STUFF((SELECT ', "FieldName": "' + 
						c.name + '", "Position": ' + convert(varchar(5), k.constraint_column_id)
						FROM sys.foreign_key_columns k WITH (NOWAIT)
						JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.parent_object_id AND c.column_id = k.parent_column_id
						WHERE k.constraint_object_id = [ForeignKey].object_id and k.parent_object_id = po.object_id 
						FOR XML PATH('')), 1, 1, '')
					+ ' }' +
					', "RefKeyMembers": { ' +
					STUFF((SELECT ', "FieldName": "' + 
						c.name + '", "Position": ' + convert(varchar(5), k.constraint_column_id)
						FROM sys.foreign_key_columns k WITH (NOWAIT)
						JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.referenced_object_id AND c.column_id = k.referenced_column_id 
						WHERE k.constraint_object_id = [ForeignKey].object_id and k.parent_object_id = po.object_id
						FOR XML PATH('')), 1, 1, '')
					+ ' }' 
					+ ' }' 
					
					FROM sys.foreign_keys [ForeignKey] WITH (NOWAIT)
					JOIN sys.objects ro WITH (NOWAIT) ON ro.[object_id] = [ForeignKey].referenced_object_id
					JOIN sys.objects po WITH (NOWAIT) ON po.[object_id] = [ForeignKey].parent_object_id
					WHERE [ForeignKey].referenced_object_id = @object_id 
					FOR XML PATH('')), 1, 1, ''))
			+ ']'

		FROM sys.objects o WITH (NOWAIT)
		WHERE o.object_id = @object_id), '')
		SELECT @json
	END
	ELSE
		SELECT ''
END
--go
CREATE PROCEDURE  [SysCat].[Get_TableSchema]
	@table_name sysname
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
			'{ "ObjectName": "' + 
				--CASE WHEN Exists (select v.object_id
				--	FROM sys.views v
				--	INNER JOIN sys.schemas s on v.schema_id = s.schema_id
				--	WHERE v.name = 'v_SysSchemas' and s.name = 'SysCat')
				--THEN IsNull((select Top 1 ObjectName from SysCat.V_SysSchemas 
				--	WHERE SchemaName = @schema_name and DbObjectName = @table_name ORDER BY ID DESC), left(@table_name, len(@table_name)-1))
				--ELSE
					left(@table_name, len(@table_name)-1)
				--END 
				+ '", "DBObjectName": { "AreaName": "' + @schema_name + '", "ObjectName": "' + @table_name + '" }, ' +
			'"Attributes": [' + 
			STUFF((SELECT ', {' +
				'"ID": ' + cast([Attribute].column_id AS varchar(5)) + ', ' +
				'"AttribName": "' + [Attribute].name + '", ' +
				'"FieldName": "' + [Attribute].name + '", ' +
				'"Position": ' + cast([Attribute].column_id AS varchar(5)) + ', ' +
				'"ServerDataType": "' + CASE WHEN [Attribute].is_computed = 1 THEN cc.[definition] ELSE tp.name END + '", ' +
				'"DataType": "' + CASE tp.name 
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
					ELSE 'string'END + '", ' +
				
				'"DataSize": { "Size": ' + cast(CASE WHEN [Attribute].is_computed = 1 THEN 0 ELSE [Attribute].max_length END as varchar(5)) + 
				', "Scale": ' + cast([Attribute].scale as varchar(5)) + 
				', "Precision": ' + cast([Attribute].[precision] as varchar(5))+ ' }, ' +
				'"IsNullable": ' + cast([Attribute].is_nullable AS varchar(1)) + ', ' +
				IsNull('"CollationName": "' + [Attribute].collation_name + '", ', '') +
				'"NullOption": "' + CASE WHEN [Attribute].is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END + '" ' +
				IsNull((Select Top 1 
					', "Identity": { "IsIdentity": ' + cast([identity].is_identity as varchar(1)) + ', ' +
					'"Seed": ' + cast([identity].seed_value AS varchar(5)) + ', ' +
					'"Increment": ' + cast([identity].increment_value AS varchar(5)) +' }'
					FROM sys.identity_columns [identity] WITH (NOWAIT) WHERE [Attribute].is_identity = 1 AND  
						[Attribute].[object_id] = [identity].[object_id] AND 
						[Attribute].column_id = [identity].column_id), '') +
				IsNull((Select Top 1 
					', "DefaultConstraint": { "ConstraintName": "' + DefaultConstraint.name + '", ' +
					'"Definition": "' + DefaultConstraint.[definition] + '" }'
					FROM sys.default_constraints [DefaultConstraint] WITH (NOWAIT) WHERE [Attribute].default_object_id != 0 AND 
					[Attribute].[object_id] = [DefaultConstraint].parent_object_id AND 
					[Attribute].column_id = [DefaultConstraint].parent_column_id), '') +
				IsNull((Select Top 1 
					', "CheckConstraint": { "ConstraintName": "' + CheckConstraint.name + '", ' +
					'"Definition": "' + CheckConstraint.[definition] + '" }'
					FROM sys.check_constraints [CheckConstraint] WITH (NOWAIT) WHERE [Attribute].default_object_id != 0 AND 
					[Attribute].[object_id] = [CheckConstraint].parent_object_id AND 
					[Attribute].column_id = [CheckConstraint].parent_column_id), '')
				+ ' }'

			FROM sys.columns[Attribute]  WITH (NOWAIT)
			JOIN sys.types tp WITH (NOWAIT) ON [Attribute].user_type_id = tp.user_type_id
			LEFT JOIN sys.computed_columns cc WITH (NOWAIT) ON [Attribute].[object_id] = cc.[object_id] AND [Attribute].column_id = cc.column_id
			WHERE [Attribute].[object_id] = @object_id
			ORDER BY [Attribute].column_id
			FOR XML PATH('')), 1, 1, '') +
			'] ' + 

		IsNull((SELECT ', "PrimaryKey": {' + 
			'"KeyName": "' + [PrimaryKey].name + '", ' +
			'"KeyMembers": [ ' +
			STUFF((SELECT ',{ "FieldName": "' + c.name + '", ' +
				'"IsDescending": ' + convert(varchar(1), ic.is_descending_key) + ' }'
				FROM sys.index_columns ic WITH (NOWAIT) 
				JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
				WHERE ic.[object_id] = [PrimaryKey].parent_object_id AND ic.index_id = [PrimaryKey].unique_index_id AND 
				[PrimaryKey].[type] = 'PK' AND ic.is_included_column = 0
				FOR XML PATH('')), 1, 1, '')
			+ ' ]'
			+ ' }'
			FROM sys.key_constraints [PrimaryKey] WITH (NOWAIT) WHERE [PrimaryKey].parent_object_id = @object_id 
			AND [PrimaryKey].[type] = 'PK'), '') +
			
			IsNull((SELECT ', "Indexes": [' + 
				STUFF((SELECT ', { "IndexName": "' + [Index].name + '", ' +
					'"IsUnique": ' + cast([Index].is_unique AS varchar(1)) + ', ' +
					'"ClusteredOption": 0, '+
					'"KeyMembers": [ ' +
					STUFF((SELECT ',{ "FieldName": "' + c.name + '", ' +
						'"Position": ' + convert(varchar(1), ic.index_column_id) + ', ' +
						'"IsDescending": ' + convert(varchar(1), ic.is_descending_key) + ', ' +
						'"IsIncluded": ' + convert(varchar(1), ic.is_included_column) + ' }'
						FROM sys.index_columns ic WITH (NOWAIT)
						JOIN sys.columns c WITH (NOWAIT) ON ic.[object_id] = c.[object_id] AND ic.column_id = c.column_id
						WHERE ic.[object_id] = [Index].[object_id] and ic.[index_id] = [Index].[index_id] 
						FOR XML PATH('')), 1, 1, '')
					+ ' ]'
					+ ' }'
					FROM sys.indexes [Index] WITH (NOWAIT)
					WHERE [Index].[object_id] = @object_id
						AND [Index].is_primary_key = 0
						AND [Index].[type] = 2
					FOR XML PATH('')), 1, 1, '')+ ' ]'), '') +

			IsNull((SELECT ', "ForeignKeys": [' + 
				STUFF((SELECT ', { "KeyName": "' + [ForeignKey].name + 
				'", "CheckOption": ' + CASE WHEN [ForeignKey].is_not_trusted = 1 THEN '0' ELSE '1'	END + 
				', "DeleteRefAction": ' + CASE 
						WHEN [ForeignKey].delete_referential_action = 1 THEN '" ON DELETE CASCADE"' 
						WHEN [ForeignKey].delete_referential_action = 2 THEN '" ON DELETE SET NULL"'
						WHEN [ForeignKey].delete_referential_action = 3 THEN '" ON DELETE SET DEFAULT"' 
						ELSE '""' END +
				', "UpdateRefAction": ' + CASE 
						WHEN [ForeignKey].update_referential_action = 1 THEN '" ON UPDATE CASCADE"' 
						WHEN [ForeignKey].update_referential_action = 2 THEN '" ON UPDATE SET NULL"'
						WHEN [ForeignKey].update_referential_action = 3 THEN '" ON UPDATE SET DEFAULT"' 
						ELSE '""' END +
					STUFF((SELECT char(10) + ', "ParentObject": { "AreaName": "' + 
						SCHEMA_NAME([ParentObject].[schema_id]) + 
						'", "ObjectName": "' + [ParentObject].name + '" }'
						FROM sys.objects [ParentObject] WITH (NOWAIT) WHERE [ParentObject].[object_id] = [ForeignKey].parent_object_id
						FOR XML PATH('')), 1, 1, '') +
					STUFF((SELECT char(10) + ', "RefDbObjectName": { "AreaName": "' + 
						SCHEMA_NAME([RefDbObjectName].[schema_id]) + 
						'", "ObjectName": "' + [RefDbObjectName].name + '" }'
						FROM sys.objects [RefDbObjectName] WITH (NOWAIT) WHERE [RefDbObjectName].[object_id] = [ForeignKey].referenced_object_id
						FOR XML PATH('')), 1, 1, '') +
						
					', "KeyMembers": [ ' +
					STUFF((SELECT ',{ "FieldName": "' + 
						c.name + '", "Position": ' + convert(varchar(5), k.constraint_column_id)
						+ ' }'
						FROM sys.foreign_key_columns k WITH (NOWAIT)
						JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.parent_object_id AND c.column_id = k.parent_column_id
						WHERE k.constraint_object_id = [ForeignKey].object_id and k.parent_object_id = @object_id 
						FOR XML PATH('')), 1, 1, '')
					+ ' ]' +
					', "RefKeyMembers": [ ' +
					STUFF((SELECT ',{ "FieldName": "' + 
						c.name + '", "Position": ' + convert(varchar(5), k.constraint_column_id)
						+ ' }'
						FROM sys.foreign_key_columns k WITH (NOWAIT)
						JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = k.referenced_object_id AND c.column_id = k.referenced_column_id 
						WHERE k.constraint_object_id = [ForeignKey].object_id and k.parent_object_id = @object_id
						FOR XML PATH('')), 1, 1, '')
					+ ' ]' 
					+ ' }'
					FROM sys.foreign_keys [ForeignKey] WITH (NOWAIT)
					WHERE [ForeignKey].parent_object_id = @object_id 
					FOR XML PATH('')), 1, 1, '')+ ' ]'), '')					 

		FROM sys.objects o WITH (NOWAIT)
		WHERE o.object_id = @object_id)+ ' }', '')
		SELECT @json
	END
	ELSE
		SELECT ''
END


