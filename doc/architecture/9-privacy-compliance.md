```
Title: 9 Privacy Compliance
Doc ID / filename: 9-privacy-compliance.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: The platform collects and processes sensitive personal data: real-time location, social interaction history, behavioural patterns, and profile info...
Contact: oparagregory
```

**TL;DR:** The platform collects and processes sensitive personal data: real-time location, social interaction history, behavioural patterns, and profile info...

## New: `9-privacy-compliance.md`

## 9. Privacy and Compliance Architecture (NDPA 2023)

### Overview

The platform collects and processes sensitive personal data: real-time location, social interaction history, behavioural patterns, and profile information. Under the Nigeria Data Protection Act 2023 (NDPA), this requires a lawful basis for processing, demonstrable consent management, enforceable data subject rights, and breach notification capability.

Compliance is not a feature to be added later. It is an architectural constraint applied from day one.

---

### Privacy Module Responsibilities

| Service | Responsibility |
|---------|---------------|
| `ConsentService` | Record and verify lawful basis per user per data category |
| `RetentionService` | Register and enforce retention windows across all modules |
| `DataSubjectService` | Execute access, portability, and erasure requests end-to-end |
| `AuditLogService` | Append-only, tamper-evident log of all personal data processing events |

---

### Data Categories and Lawful Basis

| Data Category | Examples | Lawful Basis | Retention |
|---------------|----------|-------------|-----------|
| `Identity` | Name, email, phone | Contract | Account lifetime + 12 months |
| `Location` | Real-time coordinates, movement history | Explicit consent | 30 days rolling |
| `SocialGraph` | Interactions, proximity events, compatibility checks | Explicit consent | 12 months |
| `Behavioural` | Mode selections, business views, engagement patterns | Legitimate interest + consent | 24 months, anonymized |
| `Payment` | Transaction records, subscription history | Legal obligation | 7 years |
| `Moderation` | Flagged content, reports | Legitimate interest | 36 months |

---

### Consent Gate Pattern

Every module that writes personal data must check consent before the write. This is enforced by convention and code review, not by the compiler — but the pattern is consistent and auditable.

```csharp
// PrivacyModule/Public/IConsentService.cs
public interface IConsentService
{
    Task<bool> HasConsentAsync(Guid userId, DataCategory category);
    Task RecordConsentAsync(Guid userId, DataCategory category, bool granted);
    Task WithdrawConsentAsync(Guid userId, DataCategory category);
}

// PrivacyModule/Public/Models/DataCategory.cs
public enum DataCategory
{
    Identity,
    Location,
    SocialGraph,
    Behavioural,
    Payment,
    Moderation
}
```

**Usage inside SocialModule:**
```csharp
// SocialModule/Internal/AvatarService.cs
public async Task UpdatePositionAsync(Guid userId, Vector3 position)
{
    var hasConsent = await _consentService.HasConsentAsync(userId, DataCategory.Location);
    if (!hasConsent)
        throw new ConsentRequiredException(userId, DataCategory.Location);

    await _locationCache.SetAsync(userId, position);
    await _auditLog.LogAsync(userId, "location_written", position);
}
```

---

### Retention Enforcement

A nightly Hangfire background job (`RetentionJob`) queries the retention registry and purges or anonymizes records past their window.

```csharp
// Infrastructure/BackgroundJobs/RetentionJob.cs
public class RetentionJob : IBackgroundJob
{
    public async Task ExecuteAsync()
    {
        var policies = await _retentionService.GetAllPoliciesAsync();

        foreach (var policy in policies)
        {
            if (policy.ActionOnExpiry == RetentionAction.Purge)
                await _retentionService.PurgeExpiredAsync(policy);

            if (policy.ActionOnExpiry == RetentionAction.Anonymize)
                await _retentionService.AnonymizeExpiredAsync(policy);
        }
    }
}
```

Each module registers its own retention policies at startup:

```csharp
// SocialModule registration
services.AddRetentionPolicy(new RetentionPolicy
{
    DataCategory = DataCategory.Location,
    Table = "social_location_history",
    RetentionPeriod = TimeSpan.FromDays(30),
    ActionOnExpiry = RetentionAction.Purge
});
```

---

### Data Subject Rights (Right to Erasure)

When a user requests deletion, `DataSubjectService` executes a multi-step cascade:

```
┌────────────────────────────────────────────────────────────────────┐
│  DELETE REQUEST FLOW                                               │
│                                                                    │
│  1. PrivacyController receives DELETE /api/privacy/me              │
│  2. IDataSubjectService.RequestErasureAsync(userId)                │
│  3. Cascade:                                                       │
│     a. Anonymize PII in core_users (PostgreSQL)                    │
│     b. Delete social_interactions where userId (PostgreSQL)        │
│     c. Flush location keys from Redis                              │
│     d. Delete avatar assets from S3/Blob                          │
│     e. Revoke all active sessions from Redis                       │
│     f. Record erasure completion in audit log                      │
│  4. Return confirmation with erasure timestamp                     │
│                                                                    │
│  SLA: Completed within 72 hours per NDPA 2023 requirement          │
└────────────────────────────────────────────────────────────────────┘
```

**Anonymization vs Deletion:**
Records required for legal obligation (payment history, moderation logs) are anonymized — PII replaced with a pseudonymous token — rather than deleted. All other records are hard deleted.

---

### Audit Log

All personal data processing events are written to an append-only audit table. This table is never updated or deleted. It serves as evidence of compliance.

```csharp
// PrivacyModule/Public/IAuditLogService.cs
public interface IAuditLogService
{
    Task LogAsync(Guid userId, string action, object? metadata = null);
}

// Schema: privacy_audit_log
// Columns: id, user_id, action, metadata (jsonb), created_at
// No update or delete permissions granted on this table
```

---

### Breach Notification Readiness

The NDPA 2023 requires notification of the Nigeria Data Protection Commission (NDPC) within 72 hours of a discovered breach. The audit log provides the data lineage needed to assess scope. The following must be documented and rehearsed:

- Which data categories were exposed
- How many data subjects are affected (query against audit log)
- When the breach occurred (audit log timestamp)
- What mitigation was applied

This is an operational procedure, not just a code concern. It must be documented in the runbook.


---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
