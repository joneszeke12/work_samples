/*A stored procedure that creates a fact table with column names coming from another table.*/

ALTER PROCEDURE [utility].[usp_CreateSurveyTable]
AS
BEGIN
	DECLARE @sql nvarchar(MAX) = 
	'/*CREATE TABLE [base].[Survey] (
		%s
	 );*/'
	DECLARE @fields nvarchar(MAX) = 
	STUFF(
			(
				SELECT DISTINCT 
					[Variable] + ' int,'
				FROM 
					[base].QuestionMapping
				FOR XML PATH(''), TYPE
			).value('.','nvarchar(MAX)'),
			1,
			0,
			''
	);
	SET @fields = LEFT(@fields,LEN(@fields) - 1);
	DECLARE @stmnt nvarchar(MAX) = REPLACE(@sql,'%s',@fields);
	EXEC sp_executesql @stmnt;
END
GO
