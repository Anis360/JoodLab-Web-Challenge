using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;


namespace Games_Shelf_API.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly string _connectionString = "Server=localhost;Port=3306;Database=games;Uid=anis;Pwd=XbvqNlLfCOluHsPE;";
        [HttpGet]

        public string Get(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT game FROM games WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["game"].ToString();
                        }
                    }
                }
            }
            return "Game not found";
        }


        [HttpPost]
        public void Post(string userId, string game, string genre, string playtime, string platforms)
        {
            platforms = platforms.ToUpper();
            string[] platformsArray = platforms.Split(',');
            var jsonPlatforms = JsonConvert.SerializeObject(platformsArray);
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("INSERT INTO games (userId, game, genre, playtime, platforms) VALUES (@userId, @game, @genre, @playtime, @platforms)", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@game", game);
                    command.Parameters.AddWithValue("@genre", genre);
                    command.Parameters.AddWithValue("@playtime", playtime);
                    command.Parameters.AddWithValue("@platforms", jsonPlatforms);
                    command.ExecuteNonQuery();
                }
            }
        }

        [HttpPut]
        public void Put(int id, string userId, string game, string genre, string playtime, string platforms)
        {
            platforms = platforms.ToUpper();
            string[] platformsArray = platforms.Split(',');
            var jsonPlatforms = JsonConvert.SerializeObject(platformsArray);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("UPDATE games SET userId=@userId, name=@name, genre=@genre, platforms=@platforms, playtime=@playtime WHERE id=@id", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@name", game);
                    command.Parameters.AddWithValue("@genre", genre);
                    command.Parameters.AddWithValue("@platforms", platforms);
                    command.Parameters.AddWithValue("@playtime", playtime);
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        [HttpGet]
        [Route("select_top_by_playtime")]
        public IEnumerable<string> SelectTopByPlaytime(string genre = null, string platform = null)
        {
            platform = platform.ToUpper();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT game, SUM(playtime) as total_playtime FROM games";
                if (!string.IsNullOrEmpty(genre))
                {
                    query += " WHERE genre = @genre";
                }
                if (!string.IsNullOrEmpty(platform))
                {
                    if (string.IsNullOrEmpty(genre))
                    {
                        query += " WHERE";
                    }
                    else
                    {
                        query += " AND";
                    }
                    query += $" JSON_CONTAINS(platforms, JSON_QUOTE(\"{platform}\"), '$')";
                }
                query += " GROUP BY game ORDER BY total_playtime DESC";
                using (var command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(genre))
                    {
                        command.Parameters.AddWithValue("@genre", genre);
                    }
                    if (!string.IsNullOrEmpty(platform))
                    {
                        command.Parameters.AddWithValue("@platform", $"JSON_ARRAY({platform})");
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        var games = new List<string>();
                        while (reader.Read())
                        {
                            games.Add($"Game: {reader["game"]}, Total playtime: {reader["total_playtime"]}");
                        }
                        return games;
                    }
                }
            }
        }
        [HttpGet]
        [Route("select_top_by_players")]
        public IEnumerable<string> SelectTopByPlayers(string genre = null, string platform = null)
        {
            var result = new List<string>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT game, COUNT(DISTINCT userId) as total_players FROM games";
                if (!string.IsNullOrEmpty(genre))
                {
                    query += " WHERE genre = @genre";
                }
                if (!string.IsNullOrEmpty(platform))
                {
                    if (string.IsNullOrEmpty(genre))
                    {
                        query += " WHERE";
                    }
                    else
                    {
                        query += " AND";
                    }
                    query += $" JSON_CONTAINS(platforms, JSON_QUOTE(\"{platform}\"), '$')";
                }
                query += " GROUP BY game ORDER BY total_players DESC";
                using (var command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(genre))
                    {
                        command.Parameters.AddWithValue("@genre", genre);
                    }
                    if (!string.IsNullOrEmpty(platform))
                    {
                        command.Parameters.AddWithValue("@platform", $"JSON_ARRAY({platform})");
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add($"Game: {reader["game"]}, Total players: {reader["total_players"]}");
                        }
                    }
                }
            }
            return result;
        }

        [HttpDelete]
        public void Delete(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("DELETE FROM games WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
