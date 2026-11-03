import Icon from '../layouts/Icon.jsx'

function OverviewPage() {
  return (
    <section className="page-heading" id="overview-page">
      <div>
        <h1>Good morning, user </h1>
        <p>Here’s what’s happening across your environment today.</p>
      </div>

      <select className="date-range" aria-label="Date range">
        <option value="option1">Last 24 hours</option>
        <option value="option2">Last 48 hours</option>
      </select>
    </section>
  )
}

export default OverviewPage
