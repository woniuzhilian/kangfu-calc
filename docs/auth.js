'use strict';
// 网页端登录系统：对接 Supabase Auth + profiles 表。
// 依赖 config.js 里的 SUPABASE_URL / SUPABASE_ANON_KEY（须先于本文件加载）。
// 对外暴露 window.KangfuAuth：{ isGuest(), currentUser(), logout(), onChange(fn) }
(function () {
  const SS_KEY = 'kangfu_auth_v1';

  function headers() {
    return {
      'apikey': SUPABASE_ANON_KEY,
      'Authorization': 'Bearer ' + SUPABASE_ANON_KEY,
      'Content-Type': 'application/json',
    };
  }

  // 从本地存储恢复登录态
  let session = null;
  try {
    const raw = localStorage.getItem(SS_KEY);
    if (raw) session = JSON.parse(raw);
  } catch (e) { session = null; }

  const listeners = [];
  function emit() { listeners.forEach(fn => { try { fn(api.state()); } catch (e) {} }); }
  function setSession(s) {
    session = s;
    if (s) localStorage.setItem(SS_KEY, JSON.stringify(s));
    else localStorage.removeItem(SS_KEY);
    emit();
  }

  const PHONE_RE = /^1\d{10}$/;
  const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  async function postJson(path, body, token) {
    const h = headers();
    if (token) h['Authorization'] = 'Bearer ' + token;
    const res = await fetch(SUPABASE_URL + path, { method: 'POST', headers: h, body: JSON.stringify(body) });
    let data = null; try { data = await res.json(); } catch (e) {}
    return { ok: res.ok, status: res.status, data };
  }
  async function getJson(path) {
    const res = await fetch(SUPABASE_URL + path, { method: 'GET', headers: headers() });
    let data = null; try { data = await res.json(); } catch (e) {}
    return { ok: res.ok, status: res.status, data };
  }

  function notConfigured() {
    return !SUPABASE_URL || SUPABASE_URL.indexOf('__SUPABASE_URL__') === 0;
  }

  const api = {
    state() {
      return { guest: !session, user: session ? (session.user || {}) : null };
    },
    isGuest() { return !session; },
    currentUser() { return session ? (session.user || null) : null; },
    logout() { setSession(null); },
    onChange(fn) { listeners.push(fn); },

    // 手机号→邮箱反查（找回密码用），未注册返回 null
    async lookupEmail(phone) {
      if (notConfigured()) throw new Error('尚未配置 Supabase，请先在 config.js 填入项目地址和密钥。');
      const rows = await getJson('/rest/v1/profiles?select=email&phone=eq.' + encodeURIComponent(phone));
      if (!rows.ok) throw new Error('查询用户失败：' + (rows.data && rows.data.message || rows.status));
      return (rows.data && rows.data.length) ? rows.data[0].email : null;
    },

    // 手机号+密码登录：先反查邮箱，再用邮箱走 Supabase 密码登录
    async login(phone, password) {
      if (notConfigured()) throw new Error('尚未配置 Supabase，请先在 config.js 填入项目地址和密钥。');
      if (!PHONE_RE.test(phone)) throw new Error('请输入 11 位手机号。');
      if (!password) throw new Error('请输入密码。');
      const rows = await getJson('/rest/v1/profiles?select=email&phone=eq.' + encodeURIComponent(phone));
      if (!rows.ok) throw new Error('查询用户失败：' + (rows.data && rows.data.message || rows.status));
      if (!rows.data || !rows.data.length) throw new Error('该手机号未注册。');
      const email = rows.data[0].email;
      const r = await postJson('/auth/v1/token?grant_type=password', { email, password });
      if (!r.ok) throw new Error('手机号或密码错误。');
      setSession({ access_token: r.data.access_token, user: { phone, email } });
      return api.state();
    },

    // 注册：手机号(用户名) + 邮箱(找回密码用) + 密码
    async signup(phone, email, password) {
      if (notConfigured()) throw new Error('尚未配置 Supabase，请先在 config.js 填入项目地址和密钥。');
      if (!PHONE_RE.test(phone)) throw new Error('请输入 11 位手机号。');
      if (!EMAIL_RE.test(email)) throw new Error('请输入正确的邮箱。');
      if (!password || password.length < 6) throw new Error('密码至少 6 位。');
      const r = await postJson('/auth/v1/signup', { email, password, data: { phone } });
      if (!r.ok) {
        const msg = (r.data && (r.data.error_description || r.data.msg || r.data.message)) || ('注册失败 ' + r.status);
        throw new Error(msg);
      }
      // 若项目开启了邮箱确认，signup 只返回待确认用户；这里尝试直接登录一次
      if (r.data.access_token) {
        setSession({ access_token: r.data.access_token, user: { phone, email } });
        return { state: api.state(), needConfirm: false };
      }
      const lg = await postJson('/auth/v1/token?grant_type=password', { email, password });
      if (lg.ok) {
        setSession({ access_token: lg.data.access_token, user: { phone, email } });
        return { state: api.state(), needConfirm: false };
      }
      return { state: api.state(), needConfirm: true };
    },

    // 忘记密码：向预留邮箱发送重置邮件
    async recover(email) {
      if (notConfigured()) throw new Error('尚未配置 Supabase，请先在 config.js 填入项目地址和密钥。');
      if (!EMAIL_RE.test(email)) throw new Error('请输入正确的邮箱。');
      const r = await postJson('/auth/v1/recover', { email });
      if (!r.ok) throw new Error('发送失败：' + (r.data && (r.data.error_description || r.data.message) || r.status));
      return true;
    },
  };

  window.KangfuAuth = api;
})();
