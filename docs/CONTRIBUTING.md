# CivicConnect Contribution & Engineering Governance Guide

This document outlines the workflow, branch naming standards, commit conventions, and review requirements for CivicConnect (SEN381 Milestone 3).

---

## 1. Branch Naming Rules

All work must occur on dedicated branches created off the latest `main` branch. Direct commits to `main` are strictly prohibited.

Branch names must be **lowercase**, use **kebab-case**, and start with one of the following category prefixes:

| Category | Format | Description & Purpose | Examples |
|---|---|---|---|
| **Features** | `feature/<short-description>` | New feature slice or capability | `feature/staff-rbac-slice`<br>`feature/requester-slice`<br>`feature/management-slice`<br>`feature/audit-trail` |
| **Testing** | `feature/<tests-...>` or `test/<short-description>` | Test suites, test harnesses, or regression tests | `feature/tests-api-integration`<br>`test/black-box-state-transitions`<br>`test/negative-unauthorized-suite` |
| **CI / DevOps** | `feature/<ci-...>` or `ci/<short-description>` | Workflows, quality gates, and automated pipelines | `feature/ci-pipeline`<br>`ci/quality-gate-enforcement`<br>`ci/docker-staging-config` |
| **Bug Fixes** | `fix/<defect-id-or-description>` | Fixes for issues and recorded defects | `fix/def-001-di-registration`<br>`fix/def-004-hardcoded-identities` |
| **Documentation** | `docs/<short-description>` | PED updates, registers, architectural records | `docs/ped-v3-sections-4-5`<br>`docs/adr-m3-01-repository-pattern` |
| **Chores / Config**| `chore/<short-description>` | Tooling, configuration, or governance setup | `chore/m3-gov-02-templates`<br>`chore/dependency-updates` |

### Branch Rules:
- Keep branch names concise, descriptive, and focused on a single logical change.
- Never use personal names or unstructured names (e.g. `test`, `armand-work`, `temp-fix`).
- Always pull the latest `main` before branching:
  ```bash
  git checkout main
  git pull origin main
  git checkout -b feature/staff-rbac-slice
  ```

---

## 2. Commit Message Guidelines

Commit messages must be clear and explain *what* was changed and *why*:
- Use imperative mood in the subject line (e.g., `Add role-based authorization middleware`, not `Added...` or `Adds...`).
- Reference the GitHub issue number when applicable:
  ```
  feat(auth): implement role-based access control for staff slice (#30)
  fix(di): register RequestService in application container (DEF-001, #37)
  test(integration): add negative tests for unauthorized state transitions (#56)
  ```

---

## 3. Pull Request (PR) Workflow

1. **Create Branch & Push**:
   ```bash
   git push -u origin feature/staff-rbac-slice
   ```

2. **Open PR with Template**:
   - The `.github/pull_request_template.md` will load automatically.
   - **Linked Issue**: You must reference the open issue (e.g., `Closes #30`).
   - **Changes & AC**: Detail the exact acceptance criteria fulfilled.
   - **Verification (What Was Checked)**: Specify local test results (`dotnet test`), build output (`dotnet build`), and manual exploratory verification steps.
   - **Registers & Traceability**: Confirm that the RTM, Defect Register, or Risk Register has been updated to reflect the change.
   - **AI Usage Note**: Declare any AI assistance used, explain human verification, and record the entry in the AI Usage Register.

3. **Peer Review & Approvals**:
   - `main` requires **two non-author peer approvals** prior to merging.
   - Reviewers must execute or inspect the code and document what they personally verified in the PR comments/checklist (no rubber-stamp approvals).
   - All CI automated quality gates must pass before merge.
