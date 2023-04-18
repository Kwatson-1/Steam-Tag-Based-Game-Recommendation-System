using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
Developer: Kyle Watson
Date: 18/04/2023
Project Tasks/Goals
    1. Program will fetch a list of all Steam apps which includes their name and ID from the Steam API using the GetSteamGamesAsync endpoint. 
    2. Stores the information in a CSV file for backup purposes and to minimize api calls.
    3. AppIDs are then pulled from the CSV file and stored in a List.
    4. The List is then looped through to pull the AppID which is then used in a call to the Steam API using the GetGameDetailsAsync endpoint.
    4a. Use Task.Delay to introduce a delay between each call to avoid potential issues or bans.
    5. Game information is stored in a JSON file for backup purposes and to minimize api calls.
    6. Call back the data fro the JSON file and parse the JSON response for each app and extract the necessary data.
    7. Use a library to connect the SQL Server dataase and insert the extracted data.
*/
namespace SteamAppDetailsToSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }

    }
}
