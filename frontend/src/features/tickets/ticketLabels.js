import { CATEGORIEEN } from './ticketOptions'

// Koppelt de ticketprioriteit aan de bestaande severity-stijlen uit App.css
export const PRIORITEIT_STIJL = {
  Kritiek: 'critical',
  Hoog: 'high',
  Midden: 'medium',
  Laag: 'low',
}

export const STATUSSEN = [
  { value: 'Open', label: 'Open' },
  { value: 'InBehandeling', label: 'In behandeling' },
  { value: 'Afgerond', label: 'Afgerond' },
]

export function statusLabel(status) {
  return STATUSSEN.find((optie) => optie.value === status)?.label ?? status
}

export function categorieLabel(categorie) {
  return CATEGORIEEN.find((optie) => optie.value === categorie)?.label ?? categorie
}

export function formatDatumTijd(waarde) {
  if (!waarde) return '—'
  return new Date(waarde).toLocaleString('nl-NL', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  })
}

export function initialen(naam) {
  return naam.split(/[\s.-]+/).filter(Boolean).slice(0, 2).map((deel) => deel[0].toUpperCase()).join('') || '?'
}
