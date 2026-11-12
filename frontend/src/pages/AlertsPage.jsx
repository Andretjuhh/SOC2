function AlertsPage(props) {
  const filteredEvents = props.filteredEvents
  const totalEvents = props.totalEvents
  const loading = props.loading
  const error = props.error
  const severityFilter = props.severityFilter
  const setSeverityFilter = props.setSeverityFilter

  return (
    <section id="activity">
    <select className="date-range" aria-label="Date range">
        <option value="option1">Last 24 hours</option>
        <option value="option2">Last 48 hours</option>
        <option value="option2">Last 7 days</option>
        <option value="option2">Last 30 days</option>
      </select>

      <section className="panel alerts-panel" id="alerts">
        <div className="alert-filters" aria-label="Filter alerts by severity">
          {['All events', 'Critical', 'High', 'Medium', 'Low'].map((filter) => (
            <button
              className={`filter-chip${severityFilter === filter ? ' selected' : ''}`}
              key={filter}
              onClick={() => setSeverityFilter(filter)}
              type="button"
              aria-pressed={severityFilter === filter}
            >
              {filter === 'All events' ? filter : <><i className={`severity-dot ${filter.toLowerCase()}-dot`} />{filter}</>}
            </button>
          ))}
        </div>
        <div className="table-scroll">
          <table className="alerts-table">
            <thead><tr><th>EVENT</th><th>SEVERITY</th><th>AFFECTED ASSET</th><th>DETECTED</th><th>CLOSED</th></tr></thead>
            <tbody>
              {filteredEvents.map((event) => (
                <tr key={event.id}>
                  <td><span className={`event-marker ${event.severity.toLowerCase()}`} /><strong>{event.type}</strong></td>
                  <td><span className={`severity-badge ${event.severity.toLowerCase()}`}><i className={`severity-dot ${event.severity.toLowerCase()}-dot`} />{event.severity}</span></td>
                  <td className="asset-cell">{event.target}</td>
                  <td className="time-cell">{event.time}</td>
                  <td className="time-cell">{event.closedTime ?? '—'}</td>
                </tr>
              ))}
              {loading && (
                <tr><td className="empty-state" colSpan="5">Loading alerts…</td></tr>
              )}
              {!loading && error && (
                <tr><td className="empty-state" colSpan="5">{error}</td></tr>
              )}
              {!loading && !error && filteredEvents.length === 0 && (
                <tr><td className="empty-state" colSpan="5">No alerts match your search and filters.</td></tr>
              )}
            </tbody>
          </table>
        </div>
        <div className="table-footer"><span>Showing {filteredEvents.length} of {totalEvents} alerts</span><button type="button" className="pagination-button" disabled>‹</button><button type="button" className="pagination-button" disabled>›</button></div>
      </section>

      <footer className="dashboard-footer"><span>Sentinel SOC <span className="footer-divider">·</span> Demo dashboard with sample data</span><span>Last updated just now <i className="status-dot" /></span></footer>
    </section>
  )
}

export default AlertsPage