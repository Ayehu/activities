using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using IBM.Data.DB2;
using System.Data;
using System.IO;

namespace Ayehu.Sdk.ActivityCreation
{
   public class  ActivityClass: IActivity
     {
      public string Query;
      public string ConnectionString;
      public string UserName;
      public string Password;
      public string TimeInSeconds;

      public ICustomActivityResult Execute()

                  {
                      DB2Connection con = null;
          
                      DataTable dt = new DataTable("resultSet");
                      dt.Columns.Add("rowsAffected", typeof(string));
          
                      int rowsAffected = 0;
          
                      try
                      {
                          DB2ConnectionStringBuilder cnb = new DB2ConnectionStringBuilder(ConnectionString);
                          if (!string.IsNullOrEmpty(UserName))
                          {
                              cnb.UserID = UserName;
                              cnb.Password = Password;
                          }
                          con = new DB2Connection(cnb.ConnectionString);
          
                          con.Open();
          
                          using (DB2Command command = new DB2Command(Query, con))
                          {
                              command.CommandType = System.Data.CommandType.Text;
                              command.CommandTimeout = Convert.ToInt32(TimeInSeconds);
                              rowsAffected = command.ExecuteNonQuery();
                          }
          
                          dt.Rows.Add(rowsAffected);
          
       return this.GenerateActivityResult(dt);
                      }
                      finally
                      {
                          if (con != null)
                          {
                              con.Close();
                              con.Dispose();
                          }
          
                          con = null;
                      }
          
                  }
              }
          }
