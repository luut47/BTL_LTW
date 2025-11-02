using Microsoft.AspNetCore.Hosting;
using BTL_LTW.Models;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Microsoft.AspNetCore.WebUtilities;

namespace BTL_LTW.Services
{
    public class FileStorage : IStorage
    {
        private readonly string _dataDir;
        private readonly JsonSerializerOptions _opt = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private class MenuFile
        {
            public List<MenuItem> Items { get; set; } = new();
        }

        public FileStorage(IWebHostEnvironment env)
        {
            _dataDir = Path.Combine(env.ContentRootPath, "data");
            if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
        }

        // -----------------------
        // Orders (sync)
        // -----------------------
        private List<Order> ReadOrders()
        {
            var file = Path.Combine(_dataDir, "orders.json");
            if (!File.Exists(file)) return new List<Order>();
            var txt = File.ReadAllText(file);
            try
            {
                return JsonSerializer.Deserialize<List<Order>>(txt, _opt) ?? new List<Order>();
            }
            catch
            {
                return new List<Order>();
            }
        }

        public List<Order> GetOrders()
        {
            return ReadOrders();
        }

        public Order? GetOrder(string id)
        {
            var orders = ReadOrders();
            return orders.FirstOrDefault(o => o.Id == id);
        }

        public Order CreateOrder(Order o)
        {
            // đọc menu (categories) và flatten
            var categories = LoadMenuCategories();
            var menuDict = categories.SelectMany(c => c.Items).ToDictionary(mi => mi.Id, mi => mi);

            // validate + gán giá từ menu server-side
            decimal total = 0m;
            foreach (var it in o.Items)
            {
                if (!menuDict.TryGetValue(it.MenuItemId, out var mi))
                    throw new Exception($"Menu item không tồn tại (id={it.MenuItemId})");

                it.MenuItemName = mi.Name;
                it.UnitPrice = mi.Price;
                if (it.Qty <= 0) it.Qty = 1;
                total += it.UnitPrice * it.Qty;
            }

            o.Total = total;
            o.Id = Guid.NewGuid().ToString();
            o.CreatedAt = DateTime.UtcNow;
            o.Status = "Pending";

            var orders = ReadOrders();
            orders.Add(o);
            File.WriteAllText(Path.Combine(_dataDir, "orders.json"), JsonSerializer.Serialize(orders, _opt));
            return o;
        }

        public bool UpdateOrderItemStatus(string orderId, int menuItemId, string newStatus)
        {
            var orders = ReadOrders();
            var o = orders.FirstOrDefault(x => x.Id == orderId);
            if (o == null) return false;
            var item = o.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item == null) return false;
            item.Status = newStatus;
            if (o.Items.All(i => i.Status == "Served")) o.Status = "Closed";
            File.WriteAllText(Path.Combine(_dataDir, "orders.json"), JsonSerializer.Serialize(orders, _opt));
            return true;
        }

        // -----------------------
        // Menu load (sync)
        // -----------------------
        public List<MenuItem> LoadMenu()
        {
            var file = Path.Combine(_dataDir, "menu.json");
            if (!File.Exists(file)) return new List<MenuItem>();
            var txt = File.ReadAllText(file);
            try
            {
                var doc = JsonSerializer.Deserialize<MenuFile>(txt, _opt);
                return doc?.Items ?? new List<MenuItem>();
            }
            catch
            {
                return new List<MenuItem>();
            }
        }

        public List<MenuCategory> LoadMenuCategories()
        {
            var file = Path.Combine(_dataDir, "menu.json");
            if (!File.Exists(file)) return new List<MenuCategory>();

            var txt = File.ReadAllText(file);
            try
            {
                using var doc = JsonDocument.Parse(txt);
                if (doc.RootElement.TryGetProperty("categories", out var catsEl))
                {
                    var result = new List<MenuCategory>();
                    foreach (var catEl in catsEl.EnumerateArray())
                    {
                        var cat = new MenuCategory
                        {
                            Id = catEl.GetProperty("id").GetInt32(),
                            Name = catEl.GetProperty("name").GetString()
                        };

                        if (catEl.TryGetProperty("items", out var itemsEl))
                        {
                            foreach (var itemEl in itemsEl.EnumerateArray())
                            {
                                var menuItem = new MenuItem
                                {
                                    Id = itemEl.GetProperty("id").GetInt32(),
                                    Name = itemEl.GetProperty("name").GetString(),
                                    Price = itemEl.GetProperty("price").GetDecimal()
                                };
                                cat.Items.Add(menuItem);
                            }
                        }

                        result.Add(cat);
                    }
                    return result;
                }
                else
                {
                    // fallback: old flat format
                    var old = JsonSerializer.Deserialize<List<MenuItem>>(txt, _opt);
                    var fallback = new MenuCategory { Id = 1, Name = "Đồ uống", Items = old ?? new List<MenuItem>() };
                    return new List<MenuCategory> { fallback };
                }
            }
            catch
            {
                return new List<MenuCategory>();
            }
        }

        // -----------------------
        // Seed (sync)
        // -----------------------
        public void Seed()
        {
            var menuFile = Path.Combine(_dataDir, "menu.json");
            if (!File.Exists(menuFile))
            {
                var ex = new MenuFile
                {
                    Items = new List<MenuItem>
                    {
                        new() {Id=1, Name="Ca phe sua", Price = 30000},
                        new() {Id=2, Name="Ca phe den", Price = 25000},
                        new() {Id=3, Name="Tra dao", Price = 35000}
                    }
                };
                File.WriteAllText(menuFile, JsonSerializer.Serialize(ex, _opt));
            }

            var ordersFile = Path.Combine(_dataDir, "orders.json");
            if (!File.Exists(ordersFile))
            {
                File.WriteAllText(ordersFile, "[]");
            }
        }

        //------Table Management
        private string TableFile => Path.Combine(_dataDir, "tables.json");
        public List<TableInfo> LoadTables()
        {
            Directory.CreateDirectory(_dataDir);

            List<TableInfo> seed()
            {
                // A1..A10, B1..B10 (tuỳ bạn đổi)
                var list = new List<TableInfo>();
                for (int i = 1; i <= 10; i++) list.Add(new TableInfo { Id = $"A{i}", IsOccuped = false });
                for (int i = 1; i <= 10; i++) list.Add(new TableInfo { Id = $"B{i}", IsOccuped = false });
                SaveTables(list);
                return list;
            }

            if (!System.IO.File.Exists(TableFile))
                return seed();

            try
            {
                var txt = System.IO.File.ReadAllText(TableFile);
                if (string.IsNullOrWhiteSpace(txt)) return seed();

                var list = JsonSerializer.Deserialize<List<TableInfo>>(txt, _opt);
                if (list == null || list.Count == 0) return seed();

                // đảm bảo các trường mặc định không null
                foreach (var t in list)
                    t.IsOccuped = t.IsOccuped; 

                return list;
            }
            catch
            {
                // file hỏng → hồi phục
                return seed();
            }
        }
        private void SaveTables(List<TableInfo> tables)
        {
            Directory.CreateDirectory(_dataDir);
            System.IO.File.WriteAllText(TableFile, JsonSerializer.Serialize(tables, _opt));
        }
        public List<TableInfo> GetTables()
        {
            return LoadTables();
        }

        public int GetOccupiedCount()
        {
            var t = LoadTables();
            return t.Count(x => x.IsOccuped);
        }
        public bool AssignTableOrder(string tableID, string orderID)
        {
            // 1) Lấy đơn để kiểm tra Takeaway
            var orders = ReadOrders();
            var o = orders.FirstOrDefault(x => x.Id == orderID);
            if (o == null) return false;

            // NEW: chặn gán bàn nếu đơn mang về
            if (o.IsTakeAway) return false; 
            ;

            // 2) Gán bàn như cũ
            var tables = LoadTables();
            var tab = tables.FirstOrDefault(t => string.Equals(t.Id, tableID, StringComparison.OrdinalIgnoreCase));
            if (tab == null) return false;
            if (tab.IsOccuped) return false;

            tab.IsOccuped = true;
            tab.OccupiedById = orderID;
            tab.Since = DateTime.UtcNow;
            SaveTables(tables);

            o.AssignedTable = tableID;
            o.Status = "Seated";
            File.WriteAllText(Path.Combine(_dataDir, "orders.json"), JsonSerializer.Serialize(orders, _opt));
            return true;
        }

        public bool releaseTable(string tableID)
        {
            var tables = LoadTables();
            var tab = tables.FirstOrDefault(t => string.Equals(t.Id, tableID, StringComparison.OrdinalIgnoreCase));
            if(tab == null)
            {
                return false;
            }
            tab.IsOccuped = false;
            tab.OccupiedById = null;
            tab.Since = null;
            SaveTables(tables);
            return true;
        }
        public bool MarkOrder(string orderID)
        {
            var orders = ReadOrders();
            var o = orders.FirstOrDefault(x => x.Id == orderID);
            if ( o == null)
            {
                return false;
            }
            o.Status = "Completed";
            o.IsCompleted = true;
            if (!string.IsNullOrEmpty(o.AssignedTable))
            {
                releaseTable(o.AssignedTable);
            }

            File.WriteAllText(Path.Combine(_dataDir, "orders.json"), JsonSerializer.Serialize(orders, _opt));
            return true;
        }
        // ==== Reservations (sync) ====
        private string ReservationsFile => Path.Combine(_dataDir, "reservations.json");

        public List<Reservation> ReadReservations()
        {
            Directory.CreateDirectory(_dataDir);
            if (!System.IO.File.Exists(ReservationsFile))
            {
                var empty = new List<Reservation>();
                System.IO.File.WriteAllText(ReservationsFile,
                    JsonSerializer.Serialize(empty, _opt));
                return empty;
            }
            try
            {
                var txt = System.IO.File.ReadAllText(ReservationsFile);
                return JsonSerializer.Deserialize<List<Reservation>>(txt, _opt) ?? new List<Reservation>();
            }
            catch
            {
                return new List<Reservation>();
            }
        }

        public void SaveReservations(List<Reservation> list)
        {
            Directory.CreateDirectory(_dataDir);
            System.IO.File.WriteAllText(ReservationsFile,
                JsonSerializer.Serialize(list, _opt));
        }

        public bool AssignTableReservation(string reservationId, string tableId)
        {
            var resv = ReadReservations();                        // bạn đã có ReadReservations()
            var r = resv.FirstOrDefault(x => x.Id == reservationId);
            if (r == null) return false;

            var tables = LoadTables();
            var tab = tables.FirstOrDefault(t => string.Equals(t.Id, tableId, StringComparison.OrdinalIgnoreCase));
            if (tab == null) return false;
            if (tab.IsOccuped) return false;                      // bận rồi

            // chiếm bàn
            tab.IsOccuped = true;
            tab.OccupiedById = "RES:" + reservationId;           // phân biệt với Order
            tab.Since = DateTime.UtcNow;
            SaveTables(tables);

            // cập nhật reservation
            r.AssignedTable = tableId;
            r.Status = "Confirmed";
            System.IO.File.WriteAllText(Path.Combine(_dataDir, "reservations.json"),
                JsonSerializer.Serialize(resv, _opt));
            return true;
        }

        public bool ReleaseTableByReservation(string reservationId)
        {
            var resv = ReadReservations();
            var r = resv.FirstOrDefault(x => x.Id == reservationId);
            if (r == null || string.IsNullOrEmpty(r.AssignedTable)) return false;

            var tables = LoadTables();
            var tab = tables.FirstOrDefault(t => string.Equals(t.Id, r.AssignedTable, StringComparison.OrdinalIgnoreCase));
            if (tab == null) return false;

            tab.IsOccuped = false;
            tab.OccupiedById = null;
            tab.Since = null;
            SaveTables(tables);

            r.AssignedTable = null;
            r.Status = "Pending";
            System.IO.File.WriteAllText(Path.Combine(_dataDir, "reservations.json"),
                JsonSerializer.Serialize(resv, _opt));
            return true;
        }
        public Order CreateOrderFromReservation(string reservationId, List<(int menuItemId, int qty, string? note)> items)
        {
            var reservations = ReadReservations();
            var r = reservations.FirstOrDefault(x => x.Id == reservationId);
            if (r == null) throw new InvalidOperationException("Reservation not found");

            if (string.IsNullOrWhiteSpace(r.AssignedTable))
                throw new InvalidOperationException("Reservation has no assigned table");

            var o = new Order
            {
                Id = Guid.NewGuid().ToString("N"),
                TableToken = r.AssignedTable,            // dùng mã bàn làm token hiển thị
                CustomerName = r.CustomerName,
                CustomerPhone = r.Phone,
                CustomerAddress = r.Email ?? "",
                CreatedAt = DateTime.UtcNow,
                IsTakeAway = false,
                AssignedTable = r.AssignedTable,         // gán bàn ngay từ đầu
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            foreach (var (menuItemId, qty, note) in items)
            {
                o.Items.Add(new OrderItem
                {
                    MenuItemId = menuItemId,
                    Qty = Math.Max(1, qty),
                    Note = note ?? ""
                });
            }

            var created = CreateOrder(o);

            r.LinkOrderId = created.Id;
            r.Status = "Seated"; // hoặc "Ordered" tuỳ bạn
            SaveReservations(reservations);

            return created;
        }

    }
}
