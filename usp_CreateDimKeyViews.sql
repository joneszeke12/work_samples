CREATE TABLE [dbo].[KV] (
 [Id] int
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
A stored procedure that builds views out of a table with a key and value structure. I've used this several times
for building views that function as dimensions.
*/

CREATE PROCEDURE [dbo].[usp_CreateDimKeyViews] as
BEGIN
	DECLARE curs CURSOR FOR SELECT DISTINCT [Key] FROM [dbo].[KV];
	DECLARE @sql nvarchar(max) =
	'CREATE VIEW dbo.Dim%s AS 
		SELECT 
			 [Value] [Value]
			,[Key] [Label] --Alternatively, you could define a label to use in the KV table.
		FROM
			[dbo].[KV]
		WHERE
			[Key] = ''%s''
		UNION ALL
		--Default row
		SELECT 999999999,''N/A'';';
	OPEN curs;
	DECLARE @var nvarchar(32);
	DECLARE @stmnt nvarchar(512);
	FETCH NEXT FROM curs INTO @var;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @stmnt = REPLACE(@sql,'%s',@var);
			EXEC sp_executesql @stmnt;
			FETCH NEXT FROM curs INTO @var;
		END
	CLOSE curs;
	DEALLOCATE curs;
END
GO


