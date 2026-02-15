# Support Validation Console — Architecture

## Purpose

Support Validation Console (SVC) is a small ASP.NET Core Web API designed to validate inbound support payloads against a defined input contract represented by the ValidationRequest model.
---

## High-Level Design

The API follows a simple layered design:

**Controller (HTTP boundary) → Service (business workflow) → Store (persistence abstraction)**

- After routing, model binding, and structural validation have completed, the controller action translates the HTTP request into an application operation by invoking the appropriate service method, then translates the result into an HTTP response.
- The service layer implements the core validation workflow: it applies domain rules, orchestrates rule evaluation, coordinates persistence, and produces the resulting ValidationRun. ValidationRun is the aggregate output of the validation workflow and represents a persistable execution snapshot.
- Stores provide a persistence mechanism (currently in-memory) behind an interface so it can be swapped later (e.g., SQL). ValidationRuns are saved to the store as part of the service workflow, and can be retrieved later for status checks or audits.

This structure keeps responsibilities clear and makes it easy to extend the project into a more production-like support simulator.

---

## Why Controllers Exist

Controllers are the API’s **HTTP boundary**.

They exist to:
- Define the public routes/endpoints (e.g., `POST /validate`, `GET /validations`, `GET /validations/{id}`)
- Accept and return HTTP-friendly models (JSON in/out)
- Apply request-level concerns such as:
  - model binding (JSON → C# object)
  - automatic validation via `[ApiController]`
  - returning correct HTTP status codes

Controllers **do not** implement business logic e.g, for state IE, provision A is required. Instead, they delegate to services so the workflow remains reusable and testable outside HTTP.

---

## Why Services Exist

Services contain the **core workflow and business rules** for the application.

They exist to:
- Implement “what the system does” independent of HTTP
- Orchestrate work across components (validation logic + persistence)
- Centralise business decisions such as:
  - what constitutes a successful validation
  - what data should be recorded (e.g., timestamps, status, summary, errors)
  - how status transitions occur

This is the layer that would grow if SVC becomes more production-like (e.g., adding auth, correlation IDs, persistence, and richer validation rules).

---

## Why Interfaces Exist

Interfaces define **contracts** between layers.

They exist to:
- Decouple implementation details from the calling code
- Enable swapping implementations without changing controllers/services
- Support unit testing by allowing mocks/fakes
- Keep “what we need” separate from “how it is done”

Example: the service depends on an abstraction like a validation store interface, not a concrete storage class. That allows the store to evolve from in-memory → database later with minimal churn.

---

## Why InMemoryValidationStore Exists

`InMemoryValidationStore` is the current persistence mechanism.

It exists to:
- Keep the project lightweight and runnable without external dependencies
- Support fast iteration while the API contract stabilises
- Provide a realistic separation of concerns (persistence is still a distinct component)
- Mimic a database-backed store in a simplified form

The intent is that this store can be replaced with a real database implementation (e.g., EF Core + SQL Server/Postgres) while keeping the controller/service logic largely unchanged.

---

## How Dependency Injection Is Wired (Program.cs)

ASP.NET Core provides a built-in Dependency Injection (DI) container.

DI is used so that:
- Controllers do not manually construct their dependencies
- Services and stores can be swapped by changing registrations in one place
- Lifetimes can be controlled (e.g., singleton vs scoped)

At startup (in `Program.cs`), the application registers:
- the **service** (e.g., a validation service)
- the **store** via its interface (e.g., the store interface mapped to `InMemoryValidationStore`)

At runtime:
- ASP.NET Core creates the controller per request
- Injects the service/store dependencies automatically
- The controller calls into the service, which uses the store abstraction

This makes the system easy to extend while keeping construction logic out of the business code.

---

## Folder Responsibilities

### `Controllers/`
- HTTP endpoints and routing
- Request/response handling
- Delegation into services
- Status code shaping (in collaboration with `[ApiController]` conventions)

### `Models/`
- Request/response models and domain-ish types
- Simple data structures such as:
  - request DTOs (e.g., validation input)
  - status enums
  - run/result representations

### `Services/`
- Core workflow orchestration
- Validation logic coordination
- Persistence calls via store abstractions
- Business decisions (what gets recorded, returned, and how)

### `Properties/`
- Local launch configuration (`launchSettings.json`)
- Controls dev profiles, ports, and environment variables

### Root files
- `Program.cs`: application bootstrap (DI registrations, middleware pipeline, swagger setup)
- `appsettings*.json`: configuration
- `SupportValidation.Api.http`: local request scratchpad for manual endpoint testing

---

## Design Intent Summary

SVC is structured to be:
- **Support-friendly:** predictable responses, clear status codes, easy to reason about
- **Extensible:** storage can move from in-memory to SQL without rewriting controllers/services
- **Testable:** interfaces enable unit testing and isolation
- **Realistic:** mirrors common production patterns used in application support environments
