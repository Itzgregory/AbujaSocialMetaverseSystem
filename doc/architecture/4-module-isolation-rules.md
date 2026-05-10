```
Title: 4 Module Isolation Rules
Doc ID / filename: 4-module-isolation-rules.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: | Rule | Description |
Contact: oparagregory
```

**TL;DR:** | Rule | Description |


## Updated: `4_Module_Isolation_Rules.md`

---

### Module Isolation Rules

| Rule | Description |
|------|-------------|
| **Public Interface Only** | Modules only expose interfaces (not internal classes) to other modules |
| **No Circular Dependencies** | Module A cannot reference Module B if B references A |
| **Shared Infrastructure Only** | Shared components (logging, caching) live in Infrastructure layer |
| **Database Schema Separation** | Each module owns its tables (prefix: core_users, business_listings, privacy_consent) |
| **Event-Driven Cross-Module** | For loose coupling, modules communicate via events when appropriate |
| **Admin Read-Only Projections** | Admin never calls module services directly — reads only from IAdminProjection snapshots |
| **Consent Gate on Personal Data** | Any module writing personal data must check IConsentService before the write |
| **Controller-Layer Assembly** | Cross-module data assembly (e.g. CompatibilityContext) happens only at the controller layer, never inside a module |

---

**Dependency Graph:**

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|                              CORE MODULE                                            |
|                         (No dependencies on others)                                 |
|                              │      │      │                                        |
|           ┌──────────────────┘      │      └──────────────────┐                    |
|           │                         │                         │                    |
|           ▼                         ▼                         ▼                    |
|   ┌───────────────┐         ┌───────────────┐         ┌───────────────┐            |
|   │   BUSINESS    │         │    SOCIAL     │         │     MAP       │            |
|   │   MODULE      │         │    MODULE     │         │    MODULE     │            |
|   │               │         │               │         │               │            |
|   │ Depends on:   │         │ Depends on:   │         │ Depends on:   │            |
|   │ • Core        │         │ • Core        │         │ • Core        │            |
|   │ • Map         │         │ • Privacy     │         │               │            |
|   │ • Privacy     │         │               │         │               │            |
|   └───────┬───────┘         └───────┬───────┘         └───────────────┘            |
|           │                         │                                               |
|           └─────────────┬───────────┘                                               |
|                         │                                                           |
|                         ▼                                                           |
|               ┌───────────────┐                                                    |
|               │   PAYMENT     │                                                    |
|               │   MODULE      │                                                    |
|               │               │                                                    |
|               │ Depends on:   │                                                    |
|               │ • Core        │                                                    |
|               │ • Business    │                                                    |
|               └───────────────┘                                                    |
|                                                                                     |
|   ┌───────────────────────────────────────────────────────────────────────────┐    |
|   │  PRIVACY MODULE                                                           │    |
|   │  Depends on: Core only                                                    │    |
|   │  Depended on by: Social, Business (consent gate)                          │    |
|   └───────────────────────────────────────────────────────────────────────────┘    |
|                                                                                     |
|   ADMIN MODULE reads from IAdminProjection (implemented by all modules)             |
|   It has no direct service dependency on any module                                 |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```


---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
