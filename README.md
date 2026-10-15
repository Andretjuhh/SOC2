# SOC Groep 2

## Checklist: installeren en starten

- [ ] Installeer Node.js **20.19 of hoger** (of **22.12 of hoger**). Vite 8 werkt niet met Node.js 20.11.1.
- [ ] Open een terminal in de projectmap `soc`. Vanuit de repositorymap op Windows:

  ```powershell
  cd .\soc
  ```

- [ ] Controleer of Node.js en npm beschikbaar zijn:

  ```powershell
  node --version
  npm --version
  ```

  Als je Node.js net hebt geïnstalleerd of bijgewerkt, open dan eerst een nieuwe terminal.

- [ ] Installeer de projectafhankelijkheden (de eerste keer, of nadat afhankelijkheden zijn gewijzigd):

  ```powershell
  npm install
  ```

- [ ] Start de ontwikkelserver:

  ```powershell
  npm run dev
  ```

- [ ] Open de URL die Vite in de terminal toont. Meestal is dat <http://localhost:5173/>.
- [ ] Laat de terminal met de ontwikkelserver open tijdens het werken. Stop de server met `Ctrl+C`.

> Voer de npm-commando's uit in `soc`, waar `package.json` staat, niet in de bovenliggende repositorymap.

## Optioneel: controleren

Voer deze commando's ook uit vanuit `soc`:

```powershell
npm run build
npm run lint
```
