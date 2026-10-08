using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;

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
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    char next = i + 1 < line.Length ? line[i + 1] : '\0';
                    if (closingQuote != '\0')
                    {
                        if (c == closingQuote)
                        {
                            if (next == closingQuote)
                                i++; // a doubled quote is an escaped quote
                            else
                                closingQuote = '\0';
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
                        }
                    }
                    else if (c == '-' && next == '-')
                    {
                        break; // the rest of the line is a comment
                    }
                    else if (c == '/' && next == '*')
                    {
                        commentDepth++;
                        i++;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        batchHasCode = true;
                        if (c == '\'' || c == '"')
                            closingQuote = c;
                        else if (c == '[')
                            closingQuote = ']';
                    }
                }
            }
            if (batchHasCode)
                batches.Add(batch.ToString());
            return batches.ToArray();
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
