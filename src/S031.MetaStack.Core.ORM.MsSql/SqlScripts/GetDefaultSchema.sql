select Top 1 IsNull(S.SchemaName, '') as Name From SysCat.SysAreas S 
where S.IsDefault = 1