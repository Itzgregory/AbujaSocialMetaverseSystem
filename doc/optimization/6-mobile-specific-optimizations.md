```
Title: 6 Mobile Specific Optimizations
Doc ID / filename: 6-mobile-specific-optimizations.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: The primary market for this platform is Abuja, Nigeria. Device fragmentation skews
Contact: oparagregory
```

**TL;DR:** The primary market for this platform is Abuja, Nigeria. Device fragmentation skews

## 6. Mobile-Specific Optimizations

The primary market for this platform is Abuja, Nigeria. Device fragmentation skews
heavily toward mid-range and budget Android devices (Tecno, Infinix, Itel, Samsung A-series).
iOS represents a smaller but higher-value segment. Optimizations must account for this
reality — not for a global average device profile.

### A. Device Tier Strategy

Rather than a single quality setting, the platform defines three device tiers detected
at launch based on available RAM, GPU capability, and CPU benchmark score:

| Tier | Target Devices | Frame Rate Target | Max Visible Avatars | Texture Quality |
|------|---------------|-------------------|--------------------|-----------------| 
| **High** | iPhone 12+, Samsung S21+, Pixel 6+ | 60 FPS | 30 | Full |
| **Mid** | Samsung A54, Tecno Camon 20, Infinix Note 30 | 30 FPS | 20 | Medium |
| **Low** | Tecno Spark, Infinix Hot series, Itel devices | 24 FPS | 10 | Low |

Settings are auto-detected on first launch and can be overridden manually in settings.

---

### B. Battery Consumption

| Factor | Consideration | Optimization Strategy |
|--------|---------------|----------------------|
| **Continuous Rendering** | 3D rendering at full rate drains battery within 2-3 hours. | • Reduce frame rate when user is idle for more than 30 seconds (60Hz → 30Hz → 24Hz)<br>• Drop to 2D map view option for users on low battery (<20%)<br>• Implement network coalescing — batch position updates rather than sending individually<br>• Reduce rendering resolution to 75% on battery saver mode |
| **Background Activity** | SignalR connection maintained in background drains battery. | • Disconnect SignalR when app is backgrounded for more than 60 seconds<br>• Resume connection with state restoration on foreground<br>• Use push notifications for proximity alerts when backgrounded rather than live connection |
| **Network Radio Usage** | Frequent small packets keep the radio active and prevent low-power states. | • Batch outgoing messages into 100ms windows before sending<br>• Use exponential backoff for non-critical REST polling<br>• Align network activity to reduce radio wake events |

### C. Thermal Throttling

| Factor | Consideration | Optimization Strategy |
|--------|---------------|----------------------|
| **Prolonged Sessions** | Budget Android devices throttle aggressively after 15-20 minutes of 3D rendering. | • Monitor device temperature via Unity's SystemInfo APIs<br>• Step down quality tier automatically when thermal warning is received<br>• Reduce max visible avatars and LOD quality as first response to heat<br>• Notify user with a non-intrusive message when quality is reduced |
| **Charging State** | Devices on charge can sustain higher performance without battery concern. | • Allow sustained 60 FPS on mid-tier devices when plugged in<br>• Re-enable higher texture quality on charger for low-tier devices |

### D. Memory Pressure

| Factor | Consideration | Optimization Strategy |
|--------|---------------|----------------------|
| **Memory Budgets** | iOS and Android aggressively terminate apps exceeding memory limits. | • High tier: maximum 800MB<br>• Mid tier: maximum 500MB<br>• Low tier: maximum 300MB<br>• Implement memory pressure callbacks (ApplicationMemoryUsage on iOS, onTrimMemory on Android) to release texture caches immediately |
| **Asset Loading** | Loading all assets upfront is not viable on low-tier devices. | • Stream assets on demand based on user location within the world<br>• Unload assets for chunks more than two rings away from current position<br>• Use compressed texture formats mandatory on low tier (ETC2 on Android, PVRTC on iOS) |
| **Avatar Count** | Each unique avatar in view consumes memory for its mesh, textures, and animation state. | • Hard cap unique avatars in memory to tier limit (30 / 20 / 10)<br>• Reuse avatar slots — when a new avatar enters view and cap is reached, recycle the slot of the most distant avatar<br>• Use a shared low-resolution fallback avatar for users beyond 150m on low-tier devices |

### E. Network Conditions (Nigeria-Specific)

| Factor | Consideration | Optimization Strategy |
|--------|---------------|----------------------|
| **Variable 4G Coverage** | 4G coverage in Abuja is real but inconsistent across areas. Wuse and Maitama have strong coverage; outer districts are patchier. | • Implement adaptive update rate — detect effective bandwidth and reduce position update frequency on weak connections<br>• Pre-cache nearby business data when on WiFi or strong 4G so recommendations are available during signal dips<br>• Show cached map and business pins during brief disconnections rather than a loading screen |
| **Data Cost Sensitivity** | Mobile data in Nigeria is metered and cost-sensitive for many users. | • Show data usage estimate in settings<br>• Implement a Data Saver mode that reduces texture streaming, lowers update rate to 5Hz, and disables non-essential visual effects<br>• Initial app download must stay under 50MB — remaining assets streamed on demand |
| **WiFi Handoff** | Users moving between WiFi and mobile data causes brief disconnections. | • SignalR reconnection handles this automatically<br>• Redis session state ensures no position or chat data is lost during handoff<br>• Client buffers outgoing position updates during reconnection and flushes on reconnect |

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
