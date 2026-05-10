```
Title: 3 Entity Relationship Diagram Erd
Doc ID / filename: 3-entity-relationship-diagram-erd.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: ┌─────────────────────────────────────────────────────────────────────────────────────────────┐
Contact: oparagregory
```

**TL;DR:** ┌─────────────────────────────────────────────────────────────────────────────────────────────┐

┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                             │
│  ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐                        │
│  │     User        │     │   UserSetting   │     │    Interest     │                        │
│  ├─────────────────┤     ├─────────────────┤     ├─────────────────┤                        │
│  │ PK │ id         │─────│ FK │ user_id    │     │ PK │ id         │                        │
│  │    │ email      │     │    │ open_to_   │     │    │ name       │                        │
│  │    │ password   │     │    │ networking │     └─────────────────┘                        │
│  │    │ first_name │     │    │ open_to_   │              ▲                                 │
│  │    │ last_name  │     │    │ friends    │              │                                 │
│  │    │ birth_date │     │    │ open_to_   │              │                                 │
│  │    │ current_   │     │    │ dating     │              │                                 │
│  │    │ mode       │     │    │ max_radius │              │                                 │
│  │    │ avatar_    │     │    │            │              │                                 │
│  │    │ config     │     │    └────────────┘              │                                 │
│  │    │ created_at │     │                               │                                 │
│  │    │ last_active│     │                               │                                 │
│  └────────┬────────┘     │                               │                                 │
│           │              │                               │                                 │
│           │              │                               │                                 │
│           │              │     ┌─────────────────────────┴───────────────────────┐         │
│           │              │     │                                                 │         │
│           │              │     ▼                                                 │         │
│           │              │  ┌─────────────────────┐     ┌─────────────────────┐   │         │
│           │              │  │   UserInterest     │     │   BusinessInterest  │   │         │
│           │              │  ├─────────────────────┤     ├─────────────────────┤   │         │
│           │              └──│ FK │ user_id       │     │ FK │ business_id    │   │         │
│           │                 │ FK │ interest_id   │     │ FK │ interest_id    │   │         │
│           │                 └─────────────────────┘     └─────────────────────┘   │         │
│           │                              │                        │                │         │
│           │                              │                        │                │         │
│           ▼                              ▼                        ▼                │         │
│  ┌─────────────────┐     ┌─────────────────────────────────────────────────┐       │         │
│  │   Session       │     │                   Business                      │       │         │
│  ├─────────────────┤     ├─────────────────────────────────────────────────┤       │         │
│  │ PK │ id         │     │ PK │ id                                          │       │         │
│  │ FK │ user_id    │     │    │ name                                        │       │         │
│  │    │ token      │     │    │ description                                 │       │         │
│  │    │ created_at │     │    │ category                                    │       │         │
│  │    │ expires_at │     │    │ business_type  (dating/networking/leisure) │       │         │
│  └─────────────────┘     │    │ latitude                                    │       │         │
│                          │    │ longitude                                   │       │         │
│                          │    │ address                                     │       │         │
│  ┌─────────────────┐     │    │ contact_email                               │       │         │
│  │  ActiveUser     │     │    │ contact_phone                               │       │         │
│  │  Location       │     │    │ rating                                      │       │         │
│  ├─────────────────┤     │    │ is_active                                   │       │         │
│  │ PK │ user_id    │     │    │ created_at                                  │       │         │
│  │    │ session_id │     │    │ updated_at                                  │       │         │
│  │    │ latitude   │     │    └─────────────────────────────────────────────┘       │         │
│  │    │ longitude  │     │                              │                          │         │
│  │    │ current_   │     │                              │                          │         │
│  │    │ mode       │     │                              │                          │         │
│  │    │ last_update│     │                              │                          │         │
│  └─────────────────┘     │                              │                          │         │
│                          │                              │                          │         │
│                          │                              ▼                          │         │
│                          │     ┌─────────────────────────────────────────────────┐   │         │
│                          │     │              BusinessAnalytics                  │   │         │
│                          │     ├─────────────────────────────────────────────────┤   │         │
│                          │     │ PK │ id                                         │   │         │
│                          │     │ FK │ business_id                                │   │         │
│                          │     │    │ date                                       │   │         │
│                          │     │    │ views                                      │   │         │
│                          │     │    │ clicks                                     │   │         │
│                          │     │    │ engagements                                │   │         │
│                          │     │    │ avg_visit_duration                         │   │         │
│                          │     └─────────────────────────────────────────────────┘   │         │
│                          │                                                          │         │
│                          │                                                          │         │
│                          ▼                                                          │         │
│  ┌─────────────────────────────────────────────────────────────────────────────┐    │         │
│  │                         SocialInteraction                                   │    │         │
│  ├─────────────────────────────────────────────────────────────────────────────┤    │         │
│  │ PK │ id                                                                     │    │         │
│  │ FK │ user_id_initiator                                                      │    │         │
│  │ FK │ user_id_responder                                                      │    │         │
│  │    │ interaction_type  (chat / voice / meetup)                              │    │         │
│  │    │ initiated_at                                                           │    │         │
│  │    │ ended_at                                                               │    │         │
│  │    │ duration_seconds                                                       │    │         │
│  │    │ was_successful                                                         │    │         │
│  └─────────────────────────────────────────────────────────────────────────────┘    │         │
│                                                                                      │         │
│  Legend:                                                                             │         │
│  ──── One-to-Many                                                                   │         │
│  ──── Many-to-Many                                                                  │         │
│  PK = Primary Key                                                                   │         │
│  FK = Foreign Key                                                                   │         │
│                                                                                      │         │
└──────────────────────────────────────────────────────────────────────────────────────┘         │

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
