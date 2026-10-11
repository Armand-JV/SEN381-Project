# SEN381 CivicConnect — Milestone 3 Work Split (3 Members)

M3 = controlled construction, integration, quality and release readiness. Raw 50 (30 team evidence, 20 individual defence). PED v2.0 evolves into **PED v3.0**; the RTM and all registers are **continued, not recreated**.

> **Assumptions to confirm as a team:** I don't know your final M2 stack or which M3 features are frozen, so code ownership below is by *feature slice* (based on your M1 FR/NFR IDs) rather than by technology layer. Swap slices around if your architecture splits differently. Dates are left blank — fill in from the Moodle deadline.

---

## Role summary

| | Person 1 | Person 2 | Person 3 |
|---|---|---|---|
| **Primary role** | Quality & Verification Lead | Construction, CI & Security Lead | Release, Performance & Operations Lead |
| **M3 rubric areas owned** | A (M2 conformance), D (black-box/integration/system), E (RTM/defects/traceability) | B (collaborative construction), C (CI & quality gates) | F (performance, staging), G (release review, residual risk) |
| **Code slice** | Requester slice: FR-001 to FR-006 (submit, categorise, unique ID, status, history, feedback) | Staff slice: FR-007 to FR-013 + RBAC/auth (NFR-017/018/019) | Management slice: FR-014 to FR-016 + audit trail (NFR-020) + deployment config |

---

## Person 1 — Quality & Verification Lead

**Engineering**
- Implement the Requester slice (FR-001–006) on feature branches via PRs.
- Own **baseline conformance (Area A)**: build the M2 → implementation conformance table, flag significant deviations, write the 1–3 ADR/change records (original decision → problem found → revised decision → consequence).

**Quality & testing (Areas C/D content)**
- Write the risk-based test selection: which risks get deeper verification and why (feeds PED section 6).
- Unit/component tests for the Requester slice, plus ownership of the **black-box suite**: pick the techniques that genuinely fit (likely EP/BVA on submission fields, state transition on status changes, decision table for who-can-do-what), with expected results derived *before* execution.
- Maintain the **Test Evidence Records** (basis, why selected, technique, input, expected, actual, status, evidence, trace, interpretation) for every counted test.

**Traceability & registers (Area E)**
- Own the **RTM evolution**: AC → implementation → test case → expected result → execution evidence → defect/status. Update it as tests land, not at the end (repo history must show it evolving).
- Own the **Defect Register** (defect → severity → status → fix → regression test).

**PED v3.0 sections:** 2 (M2 conformance), 3 (controlled changes), 6 (quality strategy), 7 (black-box/integration/system), 9 (defects & quality interpretation).

---

## Person 2 — Construction, CI & Security Lead

**Engineering**
- Implement the Staff slice (FR-007–013) and the RBAC/authentication layer (NFR-017/018/019).
- Own **repo governance (Area B)**: protected main, branch naming, PR template, enforcing two-approval reviews, secrets kept out of the repo (env vars / secret store), keeping history progressive.

**CI & automated verification (Area C)**
- Build the **CI workflow**: build + run selected tests/checks on PR/push.
- Configure at least one **quality gate** that genuinely blocks on failure (build or test failure) and keep evidence of a blocked/failed run.
- Add **static analysis** and a **security/dependency/secrets check** (GitHub-supported options are fine); record findings and how each was handled.
- Own test automation wiring: unit/component tests for the Staff slice, **API/integration tests** (min 3, include failure/negative cases), and the **negative/unauthorised behaviour cases** (min 3 — e.g. requester attempting staff action, staff accessing out-of-scope request).
- Explain what coverage evidence does and doesn't show (no arbitrary % target).

**PED v3.0 sections:** 4 (construction/configuration), 5 (CI & automated verification). Also owns the Risk Register update for security/process risks.

---

## Person 3 — Release, Performance & Operations Lead

**Engineering**
- Implement the Management slice (FR-014–016) and audit trail (NFR-020).
- Own configuration for the **staging/test environment** (real or credible simulated): environment-specific config, controlled secrets, release identification, proof the *correct release candidate* is what's deployed.

**Staging, performance, release (Areas F/G)**
- **E2E/system tests** (min 2 critical journeys, e.g. submit → staff acts → requester sees update), ideally also run against the staging environment.
- **One performance/load test** on a justified operation (e.g. request submission or dashboard filtering against NFR-024's 3-second target): record environment, data, workload, duration, response time and error rate; state what it does NOT prove.
- Prepare the **rollback/recovery approach** (what version/artefact is restored, what data/config concerns).
- Own the **Release Evidence Summary** (build/CI, regression, API, E2E, static/security, performance, defects, acceptance) and the **READY / CONDITIONALLY READY / NOT READY** classification, backed by evidence.
- Own the **Residual Risk Statement** and the **Technical Debt Register**. Only record real residual risks.

**PED v3.0 sections:** 8 (performance/reliability), 10 (release readiness).

---

## Test allocation (minimum 15 meaningful cases, with overlap allowed)

| Category (min) | Lead | Notes |
|---|---|---|
| Unit/component (5) | All three | ~2 each from their own slice, so each person can defend their own tests |
| Black-box functional (5) | P1 | ≥2 techniques actually justified by the behaviour |
| API/integration (3) | P2 | Include at least one failure/negative case |
| E2E/system (2) | P3 | Critical journey run on staging where feasible |
| Negative/failure/unauthorised (3 across suite) | P2 (with P1) | |
| High-priority requirement/risk coverage (5 across suite) | P1 selects | Driven by the risk-based selection |
| Performance (1 separate) | P3 | Not counted in the 15 |

Remember the **Meaningful Test Rule**: a test only counts if it has a traceable basis, purpose, deliberate data, defined expected result, real assertion, execution evidence and interpretation. No count-padding.

---

## Shared responsibilities (everyone)

| Task | Notes |
|---|---|
| Freeze M3 core scope | Agree up front; no feature creep during M3 |
| PED v3.0 assembly | Continue PED v2.0, preserve history; each person writes their sections, then one joint proofread/format pass. PED is an **evidence index** — link to repo/CI, don't duplicate |
| Submission information block | Team members, repo URL, **release candidate tag/commit**, PED v3.0, RTM link, CI/test evidence link, staging URL or demo instructions, date |
| AI Usage Register | Each person logs their own AI use and how they verified it |
| Risk Register update | Each person updates risks in their area (P2 security/process, P3 release/ops, P1 quality/requirements) |
| Cross-review of PRs | With 3 people, the two non-authors review every substantive PR — reviews must show actual checking |
| Harvard references + in-text citations | Shared, as in previous milestones |
| Final PR review + tag release candidate | Tag only once all three agree the PED/RTM match the tagged commit |
| Individual defence prep | Everyone rehearses: trace one requirement end-to-end, explain one decision, interpret one result + limitation, explain one failure/risk, defend the readiness classification — **including parts you didn't build** |

---

## Suggested phase order (from the brief's work plan)

| Phase | Focus | Owner(s) |
|---|---|---|
| 1. Freeze scope + reconcile with M2 | Agree release-candidate scope; list significant deviations only | All; P1 drives conformance table |
| 2. Stabilise integration | Core journeys working end to end before piling on tests | All (own slices) |
| 3. Select quality risks + CI setup | Risk-based test selection; first working pipeline | P1 (risks), P2 (CI) |
| 4. Automate + write tests | Unit, black-box, API, negative cases; RTM updated as tests land | P1/P2, all unit tests |
| 5. Staging + E2E + performance | Deploy RC to staging, run journeys and load test | P3 |
| 6. Release review + registers | Release Evidence Summary, residual risk, debt, defects | P3 (summary), P1 (defects/RTM) |
| 7. Freeze, tag, PED v3.0 final | Tag RC, verify PED/RTM match the tag, submit | All |
| 8. Presentation + defence rehearsal | Dry run with live evidence | All |

Fill in target dates against the actual Moodle deadline: Phase 1–2 ____ | Phase 3–4 ____ | Phase 5–6 ____ | Phase 7–8 ____

---

## Suggested presentation split (follows the brief's flow, ~25 min total)

| Stage | Evidence | Presenter | Time |
|---|---|---|---|
| 1. Release candidate | Build/tag + implemented scope | P1 | ~2 min |
| 2. Baseline → implementation | Conformance table + significant ADR/change | P1 | ~3 min |
| 3. Controlled construction | PRs, reviews, protected main, secrets handling | P2 | ~3 min |
| 4. CI & automated assurance | A CI run + a gate that actually blocked something | P2 | ~4 min |
| 5. Black-box/integration/system | High-value tests + results + interpretation | P1 | ~5 min |
| 6. Staging + critical journey | Short live demo on the identified RC | P3 | ~4 min |
| 7. Performance/release evidence | Load result, release summary, residual risk | P3 | ~4 min |
| 8. Individual defence | Assessor questions each student separately | All | Separate |

Present from **live artefacts** (repo, CI runs, RTM, PED), not screenshots. The demo is evidence, not the whole assessment.

---

## Suggested branches
- `feature/requester-slice`, `feature/staff-rbac-slice`, `feature/management-audit-slice`
- `feature/ci-pipeline`, `feature/tests-blackbox`, `feature/tests-api-integration`, `feature/e2e-staging`, `feature/perf-test`
- `docs/ped-v3`, `docs/rtm-m3`, `docs/release-evidence`
- All merge via PRs with two non-author approvals; tag the release candidate from main.

---

## Risks to this split (read before agreeing)
- **Load imbalance:** Person 2 carries the most infrastructure work (CI + security + RBAC). If CI setup slips, everyone's tests lose their gate. Start CI in Phase 1–3, not last.
- **Late RTM:** the rubric explicitly penalises an RTM that looks assembled at the end. Person 1 should update it in the same PRs that add tests.
- **Person 3 is end-loaded:** staging, performance and release summary all depend on a stable RC, so Person 3 should do their slice code + staging config early and keep the load test script ready.
- **Don't let ownership become silos:** the individual defence samples *any* team artefact — each person needs to be able to explain another person's tests and CI run.
