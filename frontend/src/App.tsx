import { BrowserRouter, NavLink, Route, Routes } from 'react-router-dom'
import { Monitoring } from './pages/Monitoring'
import { Gamification } from './pages/Gamification'

export default function App() {
  return (
    <BrowserRouter>
      <div style={{ display: 'flex', minHeight: '100vh' }}>
        <nav
          style={{
            width: 200,
            background: '#1e293b',
            color: '#f8fafc',
            padding: 16,
          }}
        >
          <div style={{ fontWeight: 700, marginBottom: 24, fontSize: 18 }}>
            ИС намотки
          </div>
          <NavLink
            to="/"
            style={({ isActive }) => ({
              display: 'block',
              padding: '8px 12px',
              marginBottom: 4,
              borderRadius: 6,
              color: isActive ? '#fff' : '#94a3b8',
              background: isActive ? '#334155' : 'transparent',
            })}
          >
            Мониторинг
          </NavLink>
          <NavLink
            to="/gamification"
            style={({ isActive }) => ({
              display: 'block',
              padding: '8px 12px',
              marginBottom: 4,
              borderRadius: 6,
              color: isActive ? '#fff' : '#94a3b8',
              background: isActive ? '#334155' : 'transparent',
            })}
          >
            Прогресс оператора
          </NavLink>
        </nav>
        <main style={{ flex: 1, overflow: 'auto', background: '#f0f2f5' }}>
          <Routes>
            <Route path="/" element={<Monitoring />} />
            <Route path="/gamification" element={<Gamification />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}
