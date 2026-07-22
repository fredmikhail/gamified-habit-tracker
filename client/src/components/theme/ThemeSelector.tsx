import {
  Check,
  Monitor,
  Moon,
  Palette,
  Sparkles,
  Sun,
  type LucideIcon,
} from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import type { ThemePreference } from '../../theme/themeTypes'
import { useTheme } from '../../theme/useTheme'

type ThemeOption = {
  value: ThemePreference
  label: string
  description: string
  previewBackground: string
  Icon: LucideIcon
}

const themeOptions: readonly ThemeOption[] = [
  {
    value: 'system',
    label: 'System',
    description: 'Follow this device',
    previewBackground:
      'linear-gradient(135deg, #050914 0%, #0d1929 49%, #eef3f8 50%, #ffffff 100%)',
    Icon: Monitor,
  },
  {
    value: 'abyss',
    label: 'Abyss',
    description: 'Cinematic blue-violet',
    previewBackground:
      'radial-gradient(circle at 80% 20%, #3b82f6 0%, transparent 34%), linear-gradient(135deg, #050914 0%, #091321 58%, #312e81 100%)',
    Icon: Sparkles,
  },
  {
    value: 'obsidian',
    label: 'Obsidian',
    description: 'Restrained graphite',
    previewBackground:
      'radial-gradient(circle at 80% 20%, #5b8cff 0%, transparent 28%), linear-gradient(135deg, #090b10 0%, #151a23 62%, #242b38 100%)',
    Icon: Moon,
  },
  {
    value: 'lumen',
    label: 'Lumen',
    description: 'Cool premium light',
    previewBackground:
      'radial-gradient(circle at 80% 20%, #93c5fd 0%, transparent 32%), linear-gradient(135deg, #e6edf6 0%, #ffffff 58%, #ddd6fe 100%)',
    Icon: Sun,
  },
]

function formatThemeName(theme: string): string {
  return theme.charAt(0).toUpperCase() + theme.slice(1)
}

export function ThemeSelector() {
  const { themePreference, resolvedTheme, setThemePreference } = useTheme()

  const [isOpen, setIsOpen] = useState(false)

  const containerRef = useRef<HTMLDivElement>(null)
  const triggerRef = useRef<HTMLButtonElement>(null)

  const panelId = useId()
  const headingId = useId()

  useEffect(() => {
    if (!isOpen) {
      return
    }

    function handlePointerDown(event: PointerEvent) {
      const target = event.target

      if (target instanceof Node && !containerRef.current?.contains(target)) {
        setIsOpen(false)
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key !== 'Escape') {
        return
      }

      setIsOpen(false)
      triggerRef.current?.focus()
    }

    document.addEventListener('pointerdown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)

    return () => {
      document.removeEventListener('pointerdown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [isOpen])

  function chooseTheme(preference: ThemePreference): void {
    setThemePreference(preference)
    setIsOpen(false)
    triggerRef.current?.focus()
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        ref={triggerRef}
        aria-controls={panelId}
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        aria-label="Choose appearance theme"
        className="inline-flex min-h-11 items-center gap-2 rounded-xl border border-line bg-surface-raised px-3.5 py-2.5 text-sm font-semibold text-content shadow-[var(--theme-panel-shadow)] transition-colors hover:border-line-strong hover:bg-surface-hover"
        type="button"
        onClick={() => setIsOpen((currentValue) => !currentValue)}
      >
        <Palette aria-hidden="true" size={18} strokeWidth={1.8} />

        <span className="hidden sm:inline">Appearance</span>
      </button>

      {isOpen && (
        <div
          id={panelId}
          aria-labelledby={headingId}
          className="absolute right-0 top-full z-50 mt-3 w-80 max-w-[calc(100vw-2rem)] rounded-2xl border border-line bg-surface-raised p-4 text-content shadow-[var(--theme-panel-shadow)]"
          role="dialog"
        >
          <div className="flex items-start justify-between gap-4">
            <div>
              <h2 id={headingId} className="font-semibold">
                Appearance
              </h2>

              <p className="mt-1 text-xs text-content-muted">
                Rendering {formatThemeName(resolvedTheme)}
              </p>
            </div>

            <Palette
              aria-hidden="true"
              className="text-accent"
              size={20}
              strokeWidth={1.7}
            />
          </div>

          <div className="mt-4 grid grid-cols-2 gap-3">
            {themeOptions.map((option) => {
              const isSelected = themePreference === option.value
              const OptionIcon = option.Icon

              return (
                <button
                  key={option.value}
                  aria-pressed={isSelected}
                  className={`group relative rounded-xl border p-2.5 text-left transition ${
                    isSelected
                      ? 'border-accent bg-accent-soft'
                      : 'border-line bg-surface-muted hover:border-line-strong hover:bg-surface-hover'
                  }`}
                  type="button"
                  onClick={() => chooseTheme(option.value)}
                >
                  {isSelected && (
                    <span className="absolute right-4 top-4 z-10 grid size-5 place-items-center rounded-full bg-accent text-white">
                      <Check aria-hidden="true" size={13} strokeWidth={2.5} />
                    </span>
                  )}

                  <span
                    aria-hidden="true"
                    className="block h-12 overflow-hidden rounded-lg border border-white/10"
                    style={{
                      background: option.previewBackground,
                    }}
                  >
                    <span className="flex h-full gap-1.5 p-2">
                      <span className="w-3 rounded-sm bg-black/20" />

                      <span className="flex flex-1 flex-col gap-1 rounded-sm border border-white/10 bg-black/15 p-1.5">
                        <span className="h-1 w-2/3 rounded-full bg-white/40" />
                        <span className="h-1 w-full rounded-full bg-white/20" />
                        <span className="mt-auto h-1.5 w-4/5 rounded-full bg-blue-400/70" />
                      </span>
                    </span>
                  </span>

                  <span className="mt-2.5 flex items-center gap-2">
                    <span
                      className={`grid size-7 shrink-0 place-items-center rounded-lg ${
                        isSelected
                          ? 'bg-accent text-white'
                          : 'bg-surface text-content-muted'
                      }`}
                    >
                      <OptionIcon
                        aria-hidden="true"
                        size={15}
                        strokeWidth={1.8}
                      />
                    </span>

                    <span className="min-w-0">
                      <span className="block text-sm font-semibold">
                        {option.label}
                      </span>

                      <span className="mt-0.5 block truncate text-[11px] text-content-muted">
                        {option.description}
                      </span>
                    </span>
                  </span>

                  <span className="sr-only">
                    {isSelected ? 'Selected theme' : 'Select theme'}
                  </span>
                </button>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
