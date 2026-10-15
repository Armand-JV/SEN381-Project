# Requester slice evidence (FR-001–FR-006)

Date: 2026-10-15  
Branch: `feature/Fr-01-to-06`

## Automated evidence

Command:

```text
dotnet test src/CivicConnect.slnx --no-restore
```

Result: **93 passed, 0 failed, 0 skipped** (local test host, 2026-10-15).

Requester-specific coverage is in `src/CivicConnect.Tests/RequesterSliceTests.cs`:

| Behaviour | Test |
|---|---|
| Category listing | `Categories_ReturnSeededDtos` |
| Required fields and no persistence on invalid input | `CreateRequest_InvalidInput_ReturnsFieldValidationAndPersistsNothing` |
| Title/description boundaries | `CreateRequest_MaxLengthsAccepted_AndOverLimitRejected` |
| Actor-only requester list, paging, readable state/category | `GetMyRequests_UsesActorAndReturnsPagedStateAndCategory` |
| Invalid paging | `GetMyRequests_InvalidPaging_ReturnsBadRequest` |
| History order, initial entry, reason, ownership | `GetHistory_IncludesInitialAndChronologicallyOrdersRows_AndEnforcesOwnership` |
| Requester-safe detail and internal comment exclusion | `RequestDetail_ProjectsSafeDtoAndFiltersInternalComments` |
| Completion gate and valid feedback | `Feedback_RequiresCompletionAndValidOwner` |
| Non-owner, rating, duplicate, trim, actor persistence | `Feedback_RejectsNonOwnerInvalidRatingAndDuplicate` |
| Feedback comment boundary | `Feedback_CommentOverLimitReturnsBadRequest` |

Existing `RequestControllerTests` cover authenticated actor binding, initial history persistence, unknown category, requester ownership, comments, state transitions, and role denial.

## Endpoint contract

- `GET /api/categories` — public category DTO list.
- `POST /api/requests` — authenticated submission. Title required, trimmed, max 200; description required, trimmed, max 2000; category must exist. Returns `201` DTO with numeric `Id`.
- `GET /api/requests/mine?page=&pageSize=` — `REQUESTER` role only. Actor comes from authenticated principal; never from query string.
- `GET /api/requests/{id}/history` — owner/request-access check; ordered by `ChangedAt`, then `Id`; includes `Reason`.
- `POST /api/requests/{id}/feedback` — requester owner only; request state `Resolved` or `Closed`; rating 1–5; optional comment max 2000; one feedback per request.

## Limits

Numeric `Request.Id` is accepted for FR-003 as unique human-readable reference. Requester history reasons are intentionally visible. Field limits and rating range above describe implemented API behavior; source M1 acceptance wording was not available in this repository, so no stronger baseline claim is made.

## Verification limits

- Local tests use EF Core InMemory unless CI supplies PostgreSQL. PostgreSQL migration/unique-index application must be verified by CI or a configured database run.
- Test authentication uses `TestHeaderAuthenticationHandler`; it proves controller authorization wiring, not production JWT identity-provider behavior.
- React verification: `npm run build` passes. `npm run lint` passes with existing warnings in `routes.tsx`, `StaffQueuePage.tsx`, and effect state-update warnings in requester/detail pages.
- Hosted CI URL and human review evidence remain pending; no hosted result is claimed here.
