using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PartnersPlatform.Utility
{
    public class DbConnectionHelper
    {
        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("dbsettings.json");
            var parseResult = builder.Build();
            string connectionString = parseResult["ConnectionStrings:ConnString2"];
            return connectionString;
        }
        public static string GetSalt()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("dbsettings.json");
            var parseResult = builder.Build();
            string salt = parseResult["Salt:Salty"];
            return salt;
        }

        public static string GetUrl()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("dbsettings.json");
            var parseResult = builder.Build();
            string url = parseResult["Url:UrlHealth"];
            return url;
        }

        public static string GetToken()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("dbsettings.json");
            var parseResult = builder.Build();
            string token = parseResult["TokenHealth:Token"];
            return token;
        }
    }
}
