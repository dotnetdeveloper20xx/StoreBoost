# StoreBoost Test

Your task is to implement the appointment booking feature.

**Feature Request:** As a user viewing the list of appointments, I want to be able to click on an "Available" slot to book it. Once I book a slot, its status should change to "Booked," and it should no longer be bookable.

**Acceptance Criteria:**
- A user can click on an "Available" appointment slot.
- After clicking, the backend must be updated to mark the slot as "Booked."
- The user interface must update to show the new "Booked" status without needing to refresh the page.
- Once a slot is "Booked," clicking on it again should have no effect. 

## ✅ Completed So Far

### 📆 Slot Management
- Create appointment slots with custom start time and maximum booking capacity  
- Prevent overlapping slots (enforced 30-minute interval)  
- Track current bookings for each slot  

### 🧍‍♂️ User Booking Flow
- View all slots via `GET /api/slots`  
- View only available slots via `GET /api/slots/available`  
- Book available slots via `POST /api/slots/{id}/book`  
- Prevent overbooking  
- Cancel bookings via `POST /api/slots/{id}/cancel`  
- Prevent cancellation of unbooked slots  

### 🛎️ Notification System
- Inline notifications for:  
  - Booking confirmation  
  - Slot full warning  
  - Cancellation confirmation  
- Implemented `INotificationService` abstraction (easily replaceable with real services like email/SMS)  

### 🧪 Testing & Architecture
- Clean Architecture (Domain, Application, Infrastructure, API)  
- CQRS with MediatR  
- Dependency Injection  
- Validators with FluentValidation  
- Unit tests using xUnit, Moq, FluentAssertions  
- Postman collection to test all endpoints  

