import styles from './AlertClaimAction.module.css'

function AlertClaimAction({ claimedByYou, claimedByOther, owner, claiming, onClaim }) {
  if (claimedByYou) {
    return (
      <span className={styles.claimed} aria-label="Claimed by you">
        <span className={styles.avatar} aria-hidden="true">Y</span>
        Claimed by you
      </span>
    )
  }

  if (claimedByOther) {
    return (
      <span className={styles.claimed} aria-label={`Claimed by ${owner}`}>
        Claimed by {owner}
      </span>
    )
  }

  return (
    <button className={styles.button} onClick={onClaim} type="button" disabled={claiming}>
      {claiming ? 'Claiming…' : 'Claim'}
    </button>
  )
}

export default AlertClaimAction
