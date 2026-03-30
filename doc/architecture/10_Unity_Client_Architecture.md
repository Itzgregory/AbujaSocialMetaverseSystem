## New: `10_Unity_Client_Architecture.md`
## 10. Unity Client Architecture

### Overview

The Unity client is the user-facing half of the platform. It is responsible for rendering the 3D city, animating avatars, managing real-time network state, and presenting UI. Its architecture mirrors the backend's modular structure — each concern is isolated, and dependencies flow in one direction.

---

### Module Structure

```
UnityClient/
├── Core/
├── World/
├── Avatar/
├── Social/
├── UI/
└── Services/
```

---

### Core

**Purpose:** Application lifecycle, authentication state, and network session management.

| Script | Responsibility |
|--------|---------------|
| `NetworkManager.cs` | Initialises HTTP client and SignalR connection. Manages reconnection logic. Single instance (DontDestroyOnLoad). |
| `AuthManager.cs` | Stores JWT token securely. Handles token refresh before expiry. Exposes current user identity to other modules. |
| `SessionManager.cs` | Tracks current user's mode (Dating/Networking/Leisure), online status, and active social session state. |

---

### World

**Purpose:** Map rendering, business pin management, and world state.

| Script | Responsibility |
|--------|---------------|
| `MapLoader.cs` | Streams map tiles from Mapbox. Manages tile loading based on camera position. Disposes tiles outside view. |
| `BusinessPinManager.cs` | Places business 3D pins on the map at geocoordinate positions. Updates visible pins when user mode changes. |
| `WorldStateManager.cs` | Authoritative local record of what is currently rendered. Other modules query this rather than the scene directly. |

---

### Avatar

**Purpose:** Local and remote avatar movement, interpolation, and proximity detection.

This is the most technically critical module. Local and remote avatars have fundamentally different update paths and must be separate classes from day one.

| Script | Responsibility |
|--------|---------------|
| `LocalAvatarController.cs` | Reads player input. Moves the local avatar immediately (client-side prediction). Sends position to server via SignalR on a fixed interval (e.g. 100ms). |
| `RemoteAvatarController.cs` | Receives position updates from server for a specific remote user. Passes updates to AvatarInterpolator. Never reads player input. |
| `AvatarInterpolator.cs` | Smooths discrete server position updates into fluid frame-by-frame movement. Uses linear interpolation between last known and current target position. Handles late or missing packets gracefully. |
| `ProximityMonitor.cs` | Runs on a fixed interval. Checks distance between local avatar and all visible remote avatars. Fires a `ProximityEvent` when threshold is crossed. Notifies Social module. |

**Position Update Cycle:**

```
┌────────────────────────────────────────────────────────────────────┐
│  LOCAL AVATAR                                                      │
│                                                                    │
│  Player Input → LocalAvatarController → Move immediately (60fps)  │
│                                       → Send to server (10/sec)   │
│                                                                    │
│  REMOTE AVATAR                                                     │
│                                                                    │
│  Server update (10/sec) → RemoteAvatarController                  │
│                         → AvatarInterpolator → Smooth (60fps)     │
└────────────────────────────────────────────────────────────────────┘
```

---

### Social

**Purpose:** Proximity-triggered interactions, chat, and compatibility notifications.

| Script | Responsibility |
|--------|---------------|
| `ChatUI.cs` | Renders chat window for an active social session. Sends and receives messages via SignalR. |
| `InteractionPrompt.cs` | Displays action options (Wave, Connect, Ignore) when ProximityMonitor fires a ProximityEvent. Handles user choice and sends response to server. |
| `CompatibilityNotifier.cs` | Listens for compatibility events pushed from the server. Displays match notification UI. Opens social session on acceptance. |

---

### UI

**Purpose:** Heads-up display, mode selection, and business detail overlays.

| Script | Responsibility |
|--------|---------------|
| `ModeSelector.cs` | Renders the Dating/Networking/Leisure switcher. On mode change, calls the REST API to persist the setting and notifies `BusinessPinManager` to refresh visible pins. |
| `BusinessCard.cs` | Overlay panel showing business details when a pin is tapped. Displays name, category, promotional content, and directions. |
| `HUD.cs` | Persistent screen overlay: minimap, notification badges, current mode indicator, online user count. |

---

### Services

**Purpose:** Abstractions over network calls. All HTTP and WebSocket communication is routed through these interfaces, never called directly from game logic scripts.

| Interface | Responsibility |
|-----------|---------------|
| `INetworkService.cs` | Wraps HTTP calls to the REST API. Handles auth headers, error responses, and retry logic. |
| `ISignalRService.cs` | Wraps the SignalR connection. Provides typed methods for sending and receiving avatar events and chat messages. |
| `IAssetService.cs` | Loads 3D models and textures from the CDN. Caches loaded assets in memory. Prevents duplicate downloads. |

**Design rule:** Game logic scripts depend on these interfaces, not on concrete implementations. This allows network calls to be mocked during testing without a live server.

---

### Data Flow Summary

```
┌────────────────────────────────────────────────────────────────────────────────┐
│                                                                                │
│  Player Input                                                                  │
│       │                                                                        │
│       ▼                                                                        │
│  LocalAvatarController ──► ISignalRService.SendPosition() ──► Server          │
│                                                                    │           │
│  Server ──► ISignalRService (receive) ──► RemoteAvatarController  │           │
│                                               │                               │
│                                          AvatarInterpolator ──► Render        │
│                                                                                │
│  ProximityMonitor ──► ProximityEvent ──► InteractionPrompt ──► User choice    │
│                                                                    │           │
│                                               CompatibilityNotifier ◄── Server│
│                                                                                │
│  ModeSelector ──► INetworkService.UpdateMode() ──► Server                     │
│              └──► BusinessPinManager.Refresh()                                │
│                                                                                │
└────────────────────────────────────────────────────────────────────────────────┘
```