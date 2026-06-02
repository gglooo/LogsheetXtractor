# Documentation Site

This folder is a static site published to GitHub Pages.

## Structure

- `index.html` - docs landing page
- `api/index.html` - ReDoc API reference page
- `openapi/logsheetxtractor.swagger.json` - OpenAPI file consumed by ReDoc

## Local preview

From repository root:

```bash
python3 -m http.server 8088
```

Then open:

- `http://localhost:8088/docs/`
- `http://localhost:8088/docs/api/`

## Notes

- CI workflow builds and runs the backend API in `Development` mode, then fetches `/swagger/v1/swagger.json` into `docs/openapi/` before publishing.
- This keeps the published API docs synchronized with backend code on every `main` push.
