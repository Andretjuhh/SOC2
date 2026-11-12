import Icon from '../layouts/Icon.jsx'

const chartPoints = '0,118 40,108 80,112 120,78 160,88 200,62 240,74 280,44 320,56 360,30 400,48 440,37 480,55 520,23 560,39 600,15'

function AlertsPage(props) {
  const filteredEvents = props.filteredEvents
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
        <div className="alerts-heading">
          <div><div className="alerts-title"><h2>Recent alerts</h2><span className="alert-count">24</span></div><p>Review and triage the latest security events.</p></div>
          <a className="view-all" href="#alerts">View all alerts <Icon name="chevron" size={15} /></a>
        </div>
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
            <thead><tr><th>EVENT</th><th>SOURCE</th><th>SEVERITY</th><th>AFFECTED ASSET</th><th>STATUS</th><th>TIME</th></tr></thead>
            <tbody>
              {filteredEvents.map((event) => (
                <tr key={`${event.time}-${event.type}`}>
                  <td><span className={`event-marker ${event.severity.toLowerCase()}`} /><strong>{event.type}</strong></td>
                  <td className="muted-cell">{event.source}</td>
                  <td><span className={`severity-badge ${event.severity.toLowerCase()}`}><i className={`severity-dot ${event.severity.toLowerCase()}-dot`} />{event.severity}</span></td>
                  <td className="asset-cell">{event.target}</td>
                  <td><span className={`status-badge ${event.status.toLowerCase()}`}><i />{event.status}</span></td>
                  <td className="time-cell">{event.time}</td>
                </tr>
              ))}
              {filteredEvents.length === 0 && (
                <tr><td className="empty-state" colSpan="6">No alerts match your search and filters.</td></tr>
              )}
            </tbody>
          </table>
        </div>
        <div className="table-footer"><span>Showing {filteredEvents.length} of 24 alerts</span><button type="button" className="pagination-button" disabled>‹</button><button type="button" className="pagination-button" disabled>›</button></div>
      </section>

      <footer className="dashboard-footer"><span>Sentinel SOC <span className="footer-divider">·</span> Demo dashboard with sample data</span><span>Last updated just now <i className="status-dot" /></span></footer>
    </section>
  )
}

export default AlertsPage