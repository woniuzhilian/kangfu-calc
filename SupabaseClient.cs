using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace 抗浮计算书
{
    /// <summary>封装对 Supabase Auth / REST 的调用（登录、注册、找回密码、手机号反查邮箱）。</summary>
    internal static class SupabaseClient
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private static HttpRequestMessage New(HttpMethod method, string path, string? jsonBody, string? bearer = null)
        {
            var req = new HttpRequestMessage(method, SupabaseConfig.Url.TrimEnd('/') + path);
            req.Headers.Add("apikey", SupabaseConfig.AnonKey);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer ?? SupabaseConfig.AnonKey);
            if (jsonBody != null) req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return req;
        }

        private static async Task<(bool ok, int status, JsonElement json)> Send(HttpRequestMessage req)
        {
            try
            {
                var resp = await Http.SendAsync(req);
                var text = await resp.Content.ReadAsStringAsync();
                JsonElement json;
                try { json = string.IsNullOrWhiteSpace(text) ? JsonDocument.Parse("{}").RootElement : JsonDocument.Parse(text).RootElement; }
                catch { json = JsonDocument.Parse("{\"raw\":\"" + JsonSerializer.Serialize(text) + "\"}").RootElement; }
                return (resp.IsSuccessStatusCode, (int)resp.StatusCode, json);
            }
            catch (TaskCanceledException)
            {
                return (false, 0, JsonDocument.Parse("{\"error_msg\":\"连接超时，请检查网络后重试\"}").RootElement);
            }
            catch (Exception ex)
            {
                return (false, 0, JsonDocument.Parse("{\"error_msg\":\"" + JsonSerializer.Serialize(ex.Message) + "\"}").RootElement);
            }
        }

        private static string Str(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v))
                return v.ValueKind == JsonValueKind.String ? (v.GetString() ?? "") : v.ToString();
            return "";
        }

        // 手机号→邮箱；未注册返回 null
        public static async Task<string?> LookupEmailByPhone(string phone)
        {
            var path = "/rest/v1/profiles?select=email&phone=eq." + Uri.EscapeDataString(phone);
            var (ok, _, json) = await Send(New(HttpMethod.Get, path, null));
            if (!ok) return null;
            if (json.ValueKind == JsonValueKind.Array && json.GetArrayLength() > 0)
                return Str(json[0], "email");
            return null;
        }

        public sealed class AuthResult
        {
            public bool Success;
            public string Message = "";
            public string AccessToken = "";
            public string Email = "";
        }

        // 手机号+密码登录：先反查邮箱再走密码登录
        public static async Task<AuthResult> Login(string phone, string password)
        {
            if (!SupabaseConfig.Configured)
                return new AuthResult { Message = "尚未配置 Supabase，请先在 SupabaseConfig.cs 填入项目地址和密钥。" };
            var email = await LookupEmailByPhone(phone);
            if (string.IsNullOrEmpty(email))
                return new AuthResult { Message = "该手机号未注册。" };
            var body = JsonSerializer.Serialize(new Dictionary<string, string> { ["email"] = email!, ["password"] = password });
            var (ok, _, json) = await Send(New(HttpMethod.Post, "/auth/v1/token?grant_type=password", body));
            if (!ok) return new AuthResult { Message = "手机号或密码错误。" };
            return new AuthResult { Success = true, AccessToken = Str(json, "access_token"), Email = email! };
        }

        // 注册：手机号(用户名) + 邮箱(找回密码) + 密码
        public static async Task<AuthResult> Signup(string phone, string email, string password)
        {
            if (!SupabaseConfig.Configured)
                return new AuthResult { Message = "尚未配置 Supabase，请先在 SupabaseConfig.cs 填入项目地址和密钥。" };
            var payload = new Dictionary<string, object>
            {
                ["email"] = email,
                ["password"] = password,
                ["data"] = new Dictionary<string, string> { ["phone"] = phone }
            };
            var body = JsonSerializer.Serialize(payload);
            var (ok, _, json) = await Send(New(HttpMethod.Post, "/auth/v1/signup", body));
            if (!ok)
            {
                string msg = Str(json, "error_description");
                if (string.IsNullOrEmpty(msg)) msg = Str(json, "msg");
                if (string.IsNullOrEmpty(msg)) msg = Str(json, "message");
                if (string.IsNullOrEmpty(msg)) msg = "注册失败";
                return new AuthResult { Message = msg };
            }
            var token = Str(json, "access_token");
            if (!string.IsNullOrEmpty(token))
                return new AuthResult { Success = true, AccessToken = token, Email = email };
            // 需邮箱确认：尝试直接登录一次
            var lg = await Login(phone, password);
            if (lg.Success) return lg;
            return new AuthResult { Message = "注册成功，请到邮箱确认后再登录。" };
        }

        // 找回密码：向邮箱发送重置邮件
        public static async Task<AuthResult> Recover(string email)
        {
            if (!SupabaseConfig.Configured)
                return new AuthResult { Message = "尚未配置 Supabase，请先在 SupabaseConfig.cs 填入项目地址和密钥。" };
            var body = JsonSerializer.Serialize(new Dictionary<string, string> { ["email"] = email });
            var (ok, _, json) = await Send(New(HttpMethod.Post, "/auth/v1/recover", body));
            if (!ok) return new AuthResult { Message = "发送失败：" + (Str(json, "error_description") ?? "") };
            return new AuthResult { Success = true, Message = "重置邮件已发送，请查收。" };
        }
    }
}
