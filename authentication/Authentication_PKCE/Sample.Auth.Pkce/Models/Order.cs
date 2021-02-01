namespace Sample.Auth.Pkce.Models
{
    public class classOrderDuration
    {
        public string DurationType { get; set; }
    }

    public class Order
    {
        // Unique id of the instrument to place the order for.
        public int Uic { get; set; }
        public string BuySell { get; set; }
        public string AssetType { get; set; }
        public int Amount { get; set; }
        public decimal OrderPrice { get; set; }
        public string OrderType { get; set; }
        public string OrderRelation { get; set; }
        public bool ManualOrder { get; set; }
        public classOrderDuration OrderDuration { get; set; }
        public string AccountKey { get; set; }
    }
}
