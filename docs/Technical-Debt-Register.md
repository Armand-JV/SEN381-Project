# Technical Debt Register

**Scope:** M3 CI and security findings. Items are verified from repository evidence; ordinary test gaps remain in the RTM unless explicitly accepted as debt.

| ID | Item | Impact | Interest / priority | Plan | Owner | Status |
|---|---|---|---|---|---|---|
| TD-SEC-01 | Historical repository commits retain a database-password literal documented as DEF-005. | A previously valid credential could remain recoverable from clone history. | High | Rotate any credential that was ever valid; assess coordinated history rewrite under repository-owner policy. | P2 / repository owner | Open; mitigation recorded in [M3-CI-07 findings](CI/M3-CI-07-findings.md#findings-record). |
| TD-SEC-02 | GitHub Dependabot security alerts and auto-fix settings require hosted repository configuration; they are not enabled by repository files. | Dependency alerts may not be surfaced or auto-remediated between CI runs. | Medium | Repository owner enables hosted alerts/updates and records ruleset evidence; CI vulnerability scan remains active. | P2 / repository owner | Accepted pending hosted configuration; linked to [residual risk RR-SEC-02](Residual-Risk-Statement.md#rr-sec-02). |

This register does not claim that either item is fixed by local documentation or local test results.
