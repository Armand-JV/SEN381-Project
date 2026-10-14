# CivicConnect Request Comments — Postman Test Cases

These test cases cover public requester comments, internal staff/manager comments, request ownership, and comment visibility.

## Postman setup

Create a Postman environment with:

```text
baseUrl = https://localhost:<port>
```

Use `{{baseUrl}}` in each request URL. Add this header to requests with a JSON body:

```text
Content-Type: application/json
```

The seeded integration-test request is:

- Request ID: `2`
- Requester: `requester-2`

## Authentication

The application currently uses the `TestHeader` authentication scheme. For authenticated requests, add these headers:

```text
X-Test-Role: REQUESTER
X-Test-User: requester-2
```

Use the following alternatives for other users:

```text
X-Test-Role: STAFF
X-Test-User: staff-1
```

```text
X-Test-Role: MANAGER
X-Test-User: manager-1
```

For anonymous requests, do not send either `X-Test-Role` or `X-Test-User`.

> `TestAuthController` can generate a debug JWT at `GET /api/TestAuth/token/{userId}?role={role}` when compiled in `DEBUG`. The current `Program.cs` configures `TestHeader`, not JWT bearer authentication, so the generated token is not currently used to authenticate these endpoints unless JWT bearer authentication is configured separately.

## Test cases

### TC-01 — Requester views their own request

**Request**

```http
GET {{baseUrl}}/api/requests/2
```

Headers:

```text
X-Test-Role: REQUESTER
X-Test-User: requester-2
```

**Expected result**

- Status: `200 OK`
- The request is returned.
- Public comments are included.
- Internal comments are excluded.

---

### TC-02 — Staff views a request

**Request**

```http
GET {{baseUrl}}/api/requests/2
```

Headers:

```text
X-Test-Role: STAFF
X-Test-User: staff-1
```

**Expected result**

- Status: `200 OK`
- Staff can view the request regardless of requester ownership.
- Public and internal comments are included.

---

### TC-03 — Manager views a request

**Request**

```http
GET {{baseUrl}}/api/requests/2
```

Headers:

```text
X-Test-Role: MANAGER
X-Test-User: manager-1
```

**Expected result**

- Status: `200 OK`
- Manager can view the request regardless of requester ownership.
- Public and internal comments are included.

---

### TC-04 — Requester adds a public comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: REQUESTER
X-Test-User: requester-2
```

Body:

```json
{
  "text": "I have added more information about the issue.",
  "isInternal": false
}
```

**Expected result**

- Status: `201 Created`
- The response contains the created comment.
- `createdBy` is `requester-2`.
- `isInternal` is `false`.
- The comment is visible when `requester-2` views the request.

---

### TC-05 — Staff adds an internal comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: STAFF
X-Test-User: staff-1
```

Body:

```json
{
  "text": "Internal review required before assigning this request.",
  "isInternal": true
}
```

**Expected result**

- Status: `201 Created`
- `createdBy` is `staff-1`.
- `isInternal` is `true`.
- The comment is visible to staff and managers.
- The comment is not visible to the requester.

---

### TC-06 — Staff adds a public comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: STAFF
X-Test-User: staff-1
```

Body:

```json
{
  "text": "Our team is investigating this report.",
  "isInternal": false
}
```

**Expected result**

- Status: `201 Created`
- `createdBy` is `staff-1`.
- `isInternal` is `false`.
- The comment is visible to the requester.

---

### TC-07 — Manager adds an internal comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: MANAGER
X-Test-User: manager-1
```

Body:

```json
{
  "text": "Manager review completed.",
  "isInternal": true
}
```

**Expected result**

- Status: `201 Created`
- `createdBy` is `manager-1`.
- `isInternal` is `true`.

---

### TC-08 — Requester attempts to add an internal comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: REQUESTER
X-Test-User: requester-2
```

Body:

```json
{
  "text": "This should not be allowed.",
  "isInternal": true
}
```

**Expected result**

- Status: `403 Forbidden`
- No comment is created.

---

### TC-09 — Requester views another user's request

**Request**

```http
GET {{baseUrl}}/api/requests/2
```

Headers:

```text
X-Test-Role: REQUESTER
X-Test-User: requester-1
```

**Expected result**

- Status: `403 Forbidden`.

---

### TC-10 — Requester comments on another user's request

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: REQUESTER
X-Test-User: requester-1
```

Body:

```json
{
  "text": "This request does not belong to me.",
  "isInternal": false
}
```

**Expected result**

- Status: `403 Forbidden`.
- No comment is created.

---

### TC-11 — Anonymous user views a request

**Request**

```http
GET {{baseUrl}}/api/requests/2
```

Do not send authentication headers.

**Expected result**

- Status: `401 Unauthorized`.

---

### TC-12 — Anonymous user adds a comment

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Header:

```text
Content-Type: application/json
```

Body:

```json
{
  "text": "Anonymous comment",
  "isInternal": false
}
```

**Expected result**

- Status: `401 Unauthorized`.
- No comment is created.

---

### TC-13 — Empty comment text

**Request**

```http
POST {{baseUrl}}/api/requests/2/comments
```

Headers:

```text
Content-Type: application/json
X-Test-Role: REQUESTER
X-Test-User: requester-2
```

Body:

```json
{
  "text": "   ",
  "isInternal": false
}
```

**Expected result**

- Status: `400 Bad Request`.
- The response indicates that comment text is required.

---

### TC-14 — Request does not exist

**Request**

```http
GET {{baseUrl}}/api/requests/99999
```

Headers:

```text
X-Test-Role: STAFF
X-Test-User: staff-1
```

**Expected result**

- Status: `404 Not Found`.

Repeat with:

```http
POST {{baseUrl}}/api/requests/99999/comments
```

and a valid comment body. The expected result is also `404 Not Found`.

## Suggested execution order

1. Run TC-05 to create an internal staff comment.
2. Run TC-01 and confirm the requester cannot see it.
3. Run TC-02 and confirm staff can see it.
4. Run TC-04 and confirm the requester can create a public comment.
5. Run TC-08, TC-09, TC-10, TC-11, and TC-12 to verify negative authorization cases.
