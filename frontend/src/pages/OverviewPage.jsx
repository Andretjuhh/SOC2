import Icon from '../layouts/Icon.jsx'

const chartPoints = '0,118 40,108 80,112 120,78 160,88 200,62 240,74 280,44 320,56 360,30 400,48 440,37 480,55 520,23 560,39 600,15'

function OverviewPage() {

  return (
    <>
    <header className="page-heading">
      <div>
    <h1 id="overview-title">Good morning, user</h1>
    <p>Here’s what’s happening across your environment today.</p>
     </div>

    </header>

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
      </>
  )
}

export default OverviewPage
