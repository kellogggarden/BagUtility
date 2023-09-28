namespace KGP.Models
{
    public class ProductionDownTimeLog
    {
        public int Id { get; set; }
        public DateTime? StartDownTime { get; set; }
        public DateTime? EndDownTime { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int HeaderId { get; set; }
        public int LineId { get; set; }
        public int Bags { get; set; }
        public int Scraps { get; set; }
        public int Pallet { get; set; }
        public int? Baler { get; set; }
        public string Note { get; set; }
    }

    public class DownTimeReason { 
        public string Reason { get; set; }
        public string Spanish { get; set; }
    }
}
