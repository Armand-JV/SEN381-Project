# CivicConnect M3 - Defect Register (Person 1)

**Owner:** Person 1 - Quality & Verification Lead
**Scope:** Defects assigned to or relevant to Person 1.
**Status rule:** A defect is only marked fixed or verified when supported by implementation and regression-test evidence.

| Defect ID | Description | Severity | Status | Fix / disposition | Regression evidence |
|---|---|---|---|---|---|
| DEF-002 | Initial request history may be linked before the request has a database-generated key. | Medium | Not reproduced - in-memory test passed | No fix indicated by current evidence; PostgreSQL verification remains outstanding. | `CreateRequest_CreatesInitialHistoryLinkedToSavedRequest` passed (1/1, EF Core InMemory). |
| DEF-007 | Category deletion could cascade-delete associated service requests if deletion is introduced or performed outside the current API. | Medium | Configuration confirmed; runtime impact unverified | The EF model and initial migration configure cascade deletion, but `CategoriesController` currently exposes only GET. Review retention requirements before implementing any category-deletion path. | `InitialCreate.cs` configures `FK_Requests_Categories_CategoryId` with `ReferentialAction.Cascade`; `CategoriesController.cs` exposes only `GET api/categories`. Runtime deletion test not performed. |

## Verification notes

- These entries originate from the M3 issue list and are not confirmed runtime defects.
- No fix or regression test is claimed as complete without evidence.
- Update each status after reproduction, correction, and regression testing.

## M4 follow-up — staff-scope risk (2026-10-18)

Regression tests cover out-of-scope staff assignment and state-update
attempts. This is partial mitigation only; PED risk R8 remains open
while staff queue scope and object-level retrieval restrictions remain
unverified. See `docs/RTM-M3.md` and the M4 regression evidence record.