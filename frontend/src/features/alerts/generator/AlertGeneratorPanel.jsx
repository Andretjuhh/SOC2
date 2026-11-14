function GeneratorButton({ label, onClick, disabled, className, children }) {
  return (
    <button
      type="button"
      aria-label={label}
      onClick={onClick}
      disabled={disabled}
      className={className}
    >
      {children}
    </button>
  )
}

export default function AlertGeneratorPanel({ generator, templates, loading, onReset }) {
  const {
    running,
    toggleRunning,
    intervalLabel,
    canSpeedUp,
    canSlowDown,
    changeSpeed,
  } = generator
  const controlsDisabled = loading || templates.length === 0

  return (
    <section className="alert-generator" aria-label="Alert generator">
      <h2>Alert generator</h2>
      <div className="alert-generator-controls">
        <GeneratorButton
          label="Make alerts arrive faster"
          onClick={() => changeSpeed('faster')}
          disabled={controlsDisabled || !canSpeedUp}
        >
          −
        </GeneratorButton>
        <span
          className="alert-generator-interval"
          aria-label={`New alert every ${intervalLabel}`}
          aria-live="polite"
        >
          {intervalLabel}
        </span>
        <GeneratorButton
          label="Make alerts arrive slower"
          onClick={() => changeSpeed('slower')}
          disabled={controlsDisabled || !canSlowDown}
        >
          +
        </GeneratorButton>
        <GeneratorButton
          label={running ? 'Pause alert generator' : 'Resume alert generator'}
          onClick={toggleRunning}
          disabled={controlsDisabled}
          className="alert-generator-toggle"
        >
          {running ? 'Pause' : 'Resume'}
        </GeneratorButton>
        <GeneratorButton
          label="Reset temporary alert list and pause generator"
          onClick={onReset}
          disabled={loading}
          className="alert-generator-reset"
        >
          Reset
        </GeneratorButton>
      </div>
    </section>
  )
}
