using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace HtmlTableGenerator
{
    class HtmlTableGenerator
    {
        #region Constructor
        public HtmlTableGenerator(string inputFilePath)
        {
            m_strInputFilePath = inputFilePath;         
            m_arrTableLines = null;
            m_arrInvertedTableLines = null;

            m_iTableWidth = 0;
            m_iTableHeight = 0;

            m_ColumnMarkers = null;
            m_RowMarkers = null;

            m_iTableColumnCount = 0;
            m_iTableRowCount = 0;

            m_htmlTable = new HtmlTable();
        }

        #endregion // Constructor

        #region public methods
        /// <Summary>
        ///     Opens the text file, parses out the table, calculates numColumns and numRows in table. 
        /// </Summary>
        /// <arguments>
        ///     No Arguments
        /// </arguments>
        /// <returns>
        ///     void
        /// </returns>
        /// <throws>
        ///     Exception or InputError
        /// </throws>
        public void LoadTableStringsFromFile()
        {
            // Read the file see if it matches table begin and end to identify the exact lines that has the table.
            System.IO.StreamReader inputFile = null;
            try
            {
                inputFile = new System.IO.StreamReader(this.m_strInputFilePath);
            }
            catch (Exception)
            {
                throw new HtmlTableGeneratorErrors.InputError(HtmlTableGeneratorErrors.HtmlTableGeneratorErrorCodes.unable_to_open_file, m_strInputFilePath);
            }
            // unable to open input file.
            if (inputFile == null) 
            {
                throw new HtmlTableGeneratorErrors.InputError(HtmlTableGeneratorErrors.HtmlTableGeneratorErrorCodes.unable_to_open_file, m_strInputFilePath);
            }

            // load the file one line at a time.
            string line;
            m_iTableColumnCount = 0;
            line = inputFile.ReadLine();
            List<string> lines = new List<string>();
            while (line != null)
            {
                string currentLine = line.Trim();
                lines.Add(currentLine);
                line = inputFile.ReadLine();

            }
            inputFile.Close();

            // Parse the input lines, and calculate width, height, ColumnCount, and columnMarkers
            this.m_iTableColumnCount = ParseTableStrings(lines, out this.m_iTableWidth, out this.m_iTableHeight, out this.m_arrTableLines, out this.m_ColumnMarkers);
            //invert the table
            invertTheTable();
            // Parse the inverted table to get the RowCount, and RowMarker
            int tableWidth, tableHeight = 0;
            this.m_iTableRowCount = ParseTableStrings(this.m_arrInvertedTableLines, out tableWidth, out tableHeight, out this.m_arrInvertedTableLines, out this.m_RowMarkers);
        }

        /// <Summary>
        ///     Build the Table Structure 
        /// </Summary>
        /// <arguments>
        ///     No Arguments
        /// </arguments>
        /// <returns>
        ///     void
        /// </returns>
        public void BuildTheTable()
        {
            m_htmlTable = new HtmlTable();
            m_htmlTable.m_tableRows = new List<HtmlTableRow>();

            foreach (int rowMarker in m_RowMarkers)
            {
                if (rowMarker != this.m_iTableHeight-1)
                {
                    string rowString = m_arrTableLines[rowMarker];
                    HtmlTableRow tableRow = new HtmlTableRow();
                    tableRow.m_tableCells = new List<HtmlTableCell>();
                    m_htmlTable.m_tableRows.Add(tableRow);

                    int colIndex = rowString.IndexOf("+");
                    while (colIndex >= 0 && colIndex < this.m_iTableWidth)
                    {
                        int cellStartColumn = colIndex;
                        int cellEndColumn = rowString.IndexOf("+", cellStartColumn + 1);
                        colIndex = cellEndColumn;
                        if (cellEndColumn >= 0 && colIndex < this.m_iTableWidth)
                        {
                            // user the markers to calculate the colSpan
                            int cellStartColMarker = this.m_ColumnMarkers.FindIndex(delegate(int c) { return c == cellStartColumn; });
                            int cellEndColMarker = this.m_ColumnMarkers.FindIndex(delegate(int c) { return c == cellEndColumn; });
                            int colSpan = cellEndColMarker - cellStartColMarker;

                            int cellStartRow = rowMarker; // use the column index to look in the inverted table
                            string columnString = m_arrInvertedTableLines[cellStartColumn];
                            int cellEndRow = columnString.IndexOf("+", rowMarker + 1);
                            if (m_arrInvertedTableLines[cellEndColumn][cellEndRow] == '|')
                            {
                                cellEndRow = columnString.IndexOf("+", cellEndRow + 1);
                            }
                            if (cellEndRow >= 0 && cellEndRow < this.m_iTableHeight)
                            {
                                // use the markers to calculate the rowSpan
                                int cellStartRowMarker = this.m_RowMarkers.FindIndex(delegate(int r) { return r == cellStartRow; });
                                int cellEndRowMaker = this.m_RowMarkers.FindIndex(delegate(int r) { return r == cellEndRow; });
                                int rowSpan = cellEndRowMaker - cellStartRowMarker;

                                HtmlTableCell tableCell = new HtmlTableCell();
                                tableRow.m_tableCells.Add(tableCell);

                                tableCell.colSpan = colSpan;
                                tableCell.rowSpan = rowSpan;
                                // extract the raw cellData
                                tableCell.cellData = GetCellData(cellStartColumn, cellEndColumn, cellStartRow, cellEndRow);
                            }
                        }
                    }
                }
                else
                {
                    // we are done processing the table
                }
                 
            }
        }

        /// <Summary>
        ///     Outputs the table structure as a html table to Console (stdout) 
        /// </Summary>
        /// <arguments>
        ///     No Arguments
        /// </arguments>
        /// <returns>
        ///     void
        /// </returns>
        public void OutputTheTableAsHtml()
        {
            string outputString = "<html>\n";
            outputString += this.m_htmlTable.ToString();
            outputString += "</html>";

            // write to the console
            System.Console.Write(outputString);

            // write to the output file
            string htmlOutputFilePath = Path.Combine(Path.GetDirectoryName(this.m_strInputFilePath), Path.GetFileNameWithoutExtension(this.m_strInputFilePath) + ".html");
            File.WriteAllText(htmlOutputFilePath, outputString);

        }

        #endregion //public methods

        #region private methods

        /// <Summary>
        ///     Parses the table data and creates column maker, if inverted table is passed in then it will create rowMarkers 
        /// </Summary>
        /// <arguments>
        ///     lines   List<string>: table represented a list of string, can be inverted table
        ///     tableWidth int out: returns the table Width in number of characters
        ///     tableHeight int out: return the table Height in number of characters/lines
        ///     tableLines List<string> out: return the trimmed table represented as list of strings.
        ///     columnMarkers List<int> out: return the positions of all the column Markers (if inverted it will return rowMarkers)
        /// </arguments>
        /// <returns>
        ///     returns the number of Columns (if inverted will return the number of rows).
        /// </returns>
        private int ParseTableStrings(List<string> lines, out int tableWidth, out int tableHeight, out List<string> tableLines, out List<int> columnMarkers)
        {
            // initialize data
            tableWidth = 0;
            tableHeight = 0;
            tableLines = new List<string>();
            columnMarkers = new List<int>();

            // identify table begin location and end location
            // trim the lines to just include the table.
            bool foundTableStart = false;
            bool foundTableEnd = false;
            int tableColumnCount = 0;
            int rowWithMaxColumns = 0;
            int currentRowCounter = 0;
            foreach (string line in lines)
            {
                string currentLine = line.Trim();
                if (s_regExForTableStartAndEnd.IsMatch(currentLine))
                {
                    if (!foundTableStart)
                    {
                        // found table start
                        foundTableStart = true;
                        string[] columns = currentLine.Split('+');
                        int columnsLength = columns.Length - 2; // exclude the empty columns in end.
                        if (columns.Length > 1 && columnsLength > tableColumnCount)
                        {
                            tableColumnCount = columnsLength;
                            rowWithMaxColumns = currentRowCounter;
                        }
                        tableWidth = currentLine.Length;
                    }
                    else
                    {
                        if (currentLine.Length == tableWidth)
                        {
                            // check for table end. assumes that last row is the table end.
                            if (currentRowCounter == lines.Count - 1)
                            {
                                foundTableEnd = true;
                            }
                            string[] columns = currentLine.Split('+');
                            int columnsLength = columns.Length - 2;  // exclude the empty columns in end.
                            if (columns.Length > 1 && columnsLength > tableColumnCount)
                            {
                                tableColumnCount = columnsLength;
                                rowWithMaxColumns = currentRowCounter;
                            }
                        }
                    }
                }

                currentRowCounter++;
                if (foundTableStart && !foundTableEnd)
                {
                    //processing middle of the table
                    if (!s_regExForTableStartAndEnd.IsMatch(currentLine) && !s_regExForTableMiddle1.IsMatch(currentLine) &&
                            !s_regExForTableMiddle2.IsMatch(currentLine) && !s_regExForTableMiddle3.IsMatch(currentLine) &&
                                                                                currentLine.Length == tableColumnCount)
                    {
                        throw new HtmlTableGeneratorErrors.InputError(HtmlTableGeneratorErrors.HtmlTableGeneratorErrorCodes.invalid_table_open_cells, m_strInputFilePath);
                    }
                }

                // collect all table strings.
                if (foundTableStart || foundTableEnd)
                {
                    tableLines.Add(currentLine);
                }

            }

            // either table not found or partial table found
            if (!foundTableStart || !foundTableEnd || tableLines.Count == 0 || tableColumnCount == 0)
            {
                throw new HtmlTableGeneratorErrors.InputError(HtmlTableGeneratorErrors.HtmlTableGeneratorErrorCodes.table_not_found, m_strInputFilePath);
            }

            tableHeight = tableLines.Count;

            // calculate the column markers (if inverted table this will be rowMarkers).
            string lineWithMaxMarkers = tableLines[rowWithMaxColumns];
            string pattern = Regex.Escape("+");
            foreach (Match match in Regex.Matches(lineWithMaxMarkers, pattern))
            {
                columnMarkers.Add(match.Index);
            }

            return tableColumnCount;
        }

        /// <Summary>
        ///     Inverts the table (columns become rows and rows becom columns).
        /// </Summary>
        /// <arguments>
        ///     No Arguments
        /// </arguments>
        /// <returns>
        ///     void
        /// </returns>
        private void invertTheTable()
        {
            // intialized the inverted table as a list of strings.
            if (this.m_arrInvertedTableLines == null)
            {
                this.m_arrInvertedTableLines = new List<string>(m_iTableWidth);
                for (int i = 0; i < m_iTableWidth; i++)
                {
                    m_arrInvertedTableLines.Add(@"");
                }
            }

            // populate the inverted table
            foreach (string line in m_arrTableLines)
            {
                int count = 0;
                foreach (char columnChar in line)
                {
                    m_arrInvertedTableLines[count] = (string)m_arrInvertedTableLines[count] + columnChar;
                    count++;
                }
            }

        }

        /// <Summary>
        ///     Extract the raw cell Data 
        /// </Summary>
        /// <arguments>
        ///     cellStartColumn int: xStart
        ///     cellEndColun int: xEnd
        ///     cellStartRow int: yStart
        ///     cellEndRow int: yEnd
        /// </arguments>
        /// <returns>
        ///     raw_cell_data string: returns pre formatted raw cell data
        /// </returns>
        private string GetCellData(int cellStartColumn, int cellEndColumn, int cellStartRow, int cellEndRow)
        {
            string cellData = "";
            for (int partialRow = cellStartRow + 1; partialRow < cellEndRow; partialRow++)
            {
                if (partialRow == cellEndRow - 1)
                {
                    cellData += this.m_arrTableLines[partialRow].Substring(cellStartColumn + 1, cellEndColumn - cellStartColumn - 1); // no need line ending for the last line of cell.
                }
                else
                {
                    cellData += this.m_arrTableLines[partialRow].Substring(cellStartColumn + 1, cellEndColumn - cellStartColumn - 1) + "\n";
                }
            }
            return cellData;
        }

        #endregion // private methods

        #region private data

        //private data members
        private string m_strInputFilePath;
        private List<string> m_arrTableLines;
        private List<string> m_arrInvertedTableLines;

        private int m_iTableWidth;
        private int m_iTableHeight;

        private int m_iTableColumnCount;
        private int m_iTableRowCount;

        // starting location of the columnString and rows.
        private List<int> m_ColumnMarkers;
        private List<int> m_RowMarkers;

        private HtmlTable m_htmlTable;

        #endregion // private data

        #region private static data
        private static string s_strEscapedPlus = Regex.Escape(@"+");
        private static string s_strEscapedVerticalLine = Regex.Escape(@"|");

        private static Regex s_regExForTableStartAndEnd  = new Regex(s_strEscapedPlus + @"*" + s_strEscapedPlus);
        private static Regex s_regExForTableMiddle1      = new Regex(s_strEscapedPlus + @"*" + s_strEscapedVerticalLine);
        private static Regex s_regExForTableMiddle2      = new Regex(s_strEscapedVerticalLine + @"*" + s_strEscapedPlus);
        private static Regex s_regExForTableMiddle3      = new Regex(s_strEscapedVerticalLine + @"*" + s_strEscapedVerticalLine);
        #endregion //private static data
    }
}
