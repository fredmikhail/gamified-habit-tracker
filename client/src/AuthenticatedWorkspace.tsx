import { Navigate, Route, Routes } from 'react-router-dom'
import { AttributeSection } from './components/attributes/AttributeSection'
import { DashboardPage } from './components/dashboard/DashboardPage'
import { HabitSection } from './components/habits/HabitSection'
import { AppShell } from './components/layout/AppShell'
import type { CurrentUserResponse } from './types/CurrentUserResponse'
import { WorkspaceDataProvider } from './workspace/WorkspaceDataProvider'
import { useWorkspaceData } from './workspace/useWorkspaceData'

type AuthenticatedWorkspaceProps = {
  currentUser: CurrentUserResponse
  isLogoutPending: boolean
  onLogout: () => void
}

function AuthenticatedRoutes({
  currentUser,
  isLogoutPending,
  onLogout,
}: AuthenticatedWorkspaceProps) {
  const { refreshProgress } = useWorkspaceData()

  function handleProgressChanged(): void {
    void refreshProgress()
  }

  return (
    <Routes>
      <Route
        element={
          <AppShell
            currentUser={currentUser}
            isLogoutPending={isLogoutPending}
            onLogout={onLogout}
          />
        }
      >
        <Route index element={<Navigate replace to="/dashboard" />} />

        <Route path="/dashboard" element={<DashboardPage />} />

        <Route
          path="/habits"
          element={<HabitSection onProgressChanged={handleProgressChanged} />}
        />

        <Route path="/attributes" element={<AttributeSection />} />

        <Route path="*" element={<Navigate replace to="/dashboard" />} />
      </Route>
    </Routes>
  )
}

export function AuthenticatedWorkspace(props: AuthenticatedWorkspaceProps) {
  return (
    <WorkspaceDataProvider>
      <AuthenticatedRoutes {...props} />
    </WorkspaceDataProvider>
  )
}
