/** Глобальный объект аутентификации */
const Auth = {
    KEY: 'cablewinding_token',
    ROLE_KEY: 'cablewinding_role',
    NAME_KEY: 'cablewinding_name',

    save(token, role, displayName) {
        localStorage.setItem(this.KEY, token);
        localStorage.setItem(this.ROLE_KEY, role || '');
        localStorage.setItem(this.NAME_KEY, displayName || '');
    },

    getToken() { return localStorage.getItem(this.KEY); },
    getRole() { return localStorage.getItem(this.ROLE_KEY) || ''; },
    getName() { return localStorage.getItem(this.NAME_KEY) || ''; },

    logout() {
        localStorage.removeItem(this.KEY);
        localStorage.removeItem(this.ROLE_KEY);
        localStorage.removeItem(this.NAME_KEY);
        window.location.href = '/login.html';
    },

    /** Добавить Authorization к options для fetch */
    authHeaders(options = {}) {
        const token = this.getToken();
        const headers = options.headers || {};
        if (token) headers['Authorization'] = 'Bearer ' + token;
        return { ...options, headers };
    },

    /** fetch с авторизацией */
    fetch(url, options = {}) {
        return fetch(url, this.authHeaders(options));
    },

    /** Проверить, может ли роль открыть раздел (по use-case диаграмме) */
    canAccess(section) {
        const role = this.getRole();
        const map = {
            'monitor': ['Operator'],           // Мониторинг параметров — только Оператор
            'process': ['Operator'],           // Запуск, ввод параметров, аварийная остановка, корректировка
            'incidents': ['Operator'],         // Предупреждения, журнал инцидентов
            'norms': ['Technologist'],         // Настройка нормативов и допусков — только Технолог
            'reports': ['Operator'],           // Формирование отчетности
            'archive': ['Operator', 'Technologist'], // Просмотр архивных данных
            'diagnostics': ['Engineer'],       // Диагностика системы — только Сервисный инженер
            'roles': ['Operator', 'Technologist', 'Engineer'], // Справка по возможностям — все
            'api': ['Operator', 'Technologist', 'Engineer']   // Swagger — все
        };
        return (map[section] || []).includes(role);
    }
};
