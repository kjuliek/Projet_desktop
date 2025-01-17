using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Projet_desktop.Models
{
    internal class BuildingType
    {
        public int    BuildingTypeID { get; set; }
        public string Type           { get; set; }
        public int    PurchasePrice  { get; set; }
        public int    BuildTime      { get; set; }
        public int    Capacity       { get; set; }
        public BuildingType()
        {
            Type = "";
        }
        public BuildingType(int building_type_id, string type, int purchase_price, int build_time, int capacity) 
        {
            BuildingTypeID = building_type_id;
            Type           = type;
            PurchasePrice  = purchase_price;
            BuildTime      = build_time;
            Capacity       = capacity;
        }

        public BuildingType(int building_type_id)
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT type, purchase_price, build_time, capacity FROM building_types WHERE building_type_id = @building_type_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@building_type_id", building_type_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                BuildingTypeID = building_type_id;
                Type           = reader.GetString(reader.GetOrdinal("type"));
                PurchasePrice  = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                BuildTime      = reader.GetInt32(reader.GetOrdinal("build_time"));
                Capacity       = reader.GetInt32(reader.GetOrdinal("capacity"));
            }
            else
            {
                throw new Exception($"No such building_type");
            }
        }

        public static List<BuildingType> LoadBuildingTypeFromDB()
        {
            List<BuildingType> building_types = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM building_types";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
                connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int    building_type_id  = reader.GetInt32(reader.GetOrdinal("building_type_id"));
                string type              = reader.GetString(reader.GetOrdinal("type"));
                int    purchase_price    = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                int    build_time        = reader.GetInt32(reader.GetOrdinal("build_time"));
                int    capacity          = reader.GetInt32(reader.GetOrdinal("capacity"));

                building_types.Add(new(building_type_id, type, purchase_price, build_time, capacity));
            }
            return building_types;
        }
    }

    internal class StorageBuilding
    {
        public int           BuildingID     { get; set; }
        public BuildingType  BuildingType   { get; set; }
        public int           TimeUntilBuild { get; set; }
        public int           PositionX      { get; set; }
        public int           PositionY      { get; set; }
        public List<Product> Products       { get; set; } = [];
        public StorageBuilding(int building_type_id, int buildings_count, int position_x, int position_y)
        {
            BuildingID     = buildings_count + 1;
            BuildingType   = new (building_type_id);
            TimeUntilBuild = BuildingType.BuildTime;
            PositionX      = position_x;
            PositionY      = position_y;
        }
        public StorageBuilding(int building_id, BuildingType building_type, int time_until_build, int position_x, int position_y)
        {
            BuildingID     = building_id;
            BuildingType   = building_type;
            TimeUntilBuild = time_until_build;
            PositionX      = position_x;
            PositionY      = position_y;
        }
        public static List<StorageBuilding> LoadBuildingFromDB()
        {
            List<StorageBuilding> buildings = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                SELECT * 
                FROM buildings 
                WHERE building_type_id = (
                    SELECT building_type_id 
                    FROM building_types 
                    WHERE type = 'Storage'
                );
            ";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          building_id      = reader.GetInt32(reader.GetOrdinal("building_id"));
                BuildingType building_type    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
                int          time_until_build = reader.GetInt32(reader.GetOrdinal("time_until_build"));
                int          position_x       = reader.GetInt32(reader.GetOrdinal("position_x"));
                int          position_y       = reader.GetInt32(reader.GetOrdinal("position_y"));


                buildings.Add(new(building_id, building_type, time_until_build, position_x, position_y));
            }
            connection.Close();

            foreach (var building in buildings)
            {
                query              = "SELECT * FROM building_products WHERE building_id = @building_id";

                using var command1 = new SQLiteCommand(query, connection);
                command1.Parameters.AddWithValue("@building_id", building.BuildingID);
                connection.Open();
                using var reader1  = command1.ExecuteReader();

                while (reader1.Read())
                {
                    building.Products.Add(new(reader1.GetInt32(reader1.GetOrdinal("product_id"))));
                }
                connection.Close();
            }
            return buildings;
        }

        public void SaveBuildingToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO buildings (building_id, position_x, position_y, time_until_build, food, water, cleanliness, building_type_id)
                VALUES (@building_id, @position_x, @position_y, @time_until_build, @food, @water, @cleanliness, @building_type_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@building_id",      BuildingID);
            command.Parameters.AddWithValue("@position_x",       PositionX);
            command.Parameters.AddWithValue("@position_y",       PositionY);
            command.Parameters.AddWithValue("@time_until_build", TimeUntilBuild);
            command.Parameters.AddWithValue("@food",             0);
            command.Parameters.AddWithValue("@water",            0);
            command.Parameters.AddWithValue("@cleanliness",      0);
            command.Parameters.AddWithValue("@building_type_id", BuildingType.BuildingTypeID);
            command.ExecuteNonQuery();
            connection.Close();

            foreach (Product product in Products)
            {
                query = @"
                INSERT OR REPLACE INTO building_products (building_id, product_id)
                VALUES (@building_id, @product_id);
            ";

                using var command1 = new SQLiteCommand(query, connection);
                connection.Open();
                command1.Parameters.AddWithValue("@building_id", BuildingID);
                command1.Parameters.AddWithValue("@product_id", product.ProductID);
                command1.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
    internal class LiveStockBuilding : StorageBuilding
    {
        public List<Animal> Animals       { get; set; } = [];
        public int          Food          { get; set; }
        public int          Water         { get; set; }
        public int          Cleanliness   { get; set; }
        public LiveStockBuilding(int building_type_id, int buildings_count, int position_x, int position_y)
            : base(building_type_id, buildings_count, position_x, position_y)
        {
            Food        = 0;
            Water       = 0;
            Cleanliness = 10;
        }
        public LiveStockBuilding(int building_id, BuildingType building_type, int time_until_build, int position_x, int position_y, 
            int food, int water, int cleanliness) : base(building_id, building_type, time_until_build, position_x, position_y)
        {
            Food        = food;
            Water       = water;
            Cleanliness = cleanliness;
        }
        public static List<LiveStockBuilding> LoadLiveStockBuildingFromDB()
        {
            List<LiveStockBuilding> live_stock_buildings = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                SELECT * 
                FROM buildings 
                WHERE building_type_id NOT IN (
                    SELECT building_type_id 
                    FROM building_types 
                    WHERE type IN ('Storage', 'Field')
                );
            ";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          building_id      = reader.GetInt32(reader.GetOrdinal("building_id"));
                BuildingType building_type    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
                int          time_until_build = reader.GetInt32(reader.GetOrdinal("time_until_build"));
                int          position_x       = reader.GetInt32(reader.GetOrdinal("position_x"));
                int          position_y       = reader.GetInt32(reader.GetOrdinal("position_y"));
                int          food             = reader.GetInt32(reader.GetOrdinal("food"));
                int          water            = reader.GetInt32(reader.GetOrdinal("water"));
                int          cleanliness      = reader.GetInt32(reader.GetOrdinal("cleanliness"));


                live_stock_buildings.Add(new(building_id, building_type, time_until_build, position_x, position_y, food, water, cleanliness));
            }
            connection.Close();

            foreach (var building in live_stock_buildings)
            {
                query              = "SELECT * FROM building_products WHERE building_id = @building_id";

                using var command1 = new SQLiteCommand(query, connection);
                command1.Parameters.AddWithValue("@building_id", building.BuildingID);
                connection.Open();
                using var reader1  = command1.ExecuteReader();

                while (reader1.Read())
                {
                    building.Products.Add(new(reader1.GetInt32(reader1.GetOrdinal("product_id"))));
                }
                connection.Close();

                query              = "SELECT * FROM building_animals WHERE building_id = @building_id";

                using var command2 = new SQLiteCommand(query, connection);
                command2.Parameters.AddWithValue("@building_id", building.BuildingID);
                connection.Open();
                using var reader2  = command2.ExecuteReader();

                while (reader2.Read())
                {
                    building.Animals.Add(new(reader2.GetInt32(reader2.GetOrdinal("animal_id"))));
                }
                connection.Close();
            }


            return live_stock_buildings;
        }

        public new void SaveBuildingToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO buildings (building_id, position_x, position_y, time_until_build, food, water, cleanliness, building_type_id)
                VALUES (@building_id, @position_x, @position_y, @time_until_build, @food, @water, @cleanliness, @building_type_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@building_id",      BuildingID);
            command.Parameters.AddWithValue("@position_x",       PositionX);
            command.Parameters.AddWithValue("@position_y",       PositionY);
            command.Parameters.AddWithValue("@time_until_build", TimeUntilBuild);
            command.Parameters.AddWithValue("@food",             Food);
            command.Parameters.AddWithValue("@water",            Water);
            command.Parameters.AddWithValue("@cleanliness",      Cleanliness);
            command.Parameters.AddWithValue("@building_type_id", BuildingType.BuildingTypeID);
            command.ExecuteNonQuery();
            connection.Close();

            foreach (Product product in Products)
            {
                query = @"
                INSERT OR REPLACE INTO building_products (building_id, product_id)
                VALUES (@building_id, @product_id);
            ";

                using var command1 = new SQLiteCommand(query, connection);
                connection.Open();
                command1.Parameters.AddWithValue("@building_id", BuildingID);
                command1.Parameters.AddWithValue("@product_id", product.ProductID);
                command1.ExecuteNonQuery();
                connection.Close();
            }
            foreach (Animal animal in Animals)
            {
                query = @"
                INSERT OR REPLACE INTO building_animals (building_id, animal_id)
                VALUES (@building_id, @product_id);
            ";

                using var command1 = new SQLiteCommand(query, connection);
                connection.Open();
                command1.Parameters.AddWithValue("@building_id", BuildingID);
                command1.Parameters.AddWithValue("@animal_id", animal.AnimalID);
                command1.ExecuteNonQuery();
                connection.Close();
            }
        }
    }

    internal class Field : StorageBuilding
    {
        public List<Crop> Crops { get; set; } = [];
        public int Water { get; set; }
        public Field(int building_type_id, int buildings_count, int position_x, int position_y)
            : base(building_type_id, buildings_count, position_x, position_y)
        {
            Water = 0;
        }
        public Field(int building_id, BuildingType building_type, int time_until_build, int position_x, int position_y, int water)
            : base(building_id, building_type, time_until_build, position_x, position_y)
        {
            Water = water;
        }
        public static List<Field> LoadFieldFromDB()
        {
            List<Field> fields = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                SELECT * 
                FROM buildings 
                WHERE building_type_id = (
                    SELECT building_type_id 
                    FROM building_types 
                    WHERE type = 'Field'
                );
            ";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          building_id      = reader.GetInt32(reader.GetOrdinal("building_id"));
                BuildingType building_type    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
                int          time_until_build = reader.GetInt32(reader.GetOrdinal("time_until_harvest"));
                int          position_x       = reader.GetInt32(reader.GetOrdinal("position_x"));
                int          position_y       = reader.GetInt32(reader.GetOrdinal("position_y"));
                int          water            = reader.GetInt32(reader.GetOrdinal("water"));


                fields.Add(new(building_id, building_type, time_until_build, position_x, position_y, water));
            }
            connection.Close();

            foreach (var building in fields)
            {
                query              = "SELECT * FROM building_products WHERE building_id = @building_id";

                using var command1 = new SQLiteCommand(query, connection);
                command1.Parameters.AddWithValue("@building_id", building.BuildingID);
                connection.Open();
                using var reader1  = command1.ExecuteReader();

                while (reader1.Read())
                {
                    building.Products.Add(new(reader1.GetInt32(reader1.GetOrdinal("product_id"))));
                }
                connection.Close();

                query              = "SELECT * FROM building_crops WHERE building_id = @building_id";

                using var command2 = new SQLiteCommand(query, connection);
                command2.Parameters.AddWithValue("@building_id", building.BuildingID);
                connection.Open();
                using var reader2  = command2.ExecuteReader();

                while (reader2.Read())
                {
                    building.Crops.Add(new(reader2.GetInt32(reader2.GetOrdinal("crop_id"))));
                }
                connection.Close();
            }
            return fields;
        }

        public new void SaveBuildingToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO buildings (building_id, position_x, position_y, time_until_build, food, water, cleanliness, building_type_id)
                VALUES (@building_id, @position_x, @position_y, @time_until_build, @food, @water, @cleanliness, @building_type_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@building_id",      BuildingID);
            command.Parameters.AddWithValue("@position_x",       PositionX);
            command.Parameters.AddWithValue("@position_y",       PositionY);
            command.Parameters.AddWithValue("@time_until_build", TimeUntilBuild);
            command.Parameters.AddWithValue("@food",             0);
            command.Parameters.AddWithValue("@water",            Water);
            command.Parameters.AddWithValue("@cleanliness",      0);
            command.Parameters.AddWithValue("@building_type_id", BuildingType.BuildingTypeID);
            command.ExecuteNonQuery();
            connection.Close();

            foreach (Product product in Products)
            {
                query = @"
                INSERT OR REPLACE INTO building_products (building_id, product_id)
                VALUES (@building_id, @product_id);
            ";

                using var command1 = new SQLiteCommand(query, connection);
                connection.Open();
                command1.Parameters.AddWithValue("@building_id", BuildingID);
                command1.Parameters.AddWithValue("@product_id", product.ProductID);
                command1.ExecuteNonQuery();
                connection.Close();
            }
            foreach (Crop crop in Crops)
            {
                query = @"
                INSERT OR REPLACE INTO building_animals (building_id, crop_id)
                VALUES (@building_id, @crop_id);
            ";

                using var command1 = new SQLiteCommand(query, connection);
                connection.Open();
                command1.Parameters.AddWithValue("@building_id", BuildingID);
                command1.Parameters.AddWithValue("@crop_id", crop.CropID);
                command1.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
