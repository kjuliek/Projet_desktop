using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Microsoft.SqlServer.Server;
using System.IO;


namespace Projet_desktop.Models
{
    internal class Database
    {
        private string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";
        public Database()
        {
            string directoryPath = Path.GetDirectoryName(dbFile);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (!System.IO.File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
                CreateDB();
                InitDB();
            }
        }

        private void CreateDB()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();

                using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }
                // TABLE users
                string createUsersTable = "CREATE TABLE IF NOT EXISTS users (" +
                                                         "user_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "money INTEGER DEFAULT 0," +
                                                         "date TEXT NOT NULL);";
                using (var command = new SQLiteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE building_types
                string createBuildingTypesTable = "CREATE TABLE IF NOT EXISTS building_types (" +
                                                         "building_type_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "type TEXT NOT NULL," +
                                                         "purchase_price INTEGER," +
                                                         "build_time INTEGER," +
                                                         "capacity INTEGER);";
                using (var command = new SQLiteCommand(createBuildingTypesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // TABLE product_types
                string createProductTypesTable = "CREATE TABLE IF NOT EXISTS product_types (" +
                                                         "product_type_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "name TEXT NOT NULL," +
                                                         "production_time INTEGER," +
                                                         "production_rate INTEGER," +
                                                         "price INTEGER," +
                                                         "building_type_id INTEGER," +
                                                         "FOREIGN KEY (building_type_id) REFERENCES building_types(building_type_id));";
                using (var command = new SQLiteCommand(createProductTypesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE products
                string createProductsTable = "CREATE TABLE IF NOT EXISTS products (" +
                                                         "product_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "product_type_id INTEGER," +
                                                         "FOREIGN KEY (product_type_id) REFERENCES product_types(product_type_id));";
                using (var command = new SQLiteCommand(createProductsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE animal_species
                string createAnimalSpeciesTable = "CREATE TABLE IF NOT EXISTS animal_species (" +
                                                         "animal_species_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "name TEXT NOT NULL," +
                                                         "average_age INTEGER," +
                                                         "purchase_price INTEGER," +
                                                         "product_type_id INTEGER," +
                                                         "building_type_id INTEGER," +
                                                         "FOREIGN KEY (product_type_id) REFERENCES product_types(product_type_id)," +
                                                         "FOREIGN KEY (building_type_id) REFERENCES building_types(building_type_id));";
                using (var command = new SQLiteCommand(createAnimalSpeciesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE plant_species
                string createPlantSpeciesTable = "CREATE TABLE IF NOT EXISTS plant_species (" +
                                                         "plant_species_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "name TEXT NOT NULL," +
                                                         "purchase_price INTEGER," +
                                                         "product_type_id INTEGER," +
                                                         "building_type_id INTEGER," +
                                                         "FOREIGN KEY (product_type_id) REFERENCES product_types(product_type_id)," +
                                                         "FOREIGN KEY (building_type_id) REFERENCES building_types(building_type_id));";
                using (var command = new SQLiteCommand(createPlantSpeciesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE crops
                string createCropsTable = "CREATE TABLE IF NOT EXISTS crops (" +
                                                         "crop_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "growth_time INTEGER," +
                                                         "thirsty BOOLEAN," +
                                                         "plant_species_id INTEGER," +
                                                         "FOREIGN KEY (plant_species_id) REFERENCES plant_species(plant_species_id));";
                using (var command = new SQLiteCommand(createCropsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE animals
                string createAnimalsTable = "CREATE TABLE IF NOT EXISTS animals (" +
                                                         "animal_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "age INTEGER," +
                                                         "time_until_harvest INTEGER," +
                                                         "thirsty BOOLEAN," +
                                                         "hungry BOOLEAN," +
                                                         "clean BOOLEAN," +
                                                         "animal_species_id INTEGER," +
                                                         "FOREIGN KEY (animal_species_id) REFERENCES animal_species(animal_species_id));";
                using (var command = new SQLiteCommand(createAnimalsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE buildings
                string createBuildingsTable = "CREATE TABLE IF NOT EXISTS buildings (" +
                                                         "building_id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                         "position_x INTEGER," +
                                                         "position_y INTEGER," +
                                                         "time_until_build INTEGER," +
                                                         "food INTEGER," +
                                                         "water INTEGER," +
                                                         "cleanliness INTEGER," +
                                                         "building_type_id INTEGER," +
                                                         "FOREIGN KEY (building_type_id) REFERENCES building_type(building_type_id));";
                using (var command = new SQLiteCommand(createBuildingsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE building_animals
                string createBuildingAnimalTable = "CREATE TABLE IF NOT EXISTS building_animals(" +
                                                          "building_id INTEGER," +
                                                          "animal_id INTEGER," +
                                                          " PRIMARY KEY (building_id, animal_id)," +
                                                          "FOREIGN KEY (building_id) REFERENCES building(building_id)," +
                                                          "FOREIGN KEY (animal_id) REFERENCES animals(animal_id));";
                using (var command = new SQLiteCommand(createBuildingAnimalTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE building_crops
                string createBuildingCropTable = "CREATE TABLE IF NOT EXISTS building_crops(" +
                                                          "building_id INTEGER," +
                                                          "crop_id INTEGER," +
                                                          "PRIMARY KEY (building_id, crop_id)," +
                                                          "FOREIGN KEY (building_id) REFERENCES building(building_id)," +
                                                          "FOREIGN KEY (crop_id) REFERENCES crops(crop_id));";
                using (var command = new SQLiteCommand(createBuildingCropTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                //TABLE building_products
                string createBuildingProductTable = "CREATE TABLE IF NOT EXISTS building_products(" +
                                                          "building_id INTEGER," +
                                                          "product_id INTEGER," +
                                                          " PRIMARY KEY (building_id, product_id)," +
                                                          "FOREIGN KEY (building_id) REFERENCES building(building_id)," +
                                                          "FOREIGN KEY (product_id) REFERENCES products(product_id));";
                using (var command = new SQLiteCommand(createBuildingProductTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void InitDB()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();

                // User
                string insertUser = @"
            INSERT OR IGNORE INTO users (money, date)
            VALUES 
            (5000, @date);";
                using (var command = new SQLiteCommand(insertUser, connection))
                {
                   command.Parameters.AddWithValue("@date", DateTime.Now);
                   command.ExecuteNonQuery();
                }

                // Building
                string insertBuildingTypes = @"
            INSERT OR IGNORE INTO building_types (type, purchase_price, build_time, capacity)
            VALUES 
            ('Field', 0, 0, 5),
            ('ChickenCoop', 500, 14, 5),
            ('SheepBarn', 1500, 30, 5),
            ('CowBarn', 2500, 45, 5),
            ('Storage', 1000, 15, 100);";
                using (var command = new SQLiteCommand(insertBuildingTypes, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Products
                string insertProductTypes = @"
            INSERT OR IGNORE INTO product_types (name, production_time, production_rate, price, building_type_id)
            VALUES 
            ('Wheat', 120, 15, 50, (SELECT building_type_id FROM building_types WHERE type = 'Storage')),
            ('Corn', 100, 10, 70, (SELECT building_type_id FROM building_types WHERE type = 'Storage')),
            ('Carrot', 70, 5, 30, (SELECT building_type_id FROM building_types WHERE type = 'Storage')),
            ('Egg', 1, 1, 1, (SELECT building_type_id FROM building_types WHERE type = 'Storage')),
            ('Wool', 40, 5, 5, (SELECT building_type_id FROM building_types WHERE type = 'Storage')),
            ('Milk', 1, 2, 1, (SELECT building_type_id FROM building_types WHERE type = 'Storage'));";
                using (var command = new SQLiteCommand(insertProductTypes, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Plants (ajouter les products)
                string insertPlantSpecies = @"
            INSERT OR IGNORE INTO plant_species (name, purchase_price, product_type_id, building_type_id)
            VALUES 
            ('Wheat', 10, (SELECT product_type_id FROM product_types WHERE name = 'Wheat'), (SELECT building_type_id FROM building_types WHERE type = 'Field')),
            ('Corn', 20, (SELECT product_type_id FROM product_types WHERE name = 'Corn'), (SELECT building_type_id FROM building_types WHERE type = 'Field')),
            ('Carrot', 10, (SELECT product_type_id FROM product_types WHERE name = 'Carrot'), (SELECT building_type_id FROM building_types WHERE type = 'Field'));";
                using (var command = new SQLiteCommand(insertPlantSpecies, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Plants (ajouter les products)
                string insertAnimalSpecies = @"
            INSERT OR IGNORE INTO animal_species (name, average_age, purchase_price, product_type_id, building_type_id)
            VALUES 
            ('Chicken', 2000, 50, (SELECT product_type_id FROM product_types WHERE name = 'Egg'), (SELECT building_type_id FROM building_types WHERE type = 'ChickenCoop')),
            ('Sheep', 3000, 800, (SELECT product_type_id FROM product_types WHERE name = 'Wool'), (SELECT building_type_id FROM building_types WHERE type = 'SheepBarn')),
            ('Cow', 4000, 1500, (SELECT product_type_id FROM product_types WHERE name = 'Milk'), (SELECT building_type_id FROM building_types WHERE type = 'CowBarn'));";
                using (var command = new SQLiteCommand(insertAnimalSpecies, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ResetDB()
        {
            string resetTable = @"
                DELETE FROM users;
                DELETE FROM sqlite_sequence WHERE name = 'users';

                DELETE FROM buildings;
                DELETE FROM sqlite_sequence WHERE name = 'buildings';

                DELETE FROM products;
                DELETE FROM sqlite_sequence WHERE name = 'products';

                DELETE FROM building_products;
                DELETE FROM sqlite_sequence WHERE name = 'building_products';

                DELETE FROM animals;
                DELETE FROM sqlite_sequence WHERE name = 'animals';

                DELETE FROM building_animals;
                DELETE FROM sqlite_sequence WHERE name = 'building_animals';

                DELETE FROM crops;
                DELETE FROM sqlite_sequence WHERE name = 'crops';

                DELETE FROM building_crops;
                DELETE FROM sqlite_sequence WHERE name = 'building_crops';
            ";

            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(resetTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // User
                string insertUser = @"
                INSERT OR IGNORE INTO users (money, date)
                VALUES 
                (5000, @date);";
                using (var command = new SQLiteCommand(insertUser, connection))
                {
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
