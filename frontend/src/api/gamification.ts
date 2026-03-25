const API_BASE = '/api/gamification'

export interface GamificationLevel {
  level: number
  title: string
  minXp: number
  maxXp: number | null
  progressPercent: number
  remainingXp: number
}

export interface GamificationAchievement {
  id: string
  title: string
  description: string
  unlocked: boolean
}

export interface GamificationHistoryEntry {
  ts: string
  speed: number
  tension: number
  step: number
  quality: number
  points: number
  xp: number
  streakAfter: number
  statusLabel: string
}

export interface GamificationSummary {
  totalCycles: number
  stableCycles: number
  totalPoints: number
  totalXp: number
  currentStreak: number
  bestStreak: number
  averageQuality: number
  recommendation: string
  currentLevel: GamificationLevel
  nextLevel: GamificationLevel | null
  achievements: GamificationAchievement[]
}

async function parseJson<T>(input: RequestInfo | URL, init?: RequestInit): Promise<T> {
  const response = await fetch(input, init)
  if (!response.ok) {
    throw new Error(`Ошибка: ${response.status}`)
  }

  return response.json() as Promise<T>
}

export function getGamificationSummary(): Promise<GamificationSummary> {
  return parseJson<GamificationSummary>(`${API_BASE}/summary`)
}

export function getGamificationHistory(limit = 8): Promise<GamificationHistoryEntry[]> {
  return parseJson<GamificationHistoryEntry[]>(`${API_BASE}/history?limit=${limit}`)
}

export function getGamificationAchievements(): Promise<GamificationAchievement[]> {
  return parseJson<GamificationAchievement[]>(`${API_BASE}/achievements`)
}
