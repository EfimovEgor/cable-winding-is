const API_BASE = '/api'

export interface TelemetryParams {
  Ts: string
  Speed: number
  Tension: number
  Step: number
}

export interface TelemetryLatestResponse {
  receivedAt: string | null
  params_: {
    Ts: string
    Speed: number
    Tension: number
    Step: number
  } | null
}

export async function getLatestTelemetry(): Promise<TelemetryLatestResponse> {
  const res = await fetch(`${API_BASE}/telemetry/latest`)
  if (!res.ok) throw new Error(`Ошибка: ${res.status}`)
  return res.json()
}

export async function pingTelemetry(): Promise<string> {
  const res = await fetch(`${API_BASE}/telemetry/ping`)
  if (!res.ok) throw new Error(`Ошибка: ${res.status}`)
  return res.text()
}

export async function postTelemetryParams(params: TelemetryParams): Promise<unknown> {
  const res = await fetch(`${API_BASE}/telemetry/params`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(params),
  })
  if (!res.ok) throw new Error(`Ошибка: ${res.status}`)
  return res.json()
}
