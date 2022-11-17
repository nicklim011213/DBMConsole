using System;
using Npgsql;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace Database_Console
{
    class Program
    {
        static void Main()
        {

            string choice = "";
            System.Console.WriteLine("-----------~~OPTIONS~~----------");
            System.Console.WriteLine("--------------------------------");
            System.Console.WriteLine("|1. Insert From File           |");
            System.Console.WriteLine("|2. Test Connection            |");
            System.Console.WriteLine("|3. Send Command (read)        |");
            System.Console.WriteLine("|4. Add Game (manual)          |");
            System.Console.WriteLine("|5. Update Game (manual)       |");
            System.Console.WriteLine("|6. Remove Game (manual)       |");
            System.Console.WriteLine("|7. Insert Dev From File       |");
            System.Console.WriteLine("--------------------------------");
            choice = System.Console.ReadLine();
            if (choice == "1")
            {
                InsertFile();
            }
            else if (choice == "2")
            {
                Test();
            }
            else if (choice == "3")
            {
                CustomCommand();
            }
            else if (choice == "4")
            {
                AddGame();
            }
            else if (choice == "5")
            {
                UpdateGame();
            }
            else if (choice == "6")
            {
                DeleteGame();
            }
            else if (choice == "7")
            {
                DevInsert();
            }
            else if (choice == "8")
            {
                Recommend();
            }
        }

        static void InsertFile()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes"); //<ip> is an actual ip address
            conn.Open();
            int totalfiles = 0;
            for (int i = 0; i < 25000; i++)
            {
                //System.Console.WriteLine("C:\\Users\\Nick\\Desktop\\C# BrainDamage\\Database_Console" + i + ".txt");
                if (File.Exists("../../../" + i + ".txt"))
                {
                    System.Console.WriteLine(i + " is an ID that exists");

                    string alldata = File.ReadAllText("../../../" + i + ".txt");
                    int posrating = alldata.IndexOf("positive:") + 9;
                    int negrating = alldata.IndexOf("negative:") + 9;
                    int posratingend = alldata.IndexOf('$', posrating);
                    int negratingend = alldata.IndexOf('$', negrating);
                    string posratingstr = alldata.Substring(posrating, posratingend - posrating);
                    string negratingstr = alldata.Substring(negrating, negratingend - negrating);
                    int posratingint = Int32.Parse(posratingstr);
                    int negratingint = Int32.Parse(negratingstr);

                    if (negratingint == 0 && posratingint == 0)
                    {
                        System.Console.WriteLine("Skipping " + i + " it has no reviews and is likely a dead ID");
                    }
                    else
                    {
                        int devpos = alldata.IndexOf("developer:") + 10;
                        int pubpos = alldata.IndexOf("publisher:") + 10;
                        int devend = alldata.IndexOf('$', devpos);
                        int pubend = alldata.IndexOf('$', pubpos);
                        int namepos = alldata.IndexOf("name:") + 5;
                        int nameend = alldata.IndexOf('$', namepos);
                        int price = alldata.IndexOf("price:") + 6;
                        int priceend = alldata.IndexOf('$', price);
                        int genrepos = alldata.IndexOf("genre:") + 6;
                        int genreend = alldata.IndexOf('$', genrepos);
                        int tagspos = alldata.IndexOf("tags:") + 5;
                        int tagsend = alldata.Length;
                        string devstr = alldata.Substring(devpos, devend - devpos);
                        string pubstr = alldata.Substring(pubpos, pubend - pubpos);
                        string name = alldata.Substring(namepos, nameend - namepos);
                        string pricecoststr = alldata.Substring(price, priceend - price);
                        double pricecost = Double.Parse(pricecoststr) / (double)100;
                        string genre = alldata.Substring(genrepos, genreend - genrepos);
                        double ratingpercent = (double)posratingint / (double)(posratingint + negratingint);
                        string tagstr = alldata.Substring(tagspos, tagsend - tagspos);

                        Console.WriteLine(" ID " + i + " Has a dev , pub , name , price, genre, rating% and a taglist of");
                        Console.WriteLine(devstr);
                        Console.WriteLine(pubstr);
                        Console.WriteLine(name);
                        Console.WriteLine(pricecost);
                        Console.WriteLine(ratingpercent);
                        Console.WriteLine(genre);
                        Console.WriteLine(tagstr);

                        var Command = new NpgsqlCommand("INSERT INTO " + "released_game" + " ( " + "title" + " ) " + "VALUES" + " ( " + "'" + name + "'" + " ) ", conn);
                        try { 
                            Command.ExecuteNonQuery();
                        }
                        catch{
                            Console.WriteLine("Dup");
                        }
                        Command = new NpgsqlCommand("UPDATE " + "released_game" + " SET " + " price " + " = " + pricecost + " WHERE " + "title = '" + name + "'", conn);
                        Command.ExecuteNonQuery();
                        Thread.Sleep(250);
                        Command = new NpgsqlCommand("UPDATE " + "released_game" + " SET " + " genre " + " = '" + genre  + "' WHERE " + "title = '" + name + "'", conn);
                        Command.ExecuteNonQuery();
                        Thread.Sleep(250);
                        Command = new NpgsqlCommand("UPDATE " + "released_game" + " SET " + " tags " + " = '" + tagstr + "' WHERE " + "title = '" + name + "'", conn);
                        Command.ExecuteNonQuery();
                        Thread.Sleep(250);
                        Command = new NpgsqlCommand("UPDATE " + "released_game" + " SET " + " rating " + " = " + ratingpercent + " WHERE " + "title = '" + name + "'", conn);
                        Command.ExecuteNonQuery();
                        Thread.Sleep(250);
                        Command = new NpgsqlCommand("SELECT devname FROM dev WHERE devname = '" + devstr + "'", conn);
                        NpgsqlDataReader dr = Command.ExecuteReader();
                        Thread.Sleep(1000);
                        if (dr.HasRows == false)
                        {
                            dr.Close();
                            Command = new NpgsqlCommand("INSERT INTO dev (devname) VALUES ('" + devstr + "')", conn);
                            Command.ExecuteNonQuery();
                            Thread.Sleep(250);
                        }
                        else
                        {
                            dr.Close();
                            Command = new NpgsqlCommand("SELECT games_released FROM dev WHERE devname = '" + devstr + "'", conn);
                            dr = Command.ExecuteReader();
                            string preconcat = "";
                            if (dr.HasRows == false)
                            {
                                while (dr.Read())
                                {
                                    preconcat = preconcat + dr.GetString(0);
                                }
                            }
                            else
                            {
                                preconcat = "";
                            }
                            dr.Close();
                            string postconcat = preconcat + "@" + name;
                            Command = new NpgsqlCommand("UPDATE dev SET games_released = '" + postconcat + "' WHERE devname LIKE '%" + devstr + "%'", conn);
                            Command.ExecuteNonQuery();
                        }
                        Command = null;
                        dr = null;

                    }
                    totalfiles++;
                }
            }
            conn = null;
            System.Console.WriteLine("We have: " + totalfiles + " Files...");
            Main();
        }

        static void Test()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes"); //<ip> is an actual ip address
            try 
            { 
            conn.Open(); 
            }
            catch
            {
                System.Console.WriteLine("ERROR: Could not connect");
                conn = null;
                Main();
            }
            System.Console.WriteLine("Your connection is active");
            conn = null;
            Main();

            //NpgsqlCommand command = new NpgsqlCommand("SELECT games FROM user_steam", conn);
            //NpgsqlDataReader dr = command.ExecuteReader();

            //while (dr.Read())
            //    Console.WriteLine(dr.GetString(0));
        }

        static void CustomCommand()
        {
            String attributeSelect = "";
            String tableSelect = "";
            String ConditionSelect = "";

            System.Console.WriteLine("What do you want to select?");
            attributeSelect = Console.ReadLine();
            System.Console.WriteLine("What table is it in?");
            tableSelect = Console.ReadLine();
            System.Console.WriteLine("On what condition?");
            ConditionSelect = Console.ReadLine();

            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
            conn.Open();
            if (ConditionSelect == "")
            {
                NpgsqlCommand command = new NpgsqlCommand("SELECT " + attributeSelect + " FROM " + tableSelect, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    if (dr.GetFieldType(0) == typeof(float))
                    {
                        if (dr.IsDBNull(0))
                        {
                            Console.WriteLine("NULL");
                        }
                        else
                        {
                            Console.WriteLine(dr.GetFloat(0));
                        }
                    }
                    else if (dr.GetFieldType(0) == typeof(string))
                    {
                        if (dr.IsDBNull(0))
                        {
                            Console.WriteLine("NULL");
                        }
                        else
                        {
                            Console.WriteLine(dr.GetString(0));
                        }
                    }
                }
                conn = null;
                dr = null;
                command = null;
                Main();
            }
            else
            {
                NpgsqlCommand command = new NpgsqlCommand("SELECT " + attributeSelect + " FROM " + tableSelect + " WHERE " + ConditionSelect, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    if (dr.GetFieldType(0) == typeof(float))
                    {
                        if (dr.IsDBNull(0))
                        {
                            Console.WriteLine("NULL");
                        }
                        else
                        {
                            Console.WriteLine(dr.GetFloat(0));
                        }
                    }
                    else if (dr.GetFieldType(0) == typeof(string))
                    {
                        if (dr.IsDBNull(0))
                        {
                            Console.WriteLine("NULL");
                        }
                        else
                        {
                            Console.WriteLine(dr.GetString(0));
                        }
                    }
                }
                conn = null;
                dr = null;
                command = null;
                Main();
            }
        }

        static void AddGame()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
            conn.Open();

            String ValueSelect = "";
            String tableSelect = "";
            String CollumSelect = "";

            System.Console.WriteLine("What table do you want to insert into?");
            tableSelect = Console.ReadLine();
            System.Console.WriteLine("On what Collums | Seperate by ,");
            CollumSelect = Console.ReadLine();
            System.Console.WriteLine("What it the values? | Seperate by , and use ' for strings");
            ValueSelect = Console.ReadLine();

            var Command = new NpgsqlCommand("INSERT INTO " + tableSelect + " (" + CollumSelect + ") VALUES (" + ValueSelect + ")", conn);
            Command.ExecuteNonQueryAsync();

            System.Console.WriteLine("Is that All y/n");
            if (System.Console.ReadLine() == "n")
            {
                Command = null;
                conn = null;
                AddGame();
            }
            else
            {
                Command = null;
                conn = null;
                Main();
            }
        }

        static void UpdateGame()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
            conn.Open();

            String ValueSelect = "";
            String CollumSelect = "";
            String TableSelect = "";
            String Condition = "";

            System.Console.WriteLine("What is the table you want to update");
            TableSelect = Console.ReadLine();
            System.Console.WriteLine("On what Collums | Seperate by ,");
            CollumSelect = Console.ReadLine();
            System.Console.WriteLine("What is the condition... EX. games = 'fun'");
            Condition = Console.ReadLine();
            System.Console.WriteLine("What is the new values? | Seperate by , and use ' for strings (even in your conditional)");
            ValueSelect = Console.ReadLine();

            var Command = new NpgsqlCommand("UPDATE " + TableSelect + " SET " + CollumSelect + " = '" + ValueSelect + "' WHERE " + Condition, conn);
            Command.ExecuteNonQueryAsync();

            System.Console.WriteLine("Is that All y/n");
            if (System.Console.ReadLine() == "n")
            {
                Command = null;
                conn = null;
                UpdateGame();
            }
            else
            {
                Command = null;
                conn = null;
                Main();
            }
        }

        static void DeleteGame()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
            conn.Open();


            String TableSelect = "";
            String Condition = "";

            System.Console.WriteLine("What is the table you want to delete from");
            TableSelect = Console.ReadLine();
            System.Console.WriteLine("What is the condition... EX. games = 'fun'... and use ' for strings (even in your conditional)");
            Condition = Console.ReadLine();


            var Command = new NpgsqlCommand("DELETE FROM " + TableSelect + " WHERE " + Condition, conn);
            Command.ExecuteNonQueryAsync();

            System.Console.WriteLine("Is that All y/n");
            if (System.Console.ReadLine() == "n")
            {
                Command = null;
                conn = null;
                DeleteGame();
            }
            else
            {
                Command = null;
                conn = null;
                Main();
            }
        }
        static void DevInsert()
        {
            string[] Devs = new string[100];
            Main();
            for (int i = 0; i < 25000; i++)
            {
                NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
                conn.Open();
             //System.Console.WriteLine("C:\\Users\\Nick\\Desktop\\C# BrainDamage\\Database_Console" + i + ".txt");
                if (File.Exists("../../../" + i + ".txt"))
                {
                    bool indb = false;
                    string alldata = File.ReadAllText("../../../" + i + ".txt");
                    int devpos = alldata.IndexOf("developer:") + 10;
                    int devend = alldata.IndexOf('$', devpos);
                    string dev = alldata.Substring(devpos, devend - devpos);
                    //if (dev != "NULL")
                    //{
                        //System.Console.WriteLine(i);
                        //System.Console.WriteLine(dev);
                    //}
                    var command = new NpgsqlCommand("SELECT dev_name FROM developer WHERE dev_name = '" + dev + "'", conn);
                    NpgsqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows == false)
                    {
                        indb = false;
                    }
                    else
                    {
                        indb = true;
                    }
                    while (dr.Read())
                    {
                        Thread.Sleep(1);
                    }
                    if (indb == false)
                    {
                        command = new NpgsqlCommand("INSERT INTO developer (dev_name) VALUES '" + dev + "'", conn);
                        command.ExecuteNonQuery();
                    }
                }
                conn.Close();
                conn = null;
            }
        }

        static void Recommend()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1; Port=5432; User Id=postgres; Password=root; Database=PotatoKinishes");
            conn.Open();
            bool loop = true;
            bool loop2 = true;
            while (loop)
            {
                System.Console.WriteLine("Enter the tag you are looking for or next to continue");
                string tag = Console.ReadLine();
                if (tag == "next")
                {
                    loop = false;
                }
                List<string> taglist = new List<string>();
            }
            while (loop2)
            {
                System.Console.WriteLine("Enter the tag you are looking for or next to continue");
                string tag = Console.ReadLine();
                if (tag == "next")
                {
                    loop = false;
                }
            }
        }
    }
}
