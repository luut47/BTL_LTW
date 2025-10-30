namespace BTL_LTW.Models
{
    public class TableInfo
    {
        public string Id { get; set; } = "";// A1 -> A20
        public bool IsOccuped { get; set; } = false;
        public string? OccupiedById { get; set; }
        public DateTime? Since { get; set; }
    }
}
