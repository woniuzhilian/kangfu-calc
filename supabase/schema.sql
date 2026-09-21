-- ============================================================
-- 抗浮计算书 用户系统 —— Supabase 初始化脚本
-- 用法：登录 supabase.com → 选择你的项目 → 左侧 SQL Editor →
--       New query → 粘贴本文件全部内容 → Run。执行一次即可。
-- ============================================================

-- 1) 用户资料表：把「手机号(用户名)」和 Supabase 的登录邮箱关联起来
create table if not exists public.profiles (
  id          uuid primary key references auth.users(id) on delete cascade,
  phone       text unique not null,          -- 注册时填的手机号，作为登录用户名
  email       text not null,                 -- Supabase 登录邮箱，用于找回密码
  created_at  timestamptz not null default now()
);

alter table public.profiles enable row level security;

-- 1.5) 表级授权：RLS 策略之外，还必须给 anon/authenticated 角色授予 SELECT，
--      否则 PostgREST 会报 "permission denied for table profiles"。
grant usage on schema public to anon, authenticated;
grant select on public.profiles to anon, authenticated;

-- 2) 登录时需要用手机号反查邮箱，所以允许匿名读取 email/phone 两列
--    （不放开写入/删除，注册只通过下面的触发器自动写入，客户端无法篡改他人资料）
drop policy if exists "profiles_select_for_login" on public.profiles;
create policy "profiles_select_for_login"
  on public.profiles for select
  to anon, authenticated
  using (true);

-- 3) 注册触发器：用户在 Auth 里注册成功后，自动往 profiles 写一行
--    客户端注册时会把手机号放进 user metadata 的 phone 字段
create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer set search_path = public
as $$
begin
  insert into public.profiles (id, phone, email)
  values (
    new.id,
    coalesce(new.raw_user_meta_data ->> 'phone', ''),
    new.email
  )
  on conflict (id) do nothing;
  return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;
create trigger on_auth_user_created
  after insert on auth.users
  for each row execute function public.handle_new_user();

-- 完成。接下来到 Authentication → Providers 确认 Email 已开启，
-- 并在 Sign In / Up 里按需关闭「必须验证邮箱」以便本地测试。
