## Description
<!-- Provide a clear, concise summary of the changes introduced in this pull request. -->

## Linked Issue
<!-- Mandatory: Link the relevant issue(s) so GitHub automatically links and closes them on merge. -->
<!-- Examples: Closes #72, Resolves #30, Refs #15 -->
Closes #

## Rubric Area & Work Slice
- [ ] Person 1: Requester slice (FR-001..006) | Area A, D, E (Conformance, Testing, Traceability)
- [ ] Person 2: Staff slice (FR-007..013, NFR-017..019) | Area B, C (Construction, CI, Security)
- [ ] Person 3: Management slice (FR-014..016, NFR-020) | Area F, G (Operations, Staging, Release)
- [ ] Shared / Cross-Cutting / Governance

## Changes & Acceptance Criteria Covered
<!-- Bullet points explaining the functional and technical changes made. -->
- 

## Verification & Testing (What was checked)
<!-- Detail what was checked and verified by the author before opening this PR. -->
- [ ] Clean build: `dotnet build` passes with zero errors/warnings
- [ ] Automated tests: `dotnet test` runs and passes completely
- [ ] Meaningful tests added or updated (unit, integration, or negative cases)
- [ ] Manual test / exploratory check performed (details below):
  <!-- Describe manual check steps, endpoints called, UI verified, or database state confirmed -->

### Governance & Traceability
- [ ] Requirements Traceability Matrix (RTM) updated with verification evidence/status (if touching FR/NFR)
- [ ] Defect Register updated with root cause and regression test (if resolving a defect)
- [ ] Technical Debt / Risk Register updated (if applicable)

## AI Usage Note
<!-- Mandatory: Declare any AI assistance used to develop these changes. -->
- **AI Tool(s) Used:** None / Claude Code / GitHub Copilot / ChatGPT / Other (specify)
- **Scope of AI Assistance:**
  <!-- Describe what was generated or assisted by AI (e.g. boilerplate, test skeletons, refactoring, none) -->
- **Independent Verification:**
  <!-- Explain how you verified the output: manual code review, step-by-step debugging, written tests, etc. -->
- [ ] Entry logged in the team AI Usage Register with verification details

---

## Peer Review Checklist (For Reviewers)
<!-- Main requires two non-author approvals. Reviewers must confirm what was actually checked. -->
- [ ] Code reviewed for architectural compliance, error handling, and clean code principles
- [ ] Verified branch follows naming rules (`feature/`, `fix/`, `test/`, `ci/`, `docs/`, `chore/`)
- [ ] Verified linked issue exists and all acceptance criteria are met
- [ ] Verified tests pass locally or in CI quality gate
- **Reviewer 1 Verification Notes:**
  <!-- Record what you personally ran or checked (e.g. pulled branch, ran dotnet test, inspected migration) -->
- **Reviewer 2 Verification Notes:**
  <!-- Record what you personally ran or checked -->
