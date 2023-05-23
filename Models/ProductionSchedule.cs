namespace KGP.Models
{
    public class ProductionSchedule
    {
        public int Id { get; set; }
        public string ProductionScheduleId { get; set; }
        public string ItemId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string WorkSpace { get; set; }
    }

    public class ScheduleAssign {
        public int Id { get; set; }
        public string WorkSpace { get; set; }
        public int Quantity { get; set; }
    }

    public class AddProductLineParam
    {
        public int Id { get; set; }
        public string WorkCenter { get; set; }
        public int PalletQty { get; set; }
        public int Qty { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class CSIProductionSchedule
    {
        public string ScheduleID { get; set; }
        public string Item { get; set; }
        public int ReleasedQty { get; set; }
        public int CompletedQty { get; set; }
        public int RemainingQty { get; set; }
        public int ScrapQty { get; set; }
        public DateTime DueDdate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class LoadScheduleParam { 
        public string Warehouse { get; set; }
        public List<CSIProductionSchedule> Schedules { get; set; }
    }

    public class ProductionMaterial
    {
        public int HeaderId { get; set; }
        public int LineId { get; set; }
        public string Material { get; set; }
        public string Description { get; set; }
        public string UM { get; set; }
        public int? StartQty { get; set; }
        public int? UsedQty { get; set; }
        public int? EndQty { get; set; }
        public string CreatedBy { get; set; }
        public int? Total { get; set; }
    }

    public class ProductionCSIMapping {
        public string ScheduleID { get; set; }
        public string Item { get; set; }
        public int CompletedQty { get; set; }
        public int ScrapQty { get; set; }
        public DateTime DueDdate { get; set; }
        public string WorkCenter { get; set; }
    }
}
