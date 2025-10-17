using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];

        public ProductRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS Product (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [Name] NVARCHAR(80) UNIQUE NOT NULL,
                    [Stock] INTEGER NOT NULL,
                    [ShelfLife] DATE NOT NULL,
                    [Price] DECIMAL(10,2) NOT NULL
                )");

            OpenConnection();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Product;";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            CloseConnection();

            if (count == 0)
            {
                List<string> insertQueries =
                [
                    @"INSERT INTO Product(Name, Stock, ShelfLife, Price) VALUES('Melk', 300, '2025-09-25', 0.95)",
                    @"INSERT INTO Product(Name, Stock, ShelfLife, Price) VALUES('Kaas', 100, '2025-09-30', 7.98)",
                    @"INSERT INTO Product(Name, Stock, ShelfLife, Price) VALUES('Brood', 400, '2025-09-12', 2.19)",
                    @"INSERT INTO Product(Name, Stock, ShelfLife, Price) VALUES('Cornflakes', 0, '2025-12-31', 1.48)"
                ];
                InsertMultipleWithTransaction(insertQueries);
            }

            GetAll();
        }

        public List<Product> GetAll()
        {
            products.Clear();
            const string query = "SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product";

            OpenConnection();
            using (var cmd = new SqliteCommand(query, Connection))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);

                    products.Add(new Product(id, name, stock, shelfLife, price));
                }
            }
            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            Product? product = null;
            const string query = "SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product WHERE Id = @Id";

            OpenConnection();
            using (var cmd = new SqliteCommand(query, Connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int pid = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);

                    product = new Product(pid, name, stock, shelfLife, price);
                }
            }
            CloseConnection();
            return product;
        }

        public Product Add(Product item)
        {
            const string query = @"
                INSERT INTO Product(Name, Stock, ShelfLife, Price)
                VALUES(@Name, @Stock, @ShelfLife, @Price)
                RETURNING Id;";

            OpenConnection();
            using (var cmd = new SqliteCommand(query, Connection))
            {
                cmd.Parameters.AddWithValue("@Name", item.Name);
                cmd.Parameters.AddWithValue("@Stock", item.Stock);
                cmd.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Price", item.Price);

                item.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            CloseConnection();
            return item;
        }

        public Product? Update(Product item)
        {
            const string query = @"
                UPDATE Product 
                SET Name = @Name,
                    Stock = @Stock,
                    ShelfLife = @ShelfLife,
                    Price = @Price
                WHERE Id = @Id;";

            OpenConnection();
            using (var cmd = new SqliteCommand(query, Connection))
            {
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.Parameters.AddWithValue("@Name", item.Name);
                cmd.Parameters.AddWithValue("@Stock", item.Stock);
                cmd.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Price", item.Price);

                cmd.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            const string query = "DELETE FROM Product WHERE Id = @Id";

            OpenConnection();
            using (var cmd = new SqliteCommand(query, Connection))
            {
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }
    }
}
