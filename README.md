# StoreBoost Test

Your task is to implement the appointment booking feature.

**Feature Request:** As a user viewing the list of appointments, I want to be able to click on an "Available" slot to book it. Once I book a slot, its status should change to "Booked," and it should no longer be bookable.

**Acceptance Criteria:**
- A user can click on an "Available" appointment slot.
- After clicking, the backend must be updated to mark the slot as "Booked."
- The user interface must update to show the new "Booked" status without needing to refresh the page.
- Once a slot is "Booked," clicking on it again should have no effect. 


# 🧪 StoreBoost Backend Setup Summary – Technical Test Report

This document summarizes the technical backend setup process of the **StoreBoost** project using modern .NET and Clean Architecture principles. It outlines the steps taken, decisions made, and challenges encountered — useful for assessment and marking purposes.

---

## ✅ Summary of Setup Steps

### 🔹 1. Cloned Repository
- **Action**: Cloned `https://github.com/dotnetdeveloper20xx/StoreBoost`
- **Purpose**: To initialize local development based on the remote GitHub repository.

---

### 🔹 2. Structured Clean Architecture Projects
Projects Created:
- `StoreBoost.Api` – Web API entry point
- `StoreBoost.Application` – Business logic (CQRS, MediatR, Validation)
- `StoreBoost.Domain` – Domain entities and rules
- `StoreBoost.Infrastructure` – Repository implementations, services
- `StoreBoost.Persistence` – Data access layer (in-memory for now)
- `StoreBoost.Shared` – DTOs, error handling, common constants

**Goal**: Follow SOLID & Clean Architecture to promote modularity and testability.

---

### 🔹 3. Verified Project References
- **Command Used**: `dotnet list StoreBoost.Api reference`
- **Result**: Confirmed correct references to all required layers.

---

### 🔹 4. Installed Required NuGet Packages
Installed into relevant projects:
- `MediatR.Extensions.Microsoft.DependencyInjection`
- `FluentValidation`
- `FluentValidation.AspNetCore`
- `Microsoft.Extensions.DependencyInjection`

**Purpose**: Enable CQRS, request validation, and modular DI.

---

### 🔹 5. Created DI Extension Methods
Added:
- `AddApplication()` → in `StoreBoost.Application`
- `AddPersistence()` → in `StoreBoost.Persistence`

**Registered in `Program.cs`** to wire up services.

---

## ❗ Challenges & Resolutions

### 🐞 Challenge 1: `AddValidatorsFromAssembly` Not Found
- **Cause**: `FluentValidation.AspNetCore` not installed in `Api` project.
- **Fix**:
  - Installed `FluentValidation.AspNetCore` in `StoreBoost.Api`
  - Registered validators directly in `Program.cs`

### 🐞 Challenge 2: `AddPersistence()` Not Found
- **Cause**: Missing `using StoreBoost.Persistence;` in top-level `Program.cs`
- **Fix**:
  - Added required `using` directive
  - Ensured method was `public static` and correctly namespaced
  - Rebuilt the solution

---

## 📌 Current State of `Program.cs`

```
using StoreBoost.Application;
using StoreBoost.Infrastructure;
using StoreBoost.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence();
builder.Services.AddValidatorsFromAssembly(typeof(SomeValidator).Assembly);
```

---

## 🚀 Next Steps (Implementation)

1. Define `AppointmentSlot` domain model
2. Create `ISlotRepository` interface
3. Implement `InMemorySlotRepository`
4. Expose `GET /api/slots` and `POST /api/slots/{id}/book` endpoints
5. Introduce CQRS with `BookSlotCommand` and `GetSlotsQuery`
6. Add FluentValidation validators
7. Write Unit + Integration tests

---

**Result**: Solid foundational setup for a professional, scalable backend, ready for further feature implementation.

