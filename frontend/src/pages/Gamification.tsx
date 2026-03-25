import { useEffect, useMemo, useState, type CSSProperties } from 'react'
import {
  getGamificationAchievements,
  getGamificationHistory,
  getGamificationSummary,
  type GamificationAchievement,
  type GamificationHistoryEntry,
  type GamificationSummary,
} from '../api/gamification'

const cardStyle: CSSProperties = {
  background: '#fff',
  borderRadius: 12,
  padding: 20,
  boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
}

function StatCard(props: { title: string; value: string; caption: string }) {
  return (
    <div style={cardStyle}>
      <div style={{ color: '#64748b', fontSize: 14, marginBottom: 10 }}>{props.title}</div>
      <div style={{ fontSize: 32, fontWeight: 700, marginBottom: 8 }}>{props.value}</div>
      <div style={{ color: '#64748b', fontSize: 14 }}>{props.caption}</div>
    </div>
  )
}

function StatusPill({ label }: { label: string }) {
  const tone =
    label === 'Отлично' ? ['#dcfce7', '#166534'] :
    label === 'Стабильно' ? ['#dbeafe', '#1d4ed8'] :
    label === 'Требует внимания' ? ['#fef3c7', '#92400e'] :
    ['#fee2e2', '#b91c1c']

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        padding: '6px 10px',
        borderRadius: 999,
        fontSize: 12,
        fontWeight: 700,
        background: tone[0],
        color: tone[1],
      }}
    >
      {label}
    </span>
  )
}

export function Gamification() {
  const [summary, setSummary] = useState<GamificationSummary | null>(null)
  const [history, setHistory] = useState<GamificationHistoryEntry[]>([])
  const [achievements, setAchievements] = useState<GamificationAchievement[]>([])
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true

    const load = async () => {
      try {
        const [summaryData, historyData, achievementsData] = await Promise.all([
          getGamificationSummary(),
          getGamificationHistory(),
          getGamificationAchievements(),
        ])

        if (!active) {
          return
        }

        setSummary(summaryData)
        setHistory(historyData)
        setAchievements(achievementsData)
        setError(null)
      } catch (e) {
        if (!active) {
          return
        }

        setError(e instanceof Error ? e.message : 'Не удалось загрузить данные раздела прогресса')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    load()
    const id = setInterval(load, 4000)
    return () => {
      active = false
      clearInterval(id)
    }
  }, [])

  const unlockedAchievements = useMemo(
    () => achievements.filter((item) => item.unlocked).length,
    [achievements],
  )

  if (loading && !summary) {
    return <div style={{ padding: 24, textAlign: 'center' }}>Загрузка данных по прогрессу оператора…</div>
  }

  if (error && !summary) {
    return (
      <div style={{ padding: 24, color: '#b91c1c' }}>
        Ошибка загрузки раздела прогресса: {error}
      </div>
    )
  }

  if (!summary) {
    return (
      <div style={{ padding: 24, color: '#64748b' }}>
        Данные по прогрессу оператора пока недоступны. Убедитесь, что телеметрия поступает в систему.
      </div>
    )
  }

  return (
    <div style={{ padding: 24, display: 'grid', gap: 24 }}>
      <header
        style={{
          ...cardStyle,
          background: 'linear-gradient(135deg, #eff6ff, #f8fafc)',
          border: '1px solid #dbeafe',
        }}
      >
        <div style={{ color: '#2563eb', fontWeight: 700, fontSize: 13, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
          Оценка и развитие смены
        </div>
        <h1 style={{ margin: '8px 0 12px', fontSize: 32 }}>Прогресс оператора намотки</h1>
        <p style={{ margin: 0, maxWidth: 760, color: '#475569', lineHeight: 1.6 }}>
          Раздел показывает состояние оператора по данным телеметрии: очки, уровень квалификации,
          серию стабильных циклов, достижения и историю операций контроля процесса.
        </p>
      </header>

      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(4, minmax(0, 1fr))', gap: 16 }}>
        <StatCard title="Очки" value={String(summary.totalPoints)} caption="накоплено по истории телеметрии" />
        <StatCard title="Уровень" value={`LVL ${summary.currentLevel.level}`} caption={summary.currentLevel.title} />
        <StatCard title="Серия" value={String(summary.currentStreak)} caption={`лучшая серия: ${summary.bestStreak}`} />
        <StatCard title="Среднее качество" value={`${summary.averageQuality}%`} caption={`стабильных циклов: ${summary.stableCycles}`} />
      </section>

      <section style={{ display: 'grid', gridTemplateColumns: '1.15fr 0.85fr', gap: 16, alignItems: 'start' }}>
        <article style={cardStyle}>
          <div style={{ display: 'flex', justifyContent: 'space-between', gap: 16, alignItems: 'start' }}>
            <div>
              <div style={{ color: '#64748b', fontSize: 14, marginBottom: 8 }}>Текущий уровень</div>
              <h2 style={{ margin: 0, fontSize: 24 }}>{summary.currentLevel.title}</h2>
            </div>
            <StatusPill label={summary.averageQuality >= 85 ? 'Стабильно' : 'Требует внимания'} />
          </div>

          <div style={{ marginTop: 18, display: 'grid', gap: 10 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', color: '#475569', fontSize: 14 }}>
              <strong>{summary.totalXp} XP</strong>
              <span>
                {summary.nextLevel
                  ? `до следующего уровня: ${summary.currentLevel.remainingXp} XP`
                  : 'максимальный уровень достигнут'}
              </span>
            </div>
            <div style={{ height: 14, borderRadius: 999, background: '#e2e8f0', overflow: 'hidden' }}>
              <div
                style={{
                  width: `${summary.currentLevel.progressPercent}%`,
                  height: '100%',
                  background: 'linear-gradient(90deg, #2563eb, #38bdf8)',
                }}
              />
            </div>
          </div>

          <div
            style={{
              marginTop: 20,
              padding: 16,
              borderRadius: 10,
              background: '#f8fafc',
              border: '1px solid #e2e8f0',
            }}
          >
            <div style={{ color: '#64748b', fontSize: 14, marginBottom: 8 }}>Рекомендация системы</div>
            <div style={{ lineHeight: 1.6, color: '#0f172a' }}>{summary.recommendation}</div>
          </div>
        </article>

        <article style={cardStyle}>
          <div style={{ color: '#64748b', fontSize: 14, marginBottom: 8 }}>Достижения</div>
          <h2 style={{ margin: '0 0 8px', fontSize: 24 }}>{unlockedAchievements} открыто</h2>
          <p style={{ margin: 0, color: '#475569', lineHeight: 1.5 }}>
            Открытые достижения отражают устойчивость процесса и качество работы оператора.
          </p>
          <div style={{ marginTop: 16, display: 'grid', gap: 10 }}>
            {achievements.map((achievement) => (
              <div
                key={achievement.id}
                style={{
                  padding: 14,
                  borderRadius: 10,
                  border: `1px solid ${achievement.unlocked ? '#86efac' : '#e2e8f0'}`,
                  background: achievement.unlocked ? '#f0fdf4' : '#f8fafc',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginBottom: 6 }}>
                  <strong>{achievement.title}</strong>
                  <StatusPill label={achievement.unlocked ? 'Отлично' : 'Требует внимания'} />
                </div>
                <div style={{ color: '#475569', fontSize: 14 }}>{achievement.description}</div>
              </div>
            ))}
          </div>
        </article>
      </section>

      <section style={cardStyle}>
        <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginBottom: 16, alignItems: 'end' }}>
          <div>
            <div style={{ color: '#64748b', fontSize: 14, marginBottom: 8 }}>История начислений и контроля</div>
            <h2 style={{ margin: 0, fontSize: 24 }}>Последние циклы по телеметрии</h2>
          </div>
          {error ? <span style={{ color: '#b91c1c', fontSize: 14 }}>Последняя ошибка: {error}</span> : null}
        </div>

        {history.length > 0 ? (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#f8fafc' }}>
                  <th style={tableHeaderStyle}>Время</th>
                  <th style={tableHeaderStyle}>Скорость</th>
                  <th style={tableHeaderStyle}>Натяжение</th>
                  <th style={tableHeaderStyle}>Шаг</th>
                  <th style={tableHeaderStyle}>Качество</th>
                  <th style={tableHeaderStyle}>Очки</th>
                  <th style={tableHeaderStyle}>XP</th>
                  <th style={tableHeaderStyle}>Статус</th>
                </tr>
              </thead>
              <tbody>
                {history.map((entry) => (
                  <tr key={`${entry.ts}-${entry.speed}-${entry.tension}`} style={{ borderBottom: '1px solid #e2e8f0' }}>
                    <td style={tableCellStyle}>{new Date(entry.ts).toLocaleString('ru')}</td>
                    <td style={tableCellStyle}>{entry.speed.toFixed(1)}</td>
                    <td style={tableCellStyle}>{entry.tension.toFixed(2)}</td>
                    <td style={tableCellStyle}>{entry.step.toFixed(2)}</td>
                    <td style={tableCellStyle}>{entry.quality}%</td>
                    <td style={tableCellStyle}>{entry.points}</td>
                    <td style={tableCellStyle}>{entry.xp}</td>
                    <td style={tableCellStyle}>
                      <StatusPill label={entry.statusLabel} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div style={{ color: '#64748b' }}>
            История появится после поступления телеметрии от симулятора или TraceConnector.
          </div>
        )}
      </section>
    </div>
  )
}

const tableHeaderStyle: CSSProperties = {
  textAlign: 'left',
  padding: '12px 10px',
  color: '#64748b',
  fontSize: 13,
  textTransform: 'uppercase',
  letterSpacing: '0.04em',
}

const tableCellStyle: CSSProperties = {
  padding: '12px 10px',
  color: '#0f172a',
}
