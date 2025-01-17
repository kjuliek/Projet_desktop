using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Projet_desktop.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Projet_desktop.Models
{
    internal class User
    {
        public int      UserID { get; set; }
        public DateTime Date   { get; set; }
        public int      Money  { get; set; }

        public User()
        {
            string dbFile     = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query      = "SELECT * FROM users WHERE user_id = 1";
            var    connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            using var reader  = command.ExecuteReader();
            if (reader.Read())
            {
                UserID = reader.GetInt32(reader.GetOrdinal("user_id"));
                Money  = reader.GetInt32(reader.GetOrdinal("money"));
                Date   = DateTime.Parse(reader.GetString(reader.GetOrdinal("date")));
            }
            else
            {
                throw new Exception($"Utilisateur avec introuvable.");
            }
        }
        public void SaveUserToDB()
        {
            string dbFile = AppDomain.CurrentDomain.BaseDirectory + "/Data/farmHarmony.db";

            string query = @"
                INSERT OR REPLACE INTO users (user_id, money, date)
                VALUES (@user_id, @money, @date);
            ";
            var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");

            using var command = new SQLiteCommand(query, connection);
            connection.Open();
            command.Parameters.AddWithValue("@user_id", UserID);
            command.Parameters.AddWithValue("@money",   Money);
            command.Parameters.AddWithValue("@date",    Date);
            command.ExecuteNonQuery();
        }
    }
}

