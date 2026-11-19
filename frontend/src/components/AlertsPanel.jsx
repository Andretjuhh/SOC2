import { useMemo, useState } from 'react'
import Icon from './Icon'

const events = [
  { time: '10:42:18', type: 'Unusual sign-in location', source: 'Identity Protection', severity: 'Critical', target: 'j.smith@acme.io', status: 'Investigating' },
  { time: '10:38:51', type: 'Multiple failed login attempts', source: 'Microsoft Entra ID', severity: 'High', target: '10.24.8.119', status: 'Open' },
  { time: '10:31:06', type: 'Suspicious PowerShell activity', source: 'Endpoint Detection', severity: 'High', target: 'WS-0421', status: 'Investigating' },
  { time: '10:24:33', type: 'New inbox forwarding rule', source: 'Email Security', severity: 'Medium', target: 'm.chen@acme.io', status: 'Open' },
  { time: '10:12:09', type: 'Connection to a new domain', source: 'Network Monitor', severity: 'Low', target: '10.24.3.54', status: 'Resolved' },
]

function AlertsPanel({ isAlertsView, query, onResetFilters }) {
  const [severityFilter, setSeverityFilter] = useState('All events')
  const [statusFilter, setStatusFilter] = useState('All statuses')

  const filteredEvents = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()

    return events.filter((event) => {
      const matchesSeverity = severityFilter === 'All events' || event.severity === severityFilter
      const matchesStatus = statusFilter === 'All statuses' || event.status === statusFilter
      const matchesQuery = !normalizedQuery
        || `${event.type} ${event.source} ${event.target} ${event.severity} ${event.status}`.toLowerCase().includes(normalizedQuery)

      return matchesSeverity && matchesStatus && matchesQuery
    })
  }, [query, severityFilter, statusFilter])

  const resetFilters = () => {
    setSeverityFilter('All events')
    setStatusFilter('All statuses')
    onResetFilters()
  }

  return (
    <section className="panel alerts-panel" id="alerts">
      <div className="alerts-heading">
        <div><div className="alerts-title"><h2>{isAlertsView ? 'All alerts' : 'Recent alerts'}</h2><span className="alert-count">24</span></div><p>Review and triage the latest security events.</p></div>
        {!isAlertsView && <a className="view-all" href="#alerts">View all alerts <Icon name="chevron" size={15} /></a>}
      </div>
      <div className="alert-filters" aria-label="Filter alerts">
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
        <span className="filter-divider" aria-hidden="true" />
        {['All statuses', 'Open', 'Investigating', 'Resolved'].map((filter) => (
          <button
            className={`filter-chip${statusFilter === filter ? ' selected' : ''}`}
            key={filter}
            onClick={() => setStatusFilter(filter)}
            type="button"
            aria-pressed={statusFilter === filter}
          >
            {filter}
          </button>
        ))}
        <button
          className="reset-filters"
          disabled={severityFilter === 'All events' && statusFilter === 'All statuses' && !query.trim()}
          onClick={resetFilters}
          type="button"
        >
          Reset filters
        </button>
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
  )
}

export default AlertsPanel
