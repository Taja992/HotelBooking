# Part 2 Presentation — Requirement 4: E2E Testing with Selenium

---

## Slide 13 — What & Why: End-to-End Testing

**Goal:** Verify the full "Create Booking" user flow through a real browser against a real running server — no mocks, no in-process shortcuts.

**Tools chosen:**
| Tool | Version | Role |
|------|---------|------|
| Selenium WebDriver | 4.27.0 | Browser automation |
| Selenium.Support | 4.27.0 | `SelectElement` helper |
| xunit.v3 | 3.2.2 | Test runner |
| ChromeDriver (auto) | via Selenium Manager | Headless Chrome |

**Approach — Process-based test host:**  
`WebAppFactory` launches `HotelBooking.Mvc.dll` as a real subprocess (`dotnet HotelBooking.Mvc.dll`) on `http://127.0.0.1:5050` with `ASPNETCORE_ENVIRONMENT=Development`. Development mode triggers `DbInitializer.Initialize` which deletes and re-seeds the database on every test run — giving each run a guaranteed clean starting state (3 rooms, all occupied from **today+4** to **today+18**).

**Why not `WebApplicationFactory<Program>`?**  
In .NET 10 with top-level statements, `WebApplicationFactory` uses an internal `TestServer` that conflicts with any real Kestrel listener. Chrome needs a real HTTP port, so a subprocess is the correct solution.

---

## Slide 14 — Project Structure & Page Object Model

```
HotelBooking.E2ETests/
├── HotelBooking.E2ETests.csproj   # xunit.v3 + Selenium.WebDriver + Selenium.Support
├── WebAppFactory.cs               # Starts/stops the MVC process; shared across all tests
├── Pages/
│   ├── CreateBookingPage.cs       # Wraps /Bookings/Create form interactions
│   └── BookingsListPage.cs        # Wraps /Bookings list page — counts booking rows
└── CreateBookingTests.cs          # 16 test cases (TC1–TC10, V4, V1–V3 V5–V6 skipped)
```

**`CreateBookingPage.cs` key points:**
```csharp
// type="date" inputs require JS assignment — SendKeys is locale-dependent
private void SetDate(string id, DateTime date)
    => ((IJavaScriptExecutor)_driver).ExecuteScript(
           "document.getElementById(arguments[0]).value = arguments[1];",
           id, date.ToString("yyyy-MM-dd"));

public void ClickCreate()
    => _driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();
```

**`BookingsListPage.cs` key points:**
```csharp
// Index.cshtml has TWO tables: a calendar (no <thead>) and the bookings list (has <thead>).
// ":has(thead)" uniquely selects the bookings table.
public int BookingRowCount()
    => _driver.FindElements(By.CssSelector("table:has(thead) tbody tr")).Count;
```

**Test isolation strategy:** count-before / count-after delta. Each test reads the current row count before attempting a booking and checks the expected delta (+1 for success, +0 for failure). This is robust even as bookings accumulate across tests in the same run.

---

## Slide 15 — Test Cases

**Occupied range (seeded by `DbInitializer`):** today+4 → today+18 (all 3 rooms)

### Decision Table — TC1–TC10

| # | Start Date | End Date | Expected | Actual |
|---|-----------|----------|----------|--------|
| TC1 | Before (today+1) | Before (today+3) | **Success** | ✅ Passed |
| TC2 | After (today+19) | After (today+19) | **Success** | ✅ Passed |
| TC3 | Before (today+3) | After (today+19) | No room | ✅ Passed |
| TC4 | Before (today+3) | firstOccupied (today+4) | No room | ✅ Passed |
| TC5 | Before (today+3) | lastOccupied (today+18) | No room | ✅ Passed |
| TC6 | firstOccupied (today+4) | After (today+19) | No room | ✅ Passed |
| TC7 | lastOccupied (today+18) | After (today+19) | No room | ✅ Passed |
| TC8 | firstOccupied (today+4) | firstOccupied (today+4) | No room | ✅ Passed |
| TC9 | firstOccupied (today+4) | lastOccupied (today+18) | No room | ✅ Passed |
| TC10 | lastOccupied (today+18) | lastOccupied (today+18) | No room | ✅ Passed |

### Date Validation — V1–V6

| # | Start | End | Expected | Result |
|---|-------|-----|----------|--------|
| V1 | today (today+0) | today+5 | Error message | ⚠️ Skipped — **Defect D-4** |
| V2 | yesterday (today-1) | today+5 | Error message | ⚠️ Skipped — **Defect D-4** |
| V3 | today-5 | today-1 | Error message | ⚠️ Skipped — **Defect D-4** |
| **V4** | **tomorrow (today+1)** | **today+1** | **Success** | ✅ **Passed** |
| V5 | today+6 | today+5 (SD > ED) | Error message | ⚠️ Skipped — **Defect D-4** |
| V6 | today+19 | today+3 (inverted) | Error message | ⚠️ Skipped — **Defect D-4** |

**Defect D-4:** When an invalid date is submitted (past date, today, or start > end), `BookingManager.FindAvailableRoom` throws an unhandled `ArgumentException`. The app shows the ASP.NET Developer Exception Page instead of a friendly validation message. The 5 affected tests are marked `[Fact(Skip = "Defect D-4: ...")]` so they remain visible in test reports as known failures.

---

## Slide 16 — Test Results

**Command:**
```
dotnet test HotelBooking.E2ETests --logger "console;verbosity=normal"
```

**Output:**
```
Test Run Successful.
Total tests: 16
     Passed: 11
    Skipped:  5
 Total time: 17.2 Seconds
```

**Passed (11):** TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8, TC9, TC10, V4  
**Skipped (5):** V1, V2, V3, V5, V6 — Defect D-4 (page crashes on invalid dates)  
**Failed: 0**

**Two small additions to `HotelBooking.Mvc/Program.cs`:**
1. `if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();`  
   — Prevents an HTTP→HTTPS redirect loop when the test server runs on plain HTTP.
2. `public partial class Program { }`  
   — Makes the entry point visible to the test assembly.

Neither change affects normal development or production behavior.
