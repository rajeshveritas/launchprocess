using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.FileIO;

namespace Veritas.LaunchProcess.BL
{
    public class LaunchProcess

    {
        private string _dbConnectionString;
        private string _filePath;
        private DataTable _dataTable;

        public string DbConnectionString => _dbConnectionString;

        public LaunchProcess(string dbConnectionString, string filePath)
        {
            if (String.IsNullOrWhiteSpace(dbConnectionString))
            {
                throw new ArgumentException("dbConnection must contain a database connection string.");
            }
            _dbConnectionString = dbConnectionString;

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} was not found.");
            }
            _filePath = filePath;
        }
        public void UpdateJNLData()
        {
           
            ReadPrimaryInputFileToDt();
            try
            {
                    


                    using (SqlConnection connection = new SqlConnection(_dbConnectionString))
                    {
                        connection.Open();
                        SqlCommand command = connection.CreateCommand();
                        SqlTransaction transaction;
                        // Start a transaction.
                        transaction = connection.BeginTransaction("LaunchProcessTransaction");
                        command.Connection = connection;
                        command.Transaction = transaction;
                        try
                        {
                            int iRowCount = 0;
                            foreach (DataRow row in _dataTable.Rows)
                            {
                                var j = 1;
//                                var formNumber = values[0].ToString();
//                                var strObsoleteDate = values[1].ToString();
//                                var strViewabilityDate = values[2].ToString();
//                                var strViewability = values[3].ToString();
//                                var strReplaceDate = values[4].ToString();
//                                var strReplacePart = values[5].ToString();

                                // Check the Form Number exists or not
/*                                if (!string.Equals(formNumber, "", StringComparison.Ordinal))
                                {
                                    var strInvId = GetInventoryId(command, formNumber);
                                    string strReplaceInvId;
                                    strReplaceInvId = !string.Equals(strReplacePart, "", StringComparison.Ordinal) ? GetInventoryId(command, strReplacePart) : "";
                                    //Obsolete Date
                                    string strUpdateQuery;
                                    if (DateTime.TryParse(strObsoleteDate, out DateTime obsoleteDate))
                                    {
                                        strUpdateQuery = "UPDATE InventoryDetails SET ObsoleteDate = '" + obsoleteDate.ToString() + "' WHERE InventoryID =" + strInvId;
                                       // ExecuteNonQuery(command, strUpdateQuery);
                                        using (SqlCommand cmd = new SqlCommand())
                                        AddSiteHistory(command, strInvId, "ObsoleteDate changed to " + obsoleteDate.ToString() + " through Launch");
                                        LogMessage("Obsolete Date successfully updated for the Form Number :" + formNumber);
                                    }
                                    //Replace Date
                                    if ((DateTime.TryParse(strReplaceDate, out DateTime replaceDate)) && (strReplacePart != ""))
                                    {
                                        strUpdateQuery = "UPDATE InventoryDetails SET ReplaceDate = '" + replaceDate.ToString() + "',ReplacePart=" + strReplaceInvId +
                                            " WHERE InventoryID =" + strInvId;
                                        ExecuteNonQuery(command, strUpdateQuery);
                                        AddSiteHistory(command, strInvId, "ReplaceDate changed to " + replaceDate.ToString() + " through Launch");
                                        AddSiteHistory(command, strInvId, "ReplacePart changed to " + strReplacePart + " through Launch");
                                        LogMessage("Replace Date & Replace Form Number successfully updated for the Form Number :" + formNumber);
                                    }
                                    //New Viewability Date
                                    if ((DateTime.TryParse(strViewabilityDate, out DateTime viewDate)) && (strViewability != ""))
                                    {
                                        strUpdateQuery = "UPDATE InventoryDetails SET NewViewabilityDate = '" + viewDate.ToString() + "',NewViewability =  " + strViewability + 
                                            " WHERE InventoryID =" + strInvId;
                                        ExecuteNonQuery(command, strUpdateQuery);
                                        AddSiteHistory(command, strInvId, "NewViewabilityDate changed to " + viewDate.ToString() + " through Launch");
                                        AddSiteHistory(command, strInvId, "NewViewability changed to " + GetViewability(command,strViewability) + "through Launch");
                                        LogMessage("NewViewabilityDate successfully updated for the Form Number :" + formNumber);
                                    }
                                }
                                iRowCount = iRowCount + 1;
                                LogMessage("Rows Processed : " + iRowCount.ToString());*/
                            }
                           // LogMessage("Total Records processed are :" + iRowCount);
                            //transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage("Exception occurred during File Processing:" + ex.Message);
                        }
                    }
                }

            
            catch (Exception ex)
            {
                LogMessage("Exception Details:" + ex.Message);
            }
        }


        private void ReadPrimaryInputFileToDt()
        {
            try
            {
                var comp = new Computer();
                var headerRow = true;
                using (var fileParser = comp.FileSystem.OpenTextFieldParser(_filePath))
                {
                    fileParser.SetDelimiters("|");
                    var lineNumber = 1;
                    while (!fileParser.EndOfData)
                    {
                        var currentRow = fileParser.ReadFields();
                        if (currentRow != null)
                        {
                            if (headerRow)
                            {
                                foreach (var field in currentRow)
                                {
                                    _dataTable.Columns.Add(field, typeof(object));
                                }
                                headerRow = false;
                                lineNumber++;
                            }
                            else
                            {
                                // ReSharper disable once CoVariantArrayConversion
                                _dataTable.Rows.Add(currentRow);
                                lineNumber++;
                            }
                        }
                        else
                        {
                            throw new FileLoadException($"There was an error reading {_filePath}. File parser returned null at line {lineNumber}");
                        }
                    }
                }
            }
            catch (MalformedLineException err)
            {
                throw new FileLoadException($"{_filePath} contains malformed lines for interpretation. See inner exception for details.",err);
            }
  

        }

        /// <summary>
        /// used for Insert/Update Query
        /// </summary>
        /// <param name="command"></param>
        /// <param name="strQuery"></param>
        private void ExecuteNonQuery(SqlCommand command,  string strQuery)
        {
            command.CommandText = strQuery;
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Get the InventoryId from the Inventory DB
        /// </summary>
        /// <param name="command"></param>
        /// <param name="strFormNumber"></param>
        /// <returns></returns>
        private string GetInventoryId(SqlCommand command, string strFormNumber)
        {
            command.CommandText = "SELECT InventoryID FROM Inventory WHERE bar_code = '" + strFormNumber + "'"; ;
            return command.ExecuteScalar().ToString();
        }

        private void AddSiteHistory(SqlCommand command, string strInvId, string strDetails)
        {   
            command.CommandText = "INSERT INTO SiteHistory(INVENTORYID, Details,PageInfo, ModUser, ModDate) " +
                " VALUES(" + strInvId + ",'"+ strDetails + "','LaunchProcess',dbo.GetSetting('Automation.AdminID'),GETDATE())"; ;
            command.ExecuteNonQuery();
        }

        private string GetViewability(SqlCommand command,string viewability)
        {
            command.CommandText = "SELECT Text FROM dbo.DropdownLists WHERE Section = 'Inventory.Viewability' AND Data = " + viewability;
            return command.ExecuteScalar().ToString();
        }
        /// <summary>
        /// Capturing Log & Error Messages
        /// </summary>
        /// <param name="strMessage"></param>
        private void LogMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
        }

    }

}
