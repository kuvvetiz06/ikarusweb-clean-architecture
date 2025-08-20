namespace IKARUSWEB.UI.Models.Currencies
{
    public sealed class CurrencyViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal CurrencyMultiplier { get; set; }
        public decimal Rate { get; set; }
    }
}
