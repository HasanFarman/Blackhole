# Blackhole Survival Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable top-down endless survival game where a black hole pulls debris into orbit, flings it at tumbling enemies, and grows stronger between waves.

**Architecture:** A single Unity scene with focused MonoBehaviours for player control, gravity, orbit management, debris state, enemies, waves, run state, camera, and HUD. An editor bootstrap script generates the layers, materials, prefabs, and scene so the setup is repeatable. Pure rules live in small static classes and are covered by EditMode tests; physics and lifecycle behavior are covered by PlayMode tests.

**Tech Stack:** Unity 6000.0.68f1, URP, Input System, uGUI, Unity Test Framework, NUnit.

**Spec:** `docs/superpowers/specs/2026-10-24-blackhole-survival-design.md`

## Global Constraints

- Unity 6000.0.68f1 with URP and Input System already installed.
- No external art, audio, or paid assets; primitives, generated meshes, and code-created materials only.
- Single playable scene at `Assets/Scenes/Main.unity`.
- Top-down perspective camera, pitch 60–70 degrees, default 65; not a 90-degree view.
- Endless mode; difficulty increases only by enemy count.
- Player dies on enemy contact; thrown debris kills enemies on the first hit.
- Layer names: `Player`, `Debris`, `Enemy`.
- Wave-break timer is a serialized field; default 5 seconds.
- Debris cap defaults to 48; six debris at run start; one debris per kill; three bonus debris per completed wave.
- No jumping.

## Review Focus

- Cursor ray misses the play plane or points at the player: throwing must do nothing and must not throw an exception.
- Clicking with no orbiting debris: no error, no cooldown consumed.
- Player/enemy contact happens more than once or while already dead: death and game over must fire once.
- A debris piece is destroyed or removed while orbiting or thrown: orbit and active-debris lists must not keep stale references.
- Player dies during a wave break or while enemies are alive: the spawner must stop and the game-over state must win.

---

### Task 1: Project Foundation, Test Harness, and Pure Rules

**Files:**
- Create: `.gitignore`
- Create: `Assets/Scripts/Runtime/Blackhole.Runtime.asmdef`
- Create: `Assets/Scripts/Runtime/Rules/WaveRules.cs`
- Create: `Assets/Scripts/Runtime/Rules/AimRules.cs`
- Create: `Assets/Scripts/Runtime/Rules/OrbitMath.cs`
- Create: `Assets/Tests/EditMode/Blackhole.Tests.EditMode.asmdef`
- Create: `Assets/Tests/EditMode/WaveRulesTests.cs`
- Create: `Assets/Tests/EditMode/AimRulesTests.cs`
- Create: `Assets/Tests/EditMode/OrbitMathTests.cs`
- Create: `Assets/Tests/PlayMode/Blackhole.Tests.PlayMode.asmdef`

**Interfaces:**
- Produces:
  - `WaveRules.EnemyCount(int wave, int baseEnemyCount, int enemiesAddedPerWave) -> int`
  - `AimRules.BestAimIndex(Vector3 origin, IReadOnlyList<Vector3> positions, Vector3 aimDirection) -> int`
  - `AimRules.TryGroundPoint(Ray ray, float planeY, out Vector3 point) -> bool`
  - `OrbitMath.WrapDegrees(float degrees) -> float`
  - `OrbitMath.OrbitPosition(Vector3 center, float radius, float angleDegrees) -> Vector3`
- Runtime namespace is `Blackhole`; test namespace is `Blackhole.Tests`.

- [ ] **Step 1: Initialize version control and Unity ignores**

Create the Unity `.gitignore` (ignore `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `TestResults/`, generated `*.csproj`, `*.sln`, `*.slnx`, `.vs/`, and `.idea/`), then run:

```powershell
git init
git add .gitignore
git commit -m "chore: initialize Unity project"
```

- [ ] **Step 2: Write the failing EditMode tests**

`WaveRulesTests.cs` assertions:

```csharp
Assert.AreEqual(5, WaveRules.EnemyCount(1, 5, 3));
Assert.AreEqual(14, WaveRules.EnemyCount(4, 5, 3));
Assert.AreEqual(5, WaveRules.EnemyCount(0, 5, 3));
```

`AimRulesTests.cs` assertions:

```csharp
var positions = new[] { new Vector3(5, 0, 0), new Vector3(0, 0, 5), new Vector3(-5, 0, 0) };
Assert.AreEqual(1, AimRules.BestAimIndex(Vector3.zero, positions, new Vector3(0, 0, 1)));
Assert.AreEqual(-1, AimRules.BestAimIndex(Vector3.zero, Array.Empty<Vector3>(), Vector3.right));
Assert.AreEqual(-1, AimRules.BestAimIndex(Vector3.zero, positions, Vector3.zero));
Assert.IsFalse(AimRules.TryGroundPoint(new Ray(Vector3.zero, Vector3.up), 0f, out _));
Assert.IsTrue(AimRules.TryGroundPoint(new Ray(Vector3.zero, Vector3.down), 0f, out var hit));
Assert.AreEqual(0f, hit.y, 0.0001f);
```

`OrbitMathTests.cs` assertions:

```csharp
var atZero = OrbitMath.OrbitPosition(Vector3.zero, 2f, 0f);
Assert.AreEqual(2f, atZero.x, 0.0001f);
Assert.AreEqual(0f, atZero.z, 0.0001f);
var atNinety = OrbitMath.OrbitPosition(Vector3.zero, 2f, 90f);
Assert.AreEqual(0f, atNinety.x, 0.0001f);
Assert.AreEqual(2f, atNinety.z, 0.0001f);
Assert.AreEqual(10f, OrbitMath.WrapDegrees(370f), 0.0001f);
```

Test assembly definitions: EditMode uses `"includePlatforms": ["Editor"]`; PlayMode uses an empty include list; both reference `Blackhole.Runtime`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, and `nunit.framework.dll`, with `defineConstraints` containing `UNITY_INCLUDE_TESTS`.

- [ ] **Step 3: Run tests to verify they fail**

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.68f1\Editor\Unity.exe'
& $unity -batchmode -nographics -projectPath 'C:\Users\LOQ\Documents\Unity\Blackhole' -runTests -testPlatform EditMode -testResults 'C:\Users\LOQ\Documents\Unity\Blackhole\TestResults\editmode.xml' -logFile 'C:\Users\LOQ\Documents\Unity\Blackhole\Logs\editmode.log' -quit
```

Expected: compilation failure because `WaveRules`, `AimRules`, and `OrbitMath` do not exist.

- [ ] **Step 4: Implement the three rule classes**

- `EnemyCount` clamps `wave` to at least 1 and returns `baseEnemyCount + (wave - 1) * enemiesAddedPerWave`, clamped to at least 0.
- `BestAimIndex` flattens positions and direction to XZ, returns `-1` for an empty list or a zero direction, otherwise returns the index with the highest dot product; ties favor the earlier index.
- `TryGroundPoint` uses `Plane.Raycast` and returns false for a parallel or away-facing ray.
- `OrbitPosition` uses degrees around the Y axis: `center + new Vector3(cos * radius, 0, sin * radius)`.
- `WrapDegrees` returns a value in `[0, 360)`.

- [ ] **Step 5: Run tests to verify they pass**

Run the command from Step 3. Expected: `TestResults/editmode.xml` contains `<test-run ... result="Passed"`.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime Assets/Tests/EditMode Assets/Tests/PlayMode
git commit -m "feat: add wave, aim, and orbit rules"
```

---

### Task 2: Debris Field, Gravity, Orbit, and Throwing

**Files:**
- Create: `Assets/Scripts/Runtime/Debris/DebrisState.cs`
- Create: `Assets/Scripts/Runtime/Debris/Debris.cs`
- Create: `Assets/Scripts/Runtime/Debris/DebrisField.cs`
- Create: `Assets/Scripts/Runtime/Debris/GravityWell.cs`
- Create: `Assets/Scripts/Runtime/Debris/BlackHoleOrbit.cs`
- Create: `Assets/Scripts/Runtime/Combat/IDamageable.cs`
- Create: `Assets/Tests/PlayMode/DebrisTests.cs`

**Interfaces:**
- Produces:
  - `interface IDamageable { bool IsDead { get; } void TakeHit(); }`
  - `enum DebrisState { Loose, Orbiting, Thrown }`
  - `Debris.Configure(BlackHoleOrbit orbit, Transform player, DebrisField field)`
  - `Debris.ApplyGravity(Vector3 center, float acceleration)`
  - `Debris.EnterOrbit(float radius, float angleDegrees, float angularSpeed)`
  - `Debris.Launch(Vector3 direction, float speed)`
  - `Debris.ReturnToLoose()`
  - `Debris.State`, `Debris.Body`
  - `DebrisField.Instance`, `DebrisField.ActiveCount`
  - `DebrisField.Configure(GameObject[] prefabs, BlackHoleOrbit orbit, Transform player, int maxActiveDebris)`
  - `DebrisField.SpawnLoose(Vector3 position, Vector3 velocity) -> Debris`
  - `DebrisField.SpawnBurst(Vector3 position, int count)`
  - `DebrisField.SpawnBonusNearPlayer(int count)`
  - `DebrisField.SpawnInitialOrbit(int count)`
  - `GravityWell.Configure(float radius, float acceleration, LayerMask debrisMask)`
  - `BlackHoleOrbit.Configure(Transform center, float captureRadius, float minOrbitRadius, float maxOrbitRadius, float angularSpeed)`
  - `BlackHoleOrbit.CaptureRadius`, `BlackHoleOrbit.OrbitingCount`
  - `BlackHoleOrbit.Capture(Debris debris)`
  - `BlackHoleOrbit.Remove(Debris debris)`
  - `BlackHoleOrbit.TryThrow(Vector3 aimDirection, float speed) -> bool`

- [ ] **Step 1: Write failing PlayMode tests**

`DebrisTests.cs` covers:

```csharp
// Capture
Assert.AreEqual(1, orbit.OrbitingCount);
Assert.AreEqual(DebrisState.Orbiting, debris.State);
Assert.IsTrue(debris.Body.isKinematic);

// Empty throw
Assert.IsFalse(orbit.TryThrow(Vector3.right, 12f));

// Throw
Assert.IsTrue(orbit.TryThrow(Vector3.right, 12f));
Assert.AreEqual(0, orbit.OrbitingCount);
Assert.AreEqual(DebrisState.Thrown, debris.State);
Assert.Greater(debris.Body.linearVelocity.x, 0f);

// Cap
Assert.LessOrEqual(field.ActiveCount, 2);

// Destruction while orbiting
Object.Destroy(debris.gameObject);
yield return null;
Assert.AreEqual(0, orbit.OrbitingCount);
```

Also assert `GravityWell` makes a loose debris piece at `(5,0,0)` gain negative X velocity. Build test debris with a Rigidbody (gravity off, Y position frozen), a trigger SphereCollider, and the `Debris` component on layer 8. Destroy all test objects at the end.

- [ ] **Step 2: Run PlayMode tests to verify they fail**

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.68f1\Editor\Unity.exe'
& $unity -batchmode -nographics -projectPath 'C:\Users\LOQ\Documents\Unity\Blackhole' -runTests -testPlatform PlayMode -testResults 'C:\Users\LOQ\Documents\Unity\Blackhole\TestResults\playmode.xml' -logFile 'C:\Users\LOQ\Documents\Unity\Blackhole\Logs\playmode.log' -quit
```

Expected: compilation failure because the debris types do not exist.

- [ ] **Step 3: Implement `Debris`**

- `Configure` caches references and forces `useGravity = false`, Y-position freeze, and continuous collision detection.
- `ApplyGravity` does nothing unless state is `Loose`; otherwise applies `AddForce(direction * acceleration, ForceMode.Acceleration)` in XZ only.
- `EnterOrbit` sets `Orbiting`, makes the body kinematic, zeroes velocity, and stores orbit motion values.
- `Launch` sets `Thrown`, makes the body dynamic, sets `linearVelocity = direction.normalized * speed`, starts a 1.8-second thrown timer, and starts a 0.35-second recapture cooldown.
- `ReturnToLoose` sets `Loose`, makes the body dynamic, keeps momentum, and starts a 0.35-second recapture cooldown.
- `FixedUpdate` returns thrown pieces to loose after the timer, spins the visual child if present, and recaptures a loose piece when inside `orbit.CaptureRadius` and the cooldown has elapsed.
- `OnTriggerEnter` only reacts while `Thrown`: find an `IDamageable`, call `TakeHit()`, then call `ReturnToLoose()`.
- `OnDestroy` calls `orbit.Remove(this)` and `field.ReportDestroyed(this)` when assigned, so neither list keeps a stale reference.

- [ ] **Step 4: Implement `BlackHoleOrbit`**

- Keep a private list of orbit slots (`Debris`, radius, angle).
- `Capture` computes angle and radius from the current offset, clamps radius to the configured range, and calls `debris.EnterOrbit`.
- `FixedUpdate` advances angles by `angularSpeed * Time.fixedDeltaTime` and uses `MovePosition` with `OrbitMath.OrbitPosition`.
- `TryThrow` uses `AimRules.BestAimIndex` over the orbit positions; on success remove the slot and call `Debris.Launch`.
- `Remove` tolerates an absent entry and is safe during destruction.

- [ ] **Step 5: Implement `GravityWell` and `DebrisField`**

- `GravityWell` runs `OverlapSphereNonAlloc` on its mask with `QueryTriggerInteraction.Collide` each `FixedUpdate`; for each `Debris` in `Loose`, call `ApplyGravity`.
- `DebrisField` sets `Instance` in `Awake`. `SpawnLoose` picks a random prefab, instantiates, configures it, applies random spin and the supplied velocity, registers it, and enforces the cap by destroying the oldest active piece.
- `SpawnBurst` spawns pieces with random outward XZ impulses.
- `SpawnBonusNearPlayer` spawns at a random 5–7 unit ring around the player.
- `SpawnInitialOrbit` spawns six pieces at the configured orbit radius and captures them.
- `ReportDestroyed` removes the piece from the active list and from the orbit.

- [ ] **Step 6: Run PlayMode tests to verify they pass**

Run the command from Step 2. Expected: `result="Passed"` in `playmode.xml`.

- [ ] **Step 7: Commit**

```powershell
git add Assets/Scripts/Runtime/Debris Assets/Tests/PlayMode
git commit -m "feat: add debris gravity, orbit, and throwing"
```

---

### Task 3: Player Movement, Aiming, Throwing, Death, and Camera

**Files:**
- Create: `Assets/Scripts/Runtime/Player/PlayerMotion.cs`
- Create: `Assets/Scripts/Runtime/Player/BlackHoleController.cs`
- Create: `Assets/Scripts/Runtime/Camera/CameraFollow.cs`
- Create: `Assets/Tests/EditMode/PlayerMotionTests.cs`
- Create: `Assets/Tests/PlayMode/PlayerTests.cs`

**Interfaces:**
- Consumes: `BlackHoleOrbit.TryThrow`, `BlackHoleOrbit.OrbitingCount`, `DebrisState`.
- Produces:
  - `PlayerMotion.CalculateVelocity(Vector3 currentVelocity, Vector2 input, float maxSpeed, float acceleration, float deltaTime) -> Vector3`
  - `BlackHoleController.Configure(BlackHoleOrbit orbit, Rigidbody body, Camera aimCamera, float moveSpeed = 7f, float acceleration = 40f, float throwSpeed = 20f, float throwCooldown = 0.12f)`
  - `BlackHoleController.TryThrowAt(Vector3 worldAimPoint) -> bool`
  - `BlackHoleController.Die()`, `BlackHoleController.IsDead`, event `Died`
  - `CameraFollow.Instance`
  - `CameraFollow.Configure(Transform target, float pitch, float distance, float followSharpness)`
  - `CameraFollow.SnapToTarget()`
  - `CameraFollow.Shake(float strength, float duration)`

- [ ] **Step 1: Write failing tests**

EditMode:

```csharp
Assert.AreEqual(7f, PlayerMotion.CalculateVelocity(Vector3.zero, new Vector2(1, 1), 7f, 100f, 1f).magnitude, 0.01f);
Assert.AreEqual(0f, PlayerMotion.CalculateVelocity(new Vector3(7, 0, 0), Vector2.zero, 7f, 100f, 1f).magnitude, 0.01f);
```

PlayMode:

```csharp
Assert.IsFalse(controller.TryThrowAt(controller.transform.position)); // no aim direction
controller.TryThrowAt(new Vector3(5, 0, 0));
Assert.AreEqual(0, orbit.OrbitingCount);

controller.Die();
controller.Die();
Assert.AreEqual(1, deathCount);

follow.SnapToTarget();
Assert.That(cameraObject.transform.eulerAngles.x, Is.InRange(64.5f, 65.5f));
Assert.Greater(cameraObject.transform.position.y, target.position.y);
```

- [ ] **Step 2: Run both test suites to verify failure**

Run the EditMode and PlayMode commands from Tasks 1 and 2. Expected: compilation failure for missing player types.

- [ ] **Step 3: Implement `PlayerMotion` and `BlackHoleController`**

- `CalculateVelocity` normalizes input above unit length, targets `input * maxSpeed`, and uses `Vector3.MoveTowards` with `acceleration * deltaTime`; output Y is zero.
- `BlackHoleController` reads WASD from `Keyboard.current` and `Mouse.current`, caches input for `FixedUpdate`, and sets `body.linearVelocity` with Y zero.
- `TryThrowAt` flattens the aim vector, rejects magnitude below 0.2, respects `Time.time`, and calls `orbit.TryThrow`.
- Screen aiming uses `Camera.ScreenPointToRay` and `AimRules.TryGroundPoint` at plane Y 0; if the ray misses, do nothing.
- `Die` is one-shot: zero velocity, set `IsDead`, and invoke `Died`.

- [ ] **Step 4: Implement `CameraFollow`**

- Default pitch 65, distance 14, sharpness 8; enforce pitch in the 60–70 range at runtime.
- Compute `desired = target.position - Quaternion.Euler(pitch, 0, 0) * Vector3.forward * distance`.
- Smooth with `Vector3.SmoothDamp` in `LateUpdate`.
- `Shake` adds a decaying random offset; `SnapToTarget` applies the desired position immediately.
- Set the static `Instance` in `OnEnable` and clear it in `OnDisable`.

- [ ] **Step 5: Run both test suites to verify they pass**

Run the EditMode and PlayMode commands from Tasks 1 and 2. Expected: both XML files report `result="Passed"`.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Player Assets/Scripts/Runtime/Camera Assets/Tests/EditMode Assets/Tests/PlayMode
git commit -m "feat: add player control, throwing, death, and camera"
```

---

### Task 4: Enemies

**Files:**
- Create: `Assets/Scripts/Runtime/Enemies/EnemyMotion.cs`
- Create: `Assets/Scripts/Runtime/Enemies/Enemy.cs`
- Create: `Assets/Tests/EditMode/EnemyMotionTests.cs`
- Create: `Assets/Tests/PlayMode/EnemyTests.cs`

**Interfaces:**
- Consumes: `IDamageable`, `DebrisField.SpawnBurst`, `BlackHoleController.Die`, `CameraFollow.Instance`.
- Produces:
  - `EnemyMotion.SeekDirection(Vector3 enemyPosition, Vector3 targetPosition, float time, float phase, float wobbleStrength) -> Vector3`
  - `Enemy.Initialize(Transform target, DebrisField debrisField)`
  - `Enemy.Die()`
  - `Enemy.IsDead`, event `Died`
  - `Enemy : IDamageable`; `TakeHit()` forwards to `Die()`

- [ ] **Step 1: Write failing tests**

EditMode:

```csharp
var straight = EnemyMotion.SeekDirection(Vector3.zero, new Vector3(5, 0, 0), 0f, 0f, 0f);
Assert.AreEqual(1f, straight.x, 0.001f);
Assert.AreEqual(0f, straight.z, 0.001f);
var wobbled = EnemyMotion.SeekDirection(Vector3.zero, new Vector3(5, 0, 0), 0.5f, 0f, 0.5f);
Assert.AreEqual(1f, wobbled.magnitude, 0.001f);
```

PlayMode:

```csharp
// Contact kills the player once.
Assert.IsTrue(player.IsDead);
Assert.AreEqual(1, deathCount);

// Die is one-shot, reports once, and spawns one debris.
enemy.Die();
enemy.Die();
Assert.AreEqual(1, diedCount);
Assert.AreEqual(1, field.ActiveCount);

// A thrown debris piece kills an enemy.
Assert.IsTrue(enemy.IsDead);
```

- [ ] **Step 2: Run both test suites to verify failure**

Use the established EditMode and PlayMode commands. Expected: compilation failure for missing enemy types.

- [ ] **Step 3: Implement `EnemyMotion`**

Normalize the flattened seek direction, add a perpendicular wobble of `sin(time * 2 + phase) * wobbleStrength`, then normalize again. A zero offset returns `Vector3.zero`.

- [ ] **Step 4: Implement `Enemy`**

- Cache Rigidbody, collider, and renderer; disable gravity and freeze Y position.
- `Initialize` stores the target and debris field.
- `FixedUpdate` applies seek acceleration, clamps horizontal velocity to 4.5, adds random spin torque, and triggers a random lunge every 2.5–5.5 seconds.
- `OnCollisionEnter` calls `player.Die()` when the other object has a `BlackHoleController` and the player is not already dead.
- `Die` is guarded by `IsDead`; disable the collider and body, set the renderer base color to white, invoke `Died`, shake the camera, then after 0.06 seconds spawn one debris burst and destroy the object.
- `TakeHit` calls `Die`; this is the contract thrown debris uses through `IDamageable`.
- Use `_BaseColor` when present so URP Lit materials flash correctly.

- [ ] **Step 5: Verify all tests pass**

Run both suites. Expected: `Passed`.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Enemies Assets/Tests/EditMode Assets/Tests/PlayMode
git commit -m "feat: add goofy enemy movement and one-hit kills"
```

---

### Task 5: Waves, Run State, HUD, and Restart

**Files:**
- Create: `Assets/Scripts/Runtime/Waves/EnemySpawner.cs`
- Create: `Assets/Scripts/Runtime/Run/RunManager.cs`
- Create: `Assets/Scripts/Runtime/UI/HUD.cs`
- Create: `Assets/Scripts/Runtime/UI/RestartButton.cs`
- Modify: `Assets/Scripts/Runtime/Blackhole.Runtime.asmdef` (add `UnityEngine.UI`)
- Create: `Assets/Tests/PlayMode/WaveAndRunTests.cs`

**Interfaces:**
- Consumes: `WaveRules.EnemyCount`, `DebrisField`, `BlackHoleController`, `Enemy`, `HUD`.
- Produces:
  - `EnemySpawner.CurrentWave`, `AliveEnemies`, `TotalEnemiesThisWave`, `IsBreak`, `BreakSecondsRemaining`, `IsRunning`
  - `EnemySpawner.Begin()`, `EnemySpawner.Stop()`
  - `EnemySpawner.GetRandomSpawnPoint() -> Vector3`
  - `RunManager.Instance`, `RunManager.IsGameOver`
  - `RunManager.GameOver(int wave)`, `RunManager.Restart()`
  - `HUD.ShowGameOver(int wave)`, `HUD.IsGameOverVisible`

- [ ] **Step 1: Write failing tests**

```csharp
var point = spawner.GetRandomSpawnPoint();
var distance = Vector3.Distance(player.position, point);
Assert.That(distance, Is.InRange(10f, 12f));
Assert.AreEqual(player.position.y, point.y, 0.001f);

Assert.IsFalse(runManager.IsGameOver);
runManager.GameOver(3);
Assert.IsTrue(runManager.IsGameOver);
Assert.IsTrue(hud.IsGameOverVisible);
Assert.IsFalse(spawner.IsRunning);
```

Set `Time.timeScale = 1f` in teardown.

- [ ] **Step 2: Run PlayMode tests to verify failure**

Use the PlayMode command. Expected: compilation failure for missing wave, run, and UI types.

- [ ] **Step 3: Implement `EnemySpawner`**

- Serialized: `baseEnemyCount = 5`, `enemiesAddedPerWave = 3`, `spawnInterval = 1.2`, `spawnRadiusMin = 13`, `spawnRadiusMax = 17`, `waveBreakSeconds = 5`, `bonusDebrisCount = 3`, and the enemy prefab array.
- `Begin` starts the wave coroutine if it is not already running.
- Each wave increments `CurrentWave`, calls `WaveRules.EnemyCount`, spawns at random angles and radii, and waits `spawnInterval` between spawns.
- After all spawns, wait until `AliveEnemies` is zero.
- Then set `IsBreak`, spawn bonus debris, count `waveBreakSeconds` down once per second, and start the next wave.
- Subscribe to each enemy's `Died` event and remove it from the alive set.
- `Begin` sets `IsRunning`; `Stop` clears `IsRunning`, stops all coroutines, and clears the break state; guard every loop with `runManager.IsGameOver`.
- `GetRandomSpawnPoint` returns a point at the player's Y, using the configured min/max radius.

- [ ] **Step 4: Implement `RunManager`, `HUD`, and restart**

- `RunManager.Awake` sets `Instance`; `RunManager.Start` spawns six initial orbiting debris through `DebrisField`, subscribes to the player's `Died` event, and calls `EnemySpawner.Begin`.
- `GameOver` is one-shot: set `IsGameOver`, stop the spawner, show the HUD panel, and set `Time.timeScale = 0`.
- `Update` restarts on R using `Keyboard.current` while `IsGameOver`.
- `Restart` resets `Time.timeScale` and reloads the active scene.
- `HUD` polls `EnemySpawner` and `BlackHoleOrbit` in `Update` for wave, enemy, debris, and break-countdown text.
- `HUD.ShowGameOver` enables the panel and writes the reached wave.
- `RestartButton.Start` wires its `Button` to `RunManager.Instance.Restart`.

- [ ] **Step 5: Run PlayMode tests to verify they pass**

Use the PlayMode command. Expected: `Passed`.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime Assets/Tests/PlayMode
git commit -m "feat: add endless waves, run state, HUD, and restart"
```

---

### Task 6: Generated Layers, Materials, Prefabs, and Main Scene

**Files:**
- Create: `Assets/Editor/Blackhole.Editor.asmdef`
- Create: `Assets/Editor/ProjectBootstrap.cs`
- Create: `Assets/Tests/EditMode/GeneratedAssetsTests.cs`
- Create: `Assets/Tests/PlayMode/MainSceneSmokeTests.cs`

**Interfaces:**
- Consumes every runtime component created above.
- Editor assembly references `Blackhole.Runtime`, `Unity.InputSystem`, and `UnityEngine.UI`.
- Produces:
  - `Blackhole.Editor.ProjectBootstrap.BuildAll()` callable from batch mode.
  - `Assets/Scenes/Main.unity`
  - Debris prefabs and three enemy prefabs.
  - Configured player, camera, ground, lights, spawner, run manager, and HUD.

- [ ] **Step 1: Write the failing generated-assets tests**

```csharp
Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Main.unity"));
Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Cube.prefab"));
Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Sphere.prefab"));
Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Triangle.prefab"));
Assert.IsTrue(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == "Assets/Scenes/Main.unity"));
```

- [ ] **Step 2: Run EditMode tests to verify they fail**

Use the EditMode command. Expected: failing assertions because the scene and prefabs do not exist.

- [ ] **Step 3: Implement `ProjectBootstrap.BuildAll`**

- Ensure layers `Player`, `Debris`, and `Enemy` exist in `TagManager.asset`; reuse an existing named layer instead of duplicating.
- Create URP Lit materials: black player, pale debris, red enemy, dark ground. Enable emission on debris and enemies.
- Generate and save a triangular prism mesh at `Assets/Meshes/TriangularPrism.asset` with a convex MeshCollider.
- Generate a debris prefab with a trigger SphereCollider, Rigidbody, `Debris`, and a child visual mesh; make three visual variants.
- Generate cube, sphere, and triangular-prism enemy prefabs with a non-trigger collider, Rigidbody, `Enemy`, and a red child visual.
- Create `Main.unity` with:
  - `Player`: tagged Player, layer Player, sphere visual, Rigidbody with gravity off and Y/rotation constraints, SphereCollider, `BlackHoleController`, `GravityWell`, `BlackHoleOrbit`, and a small child point light.
  - `Main Camera`: `CameraFollow` targeting the player, pitch 65, distance 14, and dark background.
  - Ground plane with no collider, large enough for long runs.
  - Directional light.
  - `GameManager`: `DebrisField`, `EnemySpawner`, `RunManager`, wired to the player and prefab arrays.
  - `HUD`: Canvas with wave, enemy, debris, break, controls, and game-over panel text; `HUD` wired to the spawner and orbit. Add an EventSystem with `InputSystemUIInputModule` (call `AssignDefaultActions()`) and a restart button.
- Set the camera pitch and all serialized defaults from the design.
- Save the scene, add it as the first enabled build scene, and save assets.
- `BuildAll` must be idempotent: replace generated assets and the scene without duplicating layers.

- [ ] **Step 4: Run the bootstrap and verify it passes**

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.68f1\Editor\Unity.exe'
& $unity -batchmode -nographics -projectPath 'C:\Users\LOQ\Documents\Unity\Blackhole' -executeMethod Blackhole.Editor.ProjectBootstrap.BuildAll -logFile 'C:\Users\LOQ\Documents\Unity\Blackhole\Logs\bootstrap.log' -quit
```

Expected: exit code 0 and `bootstrap.log` contains the completion message. Verify `Assets/Scenes/Main.unity` exists. Then run the EditMode tests and confirm `Passed`.

- [ ] **Step 5: Write the failing scene smoke test**

`MainSceneSmokeTests.cs` loads `Main`, waits two frames, and asserts:

```csharp
Assert.IsNotNull(Object.FindFirstObjectByType<BlackHoleController>());
Assert.AreEqual(6, Object.FindFirstObjectByType<BlackHoleOrbit>().OrbitingCount);
var pitch = Object.FindFirstObjectByType<CameraFollow>().transform.eulerAngles.x;
Assert.That(pitch, Is.InRange(60f, 70f));
Assert.IsNotNull(Object.FindFirstObjectByType<EnemySpawner>());
Assert.IsNotNull(Object.FindFirstObjectByType<HUD>());
```

Then wait 1 second and assert `spawner.AliveEnemies >= 1`.

- [ ] **Step 6: Run all tests and fix scene wiring**

Run EditMode and PlayMode. Fix any unwired references or missing assets until both report `Passed`.

- [ ] **Step 7: Commit**

```powershell
git add Assets/Editor Assets/Scenes/Main.unity Assets/Prefabs Assets/Materials Assets/Meshes Assets/Tests Assets/Settings
git commit -m "feat: generate playable main scene and prefabs"
```

---

### Task 7: Full Verification and Playable Handoff

**Files:**
- Modify only files needed for fixes found during verification.

**Interfaces:**
- No new interfaces.

- [ ] **Step 1: Run the complete automated suite**

Run EditMode and PlayMode commands. Expected: both XML result files contain `result="Passed"`.

- [ ] **Step 2: Verify the scene opens with no errors**

Open the project in Unity, open `Assets/Scenes/Main.unity`, and confirm the console is clear. This step requires the user's Unity Editor if batch mode cannot render.

- [ ] **Step 3: Manual acceptance pass**

- WASD moves; camera follows at a 65-degree pitch.
- Six debris orbit; they keep spinning while idle.
- Loose debris is pulled in and joins the orbit.
- Left click throws toward the cursor; the ring thins.
- Enemies spawn outside view, spin, wobble, and chase.
- Touching an enemy shows game over; R and the restart button work.
- A thrown hit flashes the enemy white, kills it, and produces collectible debris.
- Clearing a wave shows the serialized 5-second countdown and drops three debris.
- The next wave has more enemies.
- HUD wave, enemy, debris, and countdown values update.

- [ ] **Step 4: Commit fixes**

```powershell
git add -A
git commit -m "fix: polish playable verification issues"
```

---

## Self-Review

- Spec coverage: player movement, mouse aim, orbit, gravity, throwing, enemies, shape variety, wave spawn ring, increasing enemy count, break timer, bonus debris, HUD, death, and restart all map to Tasks 1–6.
- Interface names are consistent across tasks: `IDamageable`, `DebrisState`, `BlackHoleOrbit`, `DebrisField`, `GravityWell`, `BlackHoleController`, `CameraFollow`, `EnemyMotion`, `Enemy`, `EnemySpawner`, `RunManager`, and `HUD`.
- `IDamageable` decouples debris from the enemy type, so Task 2 compiles before enemies exist.
- Review Focus items are covered by ground-ray failure, empty-throw, one-shot death, debris cap/destruction, and game-over spawner-stop tests.
- Tuning values remain serialized and are assigned by the scene bootstrap.
