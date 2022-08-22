using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using A = DocumentFormat.OpenXml.OpenXmlAttribute;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using System.Net.Http;
using System.Web.Http;

namespace ImportExportTest.Models
{
    public class DataImportHelper
    {
        //Adjusted by Paulo
        private string fileName { get; set; }
        public DataImportHelper(string _fileName)
        {
            fileName = _fileName;
        }
        // Written by me .....
        // well mostly except the first 10 or so lines
        public DataTable OpenXLSXFile()
        {
            DataTable dataTable = new DataTable();
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                int maxRowCount = 0;
                int maxCellCount = 0;

                int maxRowIndex = 0;


                // Get the MAX Row count and MAX Cell count 
                // NOTE: this is NOT as simple as zipping thru all the Rows and Cols
                // you have to note the current cell vs the cell reference and pad with empty cells till u reach the matching current cell = cell reference
                // same for Rows later.

                // Rows are the actual number of rows with data ie blank rows are NOT kept in the rows collection
                foreach (Row row in rows)
                {
                    int cellIndex = 0;
                    string columnName = "";

                    maxRowCount++;
                    maxRowIndex = int.Parse(row.RowIndex.ToString());

                    foreach (Cell cell in row)
                    {
                        cellIndex++;

                        int thisIndex = GetColumnIndex(cell.CellReference);

                        // Add blank columns till u get to the current column
                        if (thisIndex > maxCellCount)
                        {
                            while (cellIndex != thisIndex)
                            {
                                columnName = GetColumnName(cellIndex);
                                if (!dataTable.Columns.Contains(columnName))
                                    dataTable.Columns.Add(columnName);
                                cellIndex++;
                            }
                        }

                        // Now only add the current column
                        if (cellIndex > maxCellCount)
                        {
                            columnName = GetColumnName(cellIndex);

                            dataTable.Columns.Add(columnName);

                            maxCellCount = cellIndex;
                        }
                    }

                    cellIndex = 0;
                }

                Debug.WriteLine($"{maxRowCount}:{maxCellCount}");
                Debug.WriteLine($"{maxRowIndex}");

                int currentRow = 1;

                foreach (Row row in rows)
                {
                    DataRow dataRow = dataTable.NewRow();

                    // Add blank rowss till u get to the current row
                    while (currentRow != row.RowIndex)
                    {
                        currentRow++;
                        dataTable.Rows.Add(dataRow);
                        dataRow = dataTable.NewRow();
                    }

                    // Add the actual row with data
                    foreach (Cell cell in row)
                    {
                        int index = GetCellReferenceIndex(cell);

                        dataRow[index] = GetCellValue(spreadSheetDocument, cell);
                    }

                    currentRow++;
                    dataTable.Rows.Add(dataRow);
                }

            }

            return dataTable;
        }

        // Stolen .... too tired to calc this 
        public string GetColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName =
                    Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }

            return columnName;
        }

        // Stolen .... too tired to calc this 
        public int GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return 0; // null;
            }

            //remove digits
            string columnReference = Regex.Replace(cellReference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }

            //the result is zero based so return columnnumber + 1 for a 1 based answer
            //this will match Excel's COLUMN function
            return columnNumber + 1;
        }

        // Get the actual rowIndex of this cell 
        // yes this was written by me 
        int GetCellReferenceIndex(Cell cell)
        {
            string alphas = "";
            string numbers = "";
            string CR = cell.CellReference;
            int index = 0;

            CR = CR.ToUpper();

            foreach (char c in CR)
            {
                if (IsAlpha(c))
                    alphas += c;

                if (IsNumber(c))
                    numbers += c;
            }

            int rowindex = 0;
            index = 1;
            foreach (char c in alphas.Reverse())
            {
                int i = c - 65;

                rowindex = (i * index) + rowindex;

                index++;
            }

            return rowindex;
        }

        // Is this an Alphabetical char 
        // yes this was written by me 
        bool IsAlpha(char c)
        {
            return c >= 65 && c <= 65 + 26 || c >= 97 && c <= 97 + 26;
        }

        // Is this an Numeric char
        // yes this was written by me 
        bool IsNumber(char c)
        {
            return c >= 0 && c <= 9;
        }


        // Stolen .... excel has weird stuff .... someone says this works 
        // altered to return ""  when cell is null otherwise ..... kaboom! 
        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue != null)
            {
                string value = cell.CellValue.InnerXml;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
                else
                {
                    return value;
                }
            }
            return "";
        }


        // Stolen
        // Useless Sheet info ... maybe useful later
        public string GetSheetInfo(string fileName)
        {
            StringBuilder SB = new StringBuilder();

            try
            {
                // Open file as read-only.
                using (SpreadsheetDocument mySpreadsheet = SpreadsheetDocument.Open(fileName, false))
                {
                    S sheets = mySpreadsheet.WorkbookPart.Workbook.Sheets;

                    // For each sheet, display the sheet information.
                    foreach (E sheet in sheets)
                    {
                        foreach (A attr in sheet.GetAttributes())
                        {
                            SB.Append($"{attr.LocalName} : {attr.Value}\r\n");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = ex.Message
                });
            }

            return SB.ToString();
        }
        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (System.Reflection.PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name != null)
                    {
                        if (pro.Name == column.ColumnName)
                        {
                            if (!dr.IsNull(column.ColumnName))
                                pro.SetValue(obj, dr[column.ColumnName], null);
                        }
                        else
                            continue;
                    }
                }
            }
            return obj;
        }
    }
}