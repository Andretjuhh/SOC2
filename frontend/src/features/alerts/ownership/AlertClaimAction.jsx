import styles from './AlertClaimAction.module.css'

function AlertClaimAction({ claimedByYou, onClaim }) {
  if (claimedByYou) {
    return (
      <span className={styles.claimed} aria-label="Claimed by you">
        <span className={styles.avatar} aria-hidden="true">Y</span>
        Claimed by you
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
