using System;

namespace HtmlTableGeneratorErrors
{
    public enum HtmlTableGeneratorErrorCodes
    {
        generic_error,
        file_does_not_exist,
        unable_to_open_file,
        table_not_found,
        invalid_table_no_rows,
        invalid_table_no_columns,
        invalid_table_open_cells
    }

    /// <summary>
    /// Generic error saying that input file is invalid
    /// </summary>
    public class InputError : Exception
    {
        
        public InputError(HtmlTableGeneratorErrorCodes code=HtmlTableGeneratorErrorCodes.generic_error, string filePath="")
        {
            m_enumErrorCode = code;
            m_strInputFilePath = filePath;
            m_strErrorMsg = null;

            GenerateErrorMessage();
        }

        private void GenerateErrorMessage()
        {
            switch (m_enumErrorCode)
            {
                case HtmlTableGeneratorErrorCodes.file_does_not_exist:
                    m_strErrorMsg = string.Format(@"Input file ""{0}"" not found.", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.unable_to_open_file:
                    m_strErrorMsg = string.Format(@"Unable to open input file ""{0}"", make sure it is not locked by some other application", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.table_not_found:
                    m_strErrorMsg = string.Format(@"Input file ""{0}"" does not have any table.", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.invalid_table_no_rows:
                    m_strErrorMsg = string.Format(@"Input file ""{0}"" has invalid table. table does not have any rows", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.invalid_table_no_columns:
                    m_strErrorMsg = string.Format(@"Input file ""{0}"" has invalid table. Table does not have any columns.", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.invalid_table_open_cells:
                    m_strErrorMsg = string.Format(@"Input file ""{0}"" has invalid table. Table has open cells.", m_strInputFilePath);
                    break;
                case HtmlTableGeneratorErrorCodes.generic_error:
                    // fall into default
                default:
                    m_strErrorMsg = string.Format(@"Unknown error when loading/parsing input file ""{0}"".", m_strInputFilePath);
                    break;
            }
        }

        public void PrintMessage()
        {
            if (string.IsNullOrEmpty(m_strErrorMsg)) 
            {
                GenerateErrorMessage();
            }
            System.Console.WriteLine(m_strErrorMsg);
        }

        private HtmlTableGeneratorErrorCodes m_enumErrorCode;
        public string m_strErrorMsg;
        private string m_strInputFilePath;
    }
}
