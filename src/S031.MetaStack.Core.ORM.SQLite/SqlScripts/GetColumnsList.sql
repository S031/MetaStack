SELECT c.column_id AS ID,
	c.name AS FieldName
FROM sys.objects o WITH (NOWAIT)
JOIN sys.schemas s WITH (NOWAIT) ON o.[schema_id] = s.[schema_id] AND s.name = @SchemaName AND o.name = @TableName
	AND o.[type] = 'U' AND o.is_ms_shipped = 0
JOIN sys.columns c WITH (NOWAIT) ON c.[object_id] = o.object_id
ORDER BY c.column_id