using System;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerDataHandler : MonoBehaviour
{
    private void Awake()
    {
        JsonConvert.DefaultSettings = () =>
        new JsonSerializerSettings()
        {
            DateFormatString = "ss:mm:HH MM*dd*yyyy",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.Indented
        };
    }

    [ContextMenu("Debug Player")]
    private void DebugPlayer()
    {
        var player = new PlayerData()
        {
            Email = "test@test.com",
            Experience = 0,
            Password = "password123",
            Username = "test",
            UserType = UserType.User,
            LastLoggedIn = DateTime.UtcNow.ToString("o")  
        };

        var stringJson = JsonConvert.SerializeObject(player);
        Debug.Log(stringJson);

        var stringToConvert =
            "{ \"user_name\": \"TestUser\", " +
            " \"Password\": \"password12345\", " +
            " \"Email\": \"test@test123.com\", " +
            " \"Experience\": 10000, "+
            $" \"LastLoggedIn\": \"{DateTime.UtcNow.AddDays(-1):o}\" " + "}";

        var convertedData = JsonConvert.DeserializeObject<PlayerData>(stringToConvert);
        Debug.Log($"{convertedData.Username} {convertedData.Password} {convertedData.Email} {convertedData.Experience} {convertedData.LastLoggedIn}");

        var timeLeft = DateTime.Now - convertedData.GetDateTime();
        Debug.Log($"{timeLeft.TotalHours:0}h Last Online");

    }

}
