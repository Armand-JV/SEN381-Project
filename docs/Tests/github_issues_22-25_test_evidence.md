# CivicConnect API Test Evidence — GitHub Issues #22–#25

**Test date:** 13 October 2026  
**Test environment:** Local API (`http://localhost:5287`)  
**Testing method:** Manual HTTP API testing  
**Scope:** Staff request filtering, request assignment, lifecycle transitions, and comment visibility/authorisation

> **Evidence convention:** PASS reflects the outcome recorded in the original test notes. Where supporting output does not establish every expected condition, this is identified in the verification notes rather than presented as independently verified.

## Test Case Summary

| Test ID | GitHub Issue | Scenario | Recorded Result |
| --- | --- | --- | --- |
| TC-22-01 | #22 | Filter staff requests by state and category with pagination | PASS |
| TC-23-01 | #23 | Assign an Open request to staff | PASS |
| TC-23-02* | #23 | Prevent a requester from assigning a request | PASS |
| TC-24-01 | #24 | Open → Assigned | PASS |
| TC-24-02 | #24 | Assigned → InProgress | PASS |
| TC-24-03 | #24 | InProgress → Resolved | PASS |
| TC-24-04 | #24 | Resolved → Closed | PASS |
| TC-24-05 | #24 | Reject Closed → Open | PASS |
| TC-24-06 | #24 | Reject Open → Resolved | PASS |
| TC-24-07 | #24 | Reject transition to Rejected without reason | PASS |
| TC-25-01 | #25 | Requester adds public comment | PASS |
| TC-25-02 | #25 | Staff adds public comment | PASS |
| TC-25-03 | #25 | Staff adds internal comment | PASS |
| TC-25-05 | #25 | Requester cannot add internal comment | PASS |
| TC-25-06 | #25 | Requester sees only public comments | PASS |

*`TC-23-02` is a new identifier assigned to the previously unnumbered negative test; it is not an additional test. `TC-25-04` was not present in the supplied evidence.*

---

## Issue #22 — Staff Request Listing, Filtering and Pagination

### TC-22-01 — Filter Open Requests by Category

| Field | Details |
| --- | --- |
| **Objective** | Verify that the staff request endpoint filters requests by state and category and returns pagination metadata. |
| **Actor** | Admin (test notes identify authorised roles as MANAGER or STAFF) |
| **Precondition** | Authenticated user with access to the staff requests endpoint; requests exist in category 1. |
| **HTTP Method** | `GET` |
| **Endpoint** | `/api/staff/requests?state=Open&categoryId=1` |
| **Expected Result** | `200 OK`; only Open requests in category 1 are returned, including pagination information. |
| **Actual Result** | `200 OK` recorded; seven returned requests have `state: 0` and `categoryId: 1`, with page metadata present. |
| **Status** | **PASS** |

**Response evidence (all returned records, condensed to filtering fields):**

```json
{
  "items": [
    {"id": 8, "title": "Staff posting test", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 7, "title": "Staff posting test", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 6, "title": "Broken streetlight", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 5, "title": "Test3", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 4, "title": "Test", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 3, "title": "Broken Fire Hydrant", "categoryId": 1, "categoryName": "Infrastructure", "state": 0},
    {"id": 2, "title": "Broken Sidewalk", "categoryId": 1, "categoryName": "Infrastructure", "state": 0}
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 7,
  "totalPages": 1
}
```

**Verification:** State filtering, category filtering and pagination metadata are demonstrated. Multiple pages were not tested in the supplied evidence.

---

## Issue #23 — Request Assignment and Authorisation

### TC-23-01 — Assign an Open Request to a Staff Member

| Field | Details |
| --- | --- |
| **Objective** | Verify authorised assignment and persistence of request state, assignee, history and audit evidence. |
| **Actor** | Admin (test notes identify MANAGER or STAFF as authorised roles) |
| **Precondition** | Request `3` is Open (`state: 0`). |
| **HTTP Method** | `PUT` |
| **Endpoint** | `/api/requests/3/assign` |
| **Expected Result** | `200 OK`; request becomes Assigned (`state: 1`), `assigneeId` is `staff-9`, and history/audit entries are recorded. |
| **Actual Result** | `200 OK`; `state: 1`, `assigneeId: staff-9`, transition history and `RequestAssigned` audit entry returned. |
| **Status** | **PASS** |

**Request body:**

```json
{
  "assigneeId": "staff-9",
  "reason": "Capacity match"
}
```

**Response evidence (relevant fields):**

```json
{
  "id": 3,
  "title": "Broken Fire Hydrant",
  "state": 1,
  "assigneeId": "staff-9",
  "history": [
    {
      "id": 9,
      "requestId": 3,
      "previousState": 0,
      "newState": 1,
      "changedAt": "2026-10-13T15:09:58.8476509Z",
      "changedBy": "Admin",
      "reason": "Capacity match"
    }
  ],
  "auditEntries": [
    {
      "id": 1,
      "requestId": 3,
      "action": "RequestAssigned",
      "actorId": "Admin",
      "createdAt": "2026-10-13T15:09:58.8476509Z",
      "details": "Assigned to staff-9."
    }
  ]
}
```

### TC-23-02 — Requester Cannot Assign a Request

| Field | Details |
| --- | --- |
| **Objective** | Verify that a requester is prohibited from using the assignment endpoint. |
| **Actor** | Authenticated requester |
| **HTTP Method** | `PUT` |
| **Endpoint** | `/api/requests/3/assign` (assignment endpoint tested in this issue; exact negative-test request body not recorded) |
| **Expected Result** | `403 Forbidden`. |
| **Actual Result** | `403 Forbidden` recorded with requester authentication. |
| **Status** | **PASS** |

**Evidence note:** The original notes report the response code but do not include the negative-test response payload or a subsequent query confirming no assignment change.

---

## Issue #24 — Request Lifecycle State Validation

**State mapping used in the recorded tests:** `0 = Open`, `1 = Assigned`, `2 = InProgress`, `3 = Resolved`, `4 = Closed`, `6 = Rejected`.

### TC-24-01 — Valid Open → Assigned

| Field | Details |
| --- | --- |
| **Objective** | Verify a valid Open-to-Assigned transition through assignment. |
| **Precondition** | Request `3` is Open. |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/assign` |
| **Request Body** | `{"assigneeId":"staff-9","reason":"Capacity match"}` |
| **Expected Result** | `200 OK`; request state changes to Assigned. |
| **Actual Result** | Recorded as successful; assignment response supporting this transition appears in TC-23-01. |
| **Status** | **PASS** |

### TC-24-02 — Valid Assigned → InProgress

| Field | Details |
| --- | --- |
| **Objective** | Verify a valid Assigned-to-InProgress transition. |
| **Precondition** | Request `3` is Assigned. |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/state` |
| **Request Body** | `{"newState":2,"reason":"Work started"}` |
| **Expected Result** | `200 OK`; state changes to InProgress. |
| **Actual Result** | Recorded as successful. |
| **Status** | **PASS** |

### TC-24-03 — Valid InProgress → Resolved

| Field | Details |
| --- | --- |
| **Objective** | Verify a valid InProgress-to-Resolved transition. |
| **Precondition** | Request `3` is InProgress (implied by test sequence). |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/state` |
| **Request Body** | `{"newState":3,"reason":"Repair completed"}` |
| **Expected Result** | `200 OK`; state changes to Resolved. |
| **Actual Result** | Recorded as successful. |
| **Status** | **PASS** |

### TC-24-04 — Valid Resolved → Closed

| Field | Details |
| --- | --- |
| **Objective** | Verify a valid Resolved-to-Closed transition. |
| **Precondition** | Request `3` is Resolved (implied by test sequence). |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/state` |
| **Request Body** | `{"newState":4,"reason":"Requester outcome confirmed"}` |
| **Expected Result** | `200 OK`; state changes to Closed. |
| **Actual Result** | Recorded as successful. |
| **Status** | **PASS** |

### TC-24-05 — Reject Closed → Open

| Field | Details |
| --- | --- |
| **Objective** | Verify that a Closed request cannot return directly to Open. |
| **Precondition** | Request `3` is Closed. |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/state` |
| **Request Body** | `{"newState":0,"reason":"Attempted reopen"}` |
| **Expected Result** | `400 Bad Request`; request remains Closed; no history row created. |
| **Actual Result** | Recorded error: `Cannot transition from Closed to Open.` |
| **Status** | **PASS** (recorded) |

**Verification note:** The error message supports rejection. The original evidence does not include a follow-up request or database/history inspection proving unchanged state and history.

### TC-24-06 — Reject Open → Resolved

| Field | Details |
| --- | --- |
| **Objective** | Verify that a request cannot skip directly from Open to Resolved. |
| **Precondition** | Request `4` is Open. |
| **HTTP Method / Endpoint** | `PUT /api/requests/4/state` |
| **Request Body** | `{"newState":3,"reason":"Invalid jump"}` |
| **Expected Result** | `400 Bad Request`; error indicates prohibited Open-to-Resolved transition. |
| **Actual Result** | Recorded error: `Cannot transition from Open to Resolved.` |
| **Status** | **PASS** |

### TC-24-07 — Reject Transition to Rejected Without Reason

| Field | Details |
| --- | --- |
| **Objective** | Verify that a transition to Rejected requires a nonempty reason. |
| **Precondition** | Original notes state that the request is Open. |
| **HTTP Method / Endpoint** | `PUT /api/requests/3/state` |
| **Request Body** | `{"newState":6,"reason":""}` |
| **Expected Result** | `400 Bad Request`; message states that a reason is required. |
| **Actual Result** | Recorded error: `A reason is required when transitioning to Rejected.` |
| **Status** | **PASS** |

**Verification note:** Request `3` was previously moved to Closed in TC-24-04. The evidence does not show it being reset to Open before this test. The reason-validation error is supported, but the recorded precondition is not independently established.

---

## Issue #25 — Public/Internal Comments and Visibility

**Common test record:** Request `2` (`Broken Sidewalk`) belongs to `UserA`.

### TC-25-01 — Requester Adds a Public Comment

| Field | Details |
| --- | --- |
| **Objective** | Confirm the request owner can submit a public comment. |
| **Actor** | Requester (`UserA`; notes specify `requester-2` token) |
| **Precondition** | Request `2` exists and is owned by UserA. |
| **HTTP Method / Endpoint** | `POST /api/requests/2/comments` |
| **Request Body** | `{"text":"I have added more information.","isInternal":false}` |
| **Expected Result** | `201 Created`; comment text, `createdBy: UserA` and `isInternal: false` are returned. |
| **Actual Result** | Created comment `id: 1` with the expected text, creator and visibility flag. |
| **Status** | **PASS** |

```json
{
  "id": 1,
  "requestId": 2,
  "text": "I have added more information.",
  "createdAt": "2026-10-13T17:09:23.1075958Z",
  "createdBy": "UserA",
  "isInternal": false
}
```

### TC-25-02 — Staff Adds a Public Comment

| Field | Details |
| --- | --- |
| **Objective** | Confirm an authorised staff member can submit a public comment. |
| **Actor** | Staff (`staff-1`) |
| **HTTP Method / Endpoint** | `POST /api/requests/2/comments` |
| **Request Body** | `{"text":"Our team is investigating this report.","isInternal":false}` |
| **Expected Result** | `201 Created`; creator is `staff-1` and comment is public. |
| **Actual Result** | Created comment `id: 3` with expected text, creator and visibility flag. |
| **Status** | **PASS** |

```json
{
  "id": 3,
  "requestId": 2,
  "text": "Our team is investigating this report.",
  "createdAt": "2026-10-13T17:16:14.9724612Z",
  "createdBy": "staff-1",
  "isInternal": false
}
```

### TC-25-03 — Staff Adds an Internal Comment

| Field | Details |
| --- | --- |
| **Objective** | Confirm staff can create comments marked internal. |
| **Actor** | Staff (`staff-1`) |
| **HTTP Method / Endpoint** | `POST /api/requests/2/comments` |
| **Request Body** | `{"text":"Internal review required.","isInternal":true}` |
| **Expected Result** | `201 Created`; creator is `staff-1` and `isInternal: true`. |
| **Actual Result** | Created comment `id: 4` with expected text, creator and internal flag. |
| **Status** | **PASS** |

```json
{
  "id": 4,
  "requestId": 2,
  "text": "Internal review required.",
  "createdAt": "2026-10-13T17:17:21.5842017Z",
  "createdBy": "staff-1",
  "isInternal": true
}
```

### TC-25-05 — Requester Cannot Add an Internal Comment

| Field | Details |
| --- | --- |
| **Objective** | Confirm a requester cannot create an internal comment. |
| **Actor** | Requester (notes specify `requester-2` token) |
| **HTTP Method / Endpoint** | `POST /api/requests/2/comments` |
| **Request Body** | `{"text":"This should not be internal.","isInternal":true}` |
| **Expected Result** | `403 Forbidden`; no comment is created. |
| **Actual Result** | `403 Forbidden` recorded. |
| **Status** | **PASS** (recorded) |

**Verification note:** The response confirms the access restriction. A database or subsequent GET check proving that no record was created is not shown.

### TC-25-06 — Requester Can See Public but Not Internal Comments

| Field | Details |
| --- | --- |
| **Objective** | Verify the requester-facing request details hide internal staff comments. |
| **Actor** | Requester (notes specify `requester-2` token) |
| **HTTP Method / Endpoint** | `GET /api/requests/2` |
| **Expected Result** | `200 OK`; public comments visible; internal comments omitted. |
| **Actual Result** | `200 OK` recorded; comments `1`, `2`, `3` are returned, all with `isInternal: false`; internal comment `4` is absent. |
| **Status** | **PASS** |

**Response evidence (relevant fields):**

```json
{
  "id": 2,
  "title": "Broken Sidewalk",
  "requesterId": "UserA",
  "comments": [
    {"id": 1, "requestId": 2, "text": "I have added more information.", "createdBy": "UserA", "isInternal": false},
    {"id": 2, "requestId": 2, "text": "I have added more information.", "createdBy": "staff-1", "isInternal": false},
    {"id": 3, "requestId": 2, "text": "Our team is investigating this report.", "createdBy": "staff-1", "isInternal": false}
  ],
  "history": [],
  "auditEntries": []
}
```

**Verification:** Internal comment `id: 4` was created in TC-25-03 but does not appear in the requester response, supporting the visibility requirement.

---