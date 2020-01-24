BEGIN
DECLARE 
	@table_name		sysname = 'dbo.DealValues',
	@logic_name		sysname = null, -- убрать
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

if @logic_name is null
	if Exists (select v.object_id
			FROM sys.views v
			INNER JOIN sys.schemas s on v.schema_id = s.schema_id
			WHERE v.name = 'v_SysSchemas' and s.name = 'SysCat')
	begin
		exec sp_executesql N'select Top 1 @out_name = ObjectName 
			from SysCat.V_SysSchemas 
			WHERE SchemaName = @schema and DbObjectName = @table ORDER BY ID DESC'
			,N'@table	sysname
			,@schema	sysname
			,@out_name	sysname OUTPUT'
			,@table = @table_name
			,@schema = @schema_name
			,@out_name = @logic_name OUTPUT
		if @logic_name is null
			set @logic_name = left(@table_name, len(@table_name)-1)
	end
	else
		set @logic_name = left(@table_name, len(@table_name)-1)

IF NOT @object_id IS NULL
BEGIN
	SET @result = (SELECT 
		@logic_name As ObjectName,
		@schema_name AS [DbObjectName.AreaName],
		@table_name AS [DbObjectName.ObjectName],
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
			JSON_QUERY((SELECT TOP 1
				IsNull(dc.name, '') AS [ConstraintName],
				IsNull(dc.[definition], '') AS [Definition]
			FROM sys.default_constraints dc WITH (NOWAIT) 
			WHERE c.default_object_id != 0 AND c.[object_id] = dc.parent_object_id AND c.column_id = dc.parent_column_id
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)) AS [DefaultConstraint],
			JSON_QUERY((SELECT TOP 1
				IsNull(chc.name, '') AS [ConstraintName],
				IsNull(chc.[definition], '') AS [Definition]
			FROM sys.check_constraints chc WITH (NOWAIT) 
			WHERE c.[object_id] = chc.parent_object_id AND c.column_id = chc.parent_column_id
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)) AS [CheckConstraint],			
			ic.is_identity AS [Identity.IsIdentity],
			ic.seed_value AS [Identity.Seed],
			ic.increment_value AS [Identity.Increment]
		FROM sys.columns c WITH (NOWAIT)
		JOIN sys.types tp WITH (NOWAIT) ON c.user_type_id = tp.user_type_id
		LEFT JOIN sys.computed_columns cc WITH (NOWAIT) ON c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
		LEFT JOIN sys.identity_columns ic WITH (NOWAIT) ON c.is_identity = 1 AND c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
		WHERE c.[object_id] = @object_id
		ORDER BY c.column_id
		FOR JSON PATH) AS Attributes,

		k.name AS [PrimaryKey.KeyName],
		(SELECT c.name AS [FieldName],
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
			@schema_name AS [ParentObject.AreaName],
			@table_name AS [ParentObject.ObjectName],
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
			@schema_name AS [RefDbObjectName.AreaName],
			@table_name AS [RefDbObjectName.ObjectName],
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
		JOIN sys.objects po WITH (NOWAIT) ON po.[object_id] = fk.parent_object_id
		WHERE fk.referenced_object_id = @object_id FOR JSON PATH) AS ParentRelations,

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
