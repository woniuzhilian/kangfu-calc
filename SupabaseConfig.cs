namespace 抗浮计算书
{
    /// <summary>Supabase 连接配置。到 supabase.com → 项目 → Project Settings → API 复制填入。</summary>
    internal static class SupabaseConfig
    {
        // Project URL，例如 https://xxxxxxxx.supabase.co
        public const string Url = "https://txpslvikgqjzsyqvkkct.supabase.co";

        // anon public key（公钥，可安全内置于客户端；数据由数据库 RLS 保护）
        public const string AnonKey = "sb_publishable_30qICkDKlpArNU3ym2GMDw_iAYnpL6H";

        public static bool Configured =>
            !string.IsNullOrWhiteSpace(Url) && !Url.StartsWith("__SUPABASE_URL__");
    }
}
