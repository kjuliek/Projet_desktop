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
    internal class AnimalSpecies : Species
    {
        public int AnimalSpeciesID { get; set; }
        public int AverageAge      { get; set; }

        public AnimalSpecies(int animal_species_id, string name, int average_age, int purchase_price, ProductType product_type, BuildingType building_type) : base(name, purchase_price, product_type, building_type)
        {
            AnimalSpeciesID = animal_species_id;
            AverageAge      = average_age;
        }

        public AnimalSpecies(int animal_species_id) : base()
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT name, average_age, purchase_price, product_type_id, building_type_id FROM animal_species WHERE animal_species_id = @animal_species_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@animal_species_id", animal_species_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                AnimalSpeciesID = animal_species_id;
                Name            = reader.GetString(reader.GetOrdinal("name"));
                AverageAge      = reader.GetInt32(reader.GetOrdinal("average_age"));
                PurchasePrice   = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                ProductType     = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));
                BuildingType    = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));
            }
            else
            {
                throw new Exception($"No such building_type");
            }
        }

        public static List<AnimalSpecies> LoadAnimalSpeciesFromDB()
        {
            List<AnimalSpecies> animal_species = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM animal_species";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int          animal_species_id = reader.GetInt32(reader.GetOrdinal("animal_species_id"));
                string       name              = reader.GetString(reader.GetOrdinal("name"));
                int          average_age       = reader.GetInt32(reader.GetOrdinal("average_age"));
                int          purchase_price    = reader.GetInt32(reader.GetOrdinal("purchase_price"));
                ProductType  product_type      = new(reader.GetInt32(reader.GetOrdinal("product_type_id")));
                BuildingType building_type     = new(reader.GetInt32(reader.GetOrdinal("building_type_id")));

                animal_species.Add(new(animal_species_id, name, average_age, purchase_price, product_type, building_type));
            }
            return animal_species;
        }
    }

    internal class Animal
    {
        public int           AnimalID         { get; set; }
        public AnimalSpecies AnimalSpecies    { get; set; }
        public int           Age              { get; set; }
        public int           TimeUntilHarvest { get; set; }
        public bool          Thirsty          { get; set; }
        public bool          Hungry           { get; set; }
        public bool          Clean            { get; set; }
        public Animal(int animal_species_id, int animals_count)
        {
            AnimalID         = animals_count + 1;
            AnimalSpecies    = new(animal_species_id);
            Age              = 0;
            TimeUntilHarvest = AnimalSpecies.ProductType.ProductionTime;
            Thirsty          = false;
            Hungry           = false;
            Clean            = false;
        }
        public Animal(int animal_id, AnimalSpecies species, int age, int time_until_harvest, bool thirsty, bool hungry, bool clean)
        {
            AnimalID         = animal_id;
            AnimalSpecies    = species;
            Age              = age;
            TimeUntilHarvest = time_until_harvest;
            Thirsty          = thirsty;
            Hungry           = hungry;
            Clean            = clean;
        }
        public Animal(int animal_id)
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT animal_species_id, age, time_until_harvest, thirsty, hungry, clean WHERE animal_id = @animal_id";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@animal_id", animal_id);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                AnimalSpecies    = new(reader.GetInt32(reader.GetOrdinal("animal_species_id")));
                Age              = reader.GetInt32(reader.GetOrdinal("age"));
                TimeUntilHarvest = reader.GetInt32(reader.GetOrdinal("production_time"));
                Thirsty          = reader.GetInt32(reader.GetOrdinal("thirsty")) == 1;
                Hungry           = reader.GetInt32(reader.GetOrdinal("hungry")) == 1;
                Clean            = reader.GetInt32(reader.GetOrdinal("clean")) == 1;
            }
            else
            {
                throw new Exception($"Utilisateur avec introuvable.");
            }
        }
        public static List<Animal> LoadAnimalFromDB()
        {
            List<Animal> animals = [];

            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM animals";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                int           animal_id          = reader.GetInt32(reader.GetOrdinal("animal_id"));
                AnimalSpecies animal_species     = new(reader.GetInt32(reader.GetOrdinal("animal_species_id")));
                int           age                = reader.GetInt32(reader.GetOrdinal("age"));
                int           time_until_harvest = reader.GetInt32(reader.GetOrdinal("time_until_harvest"));
                bool          thirsty            = reader.GetInt32(reader.GetOrdinal("thirsty")) == 1;
                bool          hungry             = reader.GetInt32(reader.GetOrdinal("hungry")) == 1;
                bool          clean              = reader.GetInt32(reader.GetOrdinal("clean")) == 1;


                animals.Add(new(animal_id, animal_species, age, time_until_harvest, thirsty, hungry, clean));
            }
            return animals;
        }

        public void SaveAnimalToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO animals (animal_id, age, time_until_harvest, thirsty, hungry, clean, animal_species_id)
                VALUES (@animal_id, @age, @time_until_harvest, @thirsty, @hungry, @clean, @animal_species_id);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@animal_id",          AnimalID);
            command.Parameters.AddWithValue("@age",                Age);
            command.Parameters.AddWithValue("@time_until_harvest", TimeUntilHarvest);
            command.Parameters.AddWithValue("@thirsty",            Thirsty);
            command.Parameters.AddWithValue("@hungry",             Hungry);
            command.Parameters.AddWithValue("@clean",              Clean);
            command.Parameters.AddWithValue("@animal_species_id",  AnimalSpecies.AnimalSpeciesID);
            command.ExecuteNonQuery();
        }
    }
}
