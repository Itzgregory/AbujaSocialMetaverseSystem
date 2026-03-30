## Abuja Social Metaverse — Implementation Tracker

### Phase 1 — Project Structure & Setup
- [x] Create solution and project scaffold 
- [ ] Wire project references (who depends on who)
- [ ] Install NuGet packages per project
- [ ] Create folder structure inside each project
- [ ] Configure Program.cs, appsettings.json
- [ ] Set up docker-compose (for later)

### Phase 2 — Infrastructure
- [ ] ApplicationDbContext + EF Core config
- [ ] Redis cache service (ICacheService)
- [ ] IRealTimeService abstraction + SignalR implementation
- [ ] Hangfire background jobs
- [ ] Serilog logging

### Phase 3 — Shared Layer
- [ ] Constants
- [ ] Exceptions (ConsentRequiredException, etc.)
- [ ] Validators
- [ ] Shared contracts (IAdminProjection)
- [ ] Shared models (CompatibilityContext, AdminMetricSnapshot, DataCategory)

### Phase 4 — Modules
- [ ] Core Module (User, Auth, Session)
- [ ] Privacy Module (Consent, Retention, DataSubject, AuditLog)
- [ ] Map Module (Tiles, Geocoding, Location)
- [ ] Business Module (Listings, Categories, Recommendation Engine)
- [ ] Social Module (Avatar, Proximity, Compatibility, Chat)
- [ ] Payment Module (Subscriptions, Transactions, Invoices)
- [ ] Admin Module (Dashboard, Moderation, Projections)

### Phase 5 — API Layer
- [ ] Middleware (Auth, RateLimiting, Logging)
- [ ] Controllers (Auth, Business, Recommendations, Social, Map, Admin, Privacy)
- [ ] SignalR Hubs (AvatarHub, ChatHub)

---

**Current position: Step 2 — Project references.**

Ready to proceed?