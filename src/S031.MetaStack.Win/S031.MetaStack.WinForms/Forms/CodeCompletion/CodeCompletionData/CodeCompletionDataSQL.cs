using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Collections.Generic;

namespace S031.MetaStack.WinForms
{
	internal static partial class CodeCompletionData
	{
		static ICompletionData[] _sqlTables = LoadTables();

		public static ICompletionData[] SQLTables
		{
			get { return _sqlTables; }
		}

		private static ICompletionData[] LoadTables()
		{
			List<ICompletionData> sqlTables = new List<ICompletionData>();
			//DataTable dt = dbs.GetRecordset(@"Select T.TABLE_NAME As TableName, IsNull(G.Name, '') As Name from INFORMATION_SCHEMA.TABLES T 
			//	LEFT JOIN GlobalObjects G ON g.ObjectName = LEFT(T.TABLE_NAME, LEN(T.TABLE_NAME)-1)");
			//foreach (DataRow dr in dt.Rows)
			//{
			//	sqlTables.Add(new DefaultCompletionData((string)dr["TableName"], (string)dr["Name"], _table));
			//}
			return sqlTables.ToArray();
		}

		public static ICompletionData[] SQLFields(string tableName)
		{
			List<ICompletionData> sqlFields = new List<ICompletionData>();
			//DataTable dt = dbs.GetRecordset(@"SELECT COLUMN_NAME, DATA_TYPE +  CASE WHEN CHARACTER_MAXIMUM_LENGTH is Null THEN '' ELSE 
			//	'(' + CONVERT(varchar(16), CHARACTER_MAXIMUM_LENGTH) +')' End + IsNull('. ' + Attribs.Name, '') As DataType FROM information_schema.columns Left Join
			//	Attribs On COLUMN_NAME = Attribs.AttribName and Attribs.ObjectName = LEFT(table_name, LEN(table_name) - 1)
			//	WHERE table_name = ?", tableName);

			//foreach (DataRow dr in dt.Rows)
			//	sqlFields.Add(new DefaultCompletionData((string)dr["COLUMN_NAME"], (string)dr["DataType"], _property));
			return sqlFields.ToArray();
		}

		public static ICompletionData[] SQLStatements = new ICompletionData[]{
			new DefaultCompletionData("IS NULL", @"IS NULL - true if column data is NULL", _statement),
			new DefaultCompletionData("BULK INSERT", @"BULK INSERT 
   [ database_name . [ schema_name ] . | schema_name . ] [ table_name | view_name ] 
      FROM 'data_file' 
     [ WITH 
    ( 
   [ [ , ] BATCHSIZE = batch_size ] 
   [ [ , ] CHECK_CONSTRAINTS ] 
   [ [ , ] CODEPAGE = { 'ACP' | 'OEM' | 'RAW' | 'code_page' } ] 
   [ [ , ] DATAFILETYPE = 
      { 'char' | 'native'| 'widechar' | 'widenative' } ] 
   [ [ , ] FIELDTERMINATOR = 'field_terminator' ] 
   [ [ , ] FIRSTROW = first_row ] 
   [ [ , ] FIRE_TRIGGERS ] 
   [ [ , ] FORMATFILE = 'format_file_path' ] 
   [ [ , ] KEEPIDENTITY ] 
   [ [ , ] KEEPNULLS ] 
   [ [ , ] KILOBYTES_PER_BATCH = kilobytes_per_batch ] 
   [ [ , ] LASTROW = last_row ] 
   [ [ , ] MAXERRORS = max_errors ] 
   [ [ , ] ORDER ( { column [ ASC | DESC ] } [ ,...n ] ) ] 
   [ [ , ] ROWS_PER_BATCH = rows_per_batch ] 
   [ [ , ] ROWTERMINATOR = 'row_terminator' ] 
   [ [ , ] TABLOCK ] 
   [ [ , ] ERRORFILE = 'file_name' ] 
    )]", _statement),
			new DefaultCompletionData("DELETE", @"[ WITH <common_table_expression> [ ,...n ] ]
DELETE 
    [ TOP ( expression ) [ PERCENT ] ] 
    [ FROM ] 
    { <object> | rowset_function_limited 
      [ WITH ( <table_hint_limited> [ ...n ] ) ]
    }
    [ <OUTPUT Clause> ]
    [ FROM <table_source> [ ,...n ] ] 
    [ WHERE { <search_condition> 
            | { [ CURRENT OF 
                   { { [ GLOBAL ] cursor_name } 
                       | cursor_variable_name 
                   } 
                ]
              }
            } 
    ] 
    [ OPTION ( <Query Hint> [ ,...n ] ) ] 
[; ]

<object> ::=
{ 
    [ server_name.database_name.schema_name. 
      | database_name. [ schema_name ] . 
      | schema_name.
    ]
    table_or_view_name 
}", _statement),
			new DefaultCompletionData("FROM", @"[ FROM { <table_source> } [ ,...n ] ] 
<table_source> ::= 
{
    table_or_view_name [ [ AS ] table_alias ] [ <tablesample_clause> ] 
        [ WITH ( < table_hint > [ [ , ]...n ] ) ] 
    | rowset_function [ [ AS ] table_alias ] 
        [ ( bulk_column_alias [ ,...n ] ) ] 
    | user_defined_function [ [ AS ] table_alias ] ]
    | OPENXML <openxml_clause> 
    | derived_table [ AS ] table_alias [ ( column_alias [ ,...n ] ) ] 
    | <joined_table> 
    | <pivoted_table> 
    | <unpivoted_table>
      | @variable [ [ AS ] table_alias ]
        | @variable.function_call ( expression [ ,...n ] ) [ [ AS ] table_alias ] [ (column_alias [ ,...n ] ) ]
}
<tablesample_clause> ::=
    TABLESAMPLE [SYSTEM] ( sample_number [ PERCENT | ROWS ] ) 
        [ REPEATABLE ( repeat_seed ) ] 

<joined_table> ::= 
{
    <table_source> <join_type> <table_source> ON <search_condition> 
    | <table_source> CROSS JOIN <table_source> 
    | left_table_source { CROSS | OUTER } APPLY right_table_source 
    | [ ( ] <joined_table> [ ) ] 
}
<join_type> ::= 
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN

<pivoted_table> ::=
    table_source PIVOT <pivot_clause> [ AS ] table_alias

<pivot_clause> ::=
        ( aggregate_function ( value_column [ [ , ]...n ]) 
        FOR pivot_column 
        IN ( <column_list> ) 
    ) 

<unpivoted_table> ::=
    table_source UNPIVOT <unpivot_clause> [ AS ] table_alias

<unpivot_clause> ::=
        ( value_column FOR pivot_column IN ( <column_list> ) ) 

<column_list> ::=
          column_name [ ,...n ]
", _statement),
			new DefaultCompletionData("INSERT", @"[ WITH <common_table_expression> [ ,...n ] ]
INSERT 
{
        [ TOP ( expression ) [ PERCENT ] ] 
        [ INTO ] 
        { <object> | rowset_function_limited 
          [ WITH ( <Table_Hint_Limited> [ ...n ] ) ] }
    {
        [ ( column_list ) ] 
        [ <OUTPUT Clause> ]
        { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n ] 
          | derived_table 
          | execute_statement
          | <dml_table_source>
          | DEFAULT VALUES 
        }
    }
}
", _statement),
			new DefaultCompletionData("MERGE", @"[ WITH <common_table_expression> [,...n] ]
MERGE 
    [ TOP ( expression ) [ PERCENT ] ] 
    [ INTO ] <target_table> [ WITH ( <merge_hint> ) ] [ [ AS ] table_alias ]
    USING <table_source> 
    ON <merge_search_condition>
    [ WHEN MATCHED [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ WHEN NOT MATCHED [ BY TARGET ] [ AND <clause_search_condition> ]
        THEN <merge_not_matched> ]
    [ WHEN NOT MATCHED BY SOURCE [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ <output_clause> ]
    [ OPTION ( <query_hint> [ ,...n ] ) ]    
;
", _statement),
			new DefaultCompletionData("OPTION", @"[ OPTION ( <query_hint> [ ,...n ] ) ] 
", _statement),
			new DefaultCompletionData("OUTPUT", @"<OUTPUT_CLAUSE> ::=
{
    [ OUTPUT <dml_select_list> INTO { @table_variable | output_table } [ ( column_list ) ] ]
    [ OUTPUT <dml_select_list> ]
}
", _statement),
			new DefaultCompletionData("SELECT", @"<SELECT statement> ::=  

    [WITH <common_table_expression> [,...n]]
    <query_expression> 
    [ ORDER BY { order_by_expression | column_position [ ASC | DESC ] } 
  [ ,...n ] ] 
    [ COMPUTE 
  { { AVG | COUNT | MAX | MIN | SUM } (expression )} [ ,...n ] 
  [ BY expression [ ,...n ] ] 
    ] 
    [ <FOR Clause>] 
    [ OPTION ( <query_hint> [ ,...n ] ) ] 

<query_expression> ::= 
    { <query_specification> | ( <query_expression> ) } 
    [  { UNION [ ALL ] | EXCEPT | INTERSECT }
        <query_specification> | ( <query_expression> ) [...n ] ] 

<query_specification> ::= 
SELECT [ ALL | DISTINCT ] 
    [TOP ( expression ) [PERCENT] [ WITH TIES ] ] 

    < select_list > 
    [ INTO new_table ] 
    [ FROM { <table_source> } [ ,...n ] ] 
    [ WHERE <search_condition> ] 
    [ <GROUP BY> ] 
    [ HAVING < search_condition > ] 
", _statement),
			new DefaultCompletionData("COMPUTE", @"[ COMPUTE 
    { { AVG | COUNT | MAX | MIN | STDEV | STDEVP | VAR | VARP | SUM } 
  ( expression ) } [ ,...n ] 
    [ BY expression [ ,...n ] ] 
]
", _statement),
			new DefaultCompletionData("FOR", @"[ FOR { BROWSE | <XML> } ]
<XML> ::=
XML 
{ 
    { RAW [ ( 'ElementName' ) ] | AUTO } 
    [ 
        <CommonDirectives> 
        [ , { XMLDATA | XMLSCHEMA [ ( 'TargetNameSpaceURI' ) ] } ] 
        [ , ELEMENTS [ XSINIL | ABSENT ] 
    ]
  | EXPLICIT 
    [ 
        <CommonDirectives> 
        [ , XMLDATA ] 
    ]
  | PATH [ ( 'ElementName' ) ] 
    [
        <CommonDirectives> 
        [ , ELEMENTS [ XSINIL | ABSENT ] ]
    ]
} 

<CommonDirectives> ::= 
[ , BINARY BASE64 ]
[ , TYPE ]
[ , ROOT [ ( 'RootName' ) ] ]

", _statement),
			new DefaultCompletionData("GROUP BY", @"GROUP BY <group by spec>
", _statement),
			new DefaultCompletionData("HAVING", @"[ HAVING <search condition> ]
", _statement),
			new DefaultCompletionData("INTO", @"[ INTO new_table ]
", _statement),
			new DefaultCompletionData("ORDER BY", @"[ ORDER BY 
    {
    order_by_expression 
  [ COLLATE collation_name ] 
  [ ASC | DESC ] 
    } [ ,...n ] 
] 
", _statement),
			new DefaultCompletionData("OVER", @"Ranking Window Functions 
< OVER_CLAUSE > :: =
    OVER ( [ PARTITION BY value_expression , ... [ n ] ]
           <ORDER BY_Clause> )

Aggregate Window Functions 
< OVER_CLAUSE > :: = 
    OVER ( [ PARTITION BY value_expression , ... [ n ] ] )
", _statement),
			new DefaultCompletionData("VALUES", @"VALUES ( <row value expression list> ) [ ,...n ] 

<row value expression list> ::=
    {<row value expression> } [ ,...n ]

<row value expression> ::=
    { DEFAULT | NULL | expression }
", _statement),
			new DefaultCompletionData("TOP", @"[ 
     TOP (expression) [PERCENT]
     [ WITH TIES ]
]
", _statement),
			new DefaultCompletionData("UPDATE", @"[ WITH <common_table_expression> [...n] ]
UPDATE 
    [ TOP ( expression ) [ PERCENT ] ] 
    { { table_alias | <object> | rowset_function_limited 
         [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
      }
      | @table_variable    
    }
    SET
        { column_name = { expression | DEFAULT | NULL }
          | { udt_column_name.{ { property_name = expression
                                | field_name = expression }
                                | method_name ( argument [ ,...n ] )
                              }
            }
          | column_name { .WRITE ( expression , @Offset , @Length ) }
          | @variable = expression
          | @variable = column = expression
          | column_name { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable = column { += | -= | *= | /= | %= | &= | ^= | |= } expression
        } [ ,...n ] 

    [ <OUTPUT Clause> ]
    [ FROM { <table_source> } [ ,...n ] ] 
    [ WHERE { <search_condition> 
            | { [ CURRENT OF 
                  { { [ GLOBAL ] cursor_name } 
                      | cursor_variable_name 
                  } 
                ]
              }
            } 
    ] 
    [ OPTION ( <query_hint> [ ,...n ] ) ]
[ ; ]

<object> ::=
{ 
    [ server_name . database_name . schema_name . 
    | database_name .[ schema_name ] . 
    | schema_name .
    ]
    table_or_view_name}
", _statement),
			new DefaultCompletionData("WHERE", @"[ WHERE <search_condition> ]
", _statement),
			new DefaultCompletionData("WITH", @"[ WITH <common_table_expression> [ ,...n ] ]

<common_table_expression>::=
    expression_name [ ( column_name [ ,...n ] ) ]
    AS
    ( CTE_query_definition )

", _statement),
			new DefaultCompletionData("ALTER TABLE", @"ALTER TABLE [ database_name . [ schema_name ] . | schema_name . ] table_name 
{ 
    ALTER COLUMN column_name 
    { 
        [ type_schema_name. ] type_name [ ( { precision [ , scale ] 
            | max | xml_schema_collection } ) ] 
        [ COLLATE collation_name ] 
        [ NULL | NOT NULL ] [ SPARSE ]
    | {ADD | DROP } 
        { ROWGUIDCOL | PERSISTED | NOT FOR REPLICATION | SPARSE }
    } 
        | [ WITH { CHECK | NOCHECK } ]

    | ADD 
    { 
        <column_definition>
      | <computed_column_definition>
      | <table_constraint> 
      | <column_set_definition> 
    } [ ,...n ]

    | DROP 
    { 
        [ CONSTRAINT ] constraint_name 
        [ WITH ( <drop_clustered_constraint_option> [ ,...n ] ) ]
        | COLUMN column_name 
    } [ ,...n ] 

    | [ WITH { CHECK | NOCHECK } ] { CHECK | NOCHECK } CONSTRAINT 
        { ALL | constraint_name [ ,...n ] } 

    | { ENABLE | DISABLE } TRIGGER 
        { ALL | trigger_name [ ,...n ] }

    | { ENABLE | DISABLE } CHANGE_TRACKING 
        [ WITH ( TRACK_COLUMNS_UPDATED = { ON | OFF } ) ]

    | SWITCH [ PARTITION source_partition_number_expression ]
        TO target_table 
        [ PARTITION target_partition_number_expression ]

    | SET ( FILESTREAM_ON = { partition_scheme_name | filegroup | 
                default | NULL } )

    | REBUILD 
      [ [PARTITION = ALL]
        [ WITH ( <rebuild_option> [ ,...n ] ) ] 
      | [ PARTITION = partition_number 
           [ WITH ( <single_partition_rebuild_option> [ ,...n ] )]
        ]
      ]

    | (<table_option>)
}
[ ; ]
", _statement),
			new DefaultCompletionData("EXECUTE", @"Execute a stored procedure or function
[ { EXEC | EXECUTE } ]
    { 
      [ @return_status = ]
      { module_name [ ;number ] | @module_name_var } 
        [ [ @parameter = ] { value 
                           | @variable [ OUTPUT ] 
                           | [ DEFAULT ] 
                           }
        ]
      [ ,...n ]
      [ WITH RECOMPILE ]
    }
[;]

Execute a character string
{ EXEC | EXECUTE } 
    ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    [ AS { LOGIN | USER } = ' name ' ]
[;]

Execute a pass-through command against a linked server
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'command_string [ ? ]' } [ + ...n ]
        [ { , { value | @variable [ OUTPUT ] } } [ ...n ] ]
    ) 
    [ AS { LOGIN | USER } = ' name ' ]
    [ AT linked_server_name ]
[;]
", _statement),
			new DefaultCompletionData("DECLARE", @"DECLARE 
     { 
{{ @local_variable [AS] data_type } | [ = value ] }
    | { @cursor_variable_name CURSOR }
} [,...n] 
    | { @table_variable_name [AS] <table_type_definition> | <user-defined table type> } 

<table_type_definition> ::= 
     TABLE ( { <column_definition> | <table_constraint> } [ ,... ] 
   ) 

<column_definition> ::= 
     column_name { scalar_data_type | AS computed_column_expression }
     [ COLLATE collation_name ] 
     [ [ DEFAULT constant_expression ] | IDENTITY [ (seed ,increment ) ] ] 
     [ ROWGUIDCOL ] 
     [ <column_constraint> ] 

<column_constraint> ::= 
     { [ NULL | NOT NULL ] 
     | [ PRIMARY KEY | UNIQUE ] 
     | CHECK ( logical_expression ) 
     | WITH ( < index_option > )
     } 

<table_constraint> ::= 
     { { PRIMARY KEY | UNIQUE } ( column_name [ ,... ] ) 
     | CHECK ( search_condition ) 
     } 
", _statement),
			new DefaultCompletionData("BETWEEN", @"test_expression [ NOT ] BETWEEN begin_expression AND end_expression
", _statement),
			new DefaultCompletionData("EXISTS", @"EXISTS subquery
", _statement),
			new DefaultCompletionData("ALL", @"scalar_expression { = | <> | != | > | >= | !> | < | <= | !< } ALL ( subquery )
", _statement),
			new DefaultCompletionData("AND", @"boolean_expression AND boolean_expression", _statement),
			new DefaultCompletionData("IN", @"test_expression [ NOT ] IN 
    ( subquery | expression [ ,...n ]
    ) 
", _statement),
			new DefaultCompletionData("LIKE", @"match_expression [ NOT ] LIKE pattern [ ESCAPE escape_character ]
", _statement),
			new DefaultCompletionData("NOT", @"[ NOT ] boolean_expression", _statement),
			new DefaultCompletionData("ANY", @"scalar_expression { = | < > | ! = | > | > = | ! > | < | < = | ! < } 
     { SOME | ANY } ( subquery ) 
", _statement),
			new DefaultCompletionData("SOME", @"scalar_expression { = | < > | ! = | > | > = | ! > | < | < = | ! < } 
     { SOME | ANY } ( subquery ) 
", _statement),
			new DefaultCompletionData("SET", @"SET 
{ @local_variable
    [ . { property_name | field_name } ] = { expression | udt_name { . | :: } method_name }
}
|
{ @SQLCLR_local_variable.mutator_method
}
|
{ @local_variable
    {+= | -= | *= | /= | %= | &= | ^= | |= } expression
}
| 
  { @cursor_variable = 
    { @cursor_variable | cursor_name 
    | { CURSOR [ FORWARD_ONLY | SCROLL ] 
        [ STATIC | KEYSET | DYNAMIC | FAST_FORWARD ] 
        [ READ_ONLY | SCROLL_LOCKS | OPTIMISTIC ] 
        [ TYPE_WARNING ] 
    FOR select_statement 
        [ FOR { READ ONLY | UPDATE [ OF column_name [ ,...n ] ] } ] 
      } 
    }
} 
", _statement),
			new DefaultCompletionData("BEGIN", @"BEGIN
     { 
    sql_statement | statement_block 
     } 
END", _statement),
			new DefaultCompletionData("END", @"BEGIN
     { 
    sql_statement | statement_block 
     } 
END", _statement),
			new DefaultCompletionData("BREAK", @"WHILE Boolean_expression 
     { sql_statement | statement_block | BREAK | CONTINUE } 
", _statement),
			new DefaultCompletionData("CONTINUE", @"Restarts a WHILE loop. Any statements after the CONTINUE keyword are ignored.
CONTINUE is frequently, but not always, opened by an IF test", _statement),
			new DefaultCompletionData("IF", @"IF Boolean_expression { sql_statement | statement_block } 
    [ ELSE { sql_statement | statement_block } ] 
", _statement),
			new DefaultCompletionData("ELSE", @"IF Boolean_expression { sql_statement | statement_block } 
    [ ELSE { sql_statement | statement_block } ] 
", _statement),
			new DefaultCompletionData("OR", @"OR Boolean_expression { sql_statement | statement_block } 
    [ ELSE { sql_statement | statement_block } ] 
", _statement),
			new DefaultCompletionData("GOTO", @"Define the label: 
label : 
Alter the execution:
GOTO label 
", _statement),
			new DefaultCompletionData("RETURN", @"RETURN [ integer_expression ] 
", _statement),
			new DefaultCompletionData("BEGIN TRY", @"BEGIN TRY
     { sql_statement | statement_block }
END TRY
BEGIN CATCH
     [ { sql_statement | statement_block } ]
END CATCH
[ ; ]
", _statement),
			new DefaultCompletionData("END TRY", @"BEGIN TRY
     { sql_statement | statement_block }
END TRY
BEGIN CATCH
     [ { sql_statement | statement_block } ]
END CATCH
[ ; ]
", _statement),
			new DefaultCompletionData("BEGIN CATCH", @"BEGIN TRY
     { sql_statement | statement_block }
END TRY
BEGIN CATCH
     [ { sql_statement | statement_block } ]
END CATCH
[ ; ]
", _statement),
			new DefaultCompletionData("END CATCH", @"BEGIN TRY
     { sql_statement | statement_block }
END TRY
BEGIN CATCH
     [ { sql_statement | statement_block } ]
END CATCH
[ ; ]
", _statement),
			new DefaultCompletionData("WAITFOR", @"WAITFOR 
{
    DELAY 'time_to_pass' 
  | TIME 'time_to_execute' 
  | [ ( receive_statement ) | ( get_conversation_group_statement ) ] 
    [ , TIMEOUT timeout ]
}
", _statement),
			new DefaultCompletionData("WHILE", @"WHILE Boolean_expression 
     { sql_statement | statement_block | BREAK | CONTINUE } 

", _statement),
			new DefaultCompletionData("CLOSE", @"CLOSE { { [ GLOBAL ] cursor_name } | cursor_variable_name }
", _statement),
			new DefaultCompletionData("OPEN", @"OPEN { { [ GLOBAL ] cursor_name } | cursor_variable_name }
", _statement),
			new DefaultCompletionData("FETCH", @"FETCH 
          [ [ NEXT | PRIOR | FIRST | LAST 
                    | ABSOLUTE { n | @nvar } 
                    | RELATIVE { n | @nvar } 
               ] 
               FROM 
          ] 
{ { [ GLOBAL ] cursor_name } | @cursor_variable_name } 
[ INTO @variable_name [ ,...n ] ] 
", _statement),
			new DefaultCompletionData("DEALLOCATE", @"DEALLOCATE { { [ GLOBAL ] cursor_name } | @cursor_variable_name }
", _statement),
			new DefaultCompletionData("DECLARE", @"DECLARE cursor_name [ INSENSITIVE ] [ SCROLL ] CURSOR 
     FOR select_statement 
     [ FOR { READ ONLY | UPDATE [ OF column_name [ ,...n ] ] } ]
[;]
Transact-SQL Extended Syntax
DECLARE cursor_name CURSOR [ LOCAL | GLOBAL ] 
     [ FORWARD_ONLY | SCROLL ] 
     [ STATIC | KEYSET | DYNAMIC | FAST_FORWARD ] 
     [ READ_ONLY | SCROLL_LOCKS | OPTIMISTIC ] 
     [ TYPE_WARNING ] 
     FOR select_statement 
     [ FOR UPDATE [ OF column_name [ ,...n ] ] ]
[;]
", _statement),
			new DefaultCompletionData("CASE", @"Simple CASE expression: 
CASE input_expression 
     WHEN when_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END 
Searched CASE expression:
CASE
     WHEN Boolean_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END
", _statement),
			new DefaultCompletionData("WHEN", @"Simple CASE expression: 
CASE input_expression 
     WHEN when_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END 
Searched CASE expression:
CASE
     WHEN Boolean_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END
", _statement),
			new DefaultCompletionData("THEN", @"Simple CASE expression: 
CASE input_expression 
     WHEN when_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END 
Searched CASE expression:
CASE
     WHEN Boolean_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END
", _statement),
			new DefaultCompletionData("ELSE", @"Simple CASE expression: 
CASE input_expression 
     WHEN when_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END 
Searched CASE expression:
CASE
     WHEN Boolean_expression THEN result_expression [ ...n ] 
     [ ELSE else_result_expression ] 
END
", _statement),
			new DefaultCompletionData("PRINT", @"PRINT msg_str | @local_variable | string_expr
", _statement),
			new DefaultCompletionData("RAISERROR", @"RAISERROR ( { msg_id | msg_str | @local_variable }
    { ,severity ,state }
    [ ,argument [ ,...n ] ] )
    [ WITH option [ ,...n ] ]
", _statement),
			new DefaultCompletionData("INNER JOIN", @"<join_type> ::= 
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN", _statement),
			new DefaultCompletionData("LEFT JOIN", @"<join_type> ::= 
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN", _statement),
			new DefaultCompletionData("RIGHT JOIN", @"<join_type> ::= 
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN", _statement),
			new DefaultCompletionData("FULL JOIN", @"<join_type> ::= 
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN", _statement),
			new DefaultCompletionData("ON", @"ON  <join_expression>", _statement),
			new DefaultCompletionData("AS", @"AS  <alias>", _statement),
		};

		public static ICompletionData[] SQLConstants = new ICompletionData[]{
		};
 
		public static ICompletionData[] SQLDataTypes = new ICompletionData[]{
			new DefaultCompletionData("bigint", @"8 Bytes -2^63 (-9,223,372,036,854,775,808) to 2^63-1 (9,223,372,036,854,775,807)", _statement),
			new DefaultCompletionData("int", @"4 Bytes -2^31 (-2,147,483,648) to 2^31-1 (2,147,483,647)", _statement),
			new DefaultCompletionData("smallint", @"2 Bytes -2^15 (-32,768) to 2^15-1 (32,767)", _statement),
			new DefaultCompletionData("tinyint", @"0 to 255", _statement),
			new DefaultCompletionData("decimal", @"decimal[ (p[ ,s] )] and numeric[ (p[ ,s] )] ", _statement),
			new DefaultCompletionData("bit", @"The SQL Server Database Engine optimizes storage of bit columns. 
If there are 8 or less bit columns in a table, the columns are stored as 1 byte. 
If there are from 9 up to 16 bit columns, the columns are stored as 2 bytes, and so on.
The string values TRUE and FALSE can be converted to bit values: TRUE is converted to 1 and FALSE is converted to 0. 
", _statement),
			new DefaultCompletionData("money", @"8 bytes -922,337,203,685,477.5808 to 922,337,203,685,477.5807", _statement),
			new DefaultCompletionData("smallmoney", @"4 bytes - 214,748.3648 to 214,748.3647", _statement),
 
			new DefaultCompletionData("real", @"The ISO synonym for real is float(24). - 1.79E+308 to -2.23E-308, 0 and 2.23E-308 to 1.79E+308", _statement),
			new DefaultCompletionData("float", @"4 Bytes - 3.40E + 38 to -1.18E - 38, 0 and 1.18E - 38 to 3.40E + 38", _statement),

			new DefaultCompletionData("time", @"Output:12:35:29. 1234567", _statement),
			new DefaultCompletionData("date", @"Output:2007-05-08", _statement),
			new DefaultCompletionData("smalldatetime", @"Output:2007-05-08 12:35:00", _statement),
			new DefaultCompletionData("datetime", @"Output:2007-05-08 12:35:29.123", _statement),
			new DefaultCompletionData("datetime2", @"Output:2007-05-08 12:35:29. 1234567", _statement),
			new DefaultCompletionData("datetimeoffset", @"Output:2007-05-08 12:35:29.1234567 +12:15", _statement),
			new DefaultCompletionData("varchar", @"varchar [ ( n | max ) ] ", _statement),
			new DefaultCompletionData("char", @"char [ ( n ) ] ", _statement),
			new DefaultCompletionData("text", @"Variable-length non-Unicode data in the code page of the server and 
with a maximum string length of 2^31-1 (2,147,483,647). When the server code page uses double-byte characters, 
the storage is still 2,147,483,647 bytes. Depending on the character string, 
the storage size may be less than 2,147,483,647 bytes.
", _statement),
			new DefaultCompletionData("ntext", @"Variable-length Unicode data with a maximum string length of 2^30 - 1 (1,073,741,823). 
Storage size, in bytes, is two times the string length entered. The ISO synonym for ntext is national text
", _statement),
			new DefaultCompletionData("image", @"Variable-length binary data from 0 through 2^31-1 (2,147,483,647) bytes.
", _statement),

			new DefaultCompletionData("nchar", @"nchar [ ( n ) ] 
Fixed-length Unicode string data. n defines the string length and must be a value from 1 through 4,000. 
The storage size is two times n bytes. When the collation code page uses double-byte characters, 
the storage size is still n bytes. Depending on the string, the storage size of n bytes can be less than 
the value specified for n. The ISO synonyms for nchar are national char and national character.
", _statement),
			new DefaultCompletionData("nvarchar", @"nvarchar [ ( n | max ) ] 
Variable-length Unicode string data. n defines the string length and can be a value from 1 through 4,000. 
max indicates that the maximum storage size is 2^31-1 bytes (2 GB). The storage size, in bytes, 
is two times the actual length of data entered+ 2 bytes. The ISO synonyms for nvarchar are 
national char varying and national character varying.
", _statement),
new DefaultCompletionData("", @"", _statement),
			new DefaultCompletionData("binary", @"binary [ ( n ) ] Fixed-length binary data with a length of n bytes, 
where n is a value from 1 through 8,000. The storage size is n bytes.", _statement),
			new DefaultCompletionData("varbinary", @"varbinary [ ( n | max) ] Variable-length binary data. n can be a value from 1 through 8,000. 
max indicates that the maximum storage size is 2^31-1 bytes. The storage size is the actual length of the data entered + 2 bytes. 
The data that is entered can be 0 bytes in length. The ANSI SQL synonym for varbinary is binary varying.
", _statement),
			new DefaultCompletionData("cursor", @"A data type for variables or stored procedure OUTPUT parameters that contain a reference to a cursor. 
Any variables created with the cursordata type are nullable.", _statement),
			new DefaultCompletionData("xml", @"xml ( [ CONTENT | DOCUMENT ] xml_schema_collection )Is the data type that stores XML data. You can store xml instances in a column, or a variable of xml type
", _statement),
			new DefaultCompletionData("uniqueidentifier", @"Is a 16-byte GUID", _statement),
			new DefaultCompletionData("sql_variant", @"A data type that stores values of various SQL Server-supported data types.", _statement),
			new DefaultCompletionData("table", @"table_type_definition ::= 
    TABLE ( { <column_definition> | <table_constraint> } [ ,...n ] ) 

<column_definition> ::= 
    column_name scalar_data_type 
    [ COLLATE <collation_definition> ] 
    [ [ DEFAULT constant_expression ] | IDENTITY [ ( seed , increment ) ] ] 
    [ ROWGUIDCOL ] 
    [ column_constraint ] [ ...n ] 

<column_constraint> ::= 
    { [ NULL | NOT NULL ] 
    | [ PRIMARY KEY | UNIQUE ] 
    | CHECK ( logical_expression ) 
    } 

<table_constraint> ::= 
     { { PRIMARY KEY | UNIQUE } ( column_name [ ,...n ] )
     | CHECK ( logical_expression ) 
     } 
", _statement),
		};

		
		public static ICompletionData[] SQLFunctions = new ICompletionData[]{
			new DefaultCompletionData("NULLIF", @"NULLIF ( expression , expression )
", _statement),
			new DefaultCompletionData("CAST", @"Syntax for CAST:
CAST ( expression AS data_type [ ( length ) ] )
", _statement),
			new DefaultCompletionData("CONVERT", @"Syntax for CONVERT:
CONVERT ( data_type [ ( length ) ] , expression [ , style ] )
", _statement),
			new DefaultCompletionData("AVG", @"AVG ( [ ALL | DISTINCT ] expression ) ", _statement),
			new DefaultCompletionData("CHECKSUM_AGG", @"CHECKSUM_AGG ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("COUNT_BIG", @"COUNT_BIG ( { [ ALL | DISTINCT ] expression } | * )", _statement),
			new DefaultCompletionData("COUNT", @"COUNT ( { [ [ ALL | DISTINCT ] expression ] | * } )", _statement),
			new DefaultCompletionData("GROUPING", @"GROUPING ( <column_expression> )", _statement),
			new DefaultCompletionData("GROUPING_ID", @"GROUPING_ID ( <column_expression>[ ,...n ] )", _statement),
			new DefaultCompletionData("MAX", @"MAX ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("MIN", @"MIN ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("ROWCOUNT_BIG", @"ROWCOUNT_BIG ( )", _statement),
			new DefaultCompletionData("STDEV", @"STDEV ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("STDEVP", @"STDEVP ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("SUM", @"SUM ( [ ALL | DISTINCT ] expression )", _statement),
			new DefaultCompletionData("VAR", @"VAR ( [ ALL | DISTINCT ] expression ) ", _statement),
			new DefaultCompletionData("VARP", @"VARP ( [ ALL | DISTINCT ] expression )", _statement),  
			new DefaultCompletionData("@@NESTLEVEL", @"@@NESTLEVEL Returns the nesting level of the current stored procedure execution (initially 0) 
on the local server. For information about nesting level", _statement),
			new DefaultCompletionData("@@VERSION", @"@@VERSION Returns version, processor architecture, build date, 
and operating system for the current installation of SQL Server", _statement),
			new DefaultCompletionData("@@MAX_PRECISION", @"@@MAX_PRECISION Returns the precision level used by decimal and 
numeric data types as currently set in the server.", _statement),
			new DefaultCompletionData("@@TEXTSIZE", @"@@TEXTSIZE Returns the current value of the TEXTSIZE option.", _statement),
			new DefaultCompletionData("@@MAX_CONNECTIONS", @"@@MAX_CONNECTIONS Returns the maximum number of 
simultaneous user connections allowed on an instance of SQL Server. 
The number returned is not necessarily the number currently configured", _statement),
			new DefaultCompletionData("@@SPID", @"@@SPID Returns the session ID of the current user process.", _statement),
			new DefaultCompletionData("@@LOCK_TIMEOUT", @"@@LOCK_TIMEOUT Returns the current lock time-out setting in milliseconds for the current session.", _statement),
			new DefaultCompletionData("@@SERVICENAME", @"@@SERVICENAME Returns the name of the registry key under which SQL Server is running. 
@@SERVICENAME returns 'MSSQLSERVER' if the current instance is the default instance; 
this function returns the instance name if the current instance is a named instance.", _statement),
			new DefaultCompletionData("@@LANGUAGE", @"@@LANGUAGE Returns the name of the language currently being used.", _statement),
			new DefaultCompletionData("@@SERVERNAME", @"@@SERVERNAME Returns the name of the local server that is running SQL Server. ", _statement),
			new DefaultCompletionData("@@LANGID", @"@@LANGID Returns the local language identifier (ID) of the language that is currently being used.", _statement),
			new DefaultCompletionData("@@DBTS", @"@@DBTS Returns the value of the current timestamp data type for the current database. 
This timestamp is guaranteed to be unique in the database.", _statement),
			new DefaultCompletionData("@@OPTIONS", @"@@OPTIONS Returns information about the current SET options.", _statement),
			new DefaultCompletionData("@@DATEFIRST", @"@@DATEFIRST Returns the current value, for a session, of SET DATEFIRST.", _statement),
			new DefaultCompletionData("@@CURSOR_ROWS", @"@@CURSOR_ROWS Returns the number of qualifying rows currently in the last cursor opened on the connection. 
To improve performance, SQL Server can populate large keyset and static cursors asynchronously. 
@@CURSOR_ROWS can be called to determine that the number of the rows that qualify for a cursor are 
retrieved at the time @@CURSOR_ROWS is called.", _statement),
			new DefaultCompletionData("CURSOR_STATUS", @"CURSOR_STATUS 
     (
          { 'local' , 'cursor_name' } 
          | { 'global' , 'cursor_name' } 
          | { 'variable' , 'cursor_variable' } 
     )
 ", _statement),
			new DefaultCompletionData("@@FETCH_STATUS", @"@@FETCH_STATUS Returns the status of the last cursor FETCH statement issued against any 
cursor currently opened by the connection.", _statement),
			new DefaultCompletionData("DATALENGTH", @"DATALENGTH ( expression )", _statement),
			new DefaultCompletionData("IDENT_SEED", @"IDENT_SEED ( 'table_or_view' )", _statement),
			new DefaultCompletionData("IDENT_CURRENT", @"IDENT_CURRENT( 'table_name' )", _statement),
			new DefaultCompletionData("IDENTITY", @"IDENTITY (data_type [ , seed , increment ] ) AS column_name", _statement),
			new DefaultCompletionData("IDENT_INCR", @"IDENT_INCR ( 'table_or_view' )", _statement),
			new DefaultCompletionData("SQL_VARIANT_PROPERTY", @"SQL_VARIANT_PROPERTY ( expression , property )", _statement),
			new DefaultCompletionData("SYSUTCDATETIME", @"SYSUTCDATETIME() Returns a datetime2(7) value that contains the date and time of the computer 
on which the instance of SQL Server is running. The date and time is returned as UTC time (Coordinated Universal Time).", _statement),
			new DefaultCompletionData("SYSDATETIMEOFFSET", @"SYSDATETIMEOFFSET ( ) Returns a datetimeoffset(7) value that contains the date and time 
of the computer on which the instance of SQL Server is running. The time zone offset is included.", _statement),
			new DefaultCompletionData("SYSDATETIME", @"SYSDATETIME () Returns a datetime2(7) value that contains the date and time of the computer 
on which the instance of SQL Server is running. The time zone offset is not included.", _statement),
			new DefaultCompletionData("GETUTCDATE", @"GETUTCDATE ( ) Returns a datetime2(7) value that contains the date and time of the computer 
on which the instance of SQL Server is running. The date and time is returned as UTC time (Coordinated Universal Time).", _statement),
			new DefaultCompletionData("GETDATE", @"GETDATE ( ) Returns a datetime2(7) value that contains the date and time of the computer 
on which the instance of SQL Server is running. The time zone offset is not included.", _statement),
			new DefaultCompletionData("CURRENT_TIMESTAMP", @"CURRENT_TIMESTAMP Returns a datetime2(7) value that contains the date and time of the computer 
on which the instance of SQL Server is running. The time zone offset is not included.", _statement),
			new DefaultCompletionData("YEAR", @"YEAR ( date ) Returns an integer that represents the year part of a specified date.", _statement),
			new DefaultCompletionData("MONTH", @"MONTH ( date ) Returns an integer that represents the month part of a specified date.", _statement),
			new DefaultCompletionData("DAY", @"DAY ( date ) Returns an integer that represents the day day part of the specified date.", _statement),
			new DefaultCompletionData("DATEPART", @"DATEPART ( datepart , date ) Returns an integer that represents the specified datepart of the specified date.", _statement),
			new DefaultCompletionData("DATENAME", @"DATENAME ( datepart , date ) Returns a character string that represents the specified datepart of the specified date", _statement),
 			new DefaultCompletionData("DATEDIFF", @"DATEDIFF ( datepart , startdate , enddate ) Returns the number of date or time datepart boundaries that are crossed between two specified dates.", _statement),
			new DefaultCompletionData("TODATETIMEOFFSET", @"TODATETIMEOFFSET (expression , time_zone) TODATETIMEOFFSET transforms a datetime2 value into a datetimeoffset value. 
The datetime2 value is interpreted in local time for the specified time_zone.", _statement),
			new DefaultCompletionData("SWITCHOFFSET", @"SWITCHOFFSET (DATETIMEOFFSET , time_zone) SWITCHOFFSET changes the time zone offset of a DATETIMEOFFSET value and preserves the UTC value.", _statement),
			new DefaultCompletionData("DATEADD", @"DATEADD (datepart , number , date ) Returns a new datetime value by adding an interval to the specified datepart of the specified date.", _statement),
			new DefaultCompletionData("RADIANS", @"RADIANS ( numeric_expression ) Returns radians when a numeric expression, in degrees, is entered.", _statement),
			new DefaultCompletionData("COT", @"COT ( float_expression ) A mathematical function that returns the trigonometric cotangent of the specified angle, 
in radians, in the specified float expression.", _statement),
			new DefaultCompletionData("TAN", @"TAN ( float_expression ) Returns the tangent of the input expression", _statement),
			new DefaultCompletionData("POWER", @"POWER ( float_expression , y ) Returns the value of the specified expression to the specified power. ", _statement),
			new DefaultCompletionData("COS", @"COS ( float_expression ) Is a mathematical function that returns the trigonometric cosine of the specified angle, in radians, in the specified expression.", _statement),
			new DefaultCompletionData("SQUARE", @"SQUARE ( float_expression ) Returns the square of the specified float value.", _statement),
			new DefaultCompletionData("PI", @"Returns the constant value of PI.", _statement),
			new DefaultCompletionData("CEILING", @"CEILING ( numeric_expression ) Returns the smallest integer greater than, or equal to, the specified numeric expression.", _statement),
			new DefaultCompletionData("SQRT", @"SQRT ( float_expression ) Returns the square root of the specified float value.", _statement),
			new DefaultCompletionData("LOG10", @"LOG10 ( float_expression ) Returns the base-10 logarithm of the specified float expression.", _statement),
			new DefaultCompletionData("ATN2", @"ATN2 ( float_expression , float_expression ) Returns the angle, in radians, between the positive x-axis and the ray from the origin to the point (y, x), 
where x and y are the values of the two specified float expressions.", _statement),
			new DefaultCompletionData("SIN", @"SIN ( float_expression ) Returns the trigonometric sine of the specified angle, in radians, and in an approximate numeric, float, expression.", _statement),
			new DefaultCompletionData("LOG", @"LOG ( float_expression ) Returns the natural logarithm of the specified float expression.", _statement),
			new DefaultCompletionData("ATAN", @"ATAN ( float_expression ) Returns the angle in radians whose tangent is a specified float expression. This is also called arctangent.", _statement),
			new DefaultCompletionData("SIGN", @"SIGN ( numeric_expression ) Returns the positive (+1), zero (0), or negative (-1) sign of the specified expression.", _statement),
			new DefaultCompletionData("FLOOR", @"FLOOR ( numeric_expression ) Returns the largest integer less than or equal to the specified numeric expression.", _statement),
			new DefaultCompletionData("ASIN", @"ASIN ( float_expression ) Returns the angle, in radians, whose sine is the specified float expression. This is also called arcsine.", _statement),
			new DefaultCompletionData("ROUND", @"ROUND ( numeric_expression , length [ ,function ] ) Returns a numeric value, rounded to the specified length or precision.", _statement),
			new DefaultCompletionData("EXP", @"EXP ( float_expression ) Returns the exponential value of the specified float expression.", _statement),
			new DefaultCompletionData("ACOS", @"ACOS ( float_expression ) A mathematical function that returns the angle, in radians, whose cosine is the specified float expression; also called arccosine.", _statement),
			new DefaultCompletionData("RAND", @"RAND ( [ seed ] ) Returns a pseudo-random float value from 0 through 1, exclusive.", _statement),
			new DefaultCompletionData("DEGREES", @"DEGREES ( numeric_expression ) Returns the corresponding angle in degrees for an angle specified in radians.", _statement),
			new DefaultCompletionData("ABS", @"ABS ( numeric_expression ) A mathematical function that returns the absolute (positive) value of the specified numeric expression.", _statement),
			new DefaultCompletionData("RANK", @"RANK ( )    OVER ( [ < partition_by_clause > ] < order_by_clause > )Returns the rank of each row within the partition of a result set. The rank of a 
row is one plus the number of ranks that come before the row in question.", _statement),
			new DefaultCompletionData("DENSE_RANK", @"DENSE_RANK ( )    OVER ( [ <partition_by_clause> ] < order_by_clause > ) Returns the rank of rows within the partition of a result set, without any gaps in the ranking. 
The rank of a row is one plus the number of distinct ranks that come before the row in question.", _statement),
			new DefaultCompletionData("NTILE", @"NTILE (integer_expression)    OVER ( [ <partition_by_clause> ] < order_by_clause > ) Distributes the rows in an ordered partition into a specified number of groups. 
The groups are numbered, starting at one. For each row, NTILE returns the number of the group to which the row belongs.", _statement),
			new DefaultCompletionData("ROW_NUMBER", @"ROW_NUMBER ( )     OVER ( [ <partition_by_clause> ] <order_by_clause> ) Returns the sequential number of a row within a partition of a result set, 
starting at 1 for the first row in each partition.", _statement),
			new DefaultCompletionData("OPENQUERY", @"OPENQUERY ( linked_server ,'query' ) Executes the specified pass-through query on the specified linked server. This server is an OLE DB data source. 
OPENQUERY can be referenced in the FROM clause of a query as if it were a table name. OPENQUERY can also be referenced as the target table of an INSERT, UPDATE, or DELETE statement. 
This is subject to the capabilities of the OLE DB provider. Although the query may return multiple result sets, OPENQUERY returns only the first one.", _statement),
			new DefaultCompletionData("OPENROWSET", @"OPENROWSET 
( { 'provider_name' , { 'datasource' ; 'user_id' ; 'password' 
   | 'provider_string' } 
   , {   [ catalog. ] [ schema. ] object 
       | 'query' 
     } 
   | BULK 'data_file' , 
       { FORMATFILE = 'format_file_path' [ <bulk_options> ]
       | SINGLE_BLOB | SINGLE_CLOB | SINGLE_NCLOB }
} ) 

<bulk_options> ::=
   [ , CODEPAGE = { 'ACP' | 'OEM' | 'RAW' | 'code_page' } ] 
   [ , ERRORFILE = 'file_name' ]
   [ , FIRSTROW = first_row ] 
   [ , LASTROW = last_row ] 
   [ , MAXERRORS = maximum_errors ] 
   [ , ROWS_PER_BATCH = rows_per_batch ]
   [ , ORDER ( { column [ ASC | DESC ] } [ ,...n ] ) [ UNIQUE ]", _statement),
			new DefaultCompletionData("OPENXML", @"OPENXML( idoc int [ in] , rowpattern nvarchar [ in ] , [ flags byte [ in ] ] ) 
[ WITH ( SchemaDeclaration | TableName ) ] OPENXML provides a rowset view over an XML document. Because OPENXML is a rowset provider, OPENXML can be used in Transact-SQL statements in which rowset providers such as a table, 
view, or the OPENROWSET function can appear.
", _statement),
			new DefaultCompletionData("OPENDATASOURCE", @"OPENDATASOURCE ( provider_name, init_string ) Provides ad hoc connection information as part of a four-part object name without using a linked server name.", _statement),
			new DefaultCompletionData("ASCII", @"ASCII ( character_expression ) Returns the ASCII code value of the leftmost character of a character expression.", _statement),
			new DefaultCompletionData("CHAR", @"CHAR ( integer_expression ) Converts an int ASCII code to a character.", _statement),
			new DefaultCompletionData("CHARINDEX", @"CHARINDEX ( expression1 ,expression2 [ , start_location ] ) Searches expression2 for expression1 and returns its starting position if found. 
The search starts at start_location.", _statement),
			new DefaultCompletionData("DIFFERENCE", @"DIFFERENCE ( character_expression , character_expression ) Returns an integer value that indicates the difference between the SOUNDEX values of two character expressions.", _statement),
			new DefaultCompletionData("LEFT", @"LEFT ( character_expression , integer_expression ) Returns the left part of a character string with the specified number of characters.", _statement),
			new DefaultCompletionData("LEN", @"LEN ( string_expression ) Returns the number of characters of the specified string expression, excluding trailing blanks.", _statement),
			new DefaultCompletionData("LOWER", @"LOWER ( character_expression ) Returns a character expression after converting uppercase character data to lowercase.", _statement),
			new DefaultCompletionData("LTRIM", @"LTRIM ( character_expression ) Returns a character expression after it removes leading blanks.", _statement),
			new DefaultCompletionData("NCHAR", @"NCHAR ( integer_expression ) Returns the Unicode character with the specified integer code, as defined by the Unicode standard.", _statement),
			new DefaultCompletionData("PATINDEX", @"PATINDEX ( '%pattern%' , expression ) Returns the starting position of the first occurrence of a pattern in a specified expression, or zeros if the pattern is not found, 
on all valid text and character data types.", _statement),
			new DefaultCompletionData("QUOTENAME", @"QUOTENAME ( 'character_string' [ , 'quote_character' ] )  Returns a Unicode string with the delimiters added to make the input string a valid SQL Server delimited identifier.", _statement),
			new DefaultCompletionData("REPLACE", @"REPLACE ( string_expression , string_pattern , string_replacement ) Replaces all occurrences of a specified string value with another string value.", _statement),
			new DefaultCompletionData("REPLICATE", @"REPLICATE ( string_expression ,integer_expression ) Repeats a string value a specified number of times.", _statement),
			new DefaultCompletionData("REVERSE", @"REVERSE ( string_expression ) Returns the reverse of a string value.", _statement),
			new DefaultCompletionData("REVERT", @"REVERT [ WITH COOKIE = @varbinary_variable ] Switches the execution context back to the caller of the last EXECUTE AS statement.", _statement),
			new DefaultCompletionData("RIGHT", @"RIGHT ( character_expression , integer_expression ) Returns the right part of a character string with the specified number of characters.", _statement),
			new DefaultCompletionData("RTRIM", @"RTRIM ( character_expression ) Returns a character string after truncating all trailing blanks.", _statement),
			new DefaultCompletionData("SOUNDEX", @"SOUNDEX ( character_expression ) Returns a four-character (SOUNDEX) code to evaluate the similarity of two strings.", _statement),
			new DefaultCompletionData("SPACE", @"SPACE ( integer_expression ) Returns a string of repeated spaces.", _statement),
			new DefaultCompletionData("STR", @"STR ( float_expression [ , length [ , decimal ] ] ) ", _statement),
			new DefaultCompletionData("STUFF", @"STUFF ( character_expression , start , length ,character_expression )", _statement),
			new DefaultCompletionData("SUBSTRING", @"SUBSTRING ( value_expression , start_expression , length_expression )", _statement),
			new DefaultCompletionData("UNICODE", @"UNICODE ( 'ncharacter_expression' ) Returns the integer value, as defined by the Unicode standard, 
for the first character of the input expression.", _statement),
			new DefaultCompletionData("UPPER", @"UPPER ( character_expression ) Returns a character expression with lowercase character data converted to uppercase.", _statement),
			new DefaultCompletionData("NEWID", @"NEWID ( ) Creates a unique value of type uniqueidentifier.", _statement),
			new DefaultCompletionData("ISNUMERIC", @"ISNUMERIC ( expression )Determines whether an expression is a valid numeric type ", _statement),
			new DefaultCompletionData("ISNULL", @"ISNULL ( check_expression , replacement_value ) Replaces NULL with the specified replacement value.", _statement),
			new DefaultCompletionData("@@ROWCOUNT", @"@@ROWCOUNT Returns the number of rows affected by the last statement", _statement),
			new DefaultCompletionData("@@IDENTITY", @"@@IDENTITY Is a system function that returns the last-inserted identity value.", _statement),
		};
	}
}
