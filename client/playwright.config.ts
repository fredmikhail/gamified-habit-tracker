import { defineConfig, devices } from '@playwright/test'
import { existsSync } from 'node:fs'
import { loadEnvFile } from 'node:process'

const localEnvironmentFile = '.env.e2e'

if (existsSync(localEnvironmentFile)) {
  loadEnvFile(localEnvironmentFile)
}

function getRequiredEnvironmentVariable(name: string): string {
  const value = process.env[name]

  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`)
  }

  return value
}

const backendUrl = getRequiredEnvironmentVariable('ASPNETCORE_URLS')
const frontendUrl = 'http://127.0.0.1:5174'

export default defineConfig({
  testDir: './e2e',
  workers: 1,

  forbidOnly: Boolean(process.env.CI),
  retries: process.env.CI ? 2 : 0,

  reporter: [['list'], ['html', { open: 'never' }]],

  use: {
    baseURL: frontendUrl,
    trace: 'retain-on-failure',
  },

  webServer: [
    {
      command:
        'dotnet run --project ../server/HabitTracker.Api/HabitTracker.Api.csproj --no-launch-profile',
      url: `${backendUrl}/api/health`,
      reuseExistingServer: false,
      timeout: 120_000,
    },
    {
      command: 'npm run dev -- --mode e2e --host 127.0.0.1 --port 5174',
      url: frontendUrl,
      reuseExistingServer: false,
      timeout: 120_000,
    },
  ],

  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
      },
    },
  ],
})
