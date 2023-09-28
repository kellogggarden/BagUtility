namespace KGP.Models
{
    public class Filter {
        public string Warehouse { get; set; }
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Page { get; set; }
    }

    public class OpenOrder
    {
        public string Warehouse { get; set; }
        public string SalesOrder { get; set; }
        public string CustomerPO { get; set; }
        public string Action { get; set; }
        public string PalletCount { get; set; }
        public string DeliveryStatus { get; set; }
        public string Appointment { get; set; }
        public string ReqCan { get; set; }
        public string ConfCan { get; set; }
        public string OpComments { get; set; }
        public string SalesComment { get; set; }
        public DateTime? SalesOrderDate { get; set; }
        public DateTime? OriginalReqDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string DaysLate { get; set; }
        public string DaysToShip { get; set; }
        public string Customer { get; set; }
        public string ShipToAddr { get; set; }
        public string ShipToCity { get; set; }
        public string St { get; set; }
        public string ShipToZip { get; set; }
        public string Phone { get; set; }
        public string CustomerNumber { get; set; }
        public string OrderStatus { get; set; }
        public string ShipVIA { get; set; }
        public string StatusColor { get; set; }
        public bool IsChanged { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
    }
}
