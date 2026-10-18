# PED v3 §9 — Defects and quality interpretation

## Purpose and evidence boundary

This section indexes defect handling and quality findings for the release candidate. The [M3-CI-07 findings register](../CI/M3-CI-07-findings.md) is canonical for static-analysis, dependency, CI, and security findings. It records tool, severity, disposition, owner, and residual-risk or technical-debt linkage. The [Technical Debt Register](../Technical-Debt-Register.md) and [Residual Risk Statement](../Residual-Risk-Statement.md) explain accepted or open items.

Local test, format, and vulnerability results are not hosted-CI evidence and do not prove production configuration or complete requirement satisfaction. Hosted CI and hosted GitHub security-setting evidence remain pending unless linked from an actual run or repository-settings record.

## Defect and finding disposition

| Evidence | Interpretation | Status |
|---|---|---|
| `FIND-001` through `FIND-004`, `FIND-006`, and `FIND-007` in the findings register | Formatting, analyzer, test-harness, and test-isolation findings were fixed or mitigated for their stated repository scope. | Closed for recorded scope; verify again in CI. |
| `FIND-005` | Local dependency scan reported no vulnerable packages on 2026-10-14. Advisories remain time-varying. | Accepted with monitoring; see [RR-SEC-02](../Residual-Risk-Statement.md#rr-sec-02). |
| `FIND-008` / `DEF-005` | Historical credential literal remains in Git history. Current tracked configuration is empty, but historical exposure cannot be closed by local tests. | Mitigated and open; see [TD-SEC-01](../Technical-Debt-Register.md#td-sec-01) and [RR-SEC-01](../Residual-Risk-Statement.md#rr-sec-01). |
| `FIND-009` | Unrelated pre-existing `.gitignore` change is outside this CI implementation. | Accepted as out of scope; no quality claim attached. |
| `FIND-010` | Hosted GitHub security and branch-check settings require authorized repository administration; repository files cannot prove they are enabled. | Accepted/open pending hosted configuration; see [RR-SEC-03](../Residual-Risk-Statement.md#rr-sec-03). |

## Requirement traceability

Requirement status remains in [`docs/RTM-M3.md`](../RTM-M3.md), not in this section or in coverage percentages. The RTM records incomplete staff-scope, production-authentication, workflow, resolution, and detail evidence. Findings related to those gaps must not be relabeled as fixed solely because local tests pass.

`DEF-005` is retained as a defect/history finding and cross-linked from the findings register. No history rewrite or credential rotation is claimed here; those actions require repository-owner authorization.

## Review record

AI-assisted drafting is recorded in [`docs/P2-AI-Usage-Register.md`](../P2-AI-Usage-Register.md). Human verification remains pending until P2 reviews each mapping, source citation, disposition, and register link.
