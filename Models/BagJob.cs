namespace KGP.Models
{
    public class ProductionHeader 
    {
        public int ID { get; set; }
        public string CSIID { get; set; }
        public string RunType { get; set; }
        public DateTime DueDate { get; set; }
        /// <summary>
        /// This is Required Pallet Quantity to produce
        /// </summary>
        public int PalletQty { get; set; }
        /// <summary>
        /// This is Required Bag Quantity to produce when it cannot meet the quantity for the pallet.
        /// </summary>
        public int BagQty { get; set; }
        public string Site { get; set; }
        public string WorkCenter { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string ItemID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int CompletedQty { get; set; }
        public string ProductionScheduleID { get; set; }
        public List<ProductionLine> ProductionLines { get; set; }
        public bool UpdateStatus { get; set; }
    }
    public class ProductionLine
    {
        public int ID { get; set; }
        public int RunID { get; set; }
        public string ItemID { get; set; }
        public string Description { get; set; }
        public int BagQty { get; set; }
        public DateTime DueDate { get; set; }
        public string Operator { get; set; }
        public DateTime? StartTimeStamp { get; set; }
        public DateTime? EndTimeStamp { get; set; }
        public int PalletCount { get; set; }
        public int BagCount { get; set; }
        public int ScrapCount { get; set; }
        public float DownTime { get; set; }
        public string Reason { get; set; }
        public string WorkCenter { get; set; }
        public string Status { get; set; }
        public string Site { get; set; }
        public int CompletedQty { get; set; }
        /// <summary>
        /// This is requested pallet quantity
        /// </summary>
        public int PalletQty { get; set; }
        /// <summary>
        ///  This is requested bag quantity
        /// </summary>
        public int Qty { get; set; }
        public int BagsPerPallet { get; set; }
        public bool IsBaler { get; set; }
        public string ProductionScheduleID { get; set; }
        public string ModifiedBy { get; set; }
        public List<ProductionDownTimeLog> StoppedBaler { get; set; }
    }
}
