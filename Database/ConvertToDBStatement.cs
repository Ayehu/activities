using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
//First, as others have pointed out, unless you're using 12.1, both varchar2 and nvarchar2 data types are limited in SQL to 4000 bytes. In PL/SQL, they're limited to 32767. In 12.1, you can increase the SQL limit to 32767 using the MAX_STRING_SIZE parameter.

namespace Ayehu.Sdk.ActivityCreation
{
   public class  ActivityClass: IActivity
     {
      public string DbType;
      public string TableName;
      public string ResultSetData;
      public string SchemaName;
      public string TableSpace;

      public ICustomActivityResult Execute()

                  {
                      DataTable dt = new DataTable("resultSet");
          
                      if (string.IsNullOrEmpty(DbType))
                      {
                          throw new Exception("Database type string is empty");
                      }
                      if (string.IsNullOrEmpty(TableName))
                      {
                          throw new Exception("Table name string is empty");
                      }
                      if (string.IsNullOrEmpty(ResultSetData))
                      {
                          throw new Exception("Result set string is empty");
                      }
          
          			//bool test = true;
          			//if(test)
          			//{
          			//    dt.Columns.Add("Result");
                      //    dt.Rows.Add("Here the result: " + ResultSetData);
          			//}
          			//else
          			//{ //....
                      using (StringReader srXMLtext = new System.IO.StringReader(ResultSetData))
                      {
                          DataTable dtTemp = new DataTable();
                          dtTemp.ReadXml(srXMLtext);
          
                          string queryResult = string.Empty;
          
                          switch (DbType)
                          {
                              case "TSQL":
                                  queryResult = GetTsqlQuery(dtTemp, TableName);
                                  break;
                              case "MySql":
                                  queryResult = GetMySqlQuery(dtTemp, TableName);
                                  break;
                              case "Oracle":
                                  queryResult = GetOracleQuery(dtTemp, TableName);
                                  break;
                              case "DB2":
                                  if (string.IsNullOrEmpty(SchemaName))
                                  {
                                      throw new Exception("Schema name string is empty");
                                  }
                                  if (string.IsNullOrEmpty(TableSpace))
                                  {
                                      throw new Exception("Table space name string is empty");
                                  }
                                  queryResult = GetDB2Query(dtTemp, TableName, SchemaName, TableSpace);
                                  break;
                              default:
                                  break;
                          }
                          if (!string.IsNullOrEmpty(queryResult))
                          {
                              dt.Columns.Add("Query");
                              dt.Rows.Add(queryResult);
                          }               
                      }
          			//} //....
          
       return this.GenerateActivityResult(dt);
                  }
          
                  private string GetTsqlQuery(DataTable dtTemp, string tableName)
                  {
                      string queryResult = string.Empty;
          
                      if (dtTemp.Columns.Count > 0)
                      {
                          StringBuilder sbQuery = new StringBuilder();
                          StringBuilder sbInsertQuery = new StringBuilder();
          
                          //sbQuery.Append("CREATE TABLE Persons(PersonID int,LastName varchar(255),FirstName varchar(255),Address varchar(255),City varchar(255));");
                          //[nvarchar](max)
          
                          sbQuery.Append("CREATE TABLE ");
                          sbQuery.Append(tableName).Append("(");
          
                          sbInsertQuery.Append("Insert into [dbo].[").Append(tableName).Append("](");
          
                          bool columnsFirstEntry = true;
                          foreach (DataColumn dc in dtTemp.Columns)
                          {
                              if (!columnsFirstEntry)
                              {
                                  sbQuery.Append(",");
                                  sbInsertQuery.Append(",");
                              }
                              else
                              {
                                  columnsFirstEntry = false;
                              }
                              sbQuery.Append(dc.ColumnName).Append(" nvarchar(max)");
                              sbInsertQuery.Append(dc.ColumnName);
                          }                
                          sbQuery.Append(");");
                          sbInsertQuery.Append(")Values(");
          
                          if (dtTemp.Rows.Count > 0)
                          {
                              
                              foreach (DataRow dr in dtTemp.Rows)
                              {
                                  StringBuilder sbInsertQueryNext = new StringBuilder(sbInsertQuery.ToString());
                                  bool rowsFirstEntry = true;
                                  for (int i = 0; i < dtTemp.Columns.Count; i++)
                                  {
                                      if (!rowsFirstEntry)
                                      {
                                          sbInsertQueryNext.Append(",");
                                      }
                                      else
                                      {
                                          rowsFirstEntry = false;
                                      }
                                      sbInsertQueryNext.Append("'").Append(dr[i].ToString()).Append("'");
                                  }
                                  sbInsertQueryNext.Append(");");
                                  sbQuery.Append(sbInsertQueryNext);
                              } 
                          }
                          queryResult = sbQuery.ToString();
                      }
                      return queryResult;
                  }
                  private string GetMySqlQuery(DataTable dtTemp, string tableName)
                  {
                      string queryResult = string.Empty;
          
                      if (dtTemp.Columns.Count > 0)
                      {
                          StringBuilder sbQuery = new StringBuilder();
                          StringBuilder sbInsertQuery = new StringBuilder();
          
                          //sbQuery.Append("CREATE TABLE Persons(PersonID int,LastName varchar(255),FirstName varchar(255),Address varchar(255),City varchar(255));");
                          //[nvarchar](max)
          
                          sbQuery.Append("CREATE TABLE ");
                          sbQuery.Append(tableName).Append("(");
          
                          sbInsertQuery.Append("Insert into ").Append(tableName).Append("(");
          
                          bool columnsFirstEntry = true;
                          foreach (DataColumn dc in dtTemp.Columns)
                          {
                              if (!columnsFirstEntry)
                              {
                                  sbQuery.Append(",");
                                  sbInsertQuery.Append(",");
                              }
                              else
                              {
                                  columnsFirstEntry = false;
                              }
                              sbQuery.Append(dc.ColumnName).Append(" nvarchar(4000)");
                              sbInsertQuery.Append(dc.ColumnName);
                          }
                          sbQuery.Append(");");
                          sbInsertQuery.Append(")Values(");
          
                          if (dtTemp.Rows.Count > 0)
                          {
          
                              foreach (DataRow dr in dtTemp.Rows)
                              {
                                  StringBuilder sbInsertQueryNext = new StringBuilder(sbInsertQuery.ToString());
                                  bool rowsFirstEntry = true;
                                  for (int i = 0; i < dtTemp.Columns.Count; i++)
                                  {
                                      if (!rowsFirstEntry)
                                      {
                                          sbInsertQueryNext.Append(",");
                                      }
                                      else
                                      {
                                          rowsFirstEntry = false;
                                      }
                                      sbInsertQueryNext.Append("'").Append(dr[i].ToString()).Append("'");
                                  }
                                  sbInsertQueryNext.Append(");");
                                  sbQuery.Append(sbInsertQueryNext);
                              }
                          }
                          queryResult = sbQuery.ToString();
                      }
                      return queryResult;
                  }
                  private string GetOracleQuery(DataTable dtTemp, string tableName)
                  {
                      string queryResult = string.Empty;
          
                      if (dtTemp.Columns.Count > 0)
                      {
                          StringBuilder sbQuery = new StringBuilder();
                          StringBuilder sbInsertQuery = new StringBuilder();
          
          //First, as others have pointed out, unless you're using 12.1, both varchar2 and nvarchar2 data types are limited in SQL to 4000 bytes. In PL/SQL, they're limited to 32767. In 12.1, you can increase the SQL limit to 32767 using the MAX_STRING_SIZE parameter.
          
          //Second, unless you are working with a legacy database that uses a non-Unicode character set that cannot be upgraded to use a Unicode character set, you would want to avoid nvarchar2 and nchar data types in Oracle. In SQL Server, you use nvarchar when you want to store Unicode data. In Oracle, the preference is to use varchar2 in a database whose character set supports Unicode (generally AL32UTF8) when you want to store Unicode data.
          
          //If you store Unicode data in an Oracle NVARCHAR2 column, the national character set will be used-- this is almost certainly AL16UTF16 which means that every character requires at least 2 bytes of storage. A NVARCHAR2(4000), therefore, probably can't store more than 2000 characters. If you use a VARCHAR2 column, on the other hand, you can use a variable width Unicode character set (AL32UTF8) in which case English characters generally require just 1 byte, most European characters require 2 bytes, and most Asian characters require 3 bytes (this is, of course, just a generalization). That is generally going to allow you to store substantially more data in a VARCHAR2 column.
          
          //If you do need to store more than 4000 bytes of data and you're using Oracle 11.2 or later, you'd have to use a LOB data type (CLOB or NCLOB).
          
                          //sbQuery.Append("CREATE TABLE Persons(PersonID int,LastName varchar(255),FirstName varchar(255),Address varchar(255),City varchar(255));");
                          //[nvarchar](max)
          
                          sbQuery.Append("CREATE TABLE ");
                          sbQuery.Append(tableName).Append("(");
          
                          sbInsertQuery.Append("Insert into ").Append(tableName).Append("(");
          
                          bool columnsFirstEntry = true;
                          foreach (DataColumn dc in dtTemp.Columns)
                          {
                              if (!columnsFirstEntry)
                              {
                                  sbQuery.Append(",");
                                  sbInsertQuery.Append(",");
                              }
                              else
                              {
                                  columnsFirstEntry = false;
                              }
                              sbQuery.Append(dc.ColumnName).Append(" NCLOB");
                              sbInsertQuery.Append(dc.ColumnName);
                          }
                          sbQuery.Append(");");
                          sbInsertQuery.Append(")Values(");
          
                          if (dtTemp.Rows.Count > 0)
                          {
          
                              foreach (DataRow dr in dtTemp.Rows)
                              {
                                  StringBuilder sbInsertQueryNext = new StringBuilder(sbInsertQuery.ToString());
                                  bool rowsFirstEntry = true;
                                  for (int i = 0; i < dtTemp.Columns.Count; i++)
                                  {
                                      if (!rowsFirstEntry)
                                      {
                                          sbInsertQueryNext.Append(",");
                                      }
                                      else
                                      {
                                          rowsFirstEntry = false;
                                      }
                                      sbInsertQueryNext.Append("'").Append(dr[i].ToString()).Append("'");
                                  }
                                  sbInsertQueryNext.Append(");");
                                  sbQuery.Append(sbInsertQueryNext);
                              }
                          }
                          queryResult = sbQuery.ToString();
                      }
                      return queryResult;
                  }
                  private string GetDB2Query(DataTable dtTemp, string tableName, string SchemaName, string TableSpace)
                  {
                      string queryResult = string.Empty;
          
                      //NVARCHAR and NCLOB were introduced in DB2 9.7 with Fixpack 2.
          
                      //db2 create table <schema_name>.<table_name>
                      //(column_name column_type....) in <tablespace_name>  
          
                      if (dtTemp.Columns.Count > 0)
                      {
                          StringBuilder sbQuery = new StringBuilder();
                          StringBuilder sbInsertQuery = new StringBuilder();
          
                          sbQuery.Append("CREATE TABLE ").Append(SchemaName).Append(".");
                          sbQuery.Append(tableName).Append("(");
          
                          sbInsertQuery.Append("Insert into ").Append(SchemaName).Append(".").Append(tableName).Append("(");
          
                          bool columnsFirstEntry = true;
                          foreach (DataColumn dc in dtTemp.Columns)
                          {
                              if (!columnsFirstEntry)
                              {
                                  sbQuery.Append(",");
                                  sbInsertQuery.Append(",");
                              }
                              else
                              {
                                  columnsFirstEntry = false;
                              }
                              sbQuery.Append(dc.ColumnName).Append(" nvarchar(4000)");
                              sbInsertQuery.Append(dc.ColumnName);
                          }
                          sbQuery.Append(") IN ").Append(TableSpace).Append(";");
                          sbInsertQuery.Append(")Values(");
          
                          if (dtTemp.Rows.Count > 0)
                          {
          
                              foreach (DataRow dr in dtTemp.Rows)
                              {
                                  StringBuilder sbInsertQueryNext = new StringBuilder(sbInsertQuery.ToString());
                                  bool rowsFirstEntry = true;
                                  for (int i = 0; i < dtTemp.Columns.Count; i++)
                                  {
                                      if (!rowsFirstEntry)
                                      {
                                          sbInsertQueryNext.Append(",");
                                      }
                                      else
                                      {
                                          rowsFirstEntry = false;
                                      }
                                      sbInsertQueryNext.Append("'").Append(dr[i].ToString()).Append("'");
                                  }
                                  sbInsertQueryNext.Append(");");
                                  sbQuery.Append(sbInsertQueryNext);
                              }
                          }
                          queryResult = sbQuery.ToString();
                      }
                      return queryResult;
                  }
              }
          }
