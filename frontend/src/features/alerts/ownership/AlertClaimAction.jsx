import styles from './AlertClaimAction.module.css'

function AlertClaimAction({ claimedByYou, ticketNummer, onClaim }) {
  if (claimedByYou) {
    return (
      <span className={styles.claimed} aria-label={ticketNummer ? `Claimed by you, ticket ${ticketNummer}` : 'Claimed by you'}>
        <span className={styles.avatar} aria-hidden="true">Y</span>
        {ticketNummer ? <span className={styles.ticket}>{ticketNummer}</span> : 'Claimed by you'}
      </span>
    )
  }

  return (
    <button className={styles.button} onClick={onClaim} type="button">
      Claim
    </button>
  )
}

export default AlertClaimAction
