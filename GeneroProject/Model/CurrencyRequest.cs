namespace GeneroProject.Model
{
    public class CurrencyRequest
    {
        public string BaseCurrency { get; set; }
        public List<string> Currencies { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
