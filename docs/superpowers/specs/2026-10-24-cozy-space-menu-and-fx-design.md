# Cozy space UI, main menu, pause, and particles — design

Date: 2026-10-24
Project: Blackhole (Unity 6.0.68f1, URP, uGUI, Input System)

## Intent

Make the game feel like one coherent, cozy little space toy: a warm black hole you
want to sit with, not a cold arcade shooter.

Approved in conversation:

- Mood: **cozy**.
- Menu composition: **C — poster / minimal** (big gradient title upper-left, quiet
  hairline menu lower-left, lots of empty space).
- The menu background is a **live 3D scene**, not a static image: black hole off to
  the right, camera low and off-axis, debris drifting in orbit.
- The black hole in the menu **moves** — slow wandering path — and **leans toward the
  mouse** a little, as if it noticed you.
- Opening the menu **fades in slowly** ("showing itself"): black → scene, title and
  menu items arriving a beat later, camera easing in.
- In-game **Esc pauses**, with Resume and Return to main menu.
- **Particle effects** for the black hole moving and for enemy destruction.

## Experience

### Main menu

- Full-bleed 3D scene: near-black blue sky with a procedurally painted starfield and
  soft nebula, a black hole just right of centre, warm amber accretion disc with a
  cool cyan outer wisp, a thin bright photon rim, and 8-12 chunks of debris tracing
  lazy, slightly tilted orbits. Dust motes spiral inward.
- Camera: ~22° above the horizon, slight yaw, 40° FOV, looking up at the disc. Slow
  breathing drift plus a small mouse parallax. During the intro it eases forward from
  farther away.
- The hole wanders on a slow Lissajous path around its anchor and leans up to ~1.2
  units toward the pointer (spring-smoothed, clamped), so the whole system subtly
  follows the cursor.
- UI (overlay canvas):
  - upper-left kicker `A COZY GRAVITY WELL`, then `BLACKHOLE` at ~132px with a
    white → ice-blue → warm-amber gradient, letter-spaced.
  - lower-left hairline menu: `PLAY 01`, `HOW TO PLAY 02`, `QUIT 03`. Hover/keyboard
    selection brightens the label, slides it slightly, and lights the hairline.
  - bottom-left `v0.1`, bottom-right `ORBIT · GATHER · THROW`.
  - `HOW TO PLAY` opens a quiet card listing controls; Esc/click closes it.
- Intro (unscaled time): black overlay alpha 1→0 over ~1.7s; title group fades and
  rises ~24px with a 0.25s delay; menu items stagger in at 0.1s intervals; camera
  dollies from distance 24 to 17 over ~2.5s ease-out.
- `PLAY` (or Enter) fades to black over 0.5s and loads `Main`.

### In-game HUD

- Same typeface and palette, much quieter than now: rounded translucent cards,
  letter-spaced small caps.
- Top-left: `WAVE` + number, enemy count underneath. Top-right: `DEBRIS` + count.
  Centre: `NEXT WAVE IN n` fading in during breaks. Bottom: one-line control hint.
- Game-over card: `RUN OVER`, waves reached, `RESTART` and `RETURN TO MAIN MENU`
  buttons (existing `RestartButton` on the restart button keeps the smoke test green).

### Pause

- Esc toggles. Ignored while game over.
- `Time.timeScale = 0`; game input must not leak (black hole throws/movement gated).
- Full-screen dim + centred card in the C style: `PAUSED`, `RESUME`,
  `RETURN TO MAIN MENU`, hint `ESC TO RESUME`.
- Unscaled fade/scale animation. Resume restores timeScale; return-to-menu restores
  it and loads `MainMenu`.

### Particles

- Black hole ambient: a disc of dust particles orbiting the hole and slowly
  spiralling in, warm and cyan additive motes, always on.
- Black hole movement trail: soft sparks emitted in world space proportional to the
  hole's planar speed, fading behind it as it drifts (menu and gameplay).
- Enemy destruction: warm spark burst + a few cyan flecks + a quick expanding ring
  and a one-frame point-light flash; camera keeps its existing small shake.
- Debris capture: a tiny inward sparkle at the rim (nice-to-have, cheap).

## Look and feel

- Font: Space Grotesk (OFL), bundled TTF; legacy uGUI `Text` (no TMP dependency).
  Letter-spacing approximated with hair spaces for display labels.
- Palette: night `#03050C`, panel `#0B1224` at 85%, text `#EAF2FF`, muted `#8FA9CB`,
  cyan `#7FD4FF`, warm amber `#FFB86B`, deep blue `#0A1124`.
- UI sprites (rounded panel, pill, soft glow, ring) generated procedurally and cached
  at runtime, so no sprite assets to hand-author.
- Post: URP Volume with Bloom (soft, threshold ~1.0), Vignette, slight colour lift —
  makes the emissive disc and sparks glow. Used in both scenes.
- One additive shader (`Blackhole/Additive`) shared by accretion disc, particles,
  photon glow and glow cards. Works in URP, no lighting.

## Technical shape

- Everything is regenerated from `Assets/Editor/ProjectBootstrap.cs`:
  `Blackhole/Rebuild All` builds assets, `Main.unity`, then `MainMenu.unity`, and
  sets build settings to `[MainMenu, Main]`.
- New runtime folders: `UI/` (theme, gradient, buttons, pause), `Menu/` (menu
  controller, camera, scene black hole, orbiting showcase debris), `FX/` (particle
  helpers, bursts, trail).
- `MainMenu.unity` also has its own EventSystem; gameplay scripts are not present
  there.
- `PauseMenu.IsPaused` is a static gate read by `BlackHoleController` and the pause
  code; `RunManager` remains the source of truth for game over.
- New pure helpers (`MenuWander`, `LeanTowards`) get EditMode tests. Existing
  EditMode/PlayMode suites must stay green.

## Non-goals

- No audio pass, no settings screen, no gamepad navigation, no new gameplay
  mechanics, no changes to gameplay balance or camera pitch in `Main`.
- No third-party UI package, no TextMeshPro migration.
