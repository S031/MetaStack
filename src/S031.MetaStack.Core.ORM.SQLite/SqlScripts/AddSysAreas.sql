insert into [SysAreas]
	([SchemaName]
	,[SchemaOwner]
	,[SchemaVersion]
	,[SchemaConfig]
	,[IsDefault]
	,[UpdateTime]
	,[DateBegin]
	,[DateEnd])
values (@SchemaName, @SchemaOwner, @SchemaVersion, @SchemaConfig, 
	@IsDefault, @UpdateTime, @DateBegin, @DateEnd);
select last_insert_rowid() As ID;