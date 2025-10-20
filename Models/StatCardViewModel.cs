namespace ClaimIT.Models
{
    public class StatCardViewModel
    {
        public string CardTitle { get; set; } = string.Empty;
        public string CardValue { get; set; } = string.Empty;
        public string CardTrend { get; set; } = string.Empty;
        public string CardIcon { get; set; } = string.Empty;
        public string CardBackground { get; set; } = "#E8ECEF";
        public string ValueColor { get; set; } = "#2C3E50";
        public string TrendColor { get; set; } = "#27AE60";
    }
}