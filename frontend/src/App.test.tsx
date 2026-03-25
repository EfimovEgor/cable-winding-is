import { render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, test, vi } from 'vitest'
import App from './App'

const okJson = (data: unknown) =>
  Promise.resolve({
    ok: true,
    json: async () => data,
  } as Response)

describe('App gamification integration', () => {
  beforeEach(() => {
    window.history.pushState({}, '', '/gamification')
  })

  test('renders gamification tab with summary and history data', async () => {
    vi.spyOn(globalThis, 'fetch').mockImplementation((input: RequestInfo | URL) => {
      const url = String(input)

      if (url.includes('/api/gamification/summary')) {
        return okJson({
          totalCycles: 4,
          stableCycles: 3,
          totalPoints: 280,
          totalXp: 320,
          currentStreak: 2,
          bestStreak: 3,
          averageQuality: 88,
          recommendation: 'Стабилизируйте скорость намотки.',
          currentLevel: {
            level: 2,
            title: 'Оператор линии',
            minXp: 180,
            maxXp: 359,
            progressPercent: 78,
            remainingXp: 40,
          },
          nextLevel: {
            level: 3,
            title: 'Уверенный оператор',
            minXp: 360,
            maxXp: 539,
            progressPercent: 0,
            remainingXp: 40,
          },
          achievements: [],
        })
      }

      if (url.includes('/api/gamification/history')) {
        return okJson([
          {
            ts: '2026-03-19T10:00:00Z',
            speed: 64.8,
            tension: 3.22,
            step: 1.01,
            quality: 91,
            points: 84,
            xp: 104,
            streakAfter: 2,
            statusLabel: 'Отлично',
          },
        ])
      }

      if (url.includes('/api/gamification/achievements')) {
        return okJson([
          {
            id: 'first_cycle',
            title: 'Первый стабильный цикл',
            description: 'Выполнить первую производственную операцию под контролем системы.',
            unlocked: true,
          },
        ])
      }

      return Promise.reject(new Error(`Unexpected fetch call: ${url}`))
    })

    render(<App />)

    expect(screen.getByText('Прогресс оператора')).toBeInTheDocument()

    await waitFor(() => {
      expect(screen.getByText('Прогресс оператора намотки')).toBeInTheDocument()
    })

    expect(screen.getByText('280')).toBeInTheDocument()
    expect(screen.getAllByText('Оператор линии').length).toBeGreaterThan(0)
    expect(screen.getByText('Первый стабильный цикл')).toBeInTheDocument()
    expect(screen.getByText('Стабилизируйте скорость намотки.')).toBeInTheDocument()
    expect(screen.getByText('Последние циклы по телеметрии')).toBeInTheDocument()
  })

  test('shows error when gamification endpoints fail', async () => {
    vi.spyOn(globalThis, 'fetch').mockRejectedValue(new Error('network down'))

    render(<App />)

    await waitFor(() => {
      expect(screen.getByText(/Ошибка загрузки раздела прогресса/i)).toBeInTheDocument()
    })
  })
})
