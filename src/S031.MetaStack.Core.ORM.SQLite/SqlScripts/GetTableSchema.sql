select * from sqlite_master where tbl_name = @tableName and type = 'table';
select * from pragma_table_info(@tableName);
select * from pragma_foreign_key_list(@tableName);
SELECT 
    m.name as TableName, 
    il.name as IndexName,
    il.[unique],
    ii.seqno as Position, 
    ii.name as ColumnName
  FROM sqlite_master AS m,
       pragma_index_list(m.name) AS il,
       pragma_index_info(il.name) AS ii
 WHERE m.type='table' and m.name = @tableName
 ORDER BY il.name, ii.seqno;