namespace IKARUSWEB.UI.Models.Currencies
{
    public sealed class CreateCurrencyRequest
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal CurrencyMultiplier { get; set; } = 1m;
        public decimal Rate { get; set; } = 0m;
    }
}
