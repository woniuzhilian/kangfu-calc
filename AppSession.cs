namespace 抗浮计算书
{
    /// <summary>全局登录态。游客进入时 IsGuest=true（禁止导出计算书）。</summary>
    internal static class AppSession
    {
        public static bool IsGuest = true;
        public static string Phone = "";
        public static string Email = "";
        public static string AccessToken = "";

        public static void SetGuest()
        {
            IsGuest = true; Phone = ""; Email = ""; AccessToken = "";
        }

        public static void SetLoggedIn(string phone, string email, string accessToken)
        {
            IsGuest = false; Phone = phone; Email = email; AccessToken = accessToken;
        }
    }
}
