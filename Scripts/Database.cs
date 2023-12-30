using Godot;
using System;
using Microsoft.Data.Sqlite;

public partial class Database : Node
{
    public override void _Ready()
    {

        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
        builder.DataSource = OS.GetUserDataDir() + "\\database.db";

        string connectionString = builder.ConnectionString;

        GD.Print(builder.DataSource);

        using (SqliteConnection dbConnection = new SqliteConnection(connectionString))
            try
            {
                // Open the connection
                dbConnection.Open();

                // Create a table
                CreatePlayersTable(dbConnection);


                // Insert some data into the table
                InsertData(dbConnection, 1, "John Doe", 25);
                InsertData(dbConnection, 2, "Jane Doe", 32);

                // Query and display data
                QueryAndDisplayData(dbConnection);


                GD.Print("Database connection opened successfully.");
            }
            catch (Exception ex)
            {
                GD.Print("Error opening database connection: " + ex.Message);
            }
    }
    void CreatePlayersTable(SqliteConnection dbConnection)
    {
        using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS Players (ID INTEGER PRIMARY KEY, Name TEXT, Money INTEGER);", dbConnection))
        {
            command.ExecuteNonQuery();
        }
    }

    void InsertData(SqliteConnection dbConnection, int id, string name, int money)
    {
        using (SqliteCommand command = new SqliteCommand("INSERT INTO Players (ID, Name, Money) VALUES (@id, @name, @money);", dbConnection))
        {
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@money", money);

            command.ExecuteNonQuery();
        }
    }

    void QueryAndDisplayData(SqliteConnection dbConnection)
    {
        using (SqliteCommand command = new SqliteCommand("SELECT * FROM Players;", dbConnection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                string text = "";
                GD.Print("Players table data:");
                while (reader.Read())
                {
                    text = text + $"ID: {reader["ID"]}, Name: {reader["Name"]}, Money: {reader["Money"]}\n";
                    GD.Print(text);
                    Label label = GetNode<Label>("/root/Map/Player/HUD/Info2");
                    label.Text = text;

                }
            }
        }
    }

}