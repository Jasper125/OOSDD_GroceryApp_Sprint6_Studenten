using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];

        public GroceryListItemsRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS GroceryListItem (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [GroceryListId] INTEGER NOT NULL,
                    [ProductId] INTEGER NOT NULL,
                    [Quantity] INTEGER NOT NULL
                )");

            List<string> insertQueries =
            [
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Quantity) VALUES(1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Quantity) VALUES(1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Quantity) VALUES(1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Quantity) VALUES(2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Quantity) VALUES(2, 2, 5)"
            ];

            InsertMultipleWithTransaction(insertQueries);

            GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();

            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Quantity FROM GroceryListItem";
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int quantity = reader.GetInt32(3);

                    groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, quantity));
                }
            }

            CloseConnection();
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> listItems = [];

            string selectQuery = @"SELECT Id, GroceryListId, ProductId, Quantity 
                                   FROM GroceryListItem 
                                   WHERE GroceryListId = @GroceryListId;";

            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("@GroceryListId", groceryListId);

                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int gListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int quantity = reader.GetInt32(3);

                    listItems.Add(new GroceryListItem(id, gListId, productId, quantity));
                }
            }

            CloseConnection();
            return listItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            const string insertQuery = @"
                INSERT INTO GroceryListItem(GroceryListId, ProductId, Quantity)
                VALUES(@GroceryListId, @ProductId, @Quantity)
                RETURNING Id;";

            OpenConnection();

            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Quantity", item.Amount);

                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            CloseConnection();
            return item;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            const string updateQuery = @"
                UPDATE GroceryListItem 
                SET GroceryListId = @GroceryListId, 
                    ProductId = @ProductId, 
                    Quantity = @Quantity
                WHERE Id = @Id;";

            OpenConnection();

            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Quantity", item.Amount);

                command.ExecuteNonQuery();
            }

            CloseConnection();
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            const string deleteQuery = @"DELETE FROM GroceryListItem WHERE Id = @Id;";
            OpenConnection();

            using (SqliteCommand command = new(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.ExecuteNonQuery();
            }

            CloseConnection();
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            GroceryListItem? item = null;

            const string selectQuery = @"SELECT Id, GroceryListId, ProductId, Quantity 
                                         FROM GroceryListItem 
                                         WHERE Id = @Id;";

            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using SqliteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int quantity = reader.GetInt32(3);

                    item = new GroceryListItem(Id, groceryListId, productId, quantity);
                }
            }

            CloseConnection();
            return item;
        }
    }
}
