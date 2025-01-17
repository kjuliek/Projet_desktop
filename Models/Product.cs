using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Projet_desktop.Models
{
    internal class ProductType
    {
        public int          ProductTypeID  { get; set; }
        public string       Name           { get; set; }
        public int          ProductionTime { get; set; }
        public int          ProductionRate { get; set; }
        public int          Price          { get; set; }
        public BuildingType BuildingType   { get; set; }
        public ProductType() 
        {
            Name         = "";
            BuildingType = new();
        }
        public ProductType(int product_type_id, string name, int production_time, int production_rate, int price, BuildingType building_type)
        {
            ProductTypeID  = product_type_id;
            Name           = name;
            ProductionTime = production_time;
            ProductionRate = production_rate;
            Price          = price;
            BuildingType   = building_type;
        }
        public ProductType(int product_type_id)
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT name, production_time, production_rate, price, building_type_id FROM product_types WHERE product_type_id = @product_type_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@product_type_id", product_type_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                ProductTypeID  = product_type_id;
                Name           = reader.GetString(reader.GetOrdinal("name"));
                ProductionTime = reader.GetInt32(reader.GetOrdinal("production_time"));
                ProductionRate = reader.GetInt32(reader.GetOrdinal("production_rate"));
                Price          = reader.GetInt32(reader.GetOrdinal("price"));
                BuildingType   = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
            }
            else
            {
                throw new Exception($"Utilisateur avec introuvable.");
            }
        }

        public static List<ProductType> LoadProductTypeFromDB()
        {
            List<ProductType> product_types = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM product_types";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          product_type_id  = reader.GetInt32(reader.GetOrdinal("product_type_id"));
                string       name             = reader.GetString(reader.GetOrdinal("name"));
                int          production_time  = reader.GetInt32(reader.GetOrdinal("production_time"));
                int          production_rate  = reader.GetInt32(reader.GetOrdinal("production_rate"));
                int          price            = reader.GetInt32(reader.GetOrdinal("price"));
                BuildingType building_type    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));

                product_types.Add(new(product_type_id, name, production_time, production_rate, price, building_type));
            }
            return product_types;
        }
    }

    internal class Product
    {
        public int         ProductID { get; set; }
        public ProductType ProductType { get; set; }
        public Product(int product_id, ProductType product_type)
        {
            ProductID   = product_id;
            ProductType = product_type;
        }
        public Product(int product_id)
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT product_type_id FROM products WHERE product_id = @product_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@product_id", product_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                ProductType = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));
            }
            else
            {
                throw new Exception($"Utilisateur avec introuvable.");
            }
        }
        public Product(int product_type_id, int products_count)
        {
            ProductID   = products_count + 1;
            ProductType = new(product_type_id);
        }
        public static List<Product> LoadProductFromDB()
        {
            List<Product> products = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM products";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int product_id           = reader.GetInt32(reader.GetOrdinal("product_id"));
                ProductType product_type = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));

                products.Add(new(product_id, product_type));
            }
            return products;
        }

        public void SaveProductToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO products (product_id, product_type_id)
                VALUES (@product_id, @product_type_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);

            connection.Open();
            command.Parameters.AddWithValue("@product_id", ProductID);
            command.Parameters.AddWithValue("@product_type_id", ProductType.ProductTypeID);
            command.Parameters.AddWithValue("@building_type_id", ProductType.BuildingType.BuildingTypeID);
            command.ExecuteNonQuery();
        }
    }
}
