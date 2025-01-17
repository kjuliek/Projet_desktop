using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Projet_desktop.Models
{
    internal class PlantSpecies : Species
    {
        public int PlantSpeciesID { get; set; }
        public PlantSpecies(int plant_species_id, string name, int purchase_price, ProductType product_type, BuildingType building_type)
            : base(name, purchase_price, product_type, building_type)
        {
            PlantSpeciesID = plant_species_id;
        }

        public PlantSpecies(int plant_species_id) : base()
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT name, purchase_price, product_type_id, building_type_id FROM plant_species WHERE plant_species_id = @plant_species_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@plant_species_id", plant_species_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                PlantSpeciesID = plant_species_id;
                Name           = reader.GetString(reader.GetOrdinal("name"));
                PurchasePrice  = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                ProductType    = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));
                BuildingType   = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
            }
            else
            {
                throw new Exception($"No such building_type");
            }
        }
        public static List<PlantSpecies> LoadPlantSpeciesFromDB()
        {
            List<PlantSpecies> plant_species = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM plant_species";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          plant_species_id = reader.GetInt32(reader.GetOrdinal("plant_species_id"));
                string       name             = reader.GetString(reader.GetOrdinal("name"));
                int          purchase_price   = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                ProductType  product_type     = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));
                BuildingType building_type    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));

                plant_species.Add(new(plant_species_id, name, purchase_price, product_type, building_type));
            }
            return plant_species;
        }
    }

    internal class Crop
    {
        public int          CropID       { get; set; }
        public PlantSpecies PlantSpecies { get; set; }
        public int          GrowthTime   { get; set; }
        public bool         Thirsty      { get; set; }
        public Crop(int plant_species_id, int crops_count)
        {
            CropID       = crops_count + 1;
            PlantSpecies = new(plant_species_id);
            GrowthTime   = 0;
            Thirsty      = false;
        }
        public Crop(int crop_id, PlantSpecies species, int growth_time, bool thirsty)
        {
            CropID       = crop_id;
            PlantSpecies = species;
            GrowthTime   = growth_time;
            Thirsty      = thirsty;
        }
        public Crop(int crop_id) : base()
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT plant_species_id, growth_time, thirsty FROM crops WHERE crop_id = @crop_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@crop_id", crop_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                PlantSpecies = new(reader.GetInt32(reader.GetOrdinal("plant_species_id")));
                GrowthTime   = reader.GetInt32(reader.GetOrdinal("growth_time"));
                Thirsty      = reader.GetInt32(reader.GetOrdinal("thirsty")) == 1;
            }
            else
            {
                throw new Exception($"No such building_type");
            }
        }
        public static List<Crop> LoadAnimalFromDB()
        {
            List<Crop> crops  = [];
             
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM crops";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          crop_id       = reader.GetInt32(reader.GetOrdinal("crop_id"));
                PlantSpecies plant_species = new(reader.GetInt32(reader.GetOrdinal("plant_species_id")));
                int          growth_time   = reader.GetInt32(reader.GetOrdinal("growth_time"));
                bool         thirsty       = reader.GetInt32(reader.GetOrdinal("thirsty")) == 1;


                crops.Add(new(crop_id, plant_species, growth_time, thirsty));
            }
            return crops;
        }

        public void SaveCropToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO crops (crop_id, growth_time, thirsty, plant_species_id)
                VALUES (@crop_id, @growth_time, @thirsty, @plant_species_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@crop_id", CropID);
            command.Parameters.AddWithValue("@growth_time", GrowthTime);
            command.Parameters.AddWithValue("@thirsty", Thirsty);
            command.Parameters.AddWithValue("@plant_species_id", PlantSpecies.PlantSpeciesID);
            command.ExecuteNonQuery();
        }
    }
}
