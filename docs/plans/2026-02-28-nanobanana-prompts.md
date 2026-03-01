# Nanobanana Sprite Generation Prompts

All sprites are for a 2D space fleet battle game. Dark space background in-game.

**Global style notes for all prompts:**
- Clean sci-fi vector art style, glowing neon accents, sharp lines
- PNG with fully transparent background
- High contrast so sprites read well at small sizes (64-256px display)

---

## SHIP HULL SPRITES (3)

These must be **white/light gray monochrome** (no color) so they can be tinted to any color at runtime. Include subtle shading/gradients in grayscale for depth.

Attach the corresponding reference screenshot to each prompt so nanobanana can see the shape.

---

### Prompt 1: Fighter Hull

> **Attach:** fighter.png screenshot as reference
>
> Create a spaceship sprite for a 2D game. Clean sci-fi vector art style. PNG, transparent background, 256x256 pixels.
>
> The ship is a **fighter** — fast and agile. It faces RIGHT (nose pointing to the right edge).
>
> Shape (see reference image): A sharp delta/arrow silhouette with a pointed nose on the right. Two swept-back wings extend from the mid-body toward the upper-left and lower-left. The tail (left side) has a concave notch between the wings, with a small circular engine nozzle.
>
> The ship must be drawn in **white and light gray only** (monochrome/grayscale). Use subtle gray gradients and panel line details for depth, but no color. It will be tinted programmatically.
>
> **Important for weapon attachment:** The design must have:
> - A clear pointed nose tip at the right (~90% from left edge, vertically centered) — this is where forward weapons mount
> - Visible hull surface at center-top (~50% from left, ~30% from top) and center-bottom (~50% from left, ~70% from top) — this is where turrets/torpedoes mount
> - Wing surfaces at upper-left (~25% from left, ~10% from top) and lower-left (~25% from left, ~90% from top) — this is where missiles mount
>
> These areas should have flat, visible hull surface (not empty space) so weapon overlays look attached, not floating.

---

### Prompt 2: Cruiser Hull

> **Attach:** cruiser.png screenshot as reference
>
> Create a spaceship sprite for a 2D game. Clean sci-fi vector art style. PNG, transparent background, 256x256 pixels.
>
> The ship is a **cruiser** — balanced and commanding. It faces RIGHT (nose pointing to the right edge).
>
> Shape (see reference image): A wide, symmetric hexagonal hull — almost pentagonal. The nose comes to a point on the right. The top and bottom edges are roughly parallel and flat. The rear (left side) has a flat stern with an engine section. A diamond-shaped bridge/cockpit window sits in the forward-center area of the hull as a raised section.
>
> The ship must be drawn in **white and light gray only** (monochrome/grayscale). Use subtle gray gradients and panel lines for depth, but no color.
>
> **Important for weapon attachment:** The design must have:
> - A clear nose point at the right (~92% from left edge, vertically centered) — forward weapons mount here
> - Wide hull surface at center-top (~50% from left, ~25% from top) and center-bottom (~50% from left, ~75% from top) — turrets and torpedoes mount here
> - The top and bottom edges of the hull should be broad and flat (not tapered) so weapon overlays sit naturally on the hull surface

---

### Prompt 3: Bomber Hull

> **Attach:** bomber.png screenshot as reference
>
> Create a spaceship sprite for a 2D game. Clean sci-fi vector art style. PNG, transparent background, 256x256 pixels.
>
> The ship is a **bomber** — heavy and durable. It faces RIGHT (nose pointing to the right edge).
>
> Shape (see reference image): A wide, low-profile rectangular body. A short triangular nose wedge protrudes from the right side. Two triangular dorsal fins extend from the rear — one pointing up, one pointing down, like a vertical stabilizer. Dual engine nozzles at the rear-left.
>
> The body is wider than it is tall. The fins extend well above and below the main body.
>
> The ship must be drawn in **white and light gray only** (monochrome/grayscale). Use subtle gray gradients and armor plating details for depth, but no color.
>
> **Important for weapon attachment:** The design must have:
> - A nose tip at center-right (~75% from left edge, vertically centered) — forward weapons mount here (shorter nose than fighter/cruiser)
> - Hull surface at center-top (~50% from left, ~40% from top) and center-bottom (~50% from left, ~60% from top) — the main body is narrow vertically, turrets mount on these edges
> - Fin/wing surfaces at upper-rear (~25% from left, ~15% from top) and lower-rear (~25% from left, ~85% from top) — missiles mount on the fins

---

## WEAPON OVERLAY SPRITES (5)

These are **full color** (not grayscale). They will be drawn ON TOP of the ship hull sprites at specific mount points. Keep them small and punchy.

---

### Prompt 4: Laser Cannon

> Create a weapon effect sprite for a 2D space game. Clean sci-fi style. PNG, transparent background, 64x64 pixels.
>
> **Laser cannon** — two short parallel horizontal red/orange laser beams firing to the right. Each beam is a bright red line with a glowing orange-red tip (small circular flare). The beams are close together (about 8px apart vertically), centered in the image. The left side fades/tapers in (where it connects to the ship), the right side has bright glowing tips.
>
> Colors: bright red beams (#FF3333), orange-red glow tips (#FF6633), subtle bloom/glow around tips.

---

### Prompt 5: Plasma Turret

> Create a weapon effect sprite for a 2D space game. Clean sci-fi style. PNG, transparent background, 64x64 pixels.
>
> **Plasma turret** — a single glowing energy orb/sphere centered in the image. The outer shell is semi-transparent purple/violet. The inner core is bright white/pale lavender. A thin purple outline ring surrounds the orb. The orb should have a pulsing, energetic feel with soft glow radiating outward.
>
> Colors: medium purple outer (#9370DB at ~60% opacity), white core (#FFFFFF at ~40% opacity), purple outline (#9370DB).

---

### Prompt 6: Missile Rack

> Create a weapon effect sprite for a 2D space game. Clean sci-fi style. PNG, transparent background, 64x64 pixels.
>
> **Missile rack** — two small triangular missiles arranged vertically (stacked), pointing to the right. Each missile is a small orange-red triangle (like an arrowhead pointing right) with a darker red outline. They are compact and military-looking, evenly spaced in the image.
>
> Colors: orange-red fill (#FF4500 at ~85% opacity), dark red borders (#8B0000).

---

### Prompt 7: Torpedo Bay

> Create a weapon effect sprite for a 2D space game. Clean sci-fi style. PNG, transparent background, 64x64 pixels.
>
> **Torpedo** — a single horizontal oval/capsule shape centered in the image, oriented horizontally (wider than tall). The torpedo is cyan/teal colored with a subtle glow. A thin lighter cyan outline. A small vertical line below it suggests the mounting pylon connecting it to the ship hull.
>
> Colors: dark cyan fill (#008B8B at ~80% opacity), cyan outline/glow (#00FFFF at ~60% opacity), thin mounting line in faint cyan.

---

### Prompt 8: Ion Beam

> Create a weapon effect sprite for a 2D space game. Clean sci-fi style. PNG, transparent background, 96x64 pixels (wider than tall).
>
> **Ion beam** — an energy cone expanding from left to right. The narrow end (left) starts as a small circular emitter ring. The beam expands into a wide triangular cone toward the right. Two layers: an inner brighter cyan cone and a fainter outer cyan cone. A thin white line runs down the center axis (the beam core).
>
> Colors: inner cone cyan (#00FFFF at ~20% opacity), outer cone (#00FFFF at ~10% opacity), emitter ring (#00FFFF solid), white core line (#FFFFFF at ~70% opacity).

---

## Notes for sprite generation

1. **Orientation:** All ships and forward-firing weapons face RIGHT
2. **Ship hulls are grayscale only** — they get color-tinted in code
3. **Weapon sprites are full color** — they are NOT tinted
4. **Transparent backgrounds** on everything — no solid backgrounds
5. **Attach the reference screenshots** to the ship hull prompts so the AI can match proportions
6. After receiving sprites, mount point percentages in code will be fine-tuned to match
