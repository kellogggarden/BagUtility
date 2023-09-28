using KGP.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mongoose.WinStudio.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KGP.Services
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration Configuration;

        public OrderService(IConfiguration configuration) {
            Configuration = configuration;
        }
        public List<OpenOrder> GetOrders(Filter filter)
        {
            try
            {
                var list = new List<OpenOrder>();
                using (SqlService svc = new SqlService(Config.CSIReportDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("SiteRef", filter.Warehouse, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Status", filter.Status, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Customer", "%", 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("dStartDate", filter.StartDate, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("dEndDate", filter.EndDate, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("page", filter.Page, 10, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetOpenOrders", param, 0))
                    {
                        while (read.Read())
                        {
                            var o = new OpenOrder();
                            o.Warehouse = read["SiteRef"].ToString();
                            o.SalesOrder = read["OrderNumber"].ToString();
                            o.CustomerPO = read["CustPO"].ToString();
                            o.Action = read["Action"].ToString();
                            o.PalletCount = Math.Round(Convert.ToDouble(read["PalletCount"])).ToString();
                            o.DeliveryStatus = read["DeliveryStatus"].ToString();
                            o.Appointment = read["Appointment"].ToString();
                            o.ReqCan = read["ReqCan"].ToString();
                            o.ConfCan = read["ConfCan"].ToString();
                            o.OpComments = read["OpComments"].ToString();
                            o.SalesComment = read["SalesComments"].ToString();
                            o.SalesOrderDate = string.IsNullOrEmpty(read["OrderDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(read["OrderDate"]);
                            o.OriginalReqDate = string.IsNullOrEmpty(read["OriginalOrderDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(read["OriginalOrderDate"].ToString());
                            o.DueDate = string.IsNullOrEmpty(read["DueDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(read["DueDate"]);
                            o.DaysLate = read["DaysLate"].ToString();
                            o.DaysToShip = read["DaysToShip"].ToString();
                            o.Customer = read["CustomerName"].ToString();
                            o.ShipToAddr = read["addr_1"].ToString();
                            o.ShipToCity = read["City"].ToString();
                            o.St = read["ShipToState"].ToString();
                            o.ShipToZip = read["Zip"].ToString();
                            o.Phone = read["Phone"].ToString();
                            o.CustomerNumber = read["CustNum"].ToString();
                            o.OrderStatus = read["OrderStatus"].ToString();
                            o.ShipVIA = read["ShipVIA"].ToString();
                            o.StatusColor = "NO";
                            o.Total = int.Parse(read["Total"].ToString());
                            o.Count = int.Parse(read["Count"].ToString());
                            list.Add(o);
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Save(List<OpenOrder> orders)
        {
            try
            {
                using (SqlService svc = new SqlService(Config.CSIReportDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    foreach (var o in orders)
                    {
                        var param = new List<DBParameter>();
                        param.Add(new DBParameter("salesOrder", o.SalesOrder, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("warehouse", o.Warehouse, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("deliveryStatus", o.DeliveryStatus, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("orderDate", o.SalesOrderDate, -1, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("dueDate", o.DueDate, -1, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("appointment", o.Appointment, 255, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("opComments", o.OpComments, 255, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("salesComment", o.SalesComment, 255, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                        var command = new DBCommand();
                        command.ProcedureName = "UpdateOpenOrder";
                        command.DBParameters = param;

                        cmds.Add(command);
                    }

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                        //throw new Exception("TEST ERROR");
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<ProductionLine> GetProductionLines(int? id = null, string siteRef = null, string workCenter = null, string status = null, string op = null)
        {
            try
            {
                List<ProductionLine> jobs = new List<ProductionLine>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();

                    param.Add(new DBParameter("ID", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("WorkCenter", workCenter, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("SiteRef", siteRef, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Status", status, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Operator", op, 100, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetProductionBagJobs", param, 0))
                    {
                        while (read.Read())
                        {
                            var o = new ProductionLine();
                            o.ID = int.Parse(read["ID"].ToString());
                            o.RunID = int.Parse(read["RunID"].ToString());
                            o.DueDate = DateTime.Parse(read["DueDate"].ToString());
                            o.HeaderDueDate = DateTime.Parse(read["HeaderDueDate"].ToString());
                            o.Site = read["Site"].ToString();
                            o.BagQty = int.Parse(read["BagQty"].ToString());
                            o.PalletCount = int.Parse(read["PalletCount"].ToString());
                            o.WorkCenter = read["WorkCenter"].ToString();
                            o.Description = read["Description"].ToString();
                            o.ItemID = read["ItemID"].ToString();
                            o.CompletedQty = read["CompletedQty"] == DBNull.Value ? 0 : int.Parse(read["CompletedQty"].ToString());
                            o.Status = read["Status"].ToString();
                            o.StartTimeStamp = read["StartTimeStamp"] == DBNull.Value ? null : DateTime.Parse(read["StartTimeStamp"].ToString());
                            o.Operator = read["Operator"].ToString();
                            o.Reason = read["Reason"].ToString();
                            o.Qty = int.Parse(read["Qty"].ToString());
                            o.PalletQty = int.Parse(read["PalletQty"].ToString());
                            o.BagCount = int.Parse(read["BagCount"] == DBNull.Value ? "0" : read["BagCount"].ToString());
                            o.ScrapCount = int.Parse(read["ScrapCount"] == DBNull.Value ? "0" : read["ScrapCount"].ToString());
                            o.IsBaler = Convert.ToBoolean(read["IsBaler"]);
                            o.ProductionScheduleID = read["ProductionScheduleID"].ToString();
                            o.UM = read["UM"].ToString();
                            o.Shift = int.Parse(read["Shift"].ToString());
                            o.Job = read["Job"].ToString();
                            jobs.Add(o);
                        }
                    }
                }

                return jobs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<ProductionLine> GetAllProductionLines(int? id = null, string siteRef = null, string workCenter = null, string status = null, string op = null)
        {
            try
            {
                List<ProductionLine> jobs = new List<ProductionLine>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();

                    param.Add(new DBParameter("ID", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("WorkCenter", workCenter, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("SiteRef", siteRef, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Status", status, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Operator", op, 100, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetAllProductionBagJobs", param, 0))
                    {
                        while (read.Read())
                        {
                            try
                            {

                                var o = new ProductionLine();
                                o.ID = int.Parse(read["ID"].ToString());
                                o.RunID = int.Parse(read["RunID"].ToString());
                                o.DueDate = DateTime.Parse(read["DueDate"].ToString());
                                o.HeaderDueDate = DateTime.Parse(read["HeaderDueDate"].ToString());
                                o.Site = read["Site"].ToString();
                                o.BagQty = int.Parse(read["BagQty"].ToString());
                                o.PalletCount = int.Parse(read["PalletCount"].ToString());
                                o.WorkCenter = read["WorkCenter"].ToString();
                                o.Description = read["Description"].ToString();
                                o.ItemID = read["ItemID"].ToString();
                                o.CompletedQty = read["CompletedQty"] == DBNull.Value ? 0 : int.Parse(read["CompletedQty"].ToString());
                                o.Status = read["Status"].ToString();
                                o.StartTimeStamp = read["StartTimeStamp"] == DBNull.Value ? null : DateTime.Parse(read["StartTimeStamp"].ToString());
                                o.Operator = read["Operator"].ToString();
                                o.Reason = read["Reason"].ToString();
                                o.Qty = int.Parse(read["Qty"].ToString());
                                o.PalletQty = int.Parse(read["PalletQty"].ToString());
                                o.BagCount = int.Parse(read["BagCount"] == DBNull.Value ? "0" : read["BagCount"].ToString());
                                o.ScrapCount = int.Parse(read["ScrapCount"] == DBNull.Value ? "0" : read["ScrapCount"].ToString());
                                o.IsBaler = Convert.ToBoolean(read["IsBaler"]);
                                o.ProductionScheduleID = read["ProductionScheduleID"].ToString();
                                o.UM = read["UM"].ToString();
                                o.Shift = int.Parse(read["Shift"].ToString());
                                jobs.Add(o);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                }

                return jobs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void InsertProductionLine(ProductionLine line) {

            //Check Original Status
            //If status was paused, log it
            var existing = GetProductionLines(line.ID).FirstOrDefault();

            if (existing.Status.ToLower() == "paused" && line.Status.ToLower() == "processing") {
                //insert log
                InsertOrUpdateProductionDownTimeLog(line);
            }

            if (line.Status.ToLower() == "paused") {
                //update log downendtime
                InsertOrUpdateProductionDownTimeLog(line);
            }

            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("Id", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("StartTime", line.StartTimeStamp, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("EndTime", line.EndTimeStamp, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("PalletCount", line.PalletCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("BagCount", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("ScrapCount", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("DownTime", line.DownTime, 0, System.Data.SqlDbType.Float, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Operator", line.Operator, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Status", line.Status, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("ModifiedBy", line.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("UpdateProductionBagJobs", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveProductionLine(ProductionLine line)
        {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("RunID", line.RunID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("ID", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("BagCount", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("ScrapCount", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("DownTime", line.DownTime, 0, System.Data.SqlDbType.Float, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Status", line.Status, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("SaveProductionBagJobs", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveProductionLines(ProductionHeader header) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    //if (header.Status != null && header.Status.ToLower() == "complete") {
                    //    var param3 = new List<DBParameter>();
                    //    param3.Add(new DBParameter("TaskID", 5001, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    //    param3.Add(new DBParameter("Parameters", "", 1000, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    //    param3.Add(new DBParameter("TaskBody", "", -1, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    //    param3.Add(new DBParameter("Warehouse", header.Site, 50, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    //    param3.Add(new DBParameter("OutputPath", "", 200, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    //    param3.Add(new DBParameter("scheduledon", DateTime.Today, -1, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                    //    var command3 = new DBCommand();
                    //    command3.ProcedureName = "PostTask";
                    //    command3.DBParameters = param3;
                    //}

                    foreach (var line in header.ProductionLines)
                    {
                        TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                        DateTime? start = line.StartTimeStamp == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)line.StartTimeStamp), pstZone);
                        DateTime? end = line.EndTimeStamp == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)line.EndTimeStamp), pstZone);


                        var param = new List<DBParameter>();
                        param.Add(new DBParameter("RunID", line.RunID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ID", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("PalletCount", line.PalletCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("BagCount", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ScrapCount", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("DownTime", line.DownTime, 0, System.Data.SqlDbType.Float, System.Data.ParameterDirection.Input));
                        //param.Add(new DBParameter("Reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("Status", line.Status, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("StartTime", start, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("EndTime", end, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("Operator", line.Operator == "" ? null : line.Operator, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ModifiedBy", line.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("DueDate", line.DueDate, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));


                        var command = new DBCommand();
                        command.ProcedureName = "SaveProductionBagJobs";
                        command.DBParameters = param;

                        cmds.Add(command);
                    }

                    var param2 = new List<DBParameter>();
                    param2.Add(new DBParameter("RunID", header.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param2.Add(new DBParameter("HeaderStatus", header.Status, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    var command2 = new DBCommand();
                    command2.ProcedureName = "SaveProductionBagJobs";
                    command2.DBParameters = param2;

                    cmds.Add(command2);

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                        //throw new Exception("TEST ERROR");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<string> GetWorkCenterBySite(string site) {
            try
            {
                List<string> workcenters = new List<string>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();

                    if (site != null)
                    {
                        param.Add(new DBParameter("siteref", site, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    }

                    using (SqlDataReader read = svc.ExecuteSPReader("GetWorkCenterBySite", param, 0))
                    {
                        while (read.Read())
                        {
                            workcenters.Add(read["WorkCenter"].ToString().ToUpper());
                        }
                    }
                }

                return workcenters;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<string> GetSiteRefs()
        {
            try
            {
                List<string> workcenters = new List<string>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();


                    using (SqlDataReader read = svc.ExecuteSPReader("GetSiteRefs", param, 0))
                    {
                        while (read.Read())
                        {
                            workcenters.Add(read["SiteRef"].ToString());
                        }
                    }
                }

                return workcenters;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private List<ProductionLine> GetAllLineByHeaderId(int id) {
            try
            {
                List<ProductionLine> lines = new List<ProductionLine>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("id", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetAllProductionBagJobLines", param, 0))
                    {
                        while (read.Read())
                        {
                            var line = new ProductionLine();
                            line.ID = int.Parse(read["ID"].ToString());
                            line.RunID = int.Parse(read["RunID"].ToString());
                            line.Operator = read["Operator"].ToString();
                            line.StartTimeStamp = read["StartTimeStamp"] == DBNull.Value ? null : DateTime.Parse(read["StartTimeStamp"].ToString());
                            line.EndTimeStamp = read["EndTimeStamp"] == DBNull.Value ? null : DateTime.Parse(read["EndTimeStamp"].ToString());
                            line.BagCount = int.Parse(read["BagCount"].ToString());
                            line.PalletCount = int.Parse(read["PalletCount"].ToString());

                            line.ScrapCount = int.Parse(read["ScrapCount"].ToString());
                            line.Reason = read["Reason"].ToString();
                            line.Status = read["Status"].ToString();
                            line.WorkCenter = read["WorkCenter"].ToString();
                            line.WorkCenterName = read["Name"].ToString();
                            line.Qty = int.Parse(read["Qty"].ToString());
                            line.PalletQty = int.Parse(read["PalletQty"].ToString());
                            line.IsBaler = Convert.ToBoolean(read["IsBaler"]);
                            line.DownTime = float.Parse(read["DownTime"] == DBNull.Value ? "0" : read["DownTime"].ToString());
                            line.DueDate = DateTime.Parse(read["DueDate"].ToString());
                            line.BagsPerPallet = int.Parse(read["BagsPerPallet"] == DBNull.Value ? "0" : read["BagsPerPallet"].ToString());
                            line.ProductionScheduleID = read["ProductionScheduleID"] == DBNull.Value ? "" : read["ProductionScheduleID"].ToString();
                            lines.Add(line);
                        }
                    }
                }
                return lines;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<ProductionHeader> GetAllSubmittedProductHeaders(string site, string filter, string direction) {
            try
            {
                List<ProductionHeader> headers = new List<ProductionHeader>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("Site", site, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Filter", filter, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Direction", direction, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetAllProductionBagJobHeaders", param, 0))
                    {
                        while (read.Read())
                        {
                            var header = new ProductionHeader();
                            header.ItemID = read["ItemID"].ToString();
                            header.Description = read["Description"].ToString();
                            header.BagQty = int.Parse(read["BagQty"].ToString());
                            //header.PalletQty = int.Parse(read["TotalPalletQty"].ToString());
                            header.DueDate = DateTime.Parse(read["DueDate"].ToString());
                            header.ID = int.Parse(read["ID"].ToString());
                            header.ProductionScheduleID = read["ProductionScheduleID"] == DBNull.Value ? "" : read["ProductionScheduleID"].ToString();

                            header.ProductionLines = GetAllLineByHeaderId(header.ID);
                            header.PalletQty = header.ProductionLines.Sum(x => x.PalletCount);
                            header.Status = read["Status"].ToString();
                            header.Job = read["Job"].ToString();
                            header.UM = GetUMForItem(header.ItemID);
                            header.Site = site;
                            headers.Add(header);
                        }
                    }
                }

                return headers;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<ProductionSchedule> GetProductionSchedulebyID(string id) {
            try
            {
                List<ProductionSchedule> schedules = new List<ProductionSchedule>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("id", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetAllProductionSchedule", param, 0))
                    {
                        while (read.Read())
                        {
                            var schedule = new ProductionSchedule();
                            schedule.Description = read["Description"].ToString();
                            schedule.ItemId = read["ItemId"].ToString();
                            schedule.ProductionScheduleId = read["ProductionScheduleId"].ToString();
                            schedule.DueDate = DateTime.Parse(read["DueDate"].ToString());
                            schedule.Id = int.Parse(read["id"].ToString());
                            schedules.Add(schedule);
                        }
                    }
                }

                return schedules;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AssignProductionSchedules(List<ScheduleAssign> schedules) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    foreach (var s in schedules)
                    {
                        var param = new List<DBParameter>();
                        param.Add(new DBParameter("id", s.Id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("workSpace", s.WorkSpace, 50, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("qty", s.Quantity, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                        var command = new DBCommand();
                        command.ProcedureName = "InsertProductionHeader";
                        command.DBParameters = param;

                        cmds.Add(command);
                    }

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int AssingLineToProductionHeader(AddProductLineParam lParam) {
            int id = 0;
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("id", lParam.Id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("workCenter", lParam.WorkCenter, 20, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("Qty", lParam.Qty, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("PalletQty", lParam.PalletQty, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("DueDate", lParam.DueDate, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("modifiedBy", lParam.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));


                    //svc.ExecuteSPReader("InsertProductLine", param, 0);
                    using (SqlDataReader read = svc.ExecuteSPReader("InsertProductLine", param, 0))
                    {
                        while (read.Read())
                        {
                            id = int.Parse(read["id"].ToString());
                        }
                    }
                }
                return id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void InsertOrUpdateProductionDownTimeLog(ProductionLine line) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("headerId", line.RunID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("lineId", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("startDownTime", line.StartTimeStamp, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("endDownTime", line.EndTimeStamp, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                    param.Add(new DBParameter("pallet", line.PalletCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("bags", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("scraps", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("createdBy", line.Operator, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("modifiedBy", line.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("note", line.Note, 1000, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));


                    svc.ExecuteSPReader("InsertOrUpdateDownLog", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void InsertOrUpdateBalerDownLog(ProductionLine line)
        {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("headerId", line.RunID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("lineId", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("pallet", line.PalletCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("bags", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("scraps", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("createdBy", line.Operator, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("baler", line.StoppedBaler.FirstOrDefault().Baler, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("modifiedBy", line.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("note", line.Note, 1000, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));


                    svc.ExecuteSPReader("InsertOrUpdateBalerDownLog", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task LoadProuctScheduelToSql(LoadScheduleParam sParam) {
            try
            {
                if (sParam.Warehouse.ToLower() == "tole")
                {
                    var schedules = sParam.Schedules;
                    using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                    {
                        List<DBCommand> cmds = new List<DBCommand>();

                        foreach (var s in schedules)
                        {
                            //Insert Header for Job
                            var param = new List<DBParameter>();
                            param.Add(new DBParameter("@dueDate", s.DueDdate, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@site", sParam.Warehouse, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@itemId", s.Item, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@description", s.Description, 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            //param.Add(new DBParameter("@productionScheduleID", s.ScheduleID, 30, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@qty", s.RemainingQty, 30, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@job", s.Job, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@jobWorkCenter", s.WorkCenter, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@createdBy", sParam.CreatedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));


                            var command = new DBCommand();
                            command.ProcedureName = "InsertJobHeader";
                            command.DBParameters = param;

                            cmds.Add(command);
                        }

                        if (cmds.Count > 0)
                        {
                            svc.ExecuteSPList(cmds, 0);
                        }
                    }
                }
                else
                {
                    var schedules = sParam.Schedules;
                    using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                    {
                        List<DBCommand> cmds = new List<DBCommand>();

                        foreach (var s in schedules)
                        {
                            var param = new List<DBParameter>();
                            param.Add(new DBParameter("@dueDate", s.DueDdate, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@site", sParam.Warehouse, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@itemId", s.Item, 10, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@description", s.Description, 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@productionScheduleID", s.ScheduleID, 30, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            param.Add(new DBParameter("@qty", s.RemainingQty, 30, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));


                            var command = new DBCommand();
                            command.ProcedureName = "InsertProductionHeader";
                            command.DBParameters = param;

                            cmds.Add(command);
                        }

                        if (cmds.Count > 0)
                        {
                            svc.ExecuteSPList(cmds, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteProductionLine(int lineId) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("LineId", lineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("DeleteProductionLine", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteProductionHeader(int id)
        {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("id", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("DeleteProductionHeader", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<ProductionDownTimeLog> GetDownTimeLog(int lineId) {
            try
            {
                List<ProductionDownTimeLog> downs = new List<ProductionDownTimeLog>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("LineId", lineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetProductionDownTimeLog", param, 0))
                    {
                        while (read.Read())
                        {
                            var down = new ProductionDownTimeLog();
                            down.Id = int.Parse(read["Id"].ToString());
                            down.LineId = int.Parse(read["LineID"].ToString());
                            down.HeaderId = int.Parse(read["HeaderID"].ToString());
                            down.CreatedBy = read["CreatedBy"].ToString();
                            down.Reason = read["Reason"].ToString();
                            down.StartDownTime = string.IsNullOrEmpty(read["StartDownTime"].ToString()) ? (DateTime?)null : Convert.ToDateTime(read["StartDownTime"]);
                            down.EndDownTime = string.IsNullOrEmpty(read["EndDownTime"].ToString()) ? (DateTime?)null : Convert.ToDateTime(read["EndDownTime"]);
                            down.Bags = int.Parse(read["Bags"].ToString());
                            down.Scraps = int.Parse(read["Scraps"].ToString());
                            down.Pallet = int.Parse(read["Pallet"].ToString());
                            down.Baler = read["Baler"] == DBNull.Value ? null : int.Parse(read["Baler"].ToString());
                            down.Note = read["Note"].ToString();
                            downs.Add(down);
                        }
                    }
                }

                return downs;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<string> GetOperators(string site) {
            try
            {
                List<string> ops = new List<string>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("site", site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetProductionOperatorBySite", param, 0))
                    {
                        while (read.Read())
                        {
                            ops.Add(read["Operator"].ToString());
                        }
                    }
                }

                return ops;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void SaveDownLogs(List<ProductionDownTimeLog> logs) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    foreach (var l in logs)
                    {
                        TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                        DateTime? start = l.StartDownTime == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)l.StartDownTime), pstZone);
                        DateTime? end = l.EndDownTime == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)l.EndDownTime), pstZone);

                        var param = new List<DBParameter>();
                        param.Add(new DBParameter("@headerId", l.HeaderId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@lineId", l.LineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@baler", l.Baler, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@id", l.Id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@pallet", l.Pallet, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@bag", l.Bags, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@scrap", l.Scraps, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@startDown", start, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@endDown", end, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@reason", l.Reason, 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@createdBy", l.CreatedBy, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@modifiedBy", l.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("@note", l.Note, 1000, System.Data.SqlDbType.NVarChar, System.Data.ParameterDirection.Input));


                        var command = new DBCommand();
                        command.ProcedureName = "UpdateDownLog";
                        command.DBParameters = param;

                        cmds.Add(command);
                    }

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteDownLog(int id) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("id", id, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("deleteDownLog", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void InsertDownLog(ProductionDownTimeLog log)
        {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("@headerId", log.HeaderId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("@lineId", log.LineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("@createdBy", log.CreatedBy, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("@modifiedBy", log.CreatedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    svc.ExecuteSPReader("InsertDownLog", param, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<DownTimeReason> GetProductionDownReason() {
            try {
                List<DownTimeReason> reasons = new List<DownTimeReason>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();

                    using (SqlDataReader read = svc.ExecuteSPReader("GetProductionDownReason", param, 0))
                    {
                        while (read.Read())
                        {
                            DownTimeReason reason = new DownTimeReason();
                            reason.Reason = read["Reason"].ToString();
                            reason.Spanish = read["Spanish"].ToString();

                            reasons.Add(reason);
                        }
                    }
                }
                return reasons;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ProductionMaterial>> GetProductionItemMaterials(int headerId, int lineId, string warehouse, string scheduleId, string itemId, string dueDate, string job = "") {
            var list = new List<ProductionMaterial>();

            using (var client = new HttpClient())
            {
                // Replace the URL with the actual endpoint you want to call
                string url = Config.CSIRestService + "/api/ProductionSchedule/GetProductionItemMaterials?warehouse=" + warehouse + "&scheduleId=" + scheduleId + "&itemId=" + itemId + "&dueDate=" + dueDate + "&job=" + job;

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductionMaterial>>(responseContent);

                    if (list.Count > 0) {
                        var sqlData = GetProductionMaterialById(headerId, lineId);
                        //var totalData = GetProductionMaterialById(headerId);
                        for (int i = 0; i < list.Count; i++) {
                            var matched = sqlData.Find(x => x.Material == list[i].Material);
                            //var totalMatched = totalData.Find(x => x.Material == list[i].Material);

                            if (matched != null) {
                                list[i].ScrapQty = matched.ScrapQty;
                                list[i].StartQty = matched.StartQty;
                                list[i].EndQty = matched.EndQty;
                                list[i].UsedQty = matched.UsedQty;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception($"Failed to get data from {url}. Status code: {response.StatusCode}");
                }
            }
            return list;
        }

        public List<ProductionMaterial> GetProductionMaterialById(int headerId, int? lineId = null)
        {
            try
            {
                List<ProductionMaterial> materials = new List<ProductionMaterial>();

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("headerId", headerId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    if (lineId != null)
                        param.Add(new DBParameter("lineId", lineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    using (SqlDataReader read = svc.ExecuteSPReader("GetProductionMaterialPerId", param, 0))
                    {
                        while (read.Read())
                        {
                            ProductionMaterial material = new ProductionMaterial();
                            material.Material = read["Material"].ToString();
                            material.ScrapQty = read["ScrapQty"] == DBNull.Value ? null : int.Parse(read["ScrapQty"].ToString());
                            material.StartQty = read["StartQty"] == DBNull.Value ? null : Decimal.Parse(read["StartQty"].ToString());
                            material.UsedQty = read["UsedQty"] == DBNull.Value ? null : Decimal.Parse(read["UsedQty"].ToString());
                            material.EndQty = read["EndQty"] == DBNull.Value ? null : Decimal.Parse(read["EndQty"].ToString());
                            material.CalculatedQty = read["CalculatedQty"] == DBNull.Value ? null : Decimal.Parse(read["CalculatedQty"].ToString());

                            materials.Add(material);
                        }
                    }
                }

                return materials;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void InsertProductionMaterial(List<ProductionMaterial> materials) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    //Clear Material Table
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("headerId", materials.FirstOrDefault().HeaderId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param.Add(new DBParameter("lineId", materials.FirstOrDefault().LineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));

                    var command = new DBCommand();
                    command.ProcedureName = "ClearProductionMaterialLogPerHeaderAndLine";
                    command.DBParameters = param;

                    cmds.Add(command);

                    foreach (ProductionMaterial m in materials)
                    {
                        var param2 = new List<DBParameter>();
                        param2.Add(new DBParameter("headerId", m.HeaderId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("lineId", m.LineId, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("material", m.Material, 250, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("description", m.Description, 250, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("scrapQty", m.ScrapQty, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("startQty", m.StartQty, 0, System.Data.SqlDbType.Decimal, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("usedQty", m.UsedQty, 0, System.Data.SqlDbType.Decimal, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("endQty", m.EndQty, 0, System.Data.SqlDbType.Decimal, System.Data.ParameterDirection.Input));
                        param2.Add(new DBParameter("createdBy", m.CreatedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));


                        var command2 = new DBCommand();
                        command2.ProcedureName = "InsertProductionMaterialLog";
                        command2.DBParameters = param2;

                        cmds.Add(command2);
                    }

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void PostProductionMaterial(List<ProductionMaterial> materials)
        {
            //ProductionScheduleReleaseMaterial(client, "S-LOCBAG", "8689","8267", "LOCBAG", "06-06-2023", "0.01");
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    
                    //foreach (var m in materials)
                    //{
                    //    var matTransactionCommand = new DBCommand();
                    //    var matTransaction = new ProductionParameter()
                    //    {
                    //        ProductionScheduleID = header.ProductionScheduleID,
                    //        WorkCenter = line.WorkCenterName,
                    //        TransDate = line.DueDate.ToString("MM-dd-yyyy"),
                    //        MaterialQty = Decimal.Parse(((decimal)m.CalculatedQty / (line.PalletCount * line.BagsPerPallet + line.BagCount)).ToString("N5")),
                    //        Item = header.ItemID,
                    //        Material = m.Material,
                    //        Warehouse = header.Site
                    //    };
                    //
                    //    var matParam = new List<DBParameter>();
                    //    matParam.Add(new DBParameter("@TaskID", 1720, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    //    matParam.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //    matParam.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(matParam), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //    matParam.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //    matParam.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //    matParam.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                    //
                    //    matTransactionCommand.ProcedureName = "PostTask";
                    //    matTransactionCommand.DBParameters = matParam;
                    //    cmds.Add(matTransactionCommand);
                    //}

                    if (cmds.Count > 0)
                    {
                        //svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Dictionary<int, string> ValidateJob(ProductionHeader header) {
            string error = "";

            Dictionary<int, string> pairs = new Dictionary<int, string>();

            //Check Line Level
            foreach (ProductionLine l in header.ProductionLines) {
                pairs.Add(l.ID, "");

                if ((l.StartTimeStamp == null || l.StartTimeStamp == default(DateTime)) || (l.EndTimeStamp == null || l.EndTimeStamp == default(DateTime)))
                {
                    pairs[l.ID] += "\u000d" + "Missing Start/End DateTime.";
                }
                else {
                    if ((int)((DateTime)l.EndTimeStamp).Subtract((DateTime)l.StartTimeStamp).TotalMinutes > 630)
                    {
                        pairs[l.ID] += "\u000d" + "Total Operation Time cannot be greater than 10 hours 30 minutes.";
                    }
                }

                if (l.PalletCount == 0 && l.BagCount == 0) {
                    pairs[l.ID] += "\u000d" + "Missing Pallet Or Bag Count.";
                }

                if (l.StartTimeStamp != null && l.EndTimeStamp != null && ((DateTime)l.StartTimeStamp) > ((DateTime)l.EndTimeStamp)) {
                    pairs[l.ID] += "\u000d" + "End DateTime should be later than Start DateTime.";
                }

                //Check DownTime
                var downs = GetDownTimeLog(l.ID);
                foreach (var d in downs) {
                    if (string.IsNullOrEmpty(d.CreatedBy) || string.IsNullOrEmpty(d.Reason)) {
                        pairs[l.ID] += "\u000d" + "Missing Operator Or Down Reason.";
                    }

                    if ((d.StartDownTime == null || d.StartDownTime == default(DateTime)) || (d.EndDownTime == null || d.EndDownTime == default(DateTime)))
                    {
                        pairs[l.ID] += "\u000d" + "Missing Start/End DownTime.";
                    }
                    else {
                        if ((int)((DateTime)d.EndDownTime).Subtract((DateTime)d.StartDownTime).TotalMinutes > 630)
                        {
                            pairs[l.ID] += "\u000d" + "Down Time cannot be greater than 10 hours 30 minutes.";
                        }
                    }

                    if (d.StartDownTime != null && d.EndDownTime != null && ((DateTime)d.StartDownTime) > ((DateTime)d.EndDownTime))
                    {
                        pairs[l.ID] += "\u000d" + "End DownTime should be later than Start DownTime.";
                    }
                }

                //Check Material   
                var materials = GetProductionMaterialById(header.ID, l.ID);
                foreach (var m in materials) {
                    var start = m.StartQty == null ? 0 : (decimal)m.StartQty;
                    var used = m.UsedQty == null ? 0 : (decimal)m.UsedQty;
                    var end = m.EndQty == null ? 0 : (decimal)m.EndQty;

                    if (start + used - end < 0) {
                        pairs[l.ID] += "\u000d" + "Material Calculation Invalid.";
                    }
                }
            }

            return pairs;
        }

        public static DateTime GetPacificStandardTime()
        {
            var utc = DateTime.UtcNow;
            TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utc, pacificZone);
            return pacificTime;
        }

        public void PostLine(ProductionHeader header) {
            try
            {
                var test = GetPacificStandardTime();
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    foreach (var line in header.ProductionLines)
                    {
                        TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                        DateTime? start = line.StartTimeStamp == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)line.StartTimeStamp), pstZone);
                        DateTime? end = line.EndTimeStamp == null ? null : TimeZoneInfo.ConvertTimeToUtc(((DateTime)line.EndTimeStamp), pstZone);

                        var param = new List<DBParameter>();
                        param.Add(new DBParameter("RunID", line.RunID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ID", line.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("PalletCount", line.PalletCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("BagCount", line.BagCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ScrapCount", line.ScrapCount, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("DownTime", line.DownTime, 0, System.Data.SqlDbType.Float, System.Data.ParameterDirection.Input));
                        //param.Add(new DBParameter("Reason", line.Reason, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("Status", "Complete", 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("StartTime", start, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("EndTime", end, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("Operator", line.Operator == "" ? null : line.Operator, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                        param.Add(new DBParameter("ModifiedBy", line.ModifiedBy, 100, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                        var command = new DBCommand();
                        command.ProcedureName = "SaveProductionBagJobs";
                        command.DBParameters = param;

                        cmds.Add(command);
                    }

                    foreach (var line in header.ProductionLines) {
                        if (
                            (header.Site.ToLower() == "lock") ||
                            (header.Site.ToLower() == "ont")
                            ) 
                        {
                            //Post WorkCenter
                            //if (line.ScrapCount > 0) {
                            //    var scrapTransactionCommand = new DBCommand();
                            //    var scrapTransaction = new ProductionParameter()
                            //    {
                            //        ProductionScheduleID = header.ProductionScheduleID,
                            //        WorkCenter = line.WorkCenterName,
                            //        TransDate = line.DueDate.ToString("MM-dd-yyyy"),
                            //        Qty = line.ScrapCount,
                            //        Item = header.ItemID,
                            //        UM = header.UM,
                            //        Warehouse = header.Site
                            //    };

                            //    var scrapParam = new List<DBParameter>();
                            //    scrapParam.Add(new DBParameter("@TaskID", 1710, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                            //    scrapParam.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            //    scrapParam.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(scrapTransaction), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            //    scrapParam.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            //    scrapParam.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            //    scrapParam.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                            //    scrapTransactionCommand.ProcedureName = "PostTask";
                            //    scrapTransactionCommand.DBParameters = scrapParam;
                            //    cmds.Add(scrapTransactionCommand);
                            //}

                            //Post Material
                            var materials = GetProductionMaterialById(header.ID, line.ID);
                            var materialChanged = false;
                            foreach (var m in materials)
                            {
                                if (m.CalculatedQty != null && m.CalculatedQty != 0)
                                {
                                    var matTransactionCommand = new DBCommand();
                                    var matTransaction = new ProductionParameter()
                                    {
                                        ProductionScheduleID = header.ProductionScheduleID,
                                        WorkCenter = line.WorkCenterName,
                                        TransDate = line.DueDate.ToString("MM-dd-yyyy"),
                                        MaterialQty = Decimal.Parse(((decimal)m.CalculatedQty / (line.PalletCount * line.BagsPerPallet + line.BagCount)).ToString("N5")),
                                        Item = header.ItemID,
                                        Material = m.Material,
                                        Warehouse = header.Site
                                    };

                                    var matParam = new List<DBParameter>();
                                    matParam.Add(new DBParameter("@TaskID", 1720, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                                    matParam.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    matParam.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(matTransaction), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    matParam.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    matParam.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    matParam.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                                    matTransactionCommand.ProcedureName = "PostTask";
                                    matTransactionCommand.DBParameters = matParam;
                                    cmds.Add(matTransactionCommand);

                                    materialChanged = true;
                                }


                                if (m.ScrapQty != null && m.ScrapQty != 0)
                                {
                                    var scrapTransactionCommand = new DBCommand();
                                    var scrapTransaction = new ProductionParameter()
                                    {
                                        ProductionScheduleID = header.ProductionScheduleID,
                                        WorkCenter = line.WorkCenterName,
                                        TransDate = line.DueDate.ToString("MM-dd-yyyy"),
                                        Qty = (int)m.ScrapQty,
                                        Item = m.Material,
                                        UM = header.UM,
                                        Warehouse = header.Site
                                    };

                                    var scrapParam = new List<DBParameter>();
                                    scrapParam.Add(new DBParameter("@TaskID", 1710, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                                    scrapParam.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    scrapParam.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(scrapTransaction), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    scrapParam.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    scrapParam.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                                    scrapParam.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                                    scrapTransactionCommand.ProcedureName = "PostTask";
                                    scrapTransactionCommand.DBParameters = scrapParam;
                                    cmds.Add(scrapTransactionCommand);
                                }
                            }

                            //Post Complete Transaction
                            var completeTransactionCommand = new DBCommand();
                            var completeTransaction = new ProductionParameter()
                            {
                                ProductionScheduleID = header.ProductionScheduleID,
                                WorkCenter = line.WorkCenterName,
                                TransDate = line.DueDate.ToString("MM-dd-yyyy"),
                                Qty = line.PalletCount * line.BagsPerPallet + line.BagCount,
                                Item = header.ItemID,
                                Warehouse = header.Site
                            };

                            var completeParam = new List<DBParameter>();
                            completeParam.Add(new DBParameter("@TaskID", 1700, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                            completeParam.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            completeParam.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(completeTransaction), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            completeParam.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            completeParam.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                            completeParam.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                            completeTransactionCommand.ProcedureName = "PostTask";
                            completeTransactionCommand.DBParameters = completeParam;
                            cmds.Add(completeTransactionCommand);
                        }
                    }

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void PostHeader(ProductionHeader header) {
            try
            {
                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    List<DBCommand> cmds = new List<DBCommand>();

                    var param2 = new List<DBParameter>();
                    param2.Add(new DBParameter("RunID", header.ID, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    param2.Add(new DBParameter("HeaderStatus", "Complete", 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));

                    var command2 = new DBCommand();
                    command2.ProcedureName = "SaveProductionBagJobs";
                    command2.DBParameters = param2;

                    header.UpdateStatus = true;

                    //var param = new List<DBParameter>();
                    //param.Add(new DBParameter("@TaskID", 5001, 0, System.Data.SqlDbType.Int, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("@Parameters", "", 1000, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("@TaskBody", Newtonsoft.Json.JsonConvert.SerializeObject(header), 0, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("@Warehouse", header.Site, 50, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("@OutputPath", "", 200, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));
                    //param.Add(new DBParameter("@scheduledon", DateTime.Now, 0, System.Data.SqlDbType.DateTime, System.Data.ParameterDirection.Input));

                    //var command = new DBCommand();
                    //command.ProcedureName = "PostTask";
                    //command.DBParameters = param;

                    cmds.Add(command2);
                    //cmds.Add(command);

                    if (cmds.Count > 0)
                    {
                        svc.ExecuteSPList(cmds, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GetUMForItem(string item) {
            try
            {
                string um = "";

                using (SqlService svc = new SqlService(Config.AzureDBConnectionString))
                {
                    var param = new List<DBParameter>();
                    param.Add(new DBParameter("item", item, 20, System.Data.SqlDbType.VarChar, System.Data.ParameterDirection.Input));


                    using (SqlDataReader read = svc.ExecuteSPReader("GetItemUM", param, 0))
                    {
                        while (read.Read())
                        {
                            um = read["UM"].ToString();
                        }
                    }
                }
                return um;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ProductionMaterial>> GetAllProductionItemMaterials(string warehouse, string scheduleId, string itemId, string dueDate)
        {
            var list = new List<ProductionMaterial>();

            using (var client = new HttpClient())
            {
                // Replace the URL with the actual endpoint you want to call
                string url = Config.CSIRestService + "/api/ProductionSchedule/GetProductionItemMaterials?warehouse=" + warehouse + "&scheduleId=" + scheduleId + "&itemId=" + itemId + "&dueDate=" + dueDate;

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductionMaterial>>(responseContent);
                }
                else
                {
                    throw new Exception($"Failed to get data from {url}. Status code: {response.StatusCode}");
                }
            }
            return list;
        }
    }

    public class PostMaterial {
        public string ScheduleID { get; set; }
        public string ItemID { get; set; }
        public string MaterialID { get; set; }
        public string WorkCenter { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal UnitPerBag { get; set; }
    }
}
