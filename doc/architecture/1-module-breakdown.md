```
Title: 1 Module Breakdown
Doc ID / filename: 1-module-breakdown.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: **Purpose:** User identity and foundational data
Contact: oparagregory
```

**TL;DR:** **Purpose:** User identity and foundational data

## Updated: `1_Module_Breakdown.md`

### Core Module
**Purpose:** User identity and foundational data

| Component | Responsibility |
|-----------|---------------|
| User Management | Registration, profiles, preferences |
| Authentication | Login, JWT issuance, password reset |
| Session Management | Active session tracking in Redis |
| Settings Service | User mode (dating/networking/leisure), privacy settings |

**Internal Exports:**
- `IUserService` — Get user profile, update settings
- `IAuthService` — Validate tokens, check permissions
- `IAccountVerificationService` — Email verification and password reset
- `IEmailService` — Send templated emails
- `IUserRepository` — Data access for users
- `IAdminProjection` *(implemented internally)* — Publishes user metrics snapshot to Admin read store

---

### Business Module
**Purpose:** Business listings and recommendations

| Component | Responsibility |
|-----------|---------------|
| Listing Management | CRUD for business profiles, location data |
| Category Service | Business categories and types |
| Recommendation Engine | Mode-based business recommendations with scoring |
| Business Analytics | View counts, engagement metrics |

**Internal Exports:**
- `IBusinessService` — Get businesses, search by location
- `IRecommendationService` — Get personalized recommendations, get user mode
- `ICategoryService` — Get business categories
- `IAdminProjection` *(implemented internally)* — Publishes listing and revenue metrics to Admin read store

---

### Social Module
**Purpose:** Avatar interactions and real-time communication

| Component | Responsibility |
|-----------|---------------|
| Avatar Manager | Avatar state, position tracking in Redis |
| Proximity Service | Detect nearby avatars, trigger compatibility checks |
| Compatibility Engine | Evaluate a pre-assembled `CompatibilityContext` DTO — no cross-module calls made internally |
| Chat Service | Message history, real-time messaging via SignalR |

**Design Note — Compatibility Engine:**
The engine receives a `CompatibilityContext` DTO assembled upstream at the controller layer. It does not import `IUserService` or `IRecommendationService` directly. All cross-module data is resolved before the engine is invoked, keeping Social module dependencies clean.

**Internal Exports:**
- `IAvatarService` — Update position, get nearby avatars
- `IInteractionService` — Check compatibility via CompatibilityContext, log interactions
- `IChatService` — Send/receive messages
- `IAdminProjection` *(implemented internally)* — Publishes interaction and proximity metrics to Admin read store

---

### Map Module
**Purpose:** Geospatial data and map integration

| Component | Responsibility |
|-----------|---------------|
| Tile Service | Fetch and cache map tiles from Mapbox |
| Geocoding Service | Convert addresses to coordinates |
| POI Manager | Points of interest, business pin placement |
| Location Service | Distance calculations, bounding box queries |

**Internal Exports:**
- `IMapService` — Get map tiles, geocode addresses
- `ILocationService` — Calculate distances, filter by radius

---

### Payment Module
**Purpose:** Business subscriptions and transactions

| Component | Responsibility |
|-----------|---------------|
| Subscription Manager | Business plan management, billing cycles |
| Transaction Service | Process payments via Stripe/Paystack |
| Invoice Service | Generate and email invoices |
| Plan Service | Available subscription tiers |

**Internal Exports:**
- `ISubscriptionService` — Manage business subscriptions
- `IPaymentService` — Process payments, handle webhooks
- `IAdminProjection` *(implemented internally)* — Publishes revenue and subscription metrics to Admin read store

---

### Admin Module
**Purpose:** Platform management and moderation

**Design Note — Decoupled via Projections:**
Admin no longer directly imports or calls into other module services. Instead, each module implements the shared `IAdminProjection` interface internally and publishes a read-only `AdminMetricSnapshot`. The Admin module receives all projections via DI as a collection and aggregates them. This eliminates the cross-cutting coupling smell while preserving full visibility across the platform.

| Component | Responsibility |
|-----------|---------------|
| Dashboard Service | Aggregates `IAdminProjection` snapshots from all modules |
| Moderation Service | Flagged content, user reports |
| Business Approval | Verify new business listings |
| System Monitoring | Health checks, performance metrics |

**Internal Exports:**
- `IAdminService` — Admin-only operations
- `IModerationService` — Content review, user actions

---

### Privacy Module *(New)*
**Purpose:** NDPA 2023 compliance, consent management, and data subject rights

| Component | Responsibility |
|-----------|---------------|
| Consent Service | Record and verify lawful basis for data processing per user |
| Retention Service | Enforce data lifecycle rules across all modules |
| Data Subject Service | Handle access requests, deletions, and data exports |
| Audit Log Service | Immutable record of all personal data processing events |

**Design Note:**
This module is a platform-wide cross-cutting concern. All modules that process personal data must check consent before writing. The retention service runs as a nightly Hangfire background job. Deletion requests trigger a cascade across PostgreSQL, Redis, and S3.

**Internal Exports:**
- `IConsentService` — Check and record user consent per data category
- `IRetentionService` — Register and enforce retention policies
- `IDataSubjectService` — Execute access, portability, and erasure requests
- `IAuditLogService` — Append-only processing log


---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
