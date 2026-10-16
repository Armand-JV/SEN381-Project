# M3 Batch 1 — Test-to-Evidence Mapping

- Execution date: 2026-10-16
- Automated run: docs/Tests/m3-batch1-evidence-2026-10-16.md
- Machine-readable results: docs/Tests/results/m3-batch1-2026-10-16.trx
- Console log: docs/Tests/results/m3-batch1-2026-10-16.log
- Suite result: 119 passed, 0 failed, 0 skipped

## Case mapping

| Test case | Existing test evidence | Mapping assessment |
|---|---|---|
| TC-B-01 | RequestControllerTests.CreateRequest_ValidSubmission_ReturnsCreatedDtoAndPersistsInitialHistory; RequesterSliceTests.CreateRequest_CreatesInitialHistoryLinkedToSavedRequest | Submission and initial history assertions exist. |
| TC-B-02 | RequesterSliceTests.CreateRequest_InvalidInput_ReturnsFieldValidationAndPersistsNothing | Invalid required-field input returns Bad Request and persists no request. |
| TC-B-03 | RequestControllerTests.CreateRequest_UnknownCategory_ReturnsBadRequestAndPersistsNothing | Unknown category rejection and no persistence are asserted. |
| TC-B-04 | RequesterSliceTests.CreateRequest_MaxLengthsAccepted_AndOverLimitRejected | Partial automated coverage: title length 200 and description length 2000 are accepted; title length 201 is rejected. One-below-maximum values and an over-limit description are not demonstrated by this test. |
| TC-B-05 | RequestControllerTests.UpdateState_ValidTransition_ReturnsOkAndPersistsHistory | Valid transition, persisted state and history are asserted. |
| TC-B-06 | RequestControllerTests.UpdateState_IllegalTransition_LeavesRequestUnchanged | Illegal transition rejection and unchanged state are asserted. |
| TC-B-07 | RequestControllerTests.UpdateState_FromAnyTerminalState_ReturnsBadRequest | Reopening terminal states is rejected. |
| TC-B-08 | RequestControllerTests.GetRequest_Anonymous_ReturnsUnauthorized | The anonymous GET request to a protected request-detail endpoint asserts HTTP 401 Unauthorized. This maps to the documented objective as automated evidence, not manual execution evidence. |
| TC-B-09 | RequestControllerTests.GetStaffRequests_Requester_ReturnsForbidden; JwtAuthFlowTests.RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions | Requester access to staff operations is rejected. |
| TC-B-10 | JwtAuthFlowTests.RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions | A different requester is denied access to the request and its history. |
| TC-U-01 | CategoryValidationContextTests.Validate_WithNoMatchingStrategy_UsesDefaultValidation; CategoryValidationContextTests.Validate_DefaultFallback_RejectsMissingTitleOrDescription | Unit tests assert valid fallback input passes and empty or whitespace-only title/description inputs fail. The full Release test run passed. |
| TC-U-02 | CategoryValidationContextTests strategy dispatch, fallback and rejection tests | Strategy dispatch, fallback and rejection behaviour are covered. |

## Interpretation and limitations

The full automated suite passed in Release configuration on 2026-10-16. This establishes the result of that automated run, not that every documented manual procedure was separately executed.

Keep a test case pending if its required operation, expected status, boundary values or other assertions do not match the existing automated test.

TC-B-08 is mapped to the anonymous request-detail operation, which asserts HTTP 401. This is automated test evidence; it does not claim a separate manual execution.

## Batch 1 follow-up

- [x] Map TC-B-08 to the anonymous protected request-detail test.
- [ ] Compare TC-B-04 boundary values against the test procedure.
- [x] Confirm TC-U-01 unit-level valid and invalid input assertions.
- [ ] Update the test-case execution summary only where the mapping supports the expected result.
- [ ] Attach this mapping and the saved run evidence to the relevant issue or pull request.
