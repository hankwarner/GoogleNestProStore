using System;
using System.Net;
using Newtonsoft.Json;

namespace TeamsHelper
{
    public class TeamsMessage
    {
        public TeamsMessage(string title, string text, string color, string teamsUrl)
        {
            Title = title;
            Text = text;
            ThemeColor = GetColorHex(color);
            TeamsUrl = teamsUrl;
        }

        private string GetColorHex(string color)
        {
            string colorHex;

            if (color == "red")
            {
                colorHex = "CD2626";
            }
            else if (color == "green")
            {
                colorHex = "3D8B37";
            }
            else if (color == "yellow")
            {
                colorHex = "FFFF33";
            }
            else if (color == "purple")
            {
                colorHex = "7F00FF";
            }
            else
            {
                colorHex = "F8F8FF"; // White
            }

            return colorHex;
        }

        public void LogToMicrosoftTeams(TeamsMessage teamsMessage)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var jsonRequest = JsonConvert.SerializeObject(teamsMessage);
                client.UploadString(teamsMessage.TeamsUrl, jsonRequest);
            }
        }

        public string Title { get; set; }
        public string Text { get; set; }
        public string ThemeColor { get; set; }
        public string TeamsUrl { get; set; }
    }
}
