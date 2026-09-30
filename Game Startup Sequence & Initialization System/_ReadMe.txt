# Game Startup Sequence & Initialization System

This system manages the foundational startup routine of the game. Before loading the main gameplay scene, it sequentially verifies internet connectivity, authenticates the player via Unity Gaming Services (UGS), fetches an accurate network time, locks a base timestamp, and finally loads the game scene.

## Execution Order Overview
Unity executes these scripts in a precise order based on their `[DefaultExecutionOrder]` attributes to ensure dependencies are met:
1. `GetNet.cs` (`-11`)
2. `Authentication.cs` (`-10`)
3. `GetTime.cs` (`-9`)
4. `SetBaseTime` (Implicitly managed or accompanying script)
5. `LoadSceneAsync.cs` (`-7`)

---

## Scripts Breakdown

### 1. `GetNet.cs` (`[DefaultExecutionOrder(-11)]`)
* **Purpose:** Acts as the primary gatekeeper for internet connectivity.
* **What it does:**
  * Tests for an active internet connection on startup using a web request to `google.com`.
  * Sets the public `isConnected` flag.
  * Displays a "No internet access" message and activates a **Retry** button if offline.

### 2. `Authentication.cs` (`[DefaultExecutionOrder(-10)]`)
* **Purpose:** Automatically signs the player into Unity Gaming Services (UGS).
* **What it does:**
  * Waits for `GetNet` to confirm an active connection.
  * Initializes UGS and performs an anonymous sign-in so the player has a valid backend session ID.
  * Handles authentication events, errors, and session expirations by enabling a retry mechanism if login fails.

### 3. `GetTime.cs` (`[DefaultExecutionOrder(-9)]`)
* **Purpose:** Fetches an accurate, tamper-resistant world time from an NTP server.
* **What it does:**
  * Waits until the network is connected and the player is successfully authenticated.
  * Queries a public NTP server (`pool.ntp.org`) via UDP to calculate the exact network time.
  * Stores the result globally in `StaticBaseTime.CurrentDateTime`.

### 4. `LoadSceneAsync.cs` (`[DefaultExecutionOrder(-7)]`)
* **Purpose:** Manages the background loading and smooth transition into the main game scene.
* **What it does:**
  * Monitors network status and time synchronization (via `lockBaseTime` and `isConnected`).
  * Asynchronously loads the `"Main"` scene while updating a progress bar text interface (`TextMeshProUGUI`).
  * Safely triggers the final scene activation only when all initial setup conditions are fully met.