import useAlertGenerator from './useAlertGenerator'

export default function AlertGeneratorPanel({ templates, onAlert, loading }) {
  const { running, setRunning, intervalMs, changeSpeed } = useAlertGenerator(templates, onAlert)

  return (
    <section className="alert-generator" aria-label="Test generator">
      <h2>Test generator</h2>
      <div className="alert-generator-controls">
        <button
          type="button"
          aria-label="Decrease alert generation speed"
          onClick={() => changeSpeed(-1)}
          disabled={loading || templates.length === 0 || intervalMs >= 10000}
        >
          −
        </button>
        <span
          className="alert-generator-interval"
          aria-label={`New alert every ${intervalMs / 1000} seconds`}
          aria-live="polite"
        >
          {intervalMs / 1000}s
        </span>
        <button
          type="button"
          aria-label="Increase alert generation speed"
          onClick={() => changeSpeed(1)}
          disabled={loading || templates.length === 0 || intervalMs <= 1000}
        >
          +
        </button>
        <button
          type="button"
          onClick={() => setRunning((current) => !current)}
          disabled={loading || templates.length === 0}
        >
          {running ? 'Pause' : 'Resume'}
        </button>
      </div>
    </section>
  )
}
