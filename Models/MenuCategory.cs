namespace BTL_LTW.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<MenuItem> Items { get; set; } = new();
    }
}
