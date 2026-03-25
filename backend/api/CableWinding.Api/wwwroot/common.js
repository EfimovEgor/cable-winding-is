/** Первая доступная страница для роли */
function getHomeForRole() {
    if (Auth.canAccess('monitor')) return '/index.html';
    if (Auth.canAccess('norms')) return '/norms.html';
    if (Auth.canAccess('archive')) return '/archive.html';
    if (Auth.canAccess('diagnostics')) return '/diagnostics.html';
    return '/roles.html';
}

/** Общая инициализация страницы: проверка авторизации и навигация */
function initPage(currentSection) {
    if (!Auth.getToken()) {
        location.href = '/login.html';
        return false;
    }
    if (!Auth.canAccess(currentSection)) {
        document.body.innerHTML = '<div style="font-family:Segoe UI,sans-serif;padding:48px;text-align:center;background:#f5f5f5;min-height:100vh"><h1 style="color:#374151">Доступ запрещён</h1><p style="color:#6b7280">У вас нет прав на этот раздел.</p><a href="' + getHomeForRole() + '" style="color:#ea580c">Перейти в свой раздел</a></div>';
        return false;
    }
    var userEl = document.getElementById('userInfo');
    if (userEl) userEl.innerHTML = Auth.getName() + ' <a href="/roles.html" style="margin-left:12px;color:#6b7280;text-decoration:none">о ролях</a> <a href="#" onclick="Auth.logout();return false" style="margin-left:12px;color:#6b7280;text-decoration:none">Выход</a>';
    var navEl = document.getElementById('nav');
    if (navEl) {
        var links = [
            { href: '/index.html', section: 'monitor', text: 'Мониторинг' },
            { href: '/process.html', section: 'process', text: 'Процесс' },
            { href: '/incidents.html', section: 'incidents', text: 'Инциденты' },
            { href: '/norms.html', section: 'norms', text: 'Нормативы' },
            { href: '/reports.html', section: 'reports', text: 'Отчёты' },
            { href: '/archive.html', section: 'archive', text: 'Архив' },
            { href: '/diagnostics.html', section: 'diagnostics', text: 'Диагностика' },
            { href: '/swagger', section: 'api', text: 'API' }
        ];
        links.forEach(function(l) {
            if (Auth.canAccess(l.section)) {
                var a = document.createElement('a');
                a.href = l.href;
                a.textContent = l.text;
                a.style.marginRight = '16px';
                a.style.color = (l.section === currentSection) ? '#ea580c' : '#6b7280';
                a.style.textDecoration = 'none';
                if (l.section === currentSection) a.style.fontWeight = '600';
                navEl.appendChild(a);
            }
        });
    }
    return true;
}
