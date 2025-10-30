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
            if (!File.Exists(TableFile))
            {
                var list = Enumerable.Range(1, 20)
                            .Select(i => new TableInfo { Id = "A" + i })
                            .ToList(); File.WriteAllText(TableFile, JsonSerializer.Serialize(list, _opt));
                return list;
            }
            try
            {
                var txt = File.ReadAllText(TableFile);
                var list = JsonSerializer.Deserialize<List<TableInfo>>(txt, _opt);
                return list ?? new List<TableInfo>();
            }
            catch
            {
                return new List<TableInfo>();
            }
        }
        private void SaveTables(List<TableInfo> tables)
        {
            File.WriteAllText(TableFile, JsonSerializer.Serialize(tables, _opt));
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
            var table = LoadTables();
            var tab = table.FirstOrDefault(t => string.Equals(t.Id, tableID, StringComparison.OrdinalIgnoreCase));
            if (tab == null) return false;
            if (tab.IsOccuped) return false;

            tab.IsOccuped = true;
            tab.OccupiedById = orderID;
            tab.Since = DateTime.UtcNow;
            SaveTables(table);

            var orders = ReadOrders();
            var o = orders.FirstOrDefault(x => x.Id == orderID);

            if (o != null)
            {
                o.AssignedTable = tableID;
                o.Status = "Seated";
                File.WriteAllText(Path.Combine(_dataDir, "orders.json"), JsonSerializer.Serialize(orders, _opt));
            }
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
    }
}
