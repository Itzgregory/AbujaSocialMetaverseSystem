```
Title: 6 Real Time Avatar Synchronization Sequence Diagram
Doc ID / filename: 6-real-time-avatar-synchronization-sequence-diagram.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────┐
Contact: oparagregory
```

**TL;DR:** ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────┐

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                         │
│                    REAL-TIME AVATAR SYNCHRONIZATION SEQUENCE DIAGRAM                                    │
│                                                                                                         │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                         │
│  ┌──────────┐  ┌──────────┐  ┌───────────────────────┐  ┌──────────────────┐  ┌──────────────────┐     │
│  │ User A   │  │ Client A │  │ SignalR (AvatarHub)    │  │ Client B         │  │ Client C         │     │
│  │          │  │ (Unity)  │  │ embedded in monolith   │  │ (Unity)          │  │ (Unity)          │     │
│  └────┬─────┘  └────┬─────┘  └──────────┬────────────┘  └────────┬─────────┘  └────────┬─────────┘     │
│       │              │                  │                         │                     │               │
│       │  Move Avatar │                  │                         │                     │               │
│       │─────────────►│                  │                         │                     │               │
│       │              │                  │                         │                     │               │
│       │              │  SendPosition    │                         │                     │               │
│       │              │  (x, y, z)       │                         │                     │               │
│       │              │─────────────────►│                         │                     │               │
│       │              │                  │                         │                     │               │
│       │  Local Move  │                  │                         │                     │               │
│       │  Rendered    │                  │                         │                     │               │
│       │◄─────────────│                  │                         │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Update Redis           │                     │               │
│       │              │                  │  (User A location)      │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Get region subscribers │                     │               │
│       │              │                  │  from Redis             │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Broadcast to Region    │                     │               │
│       │              │                  │  Group (nearby only)    │                     │               │
│       │              │                  │────────────────────────►│                     │               │
│       │              │                  │────────────────────────────────────────────►│               │
│       │              │                  │                         │                     │               │
│       │              │                  │                         │  Interpolate        │               │
│       │              │                  │                         │  + Render User A    │               │
│       │              │                  │                         │                     │               │
│       │              │                  │                         │                     │  Interpolate  │
│       │              │                  │                         │                     │  + Render     │
│       │              │                  │                         │                     │  User A       │
│       │              │                  │                         │                     │               │
│  ═════╪══════════════╪══════════════════╪═════════════════════════╪═════════════════════╪═══════════════╪
│       │              │                  │                         │                     │               │
│       │              │                  │  Proximity threshold    │                     │               │
│       │              │                  │  crossed: User A & B    │                     │               │
│       │              │                  │  within 20m             │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  POST /api/social/      │                     │               │
│       │              │                  │  compatibility          │                     │               │
│       │              │                  │  (HTTP call to backend) │                     │               │
│       │              │                  │─────────────────────────────────────────────────────────────►│
│       │              │                  │                         │                     │               │
│       │              │                  │  SocialController       │                     │               │
│       │              │                  │  assembles              │                     │               │
│       │              │                  │  CompatibilityContext   │                     │               │
│       │              │                  │  (Core + Business data) │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  CompatibilityEngine    │                     │               │
│       │              │                  │  evaluates context      │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Score > Threshold?     │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Result returned        │                     │               │
│       │              │                  │◄─────────────────────────────────────────────────────────────│
│       │              │                  │                         │                     │               │
│       │              │                  │  Notify both users      │                     │               │
│       │              │                  │  via SignalR            │                     │               │
│       │              │                  │────────────────────────►│                     │               │
│       │              │  Match           │                         │                     │               │
│       │              │  Notification    │                         │  Match              │               │
│       │              │◄─────────────────│                         │  Notification       │               │
│       │              │                  │                         │◄────────────────────│               │
│       │              │                  │                         │                     │               │
│       │  View Match  │                  │                         │                     │               │
│       │  Prompt      │                  │                         │                     │               │
│       │◄─────────────│                  │                         │                     │               │
│       │              │                  │                         │                     │               │
│       │  Accept      │                  │                         │                     │               │
│       │  Interaction │                  │                         │                     │               │
│       │─────────────►│                  │                         │                     │               │
│       │              │  AcceptMatch()   │                         │                     │               │
│       │              │─────────────────►│                         │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Create ChatHub         │                     │               │
│       │              │                  │  session: chat_123      │                     │               │
│       │              │                  │                         │                     │               │
│       │              │                  │  Add both clients       │                     │               │
│       │              │                  │  to session group       │                     │               │
│       │              │  Join chat_123   │                         │  Join chat_123      │               │
│       │              │◄─────────────────│                         │◄────────────────────│               │
│       │              │                  │                         │                     │               │
│       │  Chat        │                  │                         │                     │               │
│       │  Message     │                  │                         │                     │               │
│       │─────────────►│                  │                         │                     │               │
│       │              │  SendMessage()   │                         │                     │               │
│       │              │─────────────────►│                         │                     │               │
│       │              │                  │  Relay to session group │                     │               │
│       │              │                  │────────────────────────►│                     │               │
│       │              │                  │                         │  Display Message    │               │
│       │              │                  │                         │  to User B          │               │
│       │              │                  │                         │                     │               │
│       ▼              ▼                  ▼                         ▼                     ▼               │
│                                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────┘


---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
