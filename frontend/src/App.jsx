import { useMemo, useState } from 'react'
import './styles/App.css'

const events = [
  { time: '10:42:18', type: 'Unusual sign-in location', source: 'Identity Protection', severity: 'Critical', target: 'j.smith@acme.io', status: 'Investigating' },
  { time: '10:38:51', type: 'Multiple failed login attempts', source: 'Microsoft Entra ID', severity: 'High', target: '10.24.8.119', status: 'Open' },
  { time: '10:31:06', type: 'Suspicious PowerShell activity', source: 'Endpoint Detection', severity: 'High', target: 'WS-0421', status: 'Investigating' },
  { time: '10:24:33', type: 'New inbox forwarding rule', source: 'Email Security', severity: 'Medium', target: 'm.chen@acme.io', status: 'Open' },
  { time: '10:12:09', type: 'Connection to a new domain', source: 'Network Monitor', severity: 'Low', target: '10.24.3.54', status: 'Resolved' },
]

const chartPoints = '0,118 40,108 80,112 120,78 160,88 200,62 240,74 280,44 320,56 360,30 400,48 440,37 480,55 520,23 560,39 600,15'

function Icon({ name, size = 18 }) {
  const paths = {
    grid: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /></>,
    shield: <><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z" /><path d="m9 12 2 2 4-4" /></>,
    activity: <><path d="M22 12h-4l-3 9L9 3l-3 9H2" /></>,
    search: <><circle cx="11" cy="11" r="7" /><path d="m20 20-4-4" /></>,
    bell: <><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" /><path d="M10 21h4" /></>,
    chevron: <path d="m9 18 6-6-6-6" />,
    arrow: <><path d="M7 17 17 7" /><path d="M7 7h10v10" /></>,
    clock: <><circle cx="12" cy="12" r="10" /><path d="M12 6v6l4 2" /></>,
    server: <><rect x="3" y="4" width="18" height="7" rx="2" /><rect x="3" y="13" width="18" height="7" rx="2" /><path d="M7 7h.01M7 16h.01M11 7h6M11 16h6" /></>,
    more: <><circle cx="5" cy="12" r="1" /><circle cx="12" cy="12" r="1" /><circle cx="19" cy="12" r="1" /></>,
  }

  return (
    <svg
      aria-hidden="true"
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.7"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {paths[name]}
    </svg>
  )
}

function App() {
  const [query, setQuery] = useState('')
  const [severityFilter, setSeverityFilter] = useState('All events')

  const filteredEvents = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()

    return events.filter((event) => {
      const matchesSeverity = severityFilter === 'All events' || event.severity === severityFilter
      const matchesQuery = !normalizedQuery
        || `${event.type} ${event.source} ${event.target} ${event.severity}`.toLowerCase().includes(normalizedQuery)

      return matchesSeverity && matchesQuery
    })
  }, [query, severityFilter])

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <a className="brand" href="#overview" aria-label="Sentinel SOC home">
          <span className="brand-mark"><Icon name="shield" size={21} /></span>
          <span className="brand-name">sentinel<span> / SOC</span></span>
        </a>

        <div className="workspace-label">WORKSPACE</div>
        <button className="workspace-switcher" type="button">
          <span className="workspace-avatar">A</span>
          <span className="workspace-copy"><strong>Acme Corporation</strong><small>Enterprise plan</small></span>
          <Icon name="chevron" size={15} />
        </button>

        <div className="nav-label">SECURITY OPERATIONS</div>
        <nav className="main-nav" aria-label="Main navigation">
          <a className="nav-link active" href="#overview"><Icon name="grid" /><span>Overview</span></a>
          <a className="nav-link" href="#alerts"><span className="nav-icon"><Icon name="shield" /></span><span>Alerts</span><span className="nav-count">12</span></a>
          <a className="nav-link" href="#activity"><Icon name="activity" /><span>Activity</span></a>
          <a className="nav-link" href="#assets"><Icon name="server" /><span>Assets</span></a>
        </nav>

        <div className="sidebar-bottom">
          <div className="system-status"><span className="status-dot" /><span>All systems operational</span></div>
          <button className="profile-button" type="button">
            <span className="profile-avatar">JD</span>
            <span className="profile-copy"><strong>Jordan Davis</strong><small>Security analyst</small></span>
            <Icon name="more" size={18} />
          </button>
        </div>
      </aside>

      <main className="main-content" id="overview">
        <header className="topbar">
          <div className="breadcrumbs"><span>Security</span><Icon name="chevron" size={14} /><strong>Overview</strong></div>
          <div className="topbar-actions">
            <label className="search-box">
              <Icon name="search" size={17} />
              <input
                aria-label="Search alerts"
                placeholder="Search alerts..."
                value={query}
                onChange={(event) => setQuery(event.target.value)}
              />
              <kbd>⌘ K</kbd>
            </label>
            <button className="icon-button notification-button" type="button" aria-label="Notifications">
              <Icon name="bell" />
              <span className="notification-dot" />
            </button>
            <span className="topbar-avatar" aria-label="Jordan Davis">JD</span>
          </div>
        </header>

        <div className="dashboard">
          <section className="page-heading">
            <div>
              <div className="eyebrow"><span className="live-dot" /> LIVE SECURITY OVERVIEW</div>
              <h1>Good morning, Jordan <span className="wave">✳</span></h1>
              <p>Here’s what’s happening across your environment today.</p>
            </div>
            <button className="date-range" type="button"><Icon name="clock" size={16} /> Last 24 hours <span className="date-chevron">⌄</span></button>
          </section>

          <section className="metric-grid" aria-label="Security metrics">
            <article className="metric-card">
              <div className="metric-top"><span>Active alerts</span><span className="metric-icon alert-icon"><Icon name="shield" size={17} /></span></div>
              <div className="metric-value">24 <span className="metric-trend negative"><Icon name="arrow" size={13} /> 12%</span></div>
              <div className="metric-foot"><span className="severity-dot critical-dot" /> 3 critical <span className="foot-divider">·</span> 8 high</div>
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
            <article className="panel threat-panel" id="activity">
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
              <div className="panel-heading"><div><h2>Alert breakdown</h2><p>By severity · last 24 hours</p></div><button className="panel-menu" type="button" aria-label="Alert breakdown options"><Icon name="more" /></button></div>
              <div className="severity-total"><strong>24</strong><span>Total alerts</span></div>
              <div className="severity-bar" aria-label="3 critical, 8 high, 9 medium, 4 low alerts">
                <span className="bar-critical" /><span className="bar-high" /><span className="bar-medium" /><span className="bar-low" />
              </div>
              <div className="severity-list">
                <div><span><i className="severity-dot critical-dot" />Critical</span><strong>3</strong><small>12.5%</small></div>
                <div><span><i className="severity-dot high-dot" />High</span><strong>8</strong><small>33.3%</small></div>
                <div><span><i className="severity-dot medium-dot" />Medium</span><strong>9</strong><small>37.5%</small></div>
                <div><span><i className="severity-dot low-dot" />Low</span><strong>4</strong><small>16.7%</small></div>
              </div>
            </article>
          </section>

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
        </div>
      </main>
    </div>
  )
}

export default App