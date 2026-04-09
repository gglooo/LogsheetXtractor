# formHTR Submodule -> PyPI Migration Plan

Date: 2026-04-08
Status: Planning only (no implementation in this step)

## Agreed Constraints

- Use an exact package pin for now.
- Keep Python dependencies in a `requirements.txt` file.
- Do not hardcode the package dependency directly in `Dockerfile`.

## Goal

Remove the `formHTR` git submodule and run backend scripting through the installed PyPI package (`formhtr`) while preserving current backend behavior.

## 1. Invocation Model Decision

Adopt module/CLI invocation from the installed package instead of file-path wrappers.

- Current: `python <scriptsFolder>/<script>.py ...`
- Target: `python -m formhtr <subcommand> ...`

Reason:
- Removes filesystem coupling to `formHTR/`.
- Aligns runtime with official package interface.
- Makes submodule removal clean.

## 2. Script Mapping

Map current internal script constants to package CLI subcommands:

- `select_ROIs.py` -> `select-rois`
- `process_logsheet.py` -> `process-logsheet`
- `automatic_align.py` -> `automatic-align`
- `pdf_dimensions.py` -> `pdf-dimensions`
- `export_logsheet.py` -> `export-logsheet`

## 3. Backend Refactor Scope

Update scripting execution internals without changing application orchestration:

- Change `PythonScriptExecutor` to execute `-m formhtr` + subcommand + args.
- Update script type constants to subcommand values.
- Keep `PythonHtrAdapter` flow and argument building behavior intact.

## 4. Configuration Cleanup

Remove `Python:ScriptsFolder` usage and configuration keys:

- `appsettings.Development.json`
- `appsettings.Docker.json`
- env var `Python__ScriptsFolder` in container config

Keep `Python:InterpreterPath`.

## 5. Dependency Installation Strategy

Create and use a dedicated requirements file for API container Python deps (exact pin):

- New file (proposed): `LogsheetXtractor.Solution/LogsheetXtractor.API/requirements.txt`
- Include: `formhtr==<exact_version>`

Docker changes:

- Remove `COPY formHTR ...` and folder-specific install flow.
- Copy `requirements.txt` and run `pip install -r requirements.txt`.
- Keep OS-level native packages required by `formhtr` runtime (for example `libzbar0`, `qpdf`, etc.).

## 6. Docker Compose Update

Remove obsolete mount:

- `./formHTR:/app/formHTR`

Retain data and credentials mounts.

## 7. Git/Submodule Removal (Repo Root)

At the parent repo root:

- Remove `logsheetXtractor/formHTR` entry from `.gitmodules`.
- Remove submodule entry from git index.
- Remove any leftover `.git/modules/...` metadata for this submodule if present.

Note: repo root is one level above `logsheetXtractor/`.

## 8. CI and Docs Updates

- Remove `submodules: recursive` from CI checkout steps.
- Update root `README.md`:
  - remove clone-with-submodules instructions,
  - remove local `formHTR/` dependency from setup docs,
  - describe package-based backend dependency install path.
- Update `AGENTS.md` guidance from local script-folder coupling to package contract coupling.

## 9. Test and Validation Gates

Test updates:

- Adapt scripting tests to new executor contract (`-m formhtr` + subcommand).

Validation sequence:

1. Run focused scripting integration tests.
2. Run full backend test suite.
3. Build backend Docker image.
4. Run docker-compose smoke flow and verify at least:
   - ROI selection,
   - logsheet processing,
   - export flow.

## 10. Rollback Plan

If migration breaks runtime:

- Revert executor invocation/model changes.
- Re-enable previous Docker/compose wiring.
- Temporarily keep submodule while addressing compatibility gaps.

## Open Item To Confirm Before Implementation

- Exact `formhtr` version string to pin in `requirements.txt`.
