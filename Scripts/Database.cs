using Godot;
using Microsoft.Data.Sqlite;
using ProToTypeLounge.Password;

public partial class Database : Node
{
    SqliteConnection dbConnection;

    public override void _Ready()
    {

        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
        builder.DataSource = OS.GetUserDataDir() + "\\database.db";
        //builder.DataSource = "database.db";

        string connectionString = builder.ConnectionString;

        dbConnection = new SqliteConnection(connectionString);

        // Open the connection
        dbConnection.Open();

        // Create a table
        CreatePlayersTable();


        // Insert some data into the table
        //InsertData(dbConnection, "Attila", 2, 1000);
        //InsertData(dbConnection, "Yadana", 3, 1256);
        //NewUser(dbConnection, "testuser", "secret");

        // Query and display data
        //QueryAndDisplayData();
    }
    void CreatePlayersTable()
    {
        using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS Players (ID INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT, Password TEXT, Wage INTEGER, Money INTEGER);", dbConnection))
        {
            command.ExecuteNonQuery();
        }
    }
    public void NewUser(string username, string hashedPassword)
    {
        string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(hashedPassword);

        using (SqliteCommand command = new SqliteCommand("INSERT INTO Players (Username, Password, Wage, Money) VALUES (@username, @password, @wage, @money);", dbConnection))
        {
            //command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", encryptedPassword);
            command.Parameters.AddWithValue("@wage", 1);
            command.Parameters.AddWithValue("@money", 1000);

            command.ExecuteNonQuery();
        }
    }
    public void LoginUser(string username, string hashedPassword)
    {
            using (SqliteCommand command = new SqliteCommand("SELECT * FROM Players WHERE Username = @username", dbConnection))
            {
                command.Parameters.AddWithValue("@username", username);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // User found
                        GD.Print(BCrypt.Net.BCrypt.Verify(hashedPassword, $"{reader["Password"]}"));
                    }
                    else
                    {
                        GD.Print("User not found.");
                    }
                }
            }
    }
    public void QueryAndDisplayData()
    {
        using (SqliteCommand command = new SqliteCommand("SELECT * FROM Players;", dbConnection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                string text = "";
                while (reader.Read())
                {
                    text = text + $"ID: {reader["ID"]}, Username: {reader["Username"]}, Password: {reader["Password"]}, Wage: {reader["Wage"]}, Money: {reader["Money"]}\n";
                    //GD.Print(text);
                    //Label label = GetNode<Label>("/root/Map/Player/HUD/Info2");
                    //label.Text = text;

                }
            }
        }
    }
}