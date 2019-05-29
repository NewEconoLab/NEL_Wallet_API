using System.Text.RegularExpressions;

namespace NEL_Wallet_API.lib
{
    public static class EmailHelper
    {
        private static Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");

        public static bool checkEmail(this string email) {
            return r.IsMatch(email);
        }
    }
}
