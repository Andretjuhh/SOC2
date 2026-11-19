import { useEffect, useMemo, useState } from 'react'
import Icon from '../layouts/Icon.jsx'
import AlertClaimAction from '../features/alerts/ownership/AlertClaimAction'
import CreateTicketDialog from '../features/tickets/CreateTicketDialog'

const chartPoints = '0,118 40,108 80,112 120,78 160,88 200,62 240,74 280,44 320,56 360,30 400,48 440,37 480,55 520,23 560,39 600,15'
const priorities = ['Critical', 'High', 'Medium', 'Low']
const statuses = ['Open', 'Investigating', 'Resolved']

function enumLabel(value, labels) {
  if (typeof value === 'number') {
    return labels[value] ?? 'Unknown'
  }

  return typeof value === 'string' ? value : 'Unknown'
}

function getDateBounds(days) {
  const today = new Date()
  const to = today.toISOString().slice(0, 10)
  const from = new Date(Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate() - days + 1))

  return { from: from.toISOString().slice(0, 10), to }
}

function AlertsPage({ query, onQueryChange }) {
  const [alerts, setAlerts] = useState([])
  const [ticketsByAlert, setTicketsByAlert] = useState({})
  const [alertVoorTicket, setAlertVoorTicket] = useState(null)
  const [totalCount, setTotalCount] = useState(0)
  const [priorityFilter, setPriorityFilter] = useState('All events')
  const [statusFilter, setStatusFilter] = useState('All statuses')
  const [dateRange, setDateRange] = useState('')
  const [sortOrder, setSortOrder] = useState('newest')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  function handleTicketAangemaakt(ticket) {
    setTicketsByAlert((current) => ({
      ...current,
      [alertVoorTicket.id]: ticket.ticketNummer ?? null,
    }))
    setAlertVoorTicket(null)
  }

  useEffect(() => {
    const controller = new AbortController()

    async function loadAlerts() {
      const params = new URLSearchParams({ page: '1', pageSize: '100' })
      if (priorityFilter !== 'All events') params.set('priority', priorityFilter)
      if (statusFilter !== 'All statuses') params.set('status', statusFilter)
      if (dateRange) {
        const bounds = getDateBounds(Number(dateRange))
        params.set('from', bounds.from)
        params.set('to', bounds.to)
      }

      setIsLoading(true)
      setError('')
      setAlerts([])
      setTotalCount(0)

      try {
        const response = await fetch(`/api/filtered-alerts?${params}`, { signal: controller.signal })
        let payload

        try {
          payload = await response.json()
        } catch {
          throw new Error(`De API stuurde geen geldig antwoord (HTTP ${response.status}).`)
        }

        if (!response.ok) {
          const validationMessages = Object.values(payload.errors ?? {}).flat().join(' ')
          throw new Error(payload.detail || validationMessages || `Ophalen van alerts is mislukt (HTTP ${response.status}).`)
        }

        if (!Array.isArray(payload.items) || typeof payload.totalCount !== 'number') {
          throw new Error('De API stuurde een onverwacht antwoord voor alerts.')
        }

        setAlerts(payload.items)
        setTotalCount(payload.totalCount)
      } catch (requestError) {
        if (requestError.name === 'AbortError') return

        setAlerts([])
        setTotalCount(0)
        const message = requestError instanceof TypeError
          ? 'De API is niet bereikbaar. Controleer of de backend draait en probeer het opnieuw.'
          : requestError instanceof Error
            ? requestError.message
            : 'Er is een onbekende fout opgetreden bij het ophalen van alerts.'
        setError(message)
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }

    loadAlerts()
    return () => controller.abort()
  }, [priorityFilter, statusFilter, dateRange])

  const filteredAlerts = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()

    return alerts
      .filter((alert) => {
        if (!normalizedQuery) return true

        const priority = enumLabel(alert.priority, priorities)
        const status = enumLabel(alert.status, statuses)
        const searchableMetadata = [
          alert.id,
          alert.title,
          alert.source,
          alert.customerId,
          priority,
          status,
          alert.createdAt,
        ].join(' ').toLowerCase()

        return searchableMetadata.includes(normalizedQuery)
      })
      .sort((left, right) => {
        const difference = new Date(left.createdAt) - new Date(right.createdAt)
        return sortOrder === 'newest' ? -difference : difference
      })
  }, [alerts, query, sortOrder])

  const severityCounts = useMemo(() => {
    return priorities.reduce((counts, priority) => {
      counts[priority] = alerts.filter((alert) => enumLabel(alert.priority, priorities) === priority).length
      return counts
    }, {})
  }, [alerts])

  const resetFilters = () => {
    setPriorityFilter('All events')
    setStatusFilter('All statuses')
    setDateRange('')
    setSortOrder('newest')
    onQueryChange('')
  }

  return (
    <section id="activity">
      <select className="date-range" aria-label="Filter by date" value={dateRange} onChange={(event) => setDateRange(event.target.value)}>
        <option value="">All dates</option>
        <option value="1">Today (UTC)</option>
        <option value="2">Last 2 days (UTC)</option>
        <option value="7">Last 7 days (UTC)</option>
        <option value="30">Last 30 days (UTC)</option>
      </select>

      <section className="metric-grid" aria-label="Security metrics">
        <article className="metric-card">
          <div className="metric-top"><span>Active alerts</span><span className="metric-icon alert-icon"><Icon name="shield" size={17} /></span></div>
          <div className="metric-value">{totalCount}</div>
          <div className="metric-foot">Alerts matching API filters</div>
        </article>
        <article className="metric-card">
          <div className="metric-top"><span>Mean time to respond</span><span className="metric-icon time-icon"><Icon name="clock" size={17} /></span></div>
          <div className="metric-value">18<span className="metric-unit">m</span> 42<span className="metric-unit">s</span><span className="metric-trend positive"><Icon name="arrow" size={13} /> 8%</span></div>
          <div className="metric-foot">vs. 20m 19s previous period</div>
        </article>
        <article className="metric-card">
          <div className="metric-top"><span>Events analyzed</span><span className="metric-icon events-icon"><Icon name="activity" size={17} /></span></div>
          <div className="metric-value">1.28<span className="metric-unit">M</span><span className="metric-trend positive"><Icon name="arrow" size={13} /> 6.4%</span></div>
          <div className="metric-foot">Across 16 connected data sources</div>
        </article>
        <article className="metric-card">
          <div className="metric-top"><span>Assets protected</span><span className="metric-icon assets-icon"><Icon name="server" size={17} /></span></div>
          <div className="metric-value">2,847 <span className="metric-trend neutral">98.6%</span></div>
          <div className="metric-foot">Coverage across your organization</div>
        </article>
      </section>

      <section className="insights-grid">
        <article className="panel threat-panel">
          <div className="panel-heading">
            <div><h2>Threat activity</h2><p>Events detected over the last 24 hours</p></div>
            <button className="panel-menu" type="button" aria-label="Threat activity options"><Icon name="more" /></button>
          </div>
          <div className="chart-legend"><span><i className="legend-dot detected" /> Detected events</span><span><i className="legend-dot investigated" /> Investigated</span></div>
          <div className="chart-wrap" role="img" aria-label="Threat activity chart, showing detected events and investigations over the last 24 hours">
            <div className="chart-y-labels"><span>400</span><span>300</span><span>200</span><span>100</span><span>0</span></div>
            <svg className="activity-chart" viewBox="0 0 600 150" preserveAspectRatio="none" aria-hidden="true">
              <defs>
                <linearGradient id="chartFill" x1="0" x2="0" y1="0" y2="1">
                  <stop offset="0%" stopColor="#7468f0" stopOpacity=".24" />
                  <stop offset="100%" stopColor="#7468f0" stopOpacity="0" />
                </linearGradient>
              </defs>
              <path className="chart-gridline" d="M0 20H600M0 50H600M0 80H600M0 110H600M0 140H600" />
              <polygon points={`0,140 ${chartPoints} 600,140`} fill="url(#chartFill)" />
              <polyline points={chartPoints} fill="none" stroke="#8478ff" strokeWidth="2.5" vectorEffect="non-scaling-stroke" />
              <polyline points="0,132 40,128 80,133 120,118 160,121 200,108 240,115 280,99 320,110 360,88 400,103 440,92 480,105 520,78 560,91 600,70" fill="none" stroke="#39c6a5" strokeWidth="2" vectorEffect="non-scaling-stroke" />
            </svg>
          </div>
          <div className="chart-x-labels"><span>12 AM</span><span>4 AM</span><span>8 AM</span><span>12 PM</span><span>4 PM</span><span>8 PM</span><span>Now</span></div>
        </article>

        <article className="panel breakdown-panel">
          <div className="panel-heading"><div><h2>Alert breakdown</h2><p>By priority · loaded API results</p></div><button className="panel-menu" type="button" aria-label="Alert breakdown options"><Icon name="more" /></button></div>
          <div className="severity-total"><strong>{alerts.length}</strong><span>Loaded alerts</span></div>
          <div className="severity-bar" aria-label="Alert breakdown by priority">
            {priorities.map((priority) => (
              <span
                className={`bar-${priority.toLowerCase()}`}
                key={priority}
                style={{ width: `${alerts.length ? severityCounts[priority] / alerts.length * 100 : 0}%` }}
              />
            ))}
          </div>
          <div className="severity-list">
            {priorities.map((priority) => (
              <div key={priority}>
                <span><i className={`severity-dot ${priority.toLowerCase()}-dot`} />{priority}</span>
                <strong>{severityCounts[priority]}</strong>
                <small>{alerts.length ? `${(severityCounts[priority] / alerts.length * 100).toFixed(1)}%` : '0%'}</small>
              </div>
            ))}
          </div>
        </article>
      </section>

      <section className="panel alerts-panel" id="alerts">
        <div className="alerts-heading">
          <div><div className="alerts-title"><h2>Recent alerts</h2><span className="alert-count">{filteredAlerts.length}</span></div><p>Review and triage the latest security events.</p></div>
          <a className="view-all" href="#alerts">View all alerts <Icon name="chevron" size={15} /></a>
        </div>
        <div className="alert-filters" aria-label="Filter and sort alerts">
          {['All events', ...priorities].map((filter) => (
            <button
              className={`filter-chip${priorityFilter === filter ? ' selected' : ''}`}
              key={filter}
              onClick={() => setPriorityFilter(filter)}
              type="button"
              aria-pressed={priorityFilter === filter}
            >
              {filter === 'All events' ? filter : <><i className={`severity-dot ${filter.toLowerCase()}-dot`} />{filter}</>}
            </button>
          ))}
          <span className="filter-divider" aria-hidden="true" />
          {['All statuses', ...statuses].map((filter) => (
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
          <label className="filter-control">
            <span>Sort</span>
            <select className="filter-select" value={sortOrder} onChange={(event) => setSortOrder(event.target.value)}>
              <option value="newest">Newest first</option>
              <option value="oldest">Oldest first</option>
            </select>
          </label>
          <button
            className="reset-filters"
            disabled={priorityFilter === 'All events' && statusFilter === 'All statuses' && !dateRange && sortOrder === 'newest' && !query.trim()}
            onClick={resetFilters}
            type="button"
          >
            Reset filters
          </button>
        </div>
        <div className="table-scroll">
          <table className="alerts-table">
            <thead><tr><th>EVENT</th><th>SOURCE</th><th>PRIORITY</th><th>CUSTOMER</th><th>STATUS</th><th>TIME</th><th>TICKET</th></tr></thead>
            <tbody>
              {isLoading ? (
                <tr><td className="empty-state" colSpan="7" role="status">Loading alerts…</td></tr>
              ) : error ? (
                <tr><td className="empty-state error-state" colSpan="7" role="alert">{error}</td></tr>
              ) : filteredAlerts.map((alert) => {
                const priority = enumLabel(alert.priority, priorities)
                const status = enumLabel(alert.status, statuses)

                return (
                  <tr key={alert.id}>
                    <td><span className={`event-marker ${priority.toLowerCase()}`} /><strong>{alert.title}</strong></td>
                    <td className="muted-cell">{alert.source}</td>
                    <td><span className={`severity-badge ${priority.toLowerCase()}`}><i className={`severity-dot ${priority.toLowerCase()}-dot`} />{priority}</span></td>
                    <td className="asset-cell">{alert.customerId}</td>
                    <td><span className={`status-badge ${status.toLowerCase()}`}><i />{status}</span></td>
                    <td className="time-cell">{new Date(alert.createdAt).toLocaleString()}</td>
                    <td className="claim-cell">
                      <AlertClaimAction
                        ticketNummer={ticketsByAlert[alert.id]}
                        actionLabel="Create ticket"
                        onClaim={() => setAlertVoorTicket({
                          ...alert,
                          type: alert.title,
                          target: alert.customerId,
                        })}
                      />
                    </td>
                  </tr>
                )
              })}
              {!isLoading && !error && filteredAlerts.length === 0 && (
                <tr><td className="empty-state" colSpan="7">No alerts match your search and filters.</td></tr>
              )}
            </tbody>
          </table>
        </div>
        <div className="table-footer"><span>{isLoading ? 'Loading alerts…' : error ? 'Alerts could not be loaded' : `Showing ${filteredAlerts.length} of ${totalCount} alerts`}</span></div>
      </section>

      {alertVoorTicket && (
        <CreateTicketDialog
          key={alertVoorTicket.id}
          alert={alertVoorTicket}
          onClose={() => setAlertVoorTicket(null)}
          onCreated={handleTicketAangemaakt}
        />
      )}

      <footer className="dashboard-footer"><span>Sentinel SOC <span className="footer-divider">·</span> Demo dashboard with sample data</span><span>Last updated just now <i className="status-dot" /></span></footer>
    </section>
  )
}

export default AlertsPage