INSERT INTO SysSchemas (
		UID,
		SysAreaID,
		ObjectType,
		ObjectName,
		ObjectSchema,
		DbObjectName,
		Version,
		SyncState,
		UpdateTime,
		DateBegin,
		DateEnd,
		PreviosID)
	VALUES (
		@UID,
		@SysAreaID,
		@ObjectType,
		@ObjectName,
		@ObjectSchema,
		@DbObjectName,
		@Version,
		0,
		@UpdateTime,
		@DateBegin,
		@DateEnd,
		@PreviosID);
select last_insert_rowid() As ID;