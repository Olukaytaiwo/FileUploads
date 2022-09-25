using System;
using System.Collections.Generic;
using System.Text;

namespace PartnersPlatform.Utility
{
    public class PasswordGenerator
    {
        #region Method To Generate Default Password To Be Sent To Intending Partner
        public static string GenerateGuidPassword()
        {
            string password = Guid.NewGuid().ToString().Substring(0, 9);
            return password;
        }
        #endregion

        const string LOWER_CASE = "abcdefghijklmnopqursuvwxyz";
        const string UPPER_CAES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string NUMBERS = "123456789";
        const string SPECIALS = @"!@$%&#";


        // SO CRUDE MY BAD! PLS 4GIVE ME O.T
        public static string GeneratePassword()
        {
            Random _random = new Random();
   
            string pass = "";

            pass += UPPER_CAES[_random.Next(0, UPPER_CAES.Length)];
            pass += NUMBERS[_random.Next(0, NUMBERS.Length)];
            pass += LOWER_CASE[_random.Next(0, LOWER_CASE.Length)];
            pass += SPECIALS[_random.Next(0, SPECIALS.Length)];
            pass += UPPER_CAES[_random.Next(0, UPPER_CAES.Length)];
            pass += NUMBERS[_random.Next(0, NUMBERS.Length)];
            pass += LOWER_CASE[_random.Next(0, LOWER_CASE.Length)];
            pass += SPECIALS[_random.Next(0, SPECIALS.Length)];

            return pass;

        }


    }
}
