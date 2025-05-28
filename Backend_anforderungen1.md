### Complete Requirements Specification (English Version)  
*(ready to copy into an Epic or a set of backlog items)*  

---

## 1 – Back-end Goals
| # | Purpose |
|---|---------|
| 1 | Persist all calorie data and goal settings. |
| 2 | Centralise **all** calculations – the front-end only sends raw input data. |
| 3 | Provide a clean REST API (easy to extend to gRPC/GraphQL later). |
| 4 | Deliver strong validation and meaningful error codes/messages. |
| 5 | Be multi-user ready (JWT/OAuth can be switched on later). |
| 6 | Enable automated tests and CI pipelines with minimal friction. |

> **Code quality note:**  
> *Keep the solution layered and well organised – e.g.*  
> `Domain/`, `Application/`, `Infrastructure/`, `API/`.  
> Models (entities/DTOs), services, controllers, and the `DbContext` **must** follow a clear, consistent structure and naming scheme.

---

## 2 – Domain & Data Model

### 2.1 Entities
| Entity | Fields (type / constraints) | Notes |
|--------|---------------------------|-------|
| **Goal** | • `GoalId` (Guid, PK)<br>• `UserId` (Guid, FK)<br>• `TargetKcals` (int, non-zero)<br>• `TimeWindowDays` (int > 0 ≤ 3650)<br>• `StartDate` (date, ≥ today) | Only **one** active record per user; previous goals are archived. |
| **DailyLog** | • `DailyLogId` (Guid, PK)<br>• `UserId` (Guid, FK)<br>• `Date` (date, unique together with `UserId`)<br>• `KcalsBurn` (int ≥ 0)<br>• `KcalsIntake` (int ≥ 0) | Calculated fields are added on the fly (see 2.2). |

### 2.2 Calculated Fields (not stored, returned by the API)
* `KcalsDiff` = Burn – Intake  
* `SumDiffs` = running sum of `KcalsDiff` up to this day  
* `GoalDelta` = `TargetKcals – SumDiffs`  
  * Deficit goal ⇒ `TargetKcals > 0`, reached when `SumDiffs ≥ TargetKcals`  
  * Surplus goal ⇒ `TargetKcals < 0`, reached when `SumDiffs ≤ TargetKcals`  
* `Avg4Days`, `Avg7Days` = rolling averages  
* `AvgAll` = average since day 1  
* `DayNum` = 1-based index relative to `StartDate`

---

## 3 – API End-points (Version v1)

| Method | Path | Purpose | Request | Successful Response (200) |
|--------|------|---------|---------|---------------------------|
| **POST** | `/api/v1/goals` | Create a new goal | `{ targetKcals, timeWindowDays, startDate }` | `GoalDto` |
| **GET** | `/api/v1/goals/active` | Retrieve the active goal | – | `GoalDto` |
| **PUT** | `/api/v1/goals/{goalId}` | Update a goal (soft-lock if logs exist) | `{ … }` | `GoalDto` |
| **GET** | `/api/v1/dailylogs` | List logs (paged) incl. calculated fields | `?page,size,from,to` | `Paged<DailyLogDto>` |
| **POST** | `/api/v1/dailylogs` | Create a single day | `{ date, kcalsBurn, kcalsIntake }` | `DailyLogDto` |
| **PUT** | `/api/v1/dailylogs/{id}` | Update a day | `{ … }` | `DailyLogDto` |
| **DELETE** | `/api/v1/dailylogs/{id}` | Delete a day | – | `204 No Content` |
| **POST** | `/api/v1/dailylogs/bulk` | Bulk import CSV/JSON (optional) | `[ … ]` | `207 Multi-Status` |

*DTOs always include the calculated properties; POST/PUT accept only writable fields.*

---

## 4 – Business Rules & Validation

1. **Uniqueness** – Only one `DailyLog` per user per `Date`.  
2. **Date range** – `Date` must fall between `StartDate` and `StartDate + TimeWindowDays - 1`.  
3. **Immutability** – Changing/deleting a goal triggers automatic recalculation of all logs.  
4. **Overflow protection** – Return numeric results as `decimal(18,2)`.  
5. **Concurrency** – Use optimistic concurrency tokens (`RowVersion`).  
6. **Error codes**  
   * `400 Bad Request` – validation errors (return field details)  
   * `404 Not Found` – ID not found or not owned by user  
   * `409 Conflict` – duplicate date or concurrency conflict  
   * `500 Server Error` – unhandled exceptions (logged, no stack trace leaked)  

---

## 5 – Technology Stack & Layers

```
┌── API Layer (Controllers + Swagger)
│
├── Application Layer
│    • Commands / Queries (MediatR)
│    • Validation (FluentValidation)
│
├── Domain Layer
│    • Entities & Value Objects
│    • Services: CalorieCalculatorService
│
├── Infrastructure Layer
│    • EF Core 8  (SQLite Dev, PostgreSQL Prod)
│    • Identity / JWT (switchable)
│    • Logging (Serilog + Seq)
│
└── Tests
     • xUnit + FluentAssertions
     • In-memory DB for unit tests
```

> **Structure requirement**: keep folders/namespaces aligned with the layers;  
> `CalorieCalculatorService` must be purely functional and unit-tested.

---

## 6 – Error & Exception Handling

* **Global exception middleware** – converts unhandled exceptions to generic 500 responses and logs the stack trace.  
* **Model-state filter** – returns structured 400 payload (`title`, `detail`, `errors{ field:[msg] }`).  
* **Validation** – class-level attributes + FluentValidation in CQRS handlers.  
* **Retry policy** (Polly) for transient DB errors.  

---

## 7 – Security & Rate Limiting

* JWT authentication ready (can be disabled for the MVP).  
* User ID is taken from the JWT – no cross-account data access.  
* Basic ASP.NET rate-limiting middleware to prevent abuse.

---

## 8 – Non-functional Requirements

| Category | Target | Note |
|----------|--------|------|
| Performance | < 200 ms for GET / < 250 ms for POST (95th percentile) | |
| Scalability | 10 requests/s (MVP) – must scale horizontally | |
| Availability | 99 % (MVP) | |
| Localisation | Numeric format `en-US` (dot separator) | |
| Observability | Serilog JSON logs + OpenTelemetry traces | |
| API Docs | Auto-generated Swagger / OpenAPI 3.0 | |

---

## 9 – Testing Strategy

1. **Unit tests** – Domain & Application layers (≥ 90 % for `CalorieCalculatorService`).  
2. **Integration tests** – `WebApplicationFactory` + in-memory SQLite.  
3. **Contract tests** – OpenAPI snapshot + Schemathesis.  
4. **End-to-end** – Playwright can follow later.

---

## 10 – Deployment & Ops

* **Containers** – Distroless Dockerfile; `docker-compose.dev` (API, DB, Seq).  
* **Migrations** – `dotnet ef migrations`.  
* **CI** – GitHub Actions: build, test, publish image.  
* **CD** – Azure Web App or AWS Fargate (future).

---

### Summary
* **Store only raw inputs**; all derived metrics are calculated live in `CalorieCalculatorService`.  
* Provide calculated values in every API response.  
* Ensure a clean project structure: models, services, controllers, and `DbContext` are logically separated and follow SOLID principles.
