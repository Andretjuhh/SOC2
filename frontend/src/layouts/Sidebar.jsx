import Icon from './Icon.jsx'

function Sidebar() {
  return (
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
  )
}

export default Sidebar