if exists (Select Name From sys.schemas Where name = 'SysCat')
	select Top 1 IsNull(S.SchemaName, sys.schemas.Name) as Name From sys.schemas
	left join SysCat.SysAreas S On S.IsDefault = 1
	where sys.schemas.name = schema_name()
else
	select schema_name() as Name