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
