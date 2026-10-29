import '../styles/App.css'
import Sidebar from './Sidebar.jsx'
import Topbar from './Topbar.jsx'

function AppLayout (props) {

    const children = props.children
    const query = props.query
    const onQueryChange = props.queryChange


    return (
        <div className="app-shell">
            <Sidebar/>
            <main className="main-content" id="overview">
                <Topbar query={query} onQueryChange={onQueryChange} />
                <div className="dashboard"> {children} </div>
            </main>
        </div>  
    )

}

export default AppLayout