begin
	drop procedure if exists SysCat.Get_TableSchema
	drop procedure if exists SysCat.Get_TableSchema_xml
	drop procedure if exists SysCat.Get_TableSchema_ansi
	drop procedure if exists SysCat.Get_ParentRelations
	drop procedure if exists SysCat.Get_ParentRelations_ansi
	drop view if exists SysCat.V_SysSchemas
	drop procedure if exists SysCat.Add_SysSchemas
	drop procedure if exists SysCat.Del_SysSchemas
	drop procedure if exists SysCat.State_SysSchemas
	drop table if exists [SysCat].[SysSchemas]
	drop procedure if exists SysCat.Add_SysAreas
	drop table if exists [SysCat].[SysAreas]
	drop schema [SysCat]
end