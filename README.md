# 🧠 StoreBoost Backend – Request-Response & Architecture Guide

This document explains how each backend component in the StoreBoost appointment system works—from request entry to response output. It also covers architectural principles and how key files interact in the .NET 8 Web API backend.

---

## 🔧 API Endpoints Summary

| Endpoint | Method | Purpose |
|---------|--------|---------|
| `/api/slots` | GET | Fetch all slots (booked + available) |
| `/api/slots/available` | GET | Fetch only available slots |
| `/api/slots` | POST | Create new slot (Admin only) |
| `/api/slots/{id}/book` | POST | Book a slot by ID |
| `/api/slots/{id}/cancel` | POST | Cancel a booked slot |

Each request returns an `ApiResponse<T>` object with `success`, `message`, and `data`.

---

## 🔄 Request-Response Lifecycle

### Example: Booking a Slot (`POST /api/slots/{id}/book`)

1. **Controller: `SlotController`**
   - Receives the request.
   - Passes the `BookSlotCommand` to MediatR.

2. **Command: `BookSlotCommand.cs`**
   - Holds input data (slot ID).

3. **Handler: `BookSlotCommandHandler.cs`**
   - Fetches the slot from `ISlotRepository`.
   - Validates current booking state.
   - Updates in-memory store and sends notification.

4. **Notification: `INotificationService`**
   - Sends success or error messages (currently in-memory, extensible).

5. **Response: `ApiResponse<bool>`**
   - Success message and updated state returned to frontend.

---

## 🧱 How Each File Plays a Role

| Layer | Example File | Role |
|------|--------------|------|
| API (Controllers) | `SlotController.cs` | Accepts HTTP requests, returns responses |
| Application | `BookSlotCommandHandler.cs` | Core logic via CQRS pattern |
| Domain | `AppointmentSlot.cs` | Entity with booking logic |
| Infrastructure | `InMemorySlotRepository.cs` | Stores data in-memory (mock persistence) |
| Common | `ApiResponse.cs` | Unified response format |
| Notification | `FakeNotificationService.cs` | Sends inline system messages |

---

## 🏗️ Architecture Principles

| Principle | How it’s Used |
|-----------|---------------|
| Clean Architecture | Layers: API → Application → Domain → Infrastructure:contentReference[oaicite:0]{index=0} |
| CQRS | Write: Command + Handler, Read: Controller Query |
| SOLID | Single Responsibility in Handlers, Interfaces for DI |
| Inversion of Control | All dependencies injected via constructor |
| DTO Mapping | `AppointmentSlotDto` hides domain internals |
| FluentValidation | Validates slot creation and booking input |
| Testing | xUnit, FluentAssertions, Moq for mocking:contentReference[oaicite:1]{index=1} |

---

## 🧪 Testable System

| Unit | Covered With |
|------|--------------|
| Handlers | xUnit tests for all command logic |
| Repositories | Mocked via Moq |
| Notification | Tests simulate various message flows |
| Validation | Standalone validator tests |

---

## 🔔 Notification Mechanism

- `INotificationService` interface abstracts delivery.
- `FakeNotificationService` provides inline messages.
- Future support for: Email, SMS, Push, SignalR.

---

## 🧠 Developer Best Practices Followed

- Defensive Programming (`Guard clauses` in handlers)
- Async/await throughout for scalability
- Logging & exception middleware (extendable)
- Clean separation of concerns
- Scalable dependency injection model

---

## 💡 Example Workflow Summary

### Create Slot (`POST /api/slots`)

1. Request hits `SlotController`.
2. MediatR dispatches `CreateSlotCommand`.
3. `CreateSlotCommandHandler` creates new slot in memory.
4. Returns `ApiResponse<Guid>` with slot ID.

### Cancel Slot (`POST /api/slots/{id}/cancel`)

1. Controller dispatches `CancelSlotBookingCommand`.
2. Handler checks booking status, updates slot.
3. Sends cancellation notification.
4. Response: Success or failure message.

---

## 🚀 Ready for Production Upgrade

- Swap in-memory store for EF Core + SQL
- Swap `FakeNotificationService` for real implementation
- Add authentication & user identity tracking
- Extend `Slot` entity for more rules & states
- Add full logging + telemetry with Serilog or App Insights

---

Built with ❤️ using ASP.NET Core, Clean Architecture, CQRS, MediatR, xUnit, and FluentValidation.





# 🧠 StoreBoost Backend Request-Response & Architecture Guide

This document explains the inner workings of the StoreBoost backend, including request flows, core architecture, principles, C#/.NET-specific features, and how each file plays a role. Built using **ASP.NET Core (.NET 8)**, **CQRS**, **Clean Architecture**, and **modern C#**, the backend is modular, testable, and production-ready.

---

## ⚙️ Technologies Used

| Area          | Technology                                         |
| ------------- | -------------------------------------------------- |
| Language      | C# 12                                              |
| Framework     | ASP.NET Core Web API (.NET 8)                      |
| Architecture  | Clean Architecture + CQRS                          |
| DI            | Built-in .NET Dependency Injection                 |
| Validation    | FluentValidation                                   |
| Testing       | xUnit, Moq, FluentAssertions                       |
| Notifications | Interface-based abstraction (INotificationService) |
| Middleware    | Exception handling, logging                        |
| API Tools     | Swagger (optional), Postman collection             |

---

## 🧱 Clean Architecture Layers

### 1. **Domain**

* Contains core business logic.
* Entities like `Slot` expose behaviors (e.g., `Book()`, `Cancel()`, `IsAvailable()`).
* Free from dependencies.

### 2. **Application**

* Contains CQRS commands, queries, handlers.
* Uses `MediatR` to dispatch requests.
* Applies validation, orchestrates domain logic.

### 3. **Infrastructure**

* Implements interfaces (e.g., `ISlotRepository`, `INotificationService`).
* Currently uses in-memory persistence (easy to swap with EF Core).

### 4. **API**

* Controllers expose HTTP endpoints.
* Handles serialization, routing, model binding.
* Applies middleware and connects to the DI container.

---

## 📂 File/Component Responsibilities

### SlotsController.cs

* Exposes all endpoints under `/api/slots`
* Sends commands and queries to `IMediator`

### Slot.cs (Domain)

* Holds state: `Id`, `StartTime`, `MaxBookings`, `CurrentBookings`
* Encapsulates logic:

  * `IsAvailable()`
  * `Book()`
  * `Cancel()`

### Commands / Queries

| File                          | Purpose                               |
| ----------------------------- | ------------------------------------- |
| `GetAllSlotsQuery.cs`         | Returns all slots                     |
| `GetAvailableSlotsQuery.cs`   | Filters slots with available capacity |
| `CreateSlotCommand.cs`        | Creates new slot                      |
| `BookSlotCommand.cs`          | Books a specific slot                 |
| `CancelSlotBookingCommand.cs` | Cancels an existing booking           |

Each command/query is paired with a handler.

### Validators

* Implemented using `FluentValidation`
* Enforce required fields, date ranges, max bookings >= 1

### Handlers

* Apply business logic per command/query
* Use repositories to fetch/save data
* Return `ApiResponse<T>`

### SlotRepository (InMemory)

* Holds all slot data in a static dictionary
* Implements `ISlotRepository`

### NotificationService (Fake)

* Sends console-based messages
* Swappable with real email/SMS/SignalR

---

## 🧠 .NET / C# 12 Features

### ✅ Top-Level Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
...
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
```

* Clean and minimal startup
* DI and Middleware configured via `builder.Services`

### ✅ Records and Init-Only Setters

Used in DTOs and Immutable responses.

```csharp
public record SlotDto(Guid Id, DateTime StartTime, int MaxBookings, int CurrentBookings);
```

### ✅ Expression-bodied members & Null-coalescing assignment

Used throughout handlers and services to reduce verbosity.

### ✅ Async/Await and Task-returning APIs

All handlers and controller actions are asynchronous.

---

## 📦 Endpoint-by-Endpoint Flow

### 1. `GET /api/slots` — Get All Slots

* Controller → `GetAllSlotsQuery`
* Handler fetches from `ISlotRepository`
* Returns `List<SlotDto>`

### 2. `GET /api/slots/available` — Get Available Slots

* Filters slots where `IsAvailable()` = true
* Ensures accurate booking data

### 3. `POST /api/slots` — Create Slot

* Payload: `{ startTime, maxBookings }`
* Validated via `CreateSlotCommandValidator`
* Domain logic ensures no duplicates
* Returns new slot ID

### 4. `POST /api/slots/{id}/book`

* Validates:

  * Slot exists
  * Is not full
* Applies domain `Book()` method
* Triggers notification

### 5. `POST /api/slots/{id}/cancel`

* Validates:

  * Booking exists
* Applies `Cancel()`
* Notifies user

---

## 🔔 Notifications

* Interface: `INotificationService`
* Implementation: `FakeNotificationService`
* Used for: slot booked, full alert, cancel confirmation
* Console output, swappable

---

## 🧪 Testing Philosophy

| Component        | Approach                           |
| ---------------- | ---------------------------------- |
| Commands/Queries | Unit tested in isolation           |
| Validators       | Tested with valid/invalid payloads |
| Notifications    | Mocked using `Moq`                 |
| Repositories     | Verified through handler tests     |

---

## ✅ Developer Principles in Action

| Principle                 | How it is applied                                                                      |
| ------------------------- | -------------------------------------------------------------------------------------- |
| **SOLID**                 | Each class has one responsibility. Interfaces allow swapping (e.g., `ISlotRepository`) |
| **CQRS**                  | Split read (queries) and write (commands) operations                                   |
| **Encapsulation**         | Domain entity guards its own invariants (e.g., `Book()`)                               |
| **Dependency Injection**  | All services and handlers registered via `builder.Services`                            |
| **Defensive Programming** | Handlers check for nulls, overbooking, invalid state                                   |
| **Minimal API Surface**   | Only exposes necessary endpoints in `SlotsController`                                  |

---

## 🚀 Future Enhancements

* Add user identity (auth + per-user bookings)
* Replace in-memory with EF Core or Cosmos DB
* Push notifications via SignalR
* Scheduling (via Hangfire / Azure Functions)
* Audit logging and metrics via AppInsights
