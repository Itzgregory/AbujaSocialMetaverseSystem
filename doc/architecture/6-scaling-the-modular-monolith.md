```
Title: 6 Scaling The Modular Monolith
Doc ID / filename: 6-scaling-the-modular-monolith.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: When the monolith needs to scale, we use:
Contact: oparagregory
```

**TL;DR:** When the monolith needs to scale, we use:

## 6. Scaling the Modular Monolith

When the monolith needs to scale, we use:

| Scaling Strategy | Implementation |
|------------------|----------------|
| **Horizontal Scaling** | Run multiple instances behind a load balancer |
| **Sticky Sessions** | SignalR requires affinity—configure load balancer for session persistence |
| **Backplane** | SignalR Redis backplane for cross-instance communication |
| **Database Scaling** | Read replicas for analytics queries |
| **Cache Scaling** | Redis cluster for distributed caching |
| **Background Jobs** | Hangfire with SQL Server for distributed background processing |

**Horizontal Scaling Diagram:**
```
┌─────────────────────────────────────────────────────────────────────────────────────┐
|                                                                                     |
|                              LOAD BALANCER (ALB/Nginx)                              |
|                                      │                                              |
|           ┌──────────────────────────┼──────────────────────────┐                   |
|           │                          │                          │                   |
|           ▼                          ▼                          ▼                   |
|  ┌─────────────────┐        ┌─────────────────┐        ┌─────────────────┐         |
|  │  Monolith       │        │  Monolith       │        │  Monolith       │         |
|  │  Instance 1     │        │  Instance 2     │        │  Instance 3     │         |
|  │                 │        │                 │        │                 │         |
|  │ • Core          │        │ • Core          │        │ • Core          │         |
|  │ • Business      │        │ • Business      │        │ • Business      │         |
|  │ • Social        │        │ • Social        │        │ • Social        │         |
|  │ • Map           │        │ • Map           │        │ • Map           │         |
|  │ • Payment       │        │ • Payment       │        │ • Payment       │         |
|  └────────┬────────┘        └────────┬────────┘        └────────┬────────┘         |
|           │                          │                          │                   |
|           └──────────────────────────┼──────────────────────────┘                   |
|                                      │                                              |
|                    ┌─────────────────┼─────────────────┐                            |
|                    │                 │                 │                            |
|                    ▼                 ▼                 ▼                            |
|           ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐              |
|           │   PostgreSQL    │ │     Redis       │ │   Redis         │              |
|           │   Primary       │ │   Cache         │ │   SignalR       │              |
|           │                 │ │                 │ │   Backplane     │              |
|           └─────────────────┘ └─────────────────┘ └─────────────────┘              |
|                                                                                     |
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
