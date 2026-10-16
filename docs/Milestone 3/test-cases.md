CivicConnect — M3 Test Case Specification



Milestone: M3 — Controlled Construction, Integration, Quality and Release Readiness

Owner: Person 1 — Quality and Verification Lead

Document status: Initial draft — test cases require execution and evidence

Repository: SEN381-Project

Related PED: PED v3.0, Sections 2, 3, 6, 7 and 9



1\. Purpose



This document defines the test cases selected for verifying CivicConnect during Milestone 3. It supports risk-based test selection, black-box testing, unit/component testing, requirements traceability, and defect investigation.



The cases define the intended test inputs, procedures, and expected results. Actual results and execution evidence will be added only after the tests have been run.



2\. Execution Status Definitions



Status



Meaning



Not Run



The test has not yet been executed.



Passed



The observed result matches the expected result and evidence is recorded.



Failed



The observed result does not match the expected result.



Blocked



The test cannot be completed because of an identified blocker.



All test cases begin with the status Not Run.



3\. Black-Box Test Cases: Request Submission



TC-B-01 — Valid Request Submission



Related issue: M3-TEST-06

Requirement: FR-001

Technique: Equivalence partitioning

Priority: High

Initial status: Not Run



Objective: Verify that a requester can submit a valid service request.



Preconditions:



The application is running and its required database is available.



A valid requester identity is authenticated.



A valid category exists.



All mandatory fields and validation rules have been confirmed.



Test data:



A valid title.



A valid description.



An existing category identifier.



Steps:



Authenticate as a requester.



Submit a request using the valid test data.



Record the HTTP response.



Check whether the request was persisted.



Check whether the initial status-history record was persisted.



Expected result:



The submission is accepted according to the API contract.



The response identifies the newly created request.



The request is stored with the correct requester, category, and initial state.



The initial status-history record is linked to the created request.



No duplicate or orphaned request/history records are created.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-02 — Missing Required Submission Field



Related issue: M3-TEST-06

Requirement: FR-001

Technique: Equivalence partitioning

Priority: High

Initial status: Not Run



Objective: Verify that the application rejects a submission missing a required field.



Preconditions:



The application and database are available.



A requester is authenticated.



Required fields have been identified from the actual DTO validation and approved requirements.



Test data:



A request with one confirmed mandatory field omitted or blank.



All other required fields contain valid values.



Steps:



Prepare a valid request payload.



Remove or blank one required field.



Submit the request.



Record the response.



Check the database to determine whether a request was created.



Expected result:



The submission is rejected with an appropriate validation response.



The response identifies or explains the validation failure where required by the API contract.



No invalid request is persisted.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-03 — Invalid Category Identifier



Related issue: M3-TEST-06

Requirement: FR-002

Technique: Equivalence partitioning

Priority: High

Initial status: Not Run



Objective: Verify that a request cannot be created using a category that does not exist.



Preconditions:



The application and database are available.



A requester is authenticated.



Existing category identifiers have been checked.



Test data:



A request containing an identifier that does not correspond to an existing category.



Steps:



Prepare a request with otherwise valid fields.



Supply an unknown category identifier.



Submit the request.



Record the response.



Check whether the request was persisted.



Expected result:



The submission is rejected according to the category-validation rules.



The response communicates the invalid category where required by the API contract.



No request referencing a nonexistent category is persisted.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-04 — Submission Field Boundary Values



Related issue: M3-TEST-06

Requirement: FR-001 and applicable validation rules

Technique: Boundary-value analysis

Priority: High

Initial status: Not Run



Objective: Verify that documented field-length boundaries are enforced correctly.



Preconditions:



The actual title and description limits have been identified from the implementation or approved requirements.



The application is available.



A valid requester and category are available.



Test data:

For each field with a documented maximum length, prepare values:



One character below the maximum.



Exactly at the maximum.



One character above the maximum.



Where a documented minimum length exists, test its corresponding lower boundary.



Steps:



Submit a request with a value below the documented maximum.



Submit a request with a value exactly at the maximum.



Submit a request with a value above the maximum.



Record each response and persistence outcome.



Repeat for the other applicable submission fields.



Expected result:



Values within the documented limits are accepted when all other requirements are satisfied.



Values outside the permitted limits are rejected.



The application does not silently truncate or incorrectly store submitted values.



Important: Do not invent field-length limits. Fill in the test values only after confirming the actual rules.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



4\. Black-Box Test Cases: State Transitions



TC-B-05 — Permitted State Transition



Related issue: M3-TEST-07

Requirement: FR-011

Risk: R10

Technique: State-transition testing

Priority: High

Initial status: Not Run



Objective: Verify that a transition explicitly permitted by the approved lifecycle model is accepted.



Preconditions:



The approved state-transition table is available.



A request exists in a state from which the selected transition is permitted.



An authenticated staff account has the required permission.



Test data:



Existing request identifier.



A permitted source state and destination state selected from the approved transition table.



Any required transition reason or assignee information.



Steps:



Authenticate as an authorised staff member.



Retrieve the request's current state.



Submit the permitted state change using the documented API contract.



Record the response.



Retrieve the request again and inspect its status history.



Expected result:



The transition is accepted.



The request's persisted state changes to the requested destination state.



The expected history record is persisted.



Required transition data is retained.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-06 — Prohibited State Transition



Related issue: M3-TEST-07

Requirement: FR-011

Risk: R10

Technique: State-transition testing

Priority: High

Initial status: Not Run



Objective: Verify that a transition prohibited by the approved lifecycle model is rejected.



Preconditions:



The approved state-transition table is available.



A request exists in a state from which the selected transition is prohibited.



An authorised staff account is available so that the test isolates transition legality rather than authentication.



Test data:



Existing request identifier.



A source state and destination state explicitly prohibited by the approved transition table.



Steps:



Authenticate as authorised staff.



Confirm the request's current state.



Submit the prohibited transition.



Record the response.



Retrieve the request and inspect its history.



Expected result:



The transition is rejected with the appropriate response.



The request remains in its original state.



No successful-transition history record is incorrectly created.



No unintended side effects occur.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-07 — Transition from a Terminal State



Related issue: M3-TEST-07

Requirement: FR-011

Risk: R10

Technique: State-transition testing

Priority: High

Initial status: Not Run



Objective: Verify that a transition from a terminal state is rejected when the approved lifecycle does not permit it.



Preconditions:



The approved lifecycle identifies which states are terminal.



A request exists in a terminal state.



An authorised staff account is available.



Test data:



Existing request identifier in a terminal state.



A destination state that is not permitted from that terminal state.



Steps:



Authenticate as authorised staff.



Confirm the request is in the selected terminal state.



Attempt the prohibited transition.



Record the response.



Check the persisted state and history.



Expected result:



The transition is rejected if the approved lifecycle prohibits it.



The persisted state remains unchanged.



No misleading successful-transition record is created.



Important: If the approved lifecycle allows reopening or another transition from this state, choose a different prohibited transition instead.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



5\. Black-Box Test Cases: Authentication and Authorisation



TC-B-08 — Unauthenticated Protected Operation



Related issue: M3-TEST-08

Requirement: NFR-017

Risks: R3, R6

Technique: Decision-table testing / negative API testing

Priority: High

Initial status: Not Run



Objective: Verify that a protected operation cannot be accessed without valid authentication.



Preconditions:



The application is running.



The selected endpoint requires authentication.



No valid authentication credentials are supplied.



Test data:



A request to a protected endpoint without an authentication token or other accepted authentication mechanism.



Steps:



Send a request to the selected protected endpoint without credentials.



Record the response.



Check whether the protected operation was performed.



Where relevant, inspect whether protected data was returned.



Expected result:



The operation is refused according to the endpoint's authentication policy.



No protected data is disclosed.



No unauthorised state change or other protected action occurs.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-09 — Unauthorised Role Attempts Staff Operation



Related issue: M3-TEST-08

Requirement: NFR-018

Risks: R3, R6

Technique: Decision-table testing / negative API testing

Priority: High

Initial status: Not Run



Objective: Verify that a user with a valid identity but an insufficient role cannot perform a staff-only operation.



Preconditions:



The application is running.



The selected operation requires the STAFF role.



A valid authenticated account without the STAFF role is available.



Test data:



A valid request identifier.



A state-change request submitted using the authenticated account without staff permission.



Steps:



Authenticate as a requester or another role not authorised for the selected operation.



Attempt the staff-only operation.



Record the response.



Inspect the request's persisted state and history.



Expected result:



The operation is refused according to the authorisation policy.



The request is not changed.



No unauthorised successful state-change history is recorded.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-B-10 — Requester Attempts to Access Another Requester's Record



Related issue: M3-TEST-08

Requirement: NFR-019 and applicable ownership requirements

Risk: R6

Technique: Decision-table testing / negative API testing

Priority: High

Initial status: Not Run



Objective: Verify that a requester cannot access another requester's protected information.



Preconditions:



Two distinct requester identities are available.



A request belonging to requester A exists.



Requester B is authenticated.



Test data:



The identifier of requester A's request.



Valid credentials for requester B.



Steps:



Create or identify a request owned by requester A.



Authenticate as requester B.



Attempt to retrieve requester A's request.



Record the response.



Check whether any restricted request information was disclosed.



Expected result:



Requester B cannot access the protected record.



The response follows the endpoint's approved access-denial policy.



The response does not disclose restricted request information.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



6\. Unit Test Cases



TC-U-01 — Request Validation



Related issue: M3-TEST-03

Requirements: FR-001 and applicable validation requirements

Technique: Unit testing

Priority: High

Initial status: Not Run



Objective: Verify that the relevant request-validation component handles valid and invalid inputs according to its rules.



Steps:



Identify the actual validation class and its existing unit tests.



Prepare valid input and input that violates a confirmed validation rule.



Execute the relevant unit tests.



Record the assertions and test-run output.



Expected result:



Valid input passes validation.



Invalid input fails validation according to the specified rule.



The test does not depend on unverified assumptions about validation limits.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



TC-U-02 — Category Validation Strategy



Related issue: M3-TEST-03

Requirement: FR-002

Technique: Unit testing

Priority: High

Initial status: Not Run



Objective: Verify the category-validation strategy and its validation context against the actual implementation.



Steps:



Identify the category-validation strategy and its existing tests.



Test a valid category selection.



Test an invalid or unknown category selection using the supported test setup.



Run the relevant unit tests and record their output.



Expected result:



Valid category input is handled according to the approved rules.



Invalid category input is rejected.



The test confirms the behaviour of the actual component rather than a hypothetical implementation.



Actual result: To be recorded after execution.

Evidence: To be recorded.

Defect ID: None assigned unless a failure is identified.



7\. Additional Verification Cases



Additional uniquely identified tests must be added for:



FR-006: required notification or feedback side effects.



NFR-020: audit/history consistency and relevant failure handling.



Confirmed defects: the original failing test and regression verification after the fix.



Any other high-priority requirement not adequately covered by the cases above.



These cases must receive their own test IDs and be linked to the applicable requirements, risks, defects, and evidence. They must not be represented as covered by TC-B-01–TC-B-10 unless the same case explicitly verifies that behaviour.



8\. Evidence and Traceability



For each executed case, record:



Actual result.



Execution date and environment.



Test-run or assertion output.



Evidence file path or report reference.



Final status.



Related defect identifier, if applicable.



Retest or regression result, where applicable.



The test-case IDs must be aligned with PED v3.0 Section 6, the RTM, the Test Evidence Records, and the Defect Register.



No test may be marked Passed until it has been executed and its result verified.



9\. Initial Test Execution Summary



Test ID



Initial status



Evidence reference



TC-B-01



Not Run



Pending



TC-B-02



Not Run



Pending



TC-B-03



Not Run



Pending



TC-B-04



Not Run



Pending



TC-B-05



Not Run



Pending



TC-B-06



Not Run



Pending



TC-B-07



Not Run



Pending



TC-B-08



Not Run



Pending



TC-B-09



Not Run



Pending



TC-B-10



Not Run



Pending



TC-U-01



Not Run



Pending



TC-U-02



Not Run



Pending



Document status: Draft. Test execution, actual results, evidence references, and any necessary corrections remain outstanding.

