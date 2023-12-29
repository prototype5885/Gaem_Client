using Godot;
using System.Data.SQLite;

public partial class Database : Node
{
    public override void _Ready()
    {
        //string dbpath = OS.GetUserDataDir() + "/database.sqlite";
        //GD.Print(dbpath);
        SQLiteConnection.CreateFile("database.sqlite");
        //SQLiteConnection dbConnection = new SQLiteConnection("Data Source=database.db;Version=3;");
        //string idk = "Data Source=" + dbpath;
        //GD.Print(idk);
        SQLiteConnection dbConnection = new SQLiteConnection("database.sqlite");
        dbConnection.Open();
        GD.Print("wtf");

        //string sql = "create table highscores (name varchar(20), score int)";
        //SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        //command.ExecuteNonQuery();

        //sql = "insert into highscores (name, score) values ('Eric', 9002)";
        //command = new SQLiteCommand(sql, dbConnection);
        //command.ExecuteNonQuery();

        //dbConnection.Close();
    }
}