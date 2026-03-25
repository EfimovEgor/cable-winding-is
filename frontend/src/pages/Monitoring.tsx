import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getLatestTelemetry } from '../api/telemetry'
import type { TelemetryLatestResponse } from '../api/telemetry'

export function Monitoring() {
  const [data, setData] = useState<TelemetryLatestResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const poll = async () => {
      try {
        const r = await getLatestTelemetry()
        setData(r)
        setError(null)
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Ошибка загрузки')
      } finally {
        setLoading(false)
      }
    }

    poll()
    const id = setInterval(poll, 2000)
    return () => clearInterval(id)
  }, [])

  if (loading && !data) {
    return (
      <div style={{ padding: 24, textAlign: 'center' }}>
        Загрузка параметров…
      </div>
    )
  }

  if (error && !data) {
    return (
      <div style={{ padding: 24, color: '#b91c1c' }}>
        Ошибка подключения к API: {error}
      </div>
    )
  }

  const params = data?.params_ ?? null
  const receivedAt = data?.receivedAt ?? null

  return (
    <div style={{ padding: 24, maxWidth: 800 }}>
      <h1 style={{ marginBottom: 16 }}>Мониторинг параметров намотки</h1>

      <div style={{ marginBottom: 24, fontSize: 14, color: '#64748b' }}>
        {receivedAt
          ? `Последнее обновление: ${new Date(receivedAt).toLocaleString('ru')}`
          : 'Данные ещё не поступали от TraceConnector'}
      </div>

      <section
        style={{
          background: '#fff',
          borderRadius: 8,
          padding: 24,
          boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
        }}
      >
        <h2 style={{ marginTop: 0, marginBottom: 16, fontSize: 18 }}>Текущие параметры</h2>

        {params ? (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <tbody>
              <tr style={{ borderBottom: '1px solid #e5e7eb' }}>
                <td style={{ padding: '12px 0', fontWeight: 600 }}>Время (датчик)</td>
                <td style={{ padding: '12px 0' }}>
                  {new Date(params.Ts).toLocaleString('ru')}
                </td>
              </tr>
              <tr style={{ borderBottom: '1px solid #e5e7eb' }}>
                <td style={{ padding: '12px 0', fontWeight: 600 }}>Скорость</td>
                <td style={{ padding: '12px 0' }}>{params.Speed}</td>
              </tr>
              <tr style={{ borderBottom: '1px solid #e5e7eb' }}>
                <td style={{ padding: '12px 0', fontWeight: 600 }}>Натяжение</td>
                <td style={{ padding: '12px 0' }}>{params.Tension}</td>
              </tr>
              <tr>
                <td style={{ padding: '12px 0', fontWeight: 600 }}>Шаг</td>
                <td style={{ padding: '12px 0' }}>{params.Step}</td>
              </tr>
            </tbody>
          </table>
        ) : (
          <p style={{ color: '#64748b', margin: 0 }}>
            Ожидание данных от TraceConnector. Убедитесь, что симулятор или OPC UA‑клиент отправляет параметры на POST /api/telemetry/params.
          </p>
        )}
      </section>

      <section style={{ marginTop: 24 }}>
        <h3 style={{ fontSize: 16, marginBottom: 8 }}>Индикатор статуса</h3>
        <div
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 8,
            padding: '8px 16px',
            borderRadius: 8,
            background: params ? '#dcfce7' : '#fef3c7',
            color: params ? '#166534' : '#92400e',
          }}
        >
          <span
            style={{
              width: 10,
              height: 10,
              borderRadius: '50%',
              background: params ? '#22c55e' : '#eab308',
            }}
          />
          {params ? 'В норме (данные поступают)' : 'Нет данных'}
        </div>
      </section>

      <section
        style={{
          marginTop: 24,
          background: '#eff6ff',
          border: '1px solid #bfdbfe',
          borderRadius: 10,
          padding: 20,
        }}
      >
        <div style={{ fontSize: 13, fontWeight: 700, color: '#2563eb', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
          Аналитика смены
        </div>
        <h3 style={{ margin: '8px 0 10px', fontSize: 20 }}>Прогресс оператора</h3>
        <p style={{ margin: 0, color: '#475569', lineHeight: 1.6 }}>
          В отдельном разделе доступны очки, уровень квалификации, серия стабильных циклов,
          достижения и история оценки производственных операций.
        </p>
        <Link
          to="/gamification"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            marginTop: 16,
            padding: '10px 16px',
            borderRadius: 8,
            background: '#2563eb',
            color: '#fff',
            fontWeight: 700,
            textDecoration: 'none',
          }}
        >
          Открыть прогресс оператора
        </Link>
      </section>
    </div>
  )
}
