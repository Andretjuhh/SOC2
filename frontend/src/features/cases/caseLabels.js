export const CASE_STATUSSEN = [
  { value: 'Open', label: 'Open' },
  { value: 'InOnderzoek', label: 'In onderzoek' },
  { value: 'Afgesloten', label: 'Afgesloten' },
]

export function caseStatusLabel(status) {
  return CASE_STATUSSEN.find((optie) => optie.value === status)?.label ?? status
}
