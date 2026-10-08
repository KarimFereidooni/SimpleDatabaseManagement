using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DatabaseManagement.Helpers
{
    public class SqlHelper
    {
        /// <summary>
        /// Splits a T-SQL script into batches on lines that contain only GO.
        /// A GO line inside a block comment, string or delimited identifier is not a separator,
        /// and batches that contain only whitespace or comments are dropped.
        /// </summary>
        public static string[] SplitBatches(string script)
        {
            List<string> batches = new List<string>();
            if (string.IsNullOrEmpty(script))
                return batches.ToArray();

            StringBuilder batch = new StringBuilder();
            bool batchHasCode = false;
            int commentDepth = 0;        // nesting depth of /* */ comments (T-SQL allows nested comments)
            char closingQuote = '\0';    // ', " or ] while inside a string or delimited identifier

            int lineStart = 0;
            while (lineStart < script.Length)
            {
                int newLine = script.IndexOf('\n', lineStart);
                int lineEnd = newLine < 0 ? script.Length : newLine + 1;
                string line = script.Substring(lineStart, lineEnd - lineStart);
                lineStart = lineEnd;

                if (commentDepth == 0 && closingQuote == '\0' && line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    if (batchHasCode)
                        batches.Add(batch.ToString());
                    batch.Clear();
                    batchHasCode = false;
                    continue;
                }

                batch.Append(line);
                if (!string.IsNullOrWhiteSpace(ScanLine(line, ref commentDepth, ref closingQuote)))
                    batchHasCode = true;
            }
            if (batchHasCode)
                batches.Add(batch.ToString());
            return batches.ToArray();
        }

        private static readonly Regex TransactionStatementRegex = new Regex(
            @"\b(BEGIN\s+(DISTRIBUTED\s+)?TRAN(SACTION)?|COMMIT|ROLLBACK|SAVE\s+TRAN(SACTION)?)\b",
            RegexOptions.IgnoreCase);
        // Statements that SQL Server does not allow inside a user transaction
        private static readonly Regex NonTransactionalStatementRegex = new Regex(
            @"\b((CREATE|ALTER|DROP)\s+(DATABASE|FULLTEXT\s+(CATALOG|INDEX))|(BACKUP|RESTORE)\s+(DATABASE|LOG)|RECONFIGURE)\b",
            RegexOptions.IgnoreCase);
        private static readonly Regex ModuleDefinitionRegex = new Regex(
            @"^\s*(CREATE(\s+OR\s+ALTER)?|ALTER)\s+(PROC|PROCEDURE|FUNCTION|TRIGGER)\b",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns false when the script manages its own transactions (BEGIN TRAN, COMMIT, ROLLBACK, SAVE TRAN)
        /// or contains statements that are not allowed inside a user transaction, so it must not be wrapped
        /// in a transaction by the caller. Comments, strings and the bodies of procedures, functions and
        /// triggers are ignored, because their statements do not run when the script runs.
        /// </summary>
        public static bool CanRunInTransaction(string script)
        {
            foreach (string batch in SplitBatches(script))
            {
                string code = RemoveCommentsAndStrings(batch);
                if (ModuleDefinitionRegex.IsMatch(code))
                    continue;
                if (TransactionStatementRegex.IsMatch(code) || NonTransactionalStatementRegex.IsMatch(code))
                    return false;
            }
            return true;
        }

        private static string RemoveCommentsAndStrings(string script)
        {
            StringBuilder code = new StringBuilder();
            int commentDepth = 0;
            char closingQuote = '\0';
            foreach (string line in script.Split('\n'))
            {
                code.Append(ScanLine(line, ref commentDepth, ref closingQuote));
                code.Append('\n');
            }
            return code.ToString();
        }

        /// <summary>
        /// Returns the code of one script line with comments replaced by a space and the content of strings
        /// and delimited identifiers left out (only their quotes are kept). commentDepth and closingQuote
        /// carry the state of an unclosed comment, string or identifier over to the next line.
        /// </summary>
        private static string ScanLine(string line, ref int commentDepth, ref char closingQuote)
        {
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                char next = i + 1 < line.Length ? line[i + 1] : '\0';
                if (closingQuote != '\0')
                {
                    if (c == closingQuote)
                    {
                        if (next == closingQuote)
                        {
                            i++; // a doubled quote is an escaped quote
                        }
                        else
                        {
                            code.Append(closingQuote);
                            closingQuote = '\0';
                        }
                    }
                }
                else if (commentDepth > 0)
                {
                    if (c == '/' && next == '*')
                    {
                        commentDepth++;
                        i++;
                    }
                    else if (c == '*' && next == '/')
                    {
                        commentDepth--;
                        i++;
                        if (commentDepth == 0)
                            code.Append(' ');
                    }
                }
                else if (c == '-' && next == '-')
                {
                    code.Append(' ');
                    break; // the rest of the line is a comment
                }
                else if (c == '/' && next == '*')
                {
                    commentDepth++;
                    i++;
                }
                else
                {
                    code.Append(c);
                    if (c == '\'' || c == '"')
                        closingQuote = c;
                    else if (c == '[')
                        closingQuote = ']';
                }
            }
            return code.ToString();
        }

        public static bool RunQueryWithRollback(string query, string connectionString, out string error)
        {
            return RunQueryWithRollback(query, connectionString, null, out error);
        }
        public static bool RunQueryWithRollback(string query, string connectionString, string database, out string error)
        {
            SqlConnection Connection = new SqlConnection(connectionString);
            SqlCommand Command = new SqlCommand();
            Command.Connection = Connection;
            SqlTransaction transaction;
            try
            {
                Connection.Open();
                transaction = Connection.BeginTransaction();
                Command.Transaction = transaction;
                if (!string.IsNullOrEmpty(database))
                    Connection.ChangeDatabase(database);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            string[] lines = SqlHelper.SplitBatches(query);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                if (lines[i].Length > 0)
                {
                    try
                    {
                        Command.CommandText = lines[i];
                        Command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Connection.Close();
                        error = ex.Message;
                        return false;
                    }
                }
            }
            transaction.Commit();

            Connection.Close();
            transaction.Dispose();
            Command.Dispose();
            Connection.Dispose();
            error = null;
            return true;
        }
        public static bool RunQuery(string query, string connectionString, string database, out string error)
        {
            SqlConnection Connection = new SqlConnection(connectionString);
            SqlCommand Command = new SqlCommand();
            Command.Connection = Connection;
            try
            {
                Connection.Open();
                if (!string.IsNullOrEmpty(database))
                    Connection.ChangeDatabase(database);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            string[] lines = SqlHelper.SplitBatches(query);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                if (lines[i].Length > 0)
                {
                    try
                    {
                        Command.CommandText = lines[i];
                        Command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Connection.Close();
                        error = ex.Message;
                        return false;
                    }
                }
            }
            Connection.Close();
            Command.Dispose();
            Connection.Dispose();
            error = null;
            return true;
        }
        public static void RunQuery(string query, DatabaseManagement.Services.DataAccess dataAccess, params object[] parameters)
        {
            string[] lines = SqlHelper.SplitBatches(query);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                if (lines[i].Length > 0)
                {
                    dataAccess.Execute(lines[i], parameters);
                }
            }
        }
    }
}
