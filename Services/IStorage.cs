using BTL_LTW.Models;
using System.Collections.Generic;

namespace BTL_LTW.Services
{
    public interface IStorage
    {
        // Menu & categories 
        List<MenuItem> LoadMenu();
        List<MenuCategory> LoadMenuCategories();

        // Orders 
        Order CreateOrder(Order o);
        List<Order> GetOrders();
        Order? GetOrder(string id);

        // Cập nhật status
        bool UpdateOrderItemStatus(string orderId, int menuItemId, string newStatus);

        // Seed (tạo file mẫu nếu chưa có)
        void Seed();
        public List<TableInfo> GetTables();
        public int GetOccupiedCount();
        public bool AssignTableOrder(string tableID, string orderID);
        public bool MarkOrder(string orderID);
    }
}
