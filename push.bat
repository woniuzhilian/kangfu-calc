@echo off
cd /d "%~dp0"

echo ============================================
echo   抗浮计算书 - 一键推送到 GitHub
echo ============================================
echo.

git add -A
git diff --cached --quiet
if %errorlevel%==0 (
    echo 没有需要上传的改动，本地与线上一致。
    goto :end
)

set "MSG=更新 %date% %time:~0,5%"
git commit -m "%MSG%"
if %errorlevel% neq 0 goto :err

echo.
echo 正在推送，网络波动时自动重试，最多 6 次...
set /a N=0
:pushloop
set /a N+=1
git push
if %errorlevel%==0 goto :pushok
if %N% geq 6 goto :pushfail
echo 第 %N% 次推送失败，20 秒后重试，按 Ctrl+C 可取消
timeout /t 20 /nobreak >/dev/null
goto :pushloop

:pushok
echo.
echo 推送成功！线上地址：
echo https://woniuzhilian.github.io/kangfu-calc/
echo GitHub Pages 约 1~2 分钟后更新到最新版。
goto :end

:pushfail
echo.
echo 推送失败：连续 6 次连不上 GitHub，请检查网络后重新双击本脚本。
goto :end

:err
echo.
echo 发生错误，请把上面的提示信息发给我分析。

:end
echo.
pause
