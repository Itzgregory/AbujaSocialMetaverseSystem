```
Title: 3 Project Structure
Doc ID / filename: 3-project-structure.md
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: 2026-05-10
Last updated: 2026-05-10
Related Epic / Ticket(s): N/A
Short summary: AbujaSocialMetaverse.sln
Contact: oparagregory
```

**TL;DR:** AbujaSocialMetaverse.sln

## Updated: `3_Project_Structure.md`

AbujaSocialMetaverse.sln
│
├── src/
│   ├── AbujaSocialMetaverse.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── UserController.cs
│   │   │   ├── InterestController.cs
│   │   │   ├── DatingProfileController.cs
│   │   │   ├── NetworkingProfileController.cs
│   │   │   ├── BusinessController.cs
│   │   │   ├── RecommendationsController.cs
│   │   │   ├── SocialController.cs          ← assembles CompatibilityContext
│   │   │   ├── MapController.cs
│   │   │   ├── AdminController.cs
│   │   │   └── PrivacyController.cs         ← data subject requests
│   │   ├── Hubs/
│   │   │   ├── AvatarHub.cs (SignalR)
│   │   │   └── ChatHub.cs
│   │   ├── Middleware/
│   │   │   ├── AuthenticationMiddleware.cs
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   ├── GlobalExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── WebApplicationExtensions.cs
│   │   ├── Services/
│   │   │   └── EmailLinkGenerator.cs
│   │   └── Program.cs
│   │
│   ├── modules/
│   │   ├── CoreModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IUserQueryService.cs
│   │   │   │   │   ├── IUserProfileService.cs
│   │   │   │   │   ├── IUserInterestService.cs
│   │   │   │   │   ├── IModeAvailabilityService.cs
│   │   │   │   │   ├── IAuthService.cs
│   │   │   │   │   ├── IAccountVerificationService.cs
│   │   │   │   │   ├── IEmailService.cs
│   │   │   │   │   ├── ITokenService.cs
│   │   │   │   │   ├── IUserCreationService.cs
│   │   │   │   │   ├── IPasswordService.cs
│   │   │   │   │   ├── ILockoutService.cs
│   │   │   │   │   ├── ISessionService.cs
│   │   │   │   │   ├── IDatingProfileService.cs
│   │   │   │   │   └── INetworkingProfileService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── UserDto.cs
│   │   │   │       ├── UserSettingsDto.cs
│   │   │   │       ├── LoginRequest.cs
│   │   │   │       ├── RegisterRequest.cs
│   │   │   │       ├── AuthResponse.cs
│   │   │   │       ├── RefreshTokenRequest.cs
│   │   │   │       ├── UpdateProfileRequest.cs
│   │   │   │       ├── UpdateSettingsRequest.cs
│   │   │   │       ├── ChangePasswordRequest.cs
│   │   │   │       ├── DatingProfileDto.cs
│   │   │   │       └── NetworkingProfileDto.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── BaseService.cs
│   │   │   │   │   ├── UserQueryService.cs
│   │   │   │   │   ├── UserProfileService.cs
│   │   │   │   │   ├── UserInterestService.cs
│   │   │   │   │   ├── ModeAvailabilityService.cs
│   │   │   │   │   ├── UserCreationService.cs
│   │   │   │   │   ├── PasswordService.cs
│   │   │   │   │   ├── LockoutService.cs
│   │   │   │   │   ├── TokenService.cs
│   │   │   │   │   ├── SessionService.cs
│   │   │   │   │   ├── AuthService.cs
│   │   │   │   │   ├── AccountVerificationService.cs
│   │   │   │   │   ├── EmailService.cs
│   │   │   │   │   ├── DatingProfileService.cs
│   │   │   │   │   └── NetworkingProfileService.cs
│   │   │   │   ├── Templates/
│   │   │   │   │   ├── BaseEmailTemplate.cs
│   │   │   │   │   ├── VerificationEmailTemplate.cs
│   │   │   │   │   └── PasswordResetEmailTemplate.cs
│   │   │   │   ├── Providers/
│   │   │   │   │   ├── IEmailProvider.cs
│   │   │   │   │   └── SmtpEmailProvider.cs
│   │   │   │   ├── Mappers/
│   │   │   │   │   └── UserMapper.cs
│   │   │   │   └── Validators/
│   │   │   │       ├── RegisterRequestValidator.cs
│   │   │   │       ├── LoginRequestValidator.cs
│   │   │   │       ├── UpdateProfileRequestValidator.cs
│   │   │   │       └── ChangePasswordRequestValidator.cs
│   │   │   ├── Data/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   ├── UserSetting.cs
│   │   │   │   │   ├── Session.cs
│   │   │   │   │   ├── Interest.cs
│   │   │   │   │   ├── UserInterest.cs
│   │   │   │   │   ├── UserDatingProfile.cs
│   │   │   │   │   ├── UserNetworkingProfile.cs
│   │   │   │   │   └── EmailVerificationToken.cs
│   │   │   │   └── Configurations/
│   │   │   │       ├── UserConfiguration.cs
│   │   │   │       ├── SessionConfiguration.cs
│   │   │   │       ├── InterestConfiguration.cs
│   │   │   │       ├── UserInterestConfiguration.cs
│   │   │   │       ├── UserSettingConfiguration.cs
│   │   │   │       ├── UserDatingProfileConfiguration.cs
│   │   │   │       ├── UserNetworkingProfileConfiguration.cs
│   │   │   │       └── EmailVerificationTokenConfiguration.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   ├── BusinessModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IBusinessService.cs
│   │   │   │   │   └── IRecommendationService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── BusinessDto.cs
│   │   │   │       ├── BusinessCategory.cs
│   │   │   │       └── RecommendationResult.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── BusinessService.cs
│   │   │   │   │   └── RecommendationEngine.cs
│   │   │   │   ├── Scoring/
│   │   │   │   │   ├── DatingScoringStrategy.cs
│   │   │   │   │   ├── NetworkingScoringStrategy.cs
│   │   │   │   │   └── LeisureScoringStrategy.cs
│   │   │   │   └── Mappers/
│   │   │   │       └── BusinessMapper.cs
│   │   │   ├── Data/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Business.cs
│   │   │   │   │   ├── BusinessCategory.cs
│   │   │   │   │   └── BusinessAnalytics.cs
│   │   │   │   └── Configurations/
│   │   │   │       ├── BusinessConfiguration.cs
│   │   │   │       └── BusinessCategoryConfiguration.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   ├── SocialModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IAvatarService.cs
│   │   │   │   │   ├── IInteractionService.cs
│   │   │   │   │   └── IChatService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── AvatarState.cs
│   │   │   │       ├── ProximityEvent.cs
│   │   │   │       └── SocialInteraction.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── AvatarService.cs
│   │   │   │   │   ├── ProximityDetector.cs
│   │   │   │   │   ├── CompatibilityEngine.cs
│   │   │   │   │   └── ChatService.cs
│   │   │   │   └── Mappers/
│   │   │   │       └── SocialMapper.cs
│   │   │   ├── Data/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── SocialInteraction.cs
│   │   │   │   │   └── ChatSession.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   └── SocialInteractionConfiguration.cs
│   │   │   │   └── Redis/
│   │   │   │       └── LocationCache.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   ├── MapModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IMapService.cs
│   │   │   │   │   └── ILocationService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── MapTile.cs
│   │   │   │       ├── GeoCoordinate.cs
│   │   │   │       └── BoundingBox.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── MapService.cs
│   │   │   │   │   ├── GeocodingService.cs
│   │   │   │   │   └── DistanceCalculator.cs
│   │   │   │   └── Mappers/
│   │   │   │       └── MapMapper.cs
│   │   │   ├── Clients/
│   │   │   │   └── MapboxClient.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   ├── PaymentModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── ISubscriptionService.cs
│   │   │   │   │   └── IPaymentService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── SubscriptionDto.cs
│   │   │   │       ├── TransactionDto.cs
│   │   │   │       └── InvoiceDto.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── SubscriptionService.cs
│   │   │   │   │   └── PaymentProcessor.cs
│   │   │   │   ├── Mappers/
│   │   │   │   │   └── PaymentMapper.cs
│   │   │   │   └── Validators/
│   │   │   │       └── WebhookValidator.cs
│   │   │   ├── Clients/
│   │   │   │   ├── StripeClient.cs
│   │   │   │   └── PaystackClient.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   ├── AdminModule/
│   │   │   ├── Public/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IAdminService.cs
│   │   │   │   │   └── IModerationService.cs
│   │   │   │   └── Models/
│   │   │   │       ├── DashboardMetric.cs
│   │   │   │       └── ModerationQueue.cs
│   │   │   ├── Internal/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── AdminService.cs
│   │   │   │   │   ├── DashboardService.cs
│   │   │   │   │   ├── ModerationService.cs
│   │   │   │   │   └── ReportGenerator.cs
│   │   │   │   └── Mappers/
│   │   │   │       └── AdminMapper.cs
│   │   │   └── ModuleRegistration.cs
│   │   │
│   │   └── PrivacyModule/
│   │       ├── Public/
│   │       │   ├── Interfaces/
│   │       │   │   ├── IConsentService.cs
│   │       │   │   ├── IRetentionService.cs
│   │       │   │   ├── IDataSubjectService.cs
│   │       │   │   └── IAuditLogService.cs
│   │       │   └── Models/
│   │       │       ├── ConsentRecord.cs
│   │       │       ├── RetentionPolicy.cs
│   │       │       └── DataSubjectRequest.cs
│   │       ├── Internal/
│   │       │   ├── Services/
│   │       │   │   ├── ConsentService.cs
│   │       │   │   ├── RetentionService.cs
│   │       │   │   ├── DataSubjectService.cs
│   │       │   │   └── AuditLogService.cs
│   │       │   └── Mappers/
│   │       │       └── PrivacyMapper.cs
│   │       ├── Data/
│   │       │   ├── Entities/
│   │       │   │   ├── ConsentRecord.cs
│   │       │   │   ├── RetentionPolicy.cs
│   │       │   │   ├── DataSubjectRequest.cs
│   │       │   │   └── AuditLogEntry.cs
│   │       │   └── Configurations/
│   │       │       ├── ConsentRecordConfiguration.cs
│   │       │       └── AuditLogEntryConfiguration.cs
│   │       └── ModuleRegistration.cs
│   │
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Migrations/
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── IAuditableEntity.cs
│   │   │   ├── ISoftDeletable.cs
│   │   │   └── BaseEntity.cs
│   │   ├── Caching/
│   │   │   ├── ICacheService.cs
│   │   │   ├── ILocationCacheService.cs
│   │   │   ├── ICacheAdminService.cs
│   │   │   ├── RedisCacheService.cs
│   │   │   ├── RedisLocationCacheService.cs
│   │   │   ├── RedisCacheAdminService.cs
│   │   │   └── CacheKeys.cs
│   │   ├── RealTime/
│   │   │   ├── IRealTimeService.cs
│   │   │   ├── IConnectionTracker.cs
│   │   │   ├── RedisConnectionTracker.cs
│   │   │   ├── SignalRRealTimeService.cs
│   │   │   ├── HubMarkers.cs
│   │   │   └── Models/
│   │   │       ├── AvatarPositionUpdate.cs
│   │   │       ├── ChatMessage.cs
│   │   │       ├── ProximityAlert.cs
│   │   │       └── MatchNotification.cs
│   │   ├── Messaging/
│   │   │   ├── IMessageBus.cs
│   │   │   └── InMemoryMessageBus.cs
│   │   ├── Logging/
│   │   │   └── SerilogLogger.cs
│   │   └── BackgroundJobs/
│   │       ├── IBackgroundJobService.cs
│   │       ├── HangfireBackgroundJobService.cs
│   │       ├── CronSchedules.cs
│   │       ├── JobIds.cs
│   │       └── RetentionJob.cs
│   │
│   └── Shared/
│       ├── Configuration/
│       │   ├── BaseOptions.cs
│       │   ├── ConnectionOptions.cs
│       │   ├── SecurityOptions.cs
│       │   ├── FeatureOptions.cs
│       │   ├── Options/
│       │   │   ├── DatabaseOptions.cs
│       │   │   ├── RedisOptions.cs
│       │   │   ├── JwtOptions.cs
│       │   │   ├── MapboxOptions.cs
│       │   │   ├── StripeOptions.cs
│       │   │   ├── PaystackOptions.cs
│       │   │   ├── HangfireOptions.cs
│       │   │   ├── RealTimeOptions.cs
│       │   │   ├── PrivacyOptions.cs
│       │   │   ├── RecommendationOptions.cs
│       │   │   ├── RateLimitOptions.cs
│       │   │   ├── CorsOptions.cs
│       │   │   ├── LoggingOptions.cs
│       │   │   ├── UserOptions.cs
│       │   │   ├── PasswordPolicyOptions.cs
│       │   │   └── LockoutOptions.cs
│       │   └── OptionsRegistrationExtension.cs
│       ├── Constants/
│       │   ├── ErrorCodes.cs
│       │   └── AppConstants.cs
│       ├── Contracts/
│       │   ├── IAdminProjection.cs
│       │   └── IEmailLinkGenerator.cs
│       ├── Exceptions/
│       │   ├── DomainException.cs
│       │   └── ConsentRequiredException.cs
│       ├── Helpers/
│       │   └── Guard.cs
│       ├── Models/
│       │   ├── Result.cs
│       │   ├── PagedResult.cs
│       │   ├── CompatibilityContext.cs
│       │   ├── AdminMetricSnapshot.cs
│       │   └── DataCategory.cs
│       └── Validators/
│           └── CommonValidators.cs
│
├── UnityClient/                                           # Unity 3D client structure
│   ├── Core/
│   │   ├── NetworkManager.cs
│   │   ├── AuthManager.cs
│   │   └── SessionManager.cs
│   ├── World/
│   │   ├── MapLoader.cs
│   │   ├── BusinessPinManager.cs
│   │   └── WorldStateManager.cs
│   ├── Avatar/
│   │   ├── LocalAvatarController.cs
│   │   ├── RemoteAvatarController.cs
│   │   ├── AvatarInterpolator.cs
│   │   └── ProximityMonitor.cs
│   ├── Social/
│   │   ├── ChatUI.cs
│   │   ├── InteractionPrompt.cs
│   │   └── CompatibilityNotifier.cs
│   ├── UI/
│   │   ├── ModeSelector.cs
│   │   ├── BusinessCard.cs
│   │   └── HUD.cs
│   └── Services/
│       ├── INetworkService.cs
│       ├── ISignalRService.cs
│       └── IAssetService.cs
│
├── tests/
│   ├── CoreModule.Tests/
│   ├── BusinessModule.Tests/
│   ├── SocialModule.Tests/
│   ├── PrivacyModule.Tests/
│   └── Integration.Tests/
│
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
│
└── deploy/
    ├── k8s/
    └── scripts/

---

## Change History
- v1.0 – 2026-05-10 – Applied Elios Technology Documentation Standards (Antigravity)
