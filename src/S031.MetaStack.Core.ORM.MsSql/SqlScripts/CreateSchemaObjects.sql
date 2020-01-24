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

