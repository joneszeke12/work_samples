CREATE TABLE [dbo].[KV] (
 [ID] int
,[Key] varchar(32)
,[Value] int
);
GO

INSERT INTO 
	[dbo].[KV] 
VALUES
	 (1,'KEY1',1)
	,(2,'KEY1',2)
	,(3,'KEY2',10)
	,(4,'KEY2',20)
	,(5,'KEY2',30)
	,(6,'KEY3',6)
	,(7,'KEY3',12);
GO

/*
A stored procedure that creates a table dynamically with a unique identifier and columns of the integer data type.
I've used this type of procedure a few times to create fact tables with hundreds of foreign key columns.
*/
CREATE PROCEDURE [dbo].[usp_CreateDynamicTable]
AS
BEGIN
	DECLARE @sql nvarchar(MAX) = 
	'/*CREATE TABLE [dbo].[Table] (
		 ID int
		,%s
	 );*/'
	DECLARE @fields nvarchar(MAX) = 
	STUFF(
			(
				SELECT DISTINCT 
					',' + [KEY] + ' int'
				FROM 
					[dbo].[KV]
				FOR XML PATH(''), TYPE
			).value('.','nvarchar(MAX)'),
			1,
			0,
			''
	);
	SET @fields = RIGHT(@fields,LEN(@fields) - 1);
	DECLARE @stmnt nvarchar(MAX) = REPLACE(@sql,'%s',@fields);
	EXEC sp_executesql @stmnt;
END
GO
