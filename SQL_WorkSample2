/*
This is a SQL script that comes from existing code that I found on the internet.
I modified it to where it is able to send query output in an e-mail as a CSV file.
If the file went past a certain byte size, then the file was split and sent in 
multiple e-mails, to overcome e-mail server limitations. Not the most robust solution,
but it was a good challenge to undertake.
*/

--------------------------------------------------------
--  DDL for Package PKG_SEND_EMAIL
--------------------------------------------------------
CREATE OR REPLACE PACKAGE "PKG_SEND_EMAIL" AS

/*
Execute As SYS
@utlmail.sql
@prvtmail.plb
----------------------------------------------------------------------------
begin
  dbms_network_acl_admin.create_acl (
    acl         => 'utl_mail.xml',
    description => 'Allow mail to be send',
    principal   => 'ME', --Can be any schema used to set up database
    is_grant    => TRUE,
    privilege   => 'connect'
    );
    commit;
end;
---------------------------------------
begin
  dbms_network_acl_admin.add_privilege (
  acl       => 'utl_mail.xml',
  principal => 'ME', --Can be any schema used to set up database
  is_grant  => TRUE,
  privilege => 'resolve'
  );
  commit;
end;
---------------------------------------
begin
  dbms_network_acl_admin.assign_acl(
  acl  => 'utl_mail.xml',
  host => '[host]'
  );
  commit;
end;
*/
  FUNCTION agg_columns (
    agg_kind   IN NUMBER,
    tbl        IN VARCHAR2
  ) RETURN STRING;

  FUNCTION get_qry_lgnth (
    QRY_NAME IN VARCHAR2
  ) RETURN NUMBER;

  PROCEDURE SendEmail (
    From_Me     IN VARCHAR2,
    To_You      IN RECIPIENTS_LST,
    Subject     IN VARCHAR2,
    Body        IN VARCHAR2 DEFAULT NULL,
    QueryName   IN QUERY_LST DEFAULT NULL
  );

END PKG_SEND_EMAIL;
/
--------------------------------------------------------
--  DDL for Package Body PKG_SEND_EMAIL
--------------------------------------------------------

CREATE OR REPLACE PACKAGE BODY "PKG_SEND_EMAIL" AS

  FUNCTION agg_columns (
    agg_kind   IN NUMBER,
    tbl        IN VARCHAR2
  ) RETURN STRING IS

    v_string   VARCHAR2(5000 CHAR);
    delimit    VARCHAR2(30) := ' || ' || chr(39) || ',' || chr(39) || ' || ';
  BEGIN
    IF
      agg_kind = 1
    THEN
      SELECT
        LISTAGG(column_name,
        ',') WITHIN GROUP(
        ORDER BY
          COLUMN_ID
        )
      INTO
        v_string
      FROM
        all_tab_cols
      WHERE
        table_name = tbl;

    ELSIF agg_kind = 2 THEN
      SELECT
        LISTAGG(chr(39) || '"' || chr(39) || ' || "' || column_name || '" || ' || chr(39) || '"' || chr(39),
        delimit) WITHIN GROUP(
        ORDER BY
          COLUMN_ID
        )
      INTO
        v_string
      FROM
        all_tab_cols
      WHERE
        table_name = tbl;

    END IF;

    RETURN v_string;
  END agg_columns;

-----------------------------------

  FUNCTION get_qry_lgnth (
    QRY_NAME IN VARCHAR2
  ) RETURN NUMBER AS

    v_Byte_Cnt    NUMBER;
    v_SQL_Stmnt   VARCHAR2(5000 CHAR) := 'SELECT SUM(LENGTHB(' || AGG_COLUMNS(2,QRY_NAME) || '))  FROM ' || QRY_NAME;
  BEGIN
    EXECUTE IMMEDIATE v_SQL_Stmnt INTO
      v_Byte_Cnt;
    RETURN ROUND(v_Byte_Cnt / 1000000);
  END get_qry_lgnth;

----------------------------------------

  PROCEDURE SendEmail (
    From_Me     IN VARCHAR2,
    To_You      IN RECIPIENTS_LST,
    Subject     IN VARCHAR2,
    Body        IN VARCHAR2 DEFAULT NULL,
    QueryName   IN QUERY_LST DEFAULT NULL
  ) AS

    v_Mail_Host         VARCHAR2(30 CHAR) := 'forwarder.gm.com';
    v_Mail_Conn         utl_smtp.Connection;
    v_Boundary          VARCHAR2(50) := '----=*#abc1234321cba#*=';
    v_Recip_Lst         VARCHAR2(256 CHAR);
    v_SQL_Stmnt         VARCHAR2(5000 CHAR);
    v_Row               VARCHAR2(5000);
    v_Estim_MegaBytes   NUMBER;
    v_MaxBatches        NUMBER := NULL;
    v_RowCount          NUMBER := NULL;
    v_BatchSize         NUMBER := NULL;
    v_Accumulator       NUMBER := NULL;
    crlf                VARCHAR2(2) := chr(13) || chr(10);
    TYPE QueryCursRef IS REF CURSOR;
    QueryCurs           QueryCursRef;
  BEGIN
    IF
      QueryName IS NOT NULL
    THEN
      FOR i IN 1..To_You.COUNT LOOP
        v_Recip_Lst := v_Recip_Lst || To_You(i) || ';';
      END LOOP;

      FOR j IN 1..QueryName.COUNT LOOP
        v_Estim_MegaBytes := get_qry_lgnth(QueryName(j) );
        v_SQL_Stmnt := 'SELECT ' || agg_columns(2,QueryName(j) ) || '  FROM ' || QueryName(j);

        OPEN QueryCurs FOR v_SQL_Stmnt;

        IF
          v_Estim_MegaBytes <= 50
        THEN
          v_Mail_Conn := utl_smtp.Open_Connection(v_Mail_Host,25);
          utl_smtp.Helo(v_Mail_Conn,v_Mail_Host);
          utl_smtp.Mail(v_Mail_Conn,From_Me);
          FOR i IN 1..To_You.COUNT LOOP
            utl_smtp.Rcpt(v_Mail_Conn,To_You(i) );
          END LOOP;

          UTL_SMTP.open_data(v_Mail_Conn);
          UTL_SMTP.write_data(v_Mail_Conn,'Date: ' || TO_CHAR(SYSDATE,'DD-MON-YYYY HH24:MI:SS') || crlf);

          UTL_SMTP.write_data(v_Mail_Conn,'To: ' || v_Recip_Lst || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'From: ' || From_Me || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'Subject: ' || Subject || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'MIME-Version: 1.0' || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: multipart/mixed; boundary="' || v_Boundary || '"' || crlf || crlf);

          UTL_SMTP.write_data(v_Mail_Conn,'--' || v_Boundary || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: text/plain; charset="iso-8859-1"' || crlf || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,Body);
          UTL_SMTP.write_data(v_Mail_Conn,crlf || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'--' || v_Boundary || crlf);
          UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: text/plain' || '; name="' || QueryName(j) || '"' || crlf);

          UTL_SMTP.write_data(v_Mail_Conn,'Content-Disposition: attachment; filename="' || QueryName(j) || '.csv' || '"' || crlf || crlf);

          UTL_SMTP.write_data(v_mail_conn,agg_columns(1,QueryName(j) ) );
          UTL_SMTP.write_data(v_mail_conn,crlf);
          LOOP
            FETCH QueryCurs INTO v_Row;
            EXIT WHEN QueryCurs%NOTFOUND;
            UTL_SMTP.write_data(v_mail_conn,v_Row);
            UTL_SMTP.write_data(v_mail_conn,crlf);
          END LOOP;

          CLOSE QueryCurs;
          UTL_SMTP.write_data(v_mail_conn,'--' || v_boundary || '--' || crlf);
          UTL_SMTP.Close_Data(v_Mail_Conn);
          UTL_SMTP.quit(v_Mail_Conn);
        ELSE
          EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM ' || QueryName(j) INTO
            v_RowCount;
          v_MaxBatches := CEIL(v_Estim_MegaBytes / 50) + 1;
          v_BatchSize := CEIL(v_RowCount / v_MaxBatches);
          FOR k IN 1..v_MaxBatches LOOP
            v_BatchSize := v_BatchSize * k;
            v_Mail_Conn := utl_smtp.Open_Connection(v_Mail_Host,25);
            utl_smtp.Helo(v_Mail_Conn,v_Mail_Host);
            utl_smtp.Mail(v_Mail_Conn,From_Me);
            FOR i IN 1..To_You.COUNT LOOP
              utl_smtp.Rcpt(v_Mail_Conn,To_You(i) );
            END LOOP;

            UTL_SMTP.open_data(v_Mail_Conn);
            UTL_SMTP.write_data(v_Mail_Conn,'Date: ' || TO_CHAR(SYSDATE,'DD-MON-YYYY HH24:MI:SS') || crlf);

            UTL_SMTP.write_data(v_Mail_Conn,'To: ' || v_Recip_Lst || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'From: ' || From_Me || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'Subject: ' || Subject || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'MIME-Version: 1.0' || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: multipart/mixed; boundary="' || v_Boundary || '"' || crlf || crlf);

            UTL_SMTP.write_data(v_Mail_Conn,'--' || v_Boundary || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: text/plain; charset="iso-8859-1"' || crlf || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,Body);
            UTL_SMTP.write_data(v_Mail_Conn,crlf || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'--' || v_Boundary || crlf);
            UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: text/plain' || '; name="' || QueryName(j) || '_Part_' || k || '"' || crlf);

            UTL_SMTP.write_data(v_Mail_Conn,'Content-Disposition: attachment; filename="' || QueryName(j) || '_Part_' || k || '.csv' || '"' || crlf || crlf
);

            UTL_SMTP.write_data(v_mail_conn,AGG_COLUMNS(1,QueryName(j) ) );
            UTL_SMTP.write_data(v_mail_conn,crlf);
            LOOP
              FETCH QueryCurs INTO v_Row;
              EXIT WHEN QueryCurs%ROWCOUNT = v_BatchSize OR QueryCurs%NOTFOUND;
              UTL_SMTP.write_data(v_mail_conn,v_Row);
              UTL_SMTP.write_data(v_mail_conn,crlf);
            END LOOP;

            UTL_SMTP.write_data(v_mail_conn,'--' || v_boundary || '--' || crlf);
            UTL_SMTP.Close_Data(v_Mail_Conn);
            UTL_SMTP.quit(v_Mail_Conn);
            IF
              QueryCurs%NOTFOUND
            THEN
              EXIT;
            ELSE
              v_BatchSize := v_BatchSize / k;
            END IF;
          END LOOP;

          CLOSE QueryCurs;
        END IF;

      END LOOP;

    ELSE
      v_Mail_Conn := utl_smtp.Open_Connection(v_Mail_Host,25);
      utl_smtp.Helo(v_Mail_Conn,v_Mail_Host);
      utl_smtp.Mail(v_Mail_Conn,From_Me);
      FOR i IN 1..To_You.COUNT LOOP
        v_Recip_Lst := v_Recip_Lst || To_You(i) || ';';
        utl_smtp.Rcpt(v_Mail_Conn,To_You(i) );
      END LOOP;

      UTL_SMTP.open_data(v_Mail_Conn);
      UTL_SMTP.write_data(v_Mail_Conn,'Date: ' || TO_CHAR(SYSDATE,'DD-MON-YYYY HH24:MI:SS') || crlf);

      UTL_SMTP.write_data(v_Mail_Conn,'To: ' || v_Recip_Lst || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,'From: ' || From_Me || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,'Subject: ' || Subject || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,'MIME-Version: 1.0' || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: multipart/mixed; boundary="' || v_Boundary || '"' || crlf || crlf);

      UTL_SMTP.write_data(v_Mail_Conn,'--' || v_Boundary || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,'Content-Type: text/plain; charset="iso-8859-1"' || crlf || crlf);
      UTL_SMTP.write_data(v_Mail_Conn,Body);
      UTL_SMTP.write_data(v_Mail_Conn,crlf || crlf);
      UTL_SMTP.write_data(v_mail_conn,'--' || v_boundary || '--' || crlf);
      UTL_SMTP.Close_Data(v_Mail_Conn);
      UTL_SMTP.quit(v_Mail_Conn);
    END IF;
  EXCEPTION
    WHEN utl_smtp.Transient_Error OR utl_smtp.Permanent_Error THEN
      raise_application_error(-20000,'Unable to send mail.',TRUE);
  END;

END;
/
