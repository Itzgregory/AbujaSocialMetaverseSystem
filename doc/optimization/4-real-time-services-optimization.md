```
Title: 4 Real Time Services Optimization
Doc ID / filename: 4-real-time-services-optimization.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: The real-time layer is implemented as SignalR embedded within the ASP.NET Core monolith.
Contact: oparagregory
```

**TL;DR:** The real-time layer is implemented as SignalR embedded within the ASP.NET Core monolith.

## 4. Real-Time Services Optimization

The real-time layer is implemented as SignalR embedded within the ASP.NET Core monolith.
Regions are not separate server processes — they are SignalR groups backed by Redis pub/sub.
The Redis backplane coordinates position broadcasts across multiple monolith instances
behind the load balancer. This means all scaling is horizontal at the monolith level,
not at a separate real-time service level.

| Factor | Consideration | Optimization Strategy |
|--------|---------------|----------------------|
| **Region Group Distribution** | A single Redis pub/sub channel for all users creates unnecessary broadcast volume. | • Partition Abuja into 16 static grid regions (approximately 2km x 2km each)<br>• Each region maps to a dedicated SignalR group in Redis<br>• Users join their current region group and adjacent groups on connect<br>• On region boundary crossing, client leaves old group and joins new group — handled by AvatarHub with no perceptible interruption |
| **State Synchronization** | Full state sync on every reconnection wastes bandwidth. | • On reconnect, send a Redis snapshot of current region state only<br>• Send only delta changes since last acknowledged update during normal operation<br>• Use interest-based filtering — each client receives updates only for avatars in its subscribed groups<br>• Prioritize updates — avatar position and proximity events over minor animation states |
| **Proximity Detection** | O(n²) comparisons between all users in a region do not scale. | • Use spatial hashing with grid cells aligned to the 200m x 200m interest management grid<br>• Only run proximity checks between users in the same or adjacent cells<br>• Run proximity checks at 10-15Hz on the server, not every SignalR message<br>• When proximity threshold is crossed, AvatarHub fires a proximity event to the backend — compatibility scoring is never done inside the real-time layer |
| **Bandwidth Management** | High SignalR message rates saturate connections on poor networks. | • Implement adaptive update rate — reduce broadcast frequency for idle avatars<br>• Use MessagePack binary serialization for SignalR messages instead of JSON<br>• Throttle non-critical updates (idle animations, minor position jitter below 0.5m)<br>• Compress position data with delta encoding before sending through SignalR |
| **Cross-Instance Coordination** | Multiple monolith instances behind the load balancer must share real-time state. | • Redis backplane ensures SignalR messages are broadcast across all instances<br>• Load balancer configured with sticky sessions (by connection ID) to maintain WebSocket affinity<br>• Redis stores all active user locations so any instance can serve a reconnecting client<br>• No in-memory state is held exclusively on any single instance |

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
