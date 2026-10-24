# Blackhole Survival — Design

## Intent

A top-down arcade survival game where the player is a black hole. Debris orbits the player, enemies chase the player, and the player flings debris at them. Kills feed the orbit, so the fight is a loop of gathering, throwing, and surviving.

## Experience

The player moves with WASD and aims with the mouse. Debris constantly spins around the black hole; clicking sends one piece flying toward the cursor. Enemies are red primitives that tumble and wobble toward the player. Touching one kills the player. Hitting one with thrown debris kills it, bursts it into debris, and that debris gets pulled into the orbit.

The game is endless. Each wave brings more enemies. Between waves there is a short break and a small debris drop, so the player can regroup and reload the orbit.

## Goals

- A complete playable loop: move, gather, throw, kill, absorb, survive.
- Orbiting debris that feels physical and alive, even while idle.
- Goofy enemy movement and satisfying, physical kills.
- An endless wave loop that gets harder by increasing enemy count.
- Clear top-down presentation using only Unity primitives and simple materials.
- Tunable values in the Inspector, including wave-break timer.

## Non-goals

- Jumping or vertical gameplay.
- Realistic orbital mechanics.
- Upgrades, weapons, abilities, shops, or meta-progression.
- Multiple levels, bosses, story, or cutscenes.
- Polished art, animation, sound, or mobile support.
- Multiplayer or saving.

## Constraints

- Unity 6 (6000.0.68f1), URP template.
- Input System package is already installed; use the existing `InputSystem_Actions` asset where practical.
- No external art, audio, or paid assets. Use primitive meshes, generated meshes, and code-created materials.
- Single playable scene.
- Top-down 3D, floating on a horizontal plane. No jumping.

## Player

- A black sphere, visually distinct against a dark arena. A subtle rim light or small light source may sell the silhouette.
- Dynamic Rigidbody with gravity off, Y position locked, rotation locked. Movement uses WASD with a serialized speed and acceleration.
- Mouse position is raycast onto the play plane to produce an aim point.
- Left click throws one orbiting debris piece toward the aim point.
- Contact with an enemy ends the run immediately.
- On death, gameplay stops and a game-over overlay shows the wave reached and how to restart.

## Debris and orbit

- Debris uses its own layer, separate from enemies. Every piece has a Rigidbody and a collider.
- A gravity script on the player finds loose debris within a serialized attraction radius and pulls it inward.
- Once a loose piece reaches the capture radius, it joins the orbit.
- Orbiting pieces turn around the player at a serialized angular speed, at varied orbit radii. They keep spinning while the player moves and while the player is idle.
- Orbiting debris does not damage enemies. Only thrown debris can.
- Throwing:
  - The piece whose orbit angle best matches the cursor direction leaves the ring.
  - It is launched toward the cursor with a serialized throw speed.
  - While thrown, it becomes damage-enabled. On its first enemy hit, it kills the enemy, then becomes inert and re-attractable after a short grace period.
  - A missed throw flies, slows, and eventually returns to the gravity well.
  - The throw should feel sudden and snappy, not charged or lobbed.
- Each kill produces new debris that bursts outward and is then pulled into orbit.
- A serialized debris cap prevents unbounded growth. When the cap is reached, the oldest orbiting piece is removed to make room.
- Start of run: six debris pieces already orbiting.

## Enemies

- Shapes: red cubes, red spheres, and red triangular prisms. Shapes are chosen at random.
- Movement is dynamic, not a straight slide:
  - They continuously accelerate toward the player.
  - They spin constantly and wobble side to side.
  - Occasional small lunge impulses make them feel erratic.
  - Enemy-enemy collisions are enabled so they bump and jostle each other.
- Enemies have Rigidbodies with gravity off and Y movement/rotation constraints appropriate to the top-down plane.
- Contact with the player kills the player.
- A thrown debris hit kills the enemy instantly:
  - Brief white flash.
  - Burst into debris with outward impulses.
  - The thrown debris then returns to the orbit loop.
- Enemies do not physically collide with orbiting debris.

## Waves and spawning

- Endless mode. There is no victory state.
- A wave ends only after every enemy in that wave has spawned and been killed.
- Then a break begins:
  - Serialized `waveBreakSeconds`, default 5 seconds.
  - HUD shows a countdown.
  - Three bonus debris spawn near the player and drift in.
- After the break, the next wave starts.
- Difficulty increases only by enemy count for this version:
  - `baseEnemyCount` at wave 1.
  - `enemiesAddedPerWave` added each wave.
  - All other enemy behavior values stay fixed unless the player changes them.
- Spawning:
  - Enemies spawn at a random angle around the player.
  - Spawn distance is random within a serialized min/max radius, currently outside the camera view.
  - Spawn interval is serialized.
  - Each enemy gets a slight random placement and initial impulse so groups do not stack perfectly.
- HUD: wave number, enemies remaining, next-wave countdown, debris count, and game-over panel. Also show the controls at the start.
- Restart: reload the scene with R or by clicking the restart button.

## Camera and presentation

- Camera follows the player smoothly from above at a pitch of 60–70 degrees (default 65), not a straight 90-degree top-down view, so the 3D shapes read clearly.
- Dark ground plane, black background, light debris, red enemies.
- Kill feedback: white flash, debris burst, and a subtle camera shake.
- No audio in this version.

## Architecture

Single scene with these responsibilities separated:

- `BlackHoleController` — movement, aim, throw input, death.
- `GravityWell` — discovers loose debris in range and applies attraction.
- `Debris` — loose/orbiting/thrown state machine, damage trigger, recapture.
- `BlackHoleOrbit` — owns the orbiting pieces and their angles/radii.
- `Enemy` — seek movement, wobble/spin, contact death, hit reaction.
- `EnemySpawner` — wave state, break timer, spawn placement, difficulty count.
- `RunManager` — playing/break/game-over state, restart.
- `CameraFollow` — smooth follow and hit shake.
- `HUD` — text and game-over display.
- Shape helper — builds the triangular prism mesh and enemy visuals.

Layers:

- `Player`
- `Debris`
- `Enemy`

Debris is a trigger-based damage source only while thrown. Physics collision rules keep orbiting debris from blocking or damaging enemies or the player. Enemy-enemy and enemy-player collisions are enabled.

## Serialized tuning values

- Player move speed, acceleration, throw speed, throw cooldown.
- Attraction radius, capture radius, orbit speed, orbit radius range, debris cap.
- Enemy seek acceleration, max speed, spin speed range, wobble amount/frequency, lunge interval/strength.
- Enemy contact radius/collider setup.
- Wave break seconds, base enemy count, enemies added per wave, spawn interval, spawn radius min/max.
- Camera follow smoothing, height, pitch (default 65 degrees), and distance.
- Camera shake strength/duration.

## Acceptance criteria

- Pressing Play produces a working top-down scene with a black-hole player and six orbiting debris pieces.
- WASD moves the player; the camera follows; the mouse aims; left click flings a piece toward the cursor.
- Loose debris is visibly pulled in and joins the orbit. The orbit continues spinning while idle.
- Enemies spawn around the player at randomized angles and distances, tumble toward the player, and kill the player on contact.
- A thrown piece kills an enemy on hit, the enemy bursts into debris, and that debris can be absorbed.
- Clearing a wave starts the serialized break countdown, spawns bonus debris, then starts a larger wave.
- The HUD reports wave, enemy count, break countdown, and debris count.
- Game over stops the action, reports the wave reached, and R restarts.
- Key values are visible and editable in the Inspector.
- No console errors during a full wave cycle.

## Builder calls

- Camera is perspective with a 60–70 degree pitch (default 65), above and slightly behind the player, rather than perfectly orthographic or a 90-degree top-down view.
- Debris uses kinematic motion while orbiting for a readable ring; it becomes dynamic when loose or thrown. This keeps the orbit stable while preserving Rigidbody/collider behavior.
- Enemy difficulty scales only through count in this version.
- Debris cap defaults to 48.
- One enemy yields one new debris piece, plus three bonus pieces per completed wave.
- The camera shake is subtle and on by default.
