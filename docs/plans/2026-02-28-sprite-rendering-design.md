# Sprite-Based Ship Rendering — Design Document

## Goal

Replace `ShipRenderer.cs` (GDI+ polygon drawing) with `SpriteRenderer.cs` (PNG sprite compositing). Delete all procedural ship/weapon drawing code.

## Decisions

- **Art style:** Sci-fi vector/clean (glowing effects, modern look)
- **Color system:** White/grayscale sprites, tinted at runtime via `ColorMatrix`
- **Weapons:** Separate overlay sprites positioned via mount points
- **Mount points:** Percentage-based constants per ship type, tuned to actual sprite art

## Sprites Required (8 PNGs)

### Ship Hulls (3) — white/grayscale, transparent background, ~256x256px

| # | File | Shape |
|---|------|-------|
| 1 | `fighter.png` | Delta/arrow with two swept-back wings, concave tail, engine nozzle at rear |
| 2 | `cruiser.png` | Elongated hexagon with diamond bridge window in forward section, engine at rear |
| 3 | `bomber.png` | Wide rectangular body, triangular nose wedge, top/bottom dorsal fins, dual engines at rear |

### Weapon Overlays (5) — full color, transparent background, ~64x64px

| # | File | Visual |
|---|------|--------|
| 4 | `laser.png` | Two parallel red beams with orange glow tips |
| 5 | `plasma.png` | Single purple energy orb with bright white core |
| 6 | `missile.png` | Cluster of 2 small orange triangular missiles |
| 7 | `torpedo.png` | Single cyan/teal oval torpedo with faint glow |
| 8 | `ion.png` | Expanding cyan energy cone with white center line |

## Architecture

### File structure

```
Prototype/
  Assets/
    Ships/
      fighter.png
      cruiser.png
      bomber.png
    Weapons/
      laser.png
      plasma.png
      missile.png
      torpedo.png
      ion.png
  UI/
    SpriteRenderer.cs    (NEW)
    ShipRenderer.cs      (DELETE)
```

### SpriteRenderer class

```csharp
static class SpriteRenderer
{
    // Lazy-loaded sprite caches
    static Dictionary<string, Image> _shipSprites;
    static Dictionary<WeaponType, Image> _weaponSprites;

    // Percentage-based mount points per ship type (tuned to sprites)
    static Dictionary<string, MountConfig> _mounts;

    // Same public API as ShipRenderer
    public static void DrawShip(Graphics g, Starship ship, Rectangle bounds)
    {
        // 1. Load & tint hull sprite using ColorMatrix
        // 2. Draw hull centered in bounds
        // 3. Look up mount points for ship type
        // 4. Convert percentage mounts to pixel positions within bounds
        // 5. Draw weapon sprite(s) at mount positions
        // 6. Draw ship name at bottom
    }
}
```

### Mount point system

```csharp
record MountConfig(
    float NoseX, float NoseY,       // nose tip (lasers, ion beam)
    float BodyTopY, float BodyBotY,  // hull edges (plasma, torpedoes)
    float WingTopY, float WingBotY,  // wing tips (missiles)
    float WingX,                     // wing X position
    float BodyCenterX               // center X for body-mounted weapons
);

// Example — tuned after receiving actual sprites:
["Fighter"] = new(NoseX: 0.90f, NoseY: 0.50f, BodyTopY: 0.30f, BodyBotY: 0.70f,
                  WingTopY: 0.10f, WingBotY: 0.90f, WingX: 0.25f, BodyCenterX: 0.50f)
```

### Color tinting

```csharp
float r = ship.ShipColor.R / 255f;
float g = ship.ShipColor.G / 255f;
float b = ship.ShipColor.B / 255f;

var matrix = new ColorMatrix(new float[][] {
    new[] { r, 0, 0, 0, 0 },
    new[] { 0, g, 0, 0, 0 },
    new[] { 0, 0, b, 0, 0 },
    new[] { 0, 0, 0, 1, 0 },
    new[] { 0, 0, 0, 0, 1 },
});
var attrs = new ImageAttributes();
attrs.SetColorMatrix(matrix);
g.DrawImage(sprite, destRect, 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attrs);
```

### Weapon drawing rules

| Weapon | Mount point(s) | How drawn |
|--------|---------------|-----------|
| Laser Cannon | NoseX, NoseY | 1 sprite at nose |
| Plasma Turret | BodyCenterX + BodyTopY, BodyCenterX + BodyBotY | 2 sprites (top & bottom) |
| Missile Rack | WingX + WingTopY, WingX + WingBotY | 2-4 sprites along wings |
| Torpedo Bay | BodyCenterX + BodyTopY, BodyCenterX + BodyBotY | 2 sprites (top & bottom) |
| Ion Beam | NoseX, NoseY | 1 sprite at nose |

### Call sites to change (2)

- `Form1.cs:586` — `ShipRenderer.DrawShip(...)` → `SpriteRenderer.DrawShip(...)`
- `ShipCard.cs:158` — `ShipRenderer.DrawShip(...)` → `SpriteRenderer.DrawShip(...)`

### .csproj change

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\Ships\*.png" />
  <EmbeddedResource Include="Assets\Weapons\*.png" />
</ItemGroup>
```

## Implementation steps

1. Create `Assets/Ships/` and `Assets/Weapons/` folders
2. Add placeholder sprites (or real ones from nanobanana)
3. Add EmbeddedResource entries to .csproj
4. Create `SpriteRenderer.cs` with sprite loading + tinting + compositing
5. Update 2 call sites
6. Delete `ShipRenderer.cs`
7. Tune mount point percentages to match actual sprites

---

## Nanobanana Prompts

See companion file: `2026-02-28-nanobanana-prompts.md`
