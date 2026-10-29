# SOC

The repository is organized as a monorepo:

- `frontend/` contains the Vite/React application, including its `package.json`,
  lockfile, Vite configuration, HTML entry point, and `src/`.
- `backend/` contains the backend application.

## Run the frontend

From the repository root in PowerShell, install the frontend dependencies and
start Vite:

```sh
npm.cmd --prefix frontend install
npm.cmd --prefix frontend run dev
```

Or change into `frontend/` and run `npm.cmd install` followed by
`npm.cmd run dev`.

Other frontend scripts can also be run from the repository root:

```sh
npm.cmd --prefix frontend run build
npm.cmd --prefix frontend run lint
npm.cmd --prefix frontend run preview
```
