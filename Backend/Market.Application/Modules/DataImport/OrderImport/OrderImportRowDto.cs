namespace Market.Application.Modules.DataImport.OrderImport
{
    public sealed class OrderImportRowDto
    {
        public int? OrderId { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? DiningTableId { get; set; }

        public int? MealId { get; set; }

        public int? Quantity { get; set; }

        public decimal? UnitPrice { get; set; }
    }
}