using KGP.Models;
using System.Security.Cryptography;

namespace KGP.Services
{
    public interface IOrderService
    {
        List<OpenOrder> GetOrders(Filter filter);
        void Save(List<OpenOrder> orders);
        void InsertProductionLine(ProductionLine bag);

        List<ProductionLine> GetProductionLines(int? id = null, string siteRef = null, string workCenter = null, string status = null, string op = null);
        List<ProductionLine> GetAllProductionLines(int? id = null, string siteRef = null, string workCenter = null, string status = null, string op = null);

        List<string> GetWorkCenterBySite(string site);
        List<string> GetSiteRefs();
        List<ProductionHeader> GetAllSubmittedProductHeaders(string site, string filter, string direction);
        void SaveProductionLine(ProductionLine line);
        void SaveProductionLines(ProductionHeader header);
        List<ProductionSchedule> GetProductionSchedulebyID(string id);
        void AssignProductionSchedules(List<ScheduleAssign> schedules);
        public int AssingLineToProductionHeader(AddProductLineParam param);
        public Task LoadProuctScheduelToSql(LoadScheduleParam sParam);
        public void DeleteProductionLine(int lineId);
        public List<ProductionDownTimeLog> GetDownTimeLog(int lineId);
        public List<string> GetOperators(string site);
        public void SaveDownLogs(List<ProductionDownTimeLog> logs);
        public void DeleteDownLog(int id);
        public void InsertDownLog(ProductionDownTimeLog log);
        public void InsertOrUpdateBalerDownLog(ProductionLine log);
        public List<DownTimeReason> GetProductionDownReason();
        public Task<List<ProductionMaterial>> GetProductionItemMaterials(int headerId, int lineId, string warehouse, string scheduleId, string itemId, string dueDate, string job = "");
        public void InsertProductionMaterial(List<ProductionMaterial> materials);
        public Dictionary<int, string> ValidateJob(ProductionHeader header);
        public void PostLine(ProductionHeader header);
        public void PostHeader(ProductionHeader header);
        public void DeleteProductionHeader(int id);
        public void PostProductionMaterial(List<ProductionMaterial> materials);
        public Task<List<ProductionMaterial>> GetAllProductionItemMaterials(string warehouse, string scheduleId, string itemId, string dueDate);

    }
}
