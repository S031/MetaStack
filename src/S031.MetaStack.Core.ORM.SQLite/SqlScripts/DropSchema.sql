begin transaction;
	drop view if exists V_SysSchemas;
	drop table if exists [SysSchemas];
	drop table if exists [SysAreas];
commit;