import Icon from './Icon.jsx'

function Topbar({ query, onQueryChange }) {
  return (
    <header className="topbar">
      <div className="breadcrumbs"><span>Security</span><Icon name="chevron" size={14} /><strong>Overview</strong></div>
      <div className="topbar-actions">
        <label className="search-box">
          <Icon name="search" size={17} />
          <input
            aria-label="Search alerts"
            placeholder="Search alerts..."
            value={query}
            onChange={(event) => onQueryChange(event.target.value)}
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
  )
}

export default Topbar