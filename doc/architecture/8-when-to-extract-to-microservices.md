```
Title: 8 When To Extract To Microservices
Doc ID / filename: 8-when-to-extract-to-microservices.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: If the monolith grows and specific modules need independent scaling, we can extract:
Contact: oparagregory
```

**TL;DR:** If the monolith grows and specific modules need independent scaling, we can extract:

## 8. When to Extract to Microservices

If the monolith grows and specific modules need independent scaling, we can extract:

| Module | Extraction Trigger | Extraction Path |
|--------|-------------------|-----------------|
| **Recommendation Engine** | High CPU usage affecting other modules | Extract to dedicated service with GPU/ML instances |
| **Payment Module** | PCI compliance requirements | Extract to isolated service with stricter security |
| **Social Module** | Real-time traffic spikes | Extract to dedicated WebSocket service |
| **Admin Module** | Internal vs external traffic separation | Extract to internal network only |

**Extraction Path:**
```
Phase 1: Modular Monolith
┌─────────────────────────────────────────────────────────────────┐
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐                │
│  │ Core │ │ Bus. │ │ Soc. │ │ Map  │ │ Pay  │                │
│  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘                │
│         All in one process, one database                       │
└─────────────────────────────────────────────────────────────────┘

Phase 2: Extract via Interfaces (Still same process)
┌─────────────────────────────────────────────────────────────────┐
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐                │
│  │ Core │ │ Bus. │ │ Soc. │ │ Map  │ │ Pay  │                │
│  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘                │
│  Internal HTTP calls (same process, different ports)           │
└─────────────────────────────────────────────────────────────────┘

Phase 3: Extract to Microservices
┌─────────────────────────────────────────────────────────────────┐
│  ┌──────┐   ┌──────┐   ┌──────┐   ┌──────┐   ┌──────┐        │
│  │ Core │──▶│ Bus. │──▶│ Soc. │──▶│ Map  │──▶│ Pay  │        │
│  └──────┘   └──────┘   └──────┘   └──────┘   └──────┘        │
│  Network calls, separate databases, separate deploys           │
└─────────────────────────────────────────────────────────────────┘
```

---

## Summary

The **Modular Monolith** architecture gives us:

✅ **Simple deployment** — One application to build, test, and deploy  
✅ **Separation of concerns** — Modules with clear boundaries  
✅ **Low operational complexity** — No service discovery, no distributed transactions  
✅ **Fast development** — Single codebase, easy refactoring  
✅ **ACID transactions** across modules when needed  
✅ **Path to microservices** — Modules can be extracted when necessary  
✅ **Real-time support** — SignalR works seamlessly within the monolith  

This approach delivers all the functionality described earlier with significantly less complexity than a distributed microservices architecture. Start with this, scale by adding instances, and only extract modules when absolutely necessary.

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
