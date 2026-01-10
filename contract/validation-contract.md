# Validation Contract — Support Validation Console

## Purpose

This document defines the fixed validation contract for the Support Validation Console.
It describes the expected payload shape, validation rules, API responses, and endpoints.
All backend and frontend implementations must conform to this contract.

The goal is deterministic, reproducible validation behaviour suitable for support and troubleshooting workflows.

---

## Input Payload

### Canonical Example

{
  "state": "DE",
  "provisions": ["A", "B", "C"]
}

### Field Definitions

- state
  - Required
  - Type: string
  - Represents the jurisdiction to validate against

- provisions
  - Required
  - Type: array of strings
  - Represents provision codes supplied in the payload

### Notes

- Additional fields may be present and must be ignored
- Payloads may contain noise; validation logic must be resilient
---

## Validation Rules

Validation is based on a single, hard-coded rule set.

### Required Provisions by State

- DE
  - Required provisions: A, B

- MN
  - Required provisions: C

### Rule Behaviour

- If state is not listed, no validation rule applies → PASSED
- If state is listed:
  - Compare required provisions with provided provisions
  - If any required provision is missing → FAILED
  - If none are missing → PASSED

Missing provisions result in a FAILED validation status.
---

## Validation Result

### Response Shape

{
  "id": "uuid",
  "status": "FAILED",
  "state": "DE",
  "missingProvisions": ["B"],
  "createdAt": "2026-01-08T20:15:00Z"
}

### Field Definitions

- id
  - Server-generated UUID identifying the validation run

- status
  - Enum: PASSED | FAILED

- state
  - The state evaluated during validation

- missingProvisions
  - Array of missing provision codes
  - Always present (empty array if none missing)

- createdAt
  - Server-generated timestamp (ISO 8601)

---

## API Endpoints

### POST /validate

- Accepts a JSON payload
- Executes validation rules
- Stores the validation run
- Returns the validation result

### GET /validations

- Returns a list of previous validation runs
- Intended for history and review
- Sorted by creation time (most recent first)

### GET /validations/{id}

- Returns full details for a single validation run
- Includes original payload and validation result

---

## Design Principles

- Deterministic validation behaviour
- Clear failure diagnostics
- Support-focused error visibility
- Easy reproduction of validation outcomes
