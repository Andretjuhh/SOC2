import { useState } from 'react'
import Icon from './Icon.jsx'
import { NavLink } from 'react-router-dom'

function Sidebar() {
  return (
    <aside className="sidebar">
       <span className="brand-name">application name</span>

      <div className="nav-label">SECURITY OPERATIONS</div>
      <nav className="main-nav" aria-label="Main navigation">
        <NavLink className="nav-link" to="/overview"> <Icon name="grid"/> <span>Overview</span> </NavLink>
        <NavLink className="nav-link" to="/alerts"><span className="nav-icon">  <Icon name="shield"/> </span>  <span>Alerts</span>  <span className="nav-count">??</span> </NavLink>
        <NavLink className="nav-link" to="/tickets"> <Icon name = "ticket"/> <span>Tickets</span> </NavLink>
        <NavLink className="nav-link" to="/cases"> <Icon name = "cases"/> <span>Cases</span> </NavLink>
        <NavLink className="nav-link" to="/activity"> <Icon name="activity" /> <span>Activity</span> </NavLink>
        <NavLink className="nav-link" to="/assets">  <Icon name="server" /> <span>Assets</span> </NavLink>
        <NavLink className="nav-link" to="/settings">  <Icon name="settings" /> <span>Settings</span> </NavLink>

      </nav>
    </aside>
  )
}

export default Sidebar