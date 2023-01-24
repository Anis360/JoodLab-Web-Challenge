using System;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

string connectionString = @"server=localhost;userid=anis;password=XbvqNlLfCOluHsPE;database=games";

using var con = new MySqlConnection(connectionString);
con.Open();


while (true)
{
    // Display menu
    Console.WriteLine("Type the number of the action that you want to perform then press ENTER");
    Console.WriteLine("1. Create game");
    Console.WriteLine("2. Read game");
    Console.WriteLine("3. Read all games");
    Console.WriteLine("4. Update game");
    Console.WriteLine("5. Select top by playtime");
    Console.WriteLine("6. Select top by players");
    Console.WriteLine("7. Delete game");
    Console.WriteLine("8. Exit");

    // Get user choice
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            CreateGame();
            break;
        case "2":
            ReadGame();
            break;
        case "3":
            ReadAllGames();
            break;
        case "4":
            UpdateGame();
            break;
        case "5":
            SelectTopByPlaytime();
            break;
        case "6":
            SelectTopByPlayers();
            break;
        case "7":
            DeleteGame();
            break;
        case "8":
            return;
    }
    void CreateGame()
    {
        Console.WriteLine("Enter user ID:");
        var userID = Console.ReadLine();
        Console.WriteLine("Enter game name:");
        var gameName = Console.ReadLine();
        Console.WriteLine("Enter genre:");
        var genre = Console.ReadLine();
        Console.WriteLine("Enter playtime:");
        var playtime = int.Parse(Console.ReadLine());
        Console.WriteLine("Enter platform(s) (comma separated):");
        var platforms = Console.ReadLine();
        var jsonPlatforms = JsonConvert.SerializeObject(platforms.Split(','));
        // Create the connection
        using (var connection = new MySqlConnection(connectionString))
        {
            // Open the connection
            connection.Open();

            // Create the insert command
            var command = new MySqlCommand("INSERT INTO games (userid, game, genre, playtime, platforms) VALUES (@userid, @game, @genre, @playtime, @platforms)", connection);

            // Add the parameters to the command
            command.Parameters.AddWithValue("@userid", userID);
            command.Parameters.AddWithValue("@game", gameName);
            command.Parameters.AddWithValue("@genre", genre);
            command.Parameters.AddWithValue("@playtime", playtime);
            command.Parameters.AddWithValue("@platforms", jsonPlatforms);
            Console.WriteLine($"---------------");

            // Execute the command
            command.ExecuteNonQuery();

            Console.WriteLine("Game created!");
        }
        Console.WriteLine($"------------------------------");
    }
    void ReadGame()
    {
        Console.WriteLine("Enter the ID of the game you want to retrieve:");
        var id = Console.ReadLine();

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            var query = $"SELECT * FROM games WHERE id = {id}";
            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"User ID: {reader["userid"]}");
                        Console.WriteLine($"Game: {reader["game"]}");
                        Console.WriteLine($"Playtime: {reader["playtime"]}");
                        Console.WriteLine($"Genre: {reader["genre"]}");
                        Console.WriteLine($"Platforms: {reader["platforms"]}");
                    }
                }
            }
        }
        Console.WriteLine($"------------------------------");
    }
    void ReadAllGames()
    {

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            var query = $"SELECT * FROM games";
            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"User ID: {reader["userid"]}");
                        Console.WriteLine($"Game: {reader["game"]}");
                        Console.WriteLine($"Playtime: {reader["playtime"]}");
                        Console.WriteLine($"Genre: {reader["genre"]}");
                        Console.WriteLine($"Platforms: {reader["platforms"]}");
                        Console.WriteLine($"---------------");
                    }
                }
            }
        }
        Console.WriteLine($"------------------------------");
    }
    void UpdateGame()
    {
        Console.WriteLine("Enter the ID of the game you want to update:");
        var id = int.Parse(Console.ReadLine());

        Console.WriteLine("Enter the new user ID:");
        var userId = Console.ReadLine();

        Console.WriteLine("Enter the new game:");
        var game = Console.ReadLine();

        Console.WriteLine("Enter the new play time:");
        var playTime = int.Parse(Console.ReadLine());

        Console.WriteLine("Enter the new genre:");
        var genre = Console.ReadLine();

        Console.WriteLine("Enter new platform(s) (comma separated):");
        var platforms = Console.ReadLine().ToUpper();
        var jsonPlatforms = JsonConvert.SerializeObject(platforms.Split(','));

        using (var connection = new MySqlConnection(connectionString))
        {
            var updateSql = "UPDATE games SET userId = @userId, game = @game, playTime = @playTime, genre = @genre, platforms = @platforms WHERE id = @id";
            var updateCommand = new MySqlCommand(updateSql, connection);
            updateCommand.Parameters.AddWithValue("@userId", userId);
            updateCommand.Parameters.AddWithValue("@game", game);
            updateCommand.Parameters.AddWithValue("@playTime", playTime);
            updateCommand.Parameters.AddWithValue("@genre", genre);
            updateCommand.Parameters.AddWithValue("@platforms", jsonPlatforms);
            updateCommand.Parameters.AddWithValue("@id", id);
            Console.WriteLine($"---------------");

            connection.Open();
            var rowsAffected = updateCommand.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Game updated successfully!");
            }
            else
            {
                Console.WriteLine("An error occurred while updating the game.");
            }
        }
        Console.WriteLine($"------------------------------");
    }
    void SelectTopByPlaytime()
    {
        Console.WriteLine("Enter genre (optional):");
        var genre = Console.ReadLine();
        Console.WriteLine("Enter platform (optional):");
        var platforms = Console.ReadLine().ToUpper();
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "SELECT game, SUM(playtime) as total_playtime FROM games";
            var whereClauses = new List<string>();
            if (!string.IsNullOrEmpty(genre))
            {
                query += " WHERE genre = @genre";
            }
            if (!string.IsNullOrEmpty(platforms))
            {
                if (string.IsNullOrEmpty(genre))
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                query += $" JSON_CONTAINS(platforms, JSON_QUOTE(\"{platforms}\"), '$')";
            }
            query += " GROUP BY game ORDER BY total_playtime DESC";
            using (var command = new MySqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(genre))
                {
                    command.Parameters.AddWithValue("@genre", genre);
                }
                if (!string.IsNullOrEmpty(platforms))
                {
                    command.Parameters.AddWithValue("@platforms", $"JSON_ARRAY({platforms})");
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["game"]}: {reader["total_playtime"]}");
                    }
                }
            }
        }
        Console.WriteLine($"------------------------------");
    }
    void SelectTopByPlayers()
    {
        Console.WriteLine("Enter genre (optional):");
        var genre = Console.ReadLine();
        Console.WriteLine("Enter platform (optional):");
        var platforms = Console.ReadLine().ToUpper();

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "SELECT game, COUNT(DISTINCT userId) as total_players FROM games";
            if (!string.IsNullOrEmpty(genre))
            {
                query += " WHERE genre = @genre";
            }
            if (!string.IsNullOrEmpty(platforms))
            {
                if (string.IsNullOrEmpty(genre))
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                query += $" JSON_CONTAINS(platforms, JSON_QUOTE(\"{platforms}\"), '$')";
            }
            query += " GROUP BY game ORDER BY total_players DESC";
            using (var command = new MySqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(genre))
                {
                    command.Parameters.AddWithValue("@genre", genre);
                }
                if (!string.IsNullOrEmpty(platforms))
                {
                    command.Parameters.AddWithValue("@platforms", $"JSON_ARRAY({platforms})");
                }
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Game: {reader["game"]}, Total players: {reader["total_players"]}");
                    }
                }
            }
        }
        Console.WriteLine($"------------------------------");
    }
    void DeleteGame()
    {
        Console.WriteLine("Enter the ID of the game you want to delete:");
        int id = int.Parse(Console.ReadLine());

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            var cmd = new MySqlCommand("DELETE FROM games WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                Console.WriteLine("Game deleted successfully.");
            }
            else
            {
                Console.WriteLine("Game with the specified ID not found.");
            }
        }
        Console.WriteLine($"------------------------------");
    }

}