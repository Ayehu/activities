using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Management;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string SrcHostName;
        public string SrcUserName;
        public string SrcPassword;
        public string SrcPath;
        public string DstHostName;
        public string DstUserName;
        public string DstPassword;
        public string DstPath;

        public ICustomActivityResult Execute()

        {

            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            if (File.Exists(SrcPath) == false)
            {
                throw new Exception("File not found");
            }

            bool hidden = false;
            FileInfo tempf = new FileInfo(SrcPath);
            if (tempf.Attributes == FileAttributes.Hidden)
            {
                hidden = true;
            }

            long srcFileSize = GetFileSize(SrcPath);

            if (!Path.HasExtension(DstPath))
            {
                DstPath = Path.Combine(DstPath, Path.GetFileName(SrcPath));
            }
            //try
            //{
            File.Move(SrcPath, DstPath);
            //}
            //catch( Exception ex)
            //{
            //throw new Exception("DstPath: " + DstPath + " SrcPath: " + SrcPath + "   " + ex.Message);
            //}

            if (File.Exists(DstPath))
            {
                if (srcFileSize == GetFileSize(DstPath))
                {
                    FileInfo tempf2 = new FileInfo(DstPath);
                    if (hidden)
                    {
                        tempf2.Attributes = FileAttributes.Hidden;
                    }
                    dt.Rows.Add("Success");
                }
                else
                {
                    throw new Exception("Failure");
                }
            }
            else
            {
                throw new Exception("Failure");
            }
            return this.GenerateActivityResult(dt);
        }
        private long GetFileSize(string MyFilePath)
        {
            FileInfo MyFile = new FileInfo(MyFilePath);
            long FileSize = MyFile.Length;
            return FileSize;
        }



    }

}
