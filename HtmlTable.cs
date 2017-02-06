using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlTableGenerator
{
    class HtmlTableCell
    {
        public HtmlTableCell()
        {
            rowSpan = 1;
            colSpan = 1;

            cellData = "";
        }

        // format cell data as a string
        public override string ToString()
        {
            StringBuilder cellString = new StringBuilder();

            cellString.AppendFormat("        <td colSpan='{0}' rowSpan='{1}'><pre>{2}</pre></td>\n", colSpan, rowSpan, cellData);

            return cellString.ToString();
        }

        public int rowSpan;
        public int colSpan;

        public string cellData;
    }

    class HtmlTableRow
    {
        public HtmlTableRow()
        {
            m_tableCells = new List<HtmlTableCell>();
        }

        // format row data as a string
        public override string ToString()
        {
            StringBuilder rowString = new StringBuilder();

            rowString.AppendLine("      <tr>");

            foreach (HtmlTableCell tableCell in m_tableCells)
            {
                rowString.Append(tableCell.ToString());
                rowString.AppendLine();
            }

            rowString.AppendLine("      </tr>");

            return rowString.ToString();
        }

        public List<HtmlTableCell> m_tableCells;
    }

    class HtmlTable
    {
        public HtmlTable()
        {
            m_tableRows = new List<HtmlTableRow>();
        }

        // format html table as a string
        public override string ToString()
        {
            StringBuilder tableString = new StringBuilder();
            tableString.AppendLine("  <table border=\"1\">");
            tableString.AppendLine("    <tbody>");
            
            foreach (HtmlTableRow tableRow in m_tableRows) 
            {
                tableString.Append(tableRow.ToString());
                tableString.AppendLine();
            }

            tableString.AppendLine("    </tbody>");
            tableString.AppendLine("  </table>");

            return tableString.ToString();
        }

        public List<HtmlTableRow> m_tableRows;
    }
}
