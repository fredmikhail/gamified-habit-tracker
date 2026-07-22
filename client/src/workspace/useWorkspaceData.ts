import { useContext } from 'react'
import {
  WorkspaceDataContext,
  type WorkspaceDataContextValue,
} from './WorkspaceDataContext'

export function useWorkspaceData(): WorkspaceDataContextValue {
  const context = useContext(WorkspaceDataContext)

  if (!context) {
    throw new Error(
      'useWorkspaceData must be used inside WorkspaceDataProvider.',
    )
  }

  return context
}
