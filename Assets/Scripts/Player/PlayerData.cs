
using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[System.Serializable]
public struct PlayerData
{
    [JsonProperty("user_name")]
    public string Username;
    public string Password;
    public string Email;
    public float Experience;
    public UserType UserType;
    public string LastLoggedIn;
    [JsonIgnore]
    public int Level => (int)(Experience / 100);

    public DateTime GetDateTime()
    {
        if (DateTime.TryParseExact(LastLoggedIn,
                "o",
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out var dateTime))
        {
            return dateTime;
        }

        return DateTime.UtcNow;
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum UserType
{
    Admin = 0,
    User = 1,
    Manager = 2
}
