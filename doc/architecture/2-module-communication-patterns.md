```
Title: 2 Module Communication Patterns
Doc ID / filename: 2-module-communication-patterns.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: Modules communicate through **C# interfaces and dependency injection**, not HTTP calls.
Contact: oparagregory
```

**TL;DR:** Modules communicate through **C# interfaces and dependency injection**, not HTTP calls.

## Updated: `2_Module_Communication_Patterns.md`

---

### Internal Communication (Within the Monolith)

Modules communicate through **C# interfaces and dependency injection**, not HTTP calls.

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|   RECOMMENDATION FLOW (Internal Method Calls)                                       |
|                                                                                     |
|   ┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐  |
|   │   API        │     │   Core       │     │   Business   │     │   Map        │  |
|   │   Controller │────►│   Module     │────►│   Module     │────►│   Module     │  |
|   │              │     │              │     │              │     │              │  |
|   │ /api/rec/    │     │ Get User     │     │ Get Business │     │ Calculate    │  |
|   │ nearby       │     │ Settings     │     │ by Mode      │     │ Distance     │  |
|   └──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘  |
|                                                                                     |
|   All calls are in-process. No network latency. Single transaction context.         |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

### Compatibility Check Flow *(New)*

The controller layer is the only place allowed to call across multiple modules. It assembles all required data into a `CompatibilityContext` DTO before passing it down to the Social module. The compatibility engine itself has zero cross-module dependencies.

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|   COMPATIBILITY CHECK FLOW                                                          |
|                                                                                     |
|   ┌──────────────────────────────────────────────────────────────────────────────┐  |
|   │  SocialController (API Layer)                                                │  |
|   │                                                                              │  |
|   │  1. IUserService.GetPreferences(userAId)          ← Core Module             │  |
|   │  2. IUserService.GetPreferences(userBId)          ← Core Module             │  |
|   │  3. IRecommendationService.GetUserMode(userAId)   ← Business Module         │  |
|   │  4. IRecommendationService.GetUserMode(userBId)   ← Business Module         │  |
|   │  5. Assembles CompatibilityContext DTO                                       │  |
|   │  6. IInteractionService.CheckCompatibility(context) ← Social Module only    │  |
|   └──────────────────────────────────────────────────────────────────────────────┘  |
|                                                                                     |
|   CompatibilityEngine (inside Social Module) receives only the DTO.                 |
|   It never imports Core or Business interfaces directly.                            |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

```csharp
// Shared/Models/CompatibilityContext.cs
public record CompatibilityContext(
    Guid UserAId,
    Guid UserBId,
    SocialMode UserAMode,
    SocialMode UserBMode,
    List<string> UserAInterests,
    List<string> UserBInterests,
    bool UserAOpenToNetworking,
    bool UserBOpenToNetworking
);
```

---

### Admin Aggregation Flow *(New)*

Admin never calls into module services directly. Each module implements `IAdminProjection` internally and exposes a read-only snapshot. Admin receives all projections via DI and aggregates them.

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|   ADMIN DASHBOARD FLOW                                                              |
|                                                                                     |
|   CoreModule     ──► CoreAdminProjection.GetSnapshot()     ──►  ┐                  |
|   BusinessModule ──► BusinessAdminProjection.GetSnapshot() ──►  │                  |
|   SocialModule   ──► SocialAdminProjection.GetSnapshot()   ──►  ├── DashboardService|
|   PaymentModule  ──► PaymentAdminProjection.GetSnapshot()  ──►  │                  |
|                                                                  ┘                  |
|   AdminModule reads ONLY from IAdminProjection implementations.                     |
|   No direct dependency on IUserService, IBusinessService, etc.                     |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

```csharp
// Shared/Contracts/IAdminProjection.cs
public interface IAdminProjection
{
    string ModuleName { get; }
    AdminMetricSnapshot GetSnapshot();
}

// Shared/Models/AdminMetricSnapshot.cs
public record AdminMetricSnapshot(
    string ModuleName,
    Dictionary<string, object> Metrics,
    DateTime GeneratedAt
);
```

---

### Consent Gate Flow *(New)*

Before any module writes personal data, it checks consent via the Privacy module. This is a single method call, not a redesign of the module's internals.

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|   LOCATION DATA WRITE (Social Module — Avatar Position Update)                      |
|                                                                                     |
|   ┌──────────────────────────────────────────────────────────────────────────────┐  |
|   │  AvatarService.UpdatePosition(userId, position)                              │  |
|   │                                                                              │  |
|   │  1. IConsentService.HasConsent(userId, DataCategory.Location)                │  |
|   │     └── if false: throw ConsentRequiredException, do not write               │  |
|   │     └── if true: continue                                                    │  |
|   │  2. LocationCache.Set(userId, position)   ← Redis write                     │  |
|   │  3. IAuditLogService.Log(userId, "location_written", position)               │  |
|   └──────────────────────────────────────────────────────────────────────────────┘  |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

### External Communication (Client to Server)

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|   HTTP/HTTPS (REST) - For most operations                                           |
|   ┌──────────────┐                    ┌──────────────────────────────────────────┐  |
|   │   Unity      │  POST /api/auth/login   │           Monolith                  │  |
|   │   Client     │─────────────────────────►│   Authentication Module           │  |
|   │              │                         │                                    │  |
|   └──────────────┘  GET /api/business/nearby│   Business Module                 │  |
|                     ────────────────────────►│                                    │  |
|                                              └────────────────────────────────────┘  |
|                                                                                     |
|   WebSocket (SignalR) - For real-time                                              |
|   ┌──────────────┐                    ┌──────────────────────────────────────────┐  |
|   │   Unity      │  WebSocket Connect    │           Monolith                    │  |
|   │   Client     │─────────────────────►│   SignalR Hub                         │  |
|   │              │                       │                                        │  |
|   │              │  SendPosition()       │   Social Module                       │  |
|   │              │──────────────────────►│   • Consent check via PrivacyModule   │  |
|   │              │                       │   • Update Redis                      │  |
|   │              │                       │   • Broadcast to nearby               │  |
|   │              │                       │                                        │  |
|   │              │  PositionUpdate()     │                                        │  |
|   │              │◄──────────────────────│                                        │  |
|   └──────────────┘                       └────────────────────────────────────────┘  |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
