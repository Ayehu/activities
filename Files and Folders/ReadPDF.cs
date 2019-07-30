using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
//using System.Linq;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Path;
        public string FilePassword;

        public ICustomActivityResult Execute()

        {
            try
            {
                StringWriter sw = new StringWriter();
                DataTable dt = new DataTable("resultSet");
                dt.Columns.Add("Result", typeof(string));

                if (string.IsNullOrEmpty(Path))
                {
                    throw new Exception("File not found");
                }
                if (File.Exists(Path) == false)
                {
                    throw new Exception("File not found");
                }

                PdfLoadedDocument ldoc = null;
                if (string.IsNullOrEmpty(FilePassword))
                    ldoc = new PdfLoadedDocument(Path);
                else
                    ldoc = new PdfLoadedDocument(Path, FilePassword);

                // Loading Page collections
                PdfLoadedPageCollection loadedPages = ldoc.Pages;
                // Extract text from PDF document pages
                StringBuilder sb = new StringBuilder();
                foreach (PdfLoadedPage lpage in loadedPages)
                {
                    sb.Append(lpage.ExtractText());
                }


                /* Start Form */

                PdfLoadedForm pdfForm = ldoc.Form;
                bool found = false;
                if (pdfForm != null)
                {
                    if (pdfForm.Fields.Count > 0)
                    {
                        sb.AppendLine("");
                        sb.AppendLine("-------- Form Controls Values --------");
                    }
                    foreach (PdfLoadedField field in pdfForm.Fields)
                    {
                        string fname = field.Name;
                        string fValue = string.Empty;

                        try
                        {
                            if (field is PdfLoadedTextBoxField)
                            {
                                PdfLoadedTextBoxField textField = (field as PdfLoadedTextBoxField);
                                fValue = textField.Text;
                                found = true;
                            }
                            if (field is PdfLoadedCheckBoxField)
                            {
                                PdfLoadedCheckBoxField chbField = (field as PdfLoadedCheckBoxField);
                                fValue = chbField.Checked ? "checked" : string.Empty;
                                found = true;
                            }
                            if (field is PdfLoadedComboBoxField)
                            {
                                PdfLoadedComboBoxField listField = (field as PdfLoadedComboBoxField);
                                fValue = listField.SelectedValue;
                                found = true;
                            }
                            if (field is PdfLoadedRadioButtonListField)
                            {
                                PdfLoadedRadioButtonListField listField = (field as PdfLoadedRadioButtonListField);
                                fValue = listField.SelectedItem.Value;
                                found = true;
                            }
                            if (field is PdfLoadedListBoxField)
                            {
                                PdfLoadedListBoxField listField = (field as PdfLoadedListBoxField);
                                string[] arrValues = listField.SelectedValue;
                                if (arrValues != null)
                                {
                                    string allValues = string.Empty;
                                    foreach (string cValue in arrValues)
                                    {
                                        allValues += cValue + ";";
                                    }
                                    int lindex = allValues.LastIndexOf(";");
                                    if (lindex > -1)
                                    {
                                        fValue = allValues.Substring(0, lindex);
                                    }
                                    else
                                    {
                                        fValue = allValues;
                                    }

                                }
                                else
                                {
                                    fValue = string.Empty;
                                }
                                found = true;
                            }

                            if (found)
                            {
                                sb.AppendLine(string.Format("Field Name: {0} Value: {1}", fname, fValue));
                                found = false;
                            }
                            else
                            {
                                sb.AppendLine(string.Format("Field Name: {0} Error: {1}", fname, "The control type was not recognized."));
                            }
                        }
                        catch
                        {
                            sb.AppendLine(string.Format("Field Name: {0} Error: {1}", fname, "Failed to retrieve the value."));
                        }


                    }
                }
                /* End Form */

                dt.Rows.Add(sb.ToString());

                return this.GenerateActivityResult(dt);

            }
            catch
            {
                throw new Exception("Failed to process the file.");
            }

        }

    }

}
