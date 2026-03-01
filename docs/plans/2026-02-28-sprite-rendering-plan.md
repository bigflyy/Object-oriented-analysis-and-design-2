# Sprite-Based Ship Rendering — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace GDI+ polygon-based ShipRenderer with PNG sprite compositing (SpriteRenderer), using 8 pre-made sprites.

**Architecture:** New static `SpriteRenderer` class loads ship hull PNGs (white/grayscale, tinted at runtime via ColorMatrix) and weapon overlay PNGs (full color). Ships and weapons are composited by drawing the hull first, then overlaying weapon sprites at percentage-based mount points. Same public API — only 2 call sites change.

**Tech Stack:** .NET 8, WinForms, System.Drawing (GDI+), EmbeddedResources for PNG loading

---

### Task 1: Add sprite assets as embedded resources

**Files:**
- Modify: `lab01/Prototype/Prototype/Prototype.csproj`

**Step 1: Add EmbeddedResource entries to .csproj**

Open `Prototype.csproj` and add an ItemGroup so the PNGs are compiled into the assembly:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>disable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include="Assets\Ships\*.png" />
    <EmbeddedResource Include="Assets\Weapons\*.png" />
  </ItemGroup>

</Project>
```

**Step 2: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded with 0 errors.

**Step 3: Verify resources are embedded**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj -v:n 2>&1 | grep -i "Assets"`
Expected: Should show the 8 PNG files being included as embedded resources.

---

### Task 2: Create SpriteRenderer — sprite loading

**Files:**
- Create: `lab01/Prototype/Prototype/UI/SpriteRenderer.cs`

**Step 1: Create the SpriteRenderer class with sprite loading**

Create `lab01/Prototype/Prototype/UI/SpriteRenderer.cs` with lazy-loaded sprite dictionaries:

```csharp
using System.Drawing.Imaging;
using System.Reflection;
using Prototype.Models;

namespace Prototype.UI
{
    public static class SpriteRenderer
    {
        private static readonly Dictionary<string, Bitmap> ShipSprites = new();
        private static readonly Dictionary<WeaponType, Bitmap> WeaponSprites = new();
        private static bool _loaded;

        private static void EnsureLoaded()
        {
            if (_loaded) return;

            var asm = Assembly.GetExecutingAssembly();

            // Load ship hulls
            ShipSprites["Fighter"] = LoadSprite(asm, "Assets.Ships.fighter.png");
            ShipSprites["Cruiser"] = LoadSprite(asm, "Assets.Ships.cruiser.png");
            ShipSprites["Bomber"]  = LoadSprite(asm, "Assets.Ships.bomber.png");

            // Load weapon overlays
            WeaponSprites[WeaponType.LaserCannon] = LoadSprite(asm, "Assets.Weapons.laser.png");
            WeaponSprites[WeaponType.PlasmaTurret] = LoadSprite(asm, "Assets.Weapons.plasma.png");
            WeaponSprites[WeaponType.MissileRack]  = LoadSprite(asm, "Assets.Weapons.missile.png");
            WeaponSprites[WeaponType.TorpedoBay]   = LoadSprite(asm, "Assets.Weapons.torpedo.png");
            WeaponSprites[WeaponType.IonBeam]      = LoadSprite(asm, "Assets.Weapons.ion.png");

            _loaded = true;
        }

        private static Bitmap LoadSprite(Assembly asm, string resourceSuffix)
        {
            // Resource names use the default namespace + folder path with dots
            string name = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(resourceSuffix))
                ?? throw new FileNotFoundException($"Embedded resource not found: {resourceSuffix}");

            using var stream = asm.GetManifestResourceStream(name);
            var bmp = new Bitmap(stream);

            // AI-generated sprites often have solid dark backgrounds instead of
            // transparency. Sample the top-left corner pixel and make that color
            // transparent. This works because all sprites have uniform backgrounds
            // and no ship/weapon art touches the corner pixels.
            Color corner = bmp.GetPixel(0, 0);
            if (corner.A > 200) // only strip if the corner is mostly opaque
            {
                bmp.MakeTransparent(corner);
            }

            return bmp;
        }
    }
}
```

**Step 2: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded. (Class exists but DrawShip not yet implemented.)

---

### Task 3: Create SpriteRenderer — mount point config

**Files:**
- Modify: `lab01/Prototype/Prototype/UI/SpriteRenderer.cs`

**Step 1: Add percentage-based mount point configuration**

Add the mount config record and per-ship-type mount points inside `SpriteRenderer`. These are percentages (0.0–1.0) of the hull sprite's bounding rectangle.

Add this inside the `SpriteRenderer` class, after the `_loaded` field:

```csharp
        // Mount points as percentages of the ship bounding rectangle.
        // (0,0) = top-left, (1,1) = bottom-right.
        // Tuned to match the actual sprite artwork.
        private record MountConfig(
            float NoseX, float NoseY,
            float BodyTopY, float BodyBotY,
            float BodyCenterX,
            float WingTopY, float WingBotY,
            float WingX
        );

        private static readonly Dictionary<string, MountConfig> Mounts = new()
        {
            // Fighter: sharp nose far right, swept wings far apart
            ["Fighter"] = new(
                NoseX: 0.92f, NoseY: 0.45f,
                BodyTopY: 0.32f, BodyBotY: 0.58f,
                BodyCenterX: 0.50f,
                WingTopY: 0.10f, WingBotY: 0.80f,
                WingX: 0.22f
            ),
            // Cruiser: wide hexagonal body, broad top/bottom
            ["Cruiser"] = new(
                NoseX: 0.90f, NoseY: 0.50f,
                BodyTopY: 0.18f, BodyBotY: 0.82f,
                BodyCenterX: 0.45f,
                WingTopY: 0.18f, WingBotY: 0.82f,
                WingX: 0.20f
            ),
            // Bomber: short nose, narrow body, fins extend far
            ["Bomber"] = new(
                NoseX: 0.80f, NoseY: 0.50f,
                BodyTopY: 0.35f, BodyBotY: 0.65f,
                BodyCenterX: 0.45f,
                WingTopY: 0.12f, WingBotY: 0.88f,
                WingX: 0.25f
            ),
        };
```

**Step 2: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded.

---

### Task 4: Create SpriteRenderer — DrawShip method (hull + tinting)

**Files:**
- Modify: `lab01/Prototype/Prototype/UI/SpriteRenderer.cs`

**Step 1: Implement DrawShip with hull tinting**

Add the main `DrawShip` method and the color tinting helper inside `SpriteRenderer`:

```csharp
        /// Main drawing method — same signature as the old ShipRenderer.DrawShip.
        public static void DrawShip(Graphics g, Starship ship, Rectangle bounds)
        {
            EnsureLoaded();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (!ShipSprites.TryGetValue(ship.ShipType, out var hullSprite)) return;
            if (!Mounts.TryGetValue(ship.ShipType, out var mounts)) return;

            // Reserve space at bottom for ship name
            int nameSpace = 25;
            var drawBounds = new Rectangle(bounds.X, bounds.Y,
                bounds.Width, bounds.Height - nameSpace);

            // Scale hull sprite to fit drawBounds while preserving aspect ratio
            float scaleX = (float)drawBounds.Width / hullSprite.Width;
            float scaleY = (float)drawBounds.Height / hullSprite.Height;
            float scale = Math.Min(scaleX, scaleY);

            int drawW = (int)(hullSprite.Width * scale);
            int drawH = (int)(hullSprite.Height * scale);
            int drawX = drawBounds.X + (drawBounds.Width - drawW) / 2;
            int drawY = drawBounds.Y + (drawBounds.Height - drawH) / 2;

            var destRect = new Rectangle(drawX, drawY, drawW, drawH);

            // Draw tinted hull
            DrawTinted(g, hullSprite, destRect, ship.ShipColor);

            // Draw weapon overlay
            DrawWeapon(g, ship.Weapon.Type, destRect, mounts);

            // Draw ship name at bottom center
            using var font = new Font("Segoe UI", 9, FontStyle.Bold);
            var nameSize = g.MeasureString(ship.Name, font);
            g.DrawString(ship.Name, font, Brushes.White,
                bounds.X + bounds.Width / 2 - nameSize.Width / 2,
                bounds.Bottom - nameSpace);
        }

        /// Draws an image with a color tint using ColorMatrix.
        /// The sprite should be white/grayscale — the tint multiplies RGB channels.
        private static void DrawTinted(Graphics g, Image sprite, Rectangle dest, Color tint)
        {
            float r = tint.R / 255f;
            float gr = tint.G / 255f;
            float b = tint.B / 255f;

            var matrix = new ColorMatrix(new float[][] {
                new[] { r,  0,  0,  0, 0 },
                new[] { 0,  gr, 0,  0, 0 },
                new[] { 0,  0,  b,  0, 0 },
                new[] { 0,  0,  0,  1f, 0 },
                new[] { 0,  0,  0,  0, 1f },
            });

            using var attrs = new ImageAttributes();
            attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(sprite, dest,
                0, 0, sprite.Width, sprite.Height,
                GraphicsUnit.Pixel, attrs);
        }
```

**Step 2: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded. (DrawWeapon not yet implemented — add a stub or implement in next task.)

---

### Task 5: Create SpriteRenderer — weapon overlay drawing

**Files:**
- Modify: `lab01/Prototype/Prototype/UI/SpriteRenderer.cs`

**Step 1: Implement weapon overlay drawing**

Add `DrawWeapon` and `DrawWeaponSprite` helpers inside `SpriteRenderer`:

```csharp
        /// Draws weapon overlay sprite(s) at mount-point positions.
        private static void DrawWeapon(Graphics g, WeaponType type, Rectangle hullRect, MountConfig m)
        {
            if (!WeaponSprites.TryGetValue(type, out var weaponSprite)) return;

            // Convert percentage mounts to pixel coordinates within the hull rect
            int hx = hullRect.X;
            int hy = hullRect.Y;
            int hw = hullRect.Width;
            int hh = hullRect.Height;

            switch (type)
            {
                case WeaponType.LaserCannon:
                    // Single sprite at nose
                    DrawWeaponSprite(g, weaponSprite, hx + (int)(hw * m.NoseX), hy + (int)(hh * m.NoseY), hw / 5, hh / 6);
                    break;

                case WeaponType.PlasmaTurret:
                    // Two orbs: top and bottom of body
                    {
                        int size = Math.Min(hw / 5, hh / 5);
                        int cx = hx + (int)(hw * m.BodyCenterX);
                        DrawWeaponSprite(g, weaponSprite, cx, hy + (int)(hh * m.BodyTopY), size, size);
                        DrawWeaponSprite(g, weaponSprite, cx, hy + (int)(hh * m.BodyBotY), size, size);
                    }
                    break;

                case WeaponType.MissileRack:
                    // Missiles on wings: 2 top, 2 bottom
                    {
                        int mw = hw / 8;
                        int mh = hh / 8;
                        int wx = hx + (int)(hw * m.WingX);
                        int topY = hy + (int)(hh * m.WingTopY);
                        int botY = hy + (int)(hh * m.WingBotY);
                        int midTopY = hy + (int)(hh * (m.WingTopY + m.BodyTopY) / 2);
                        int midBotY = hy + (int)(hh * (m.BodyBotY + m.WingBotY) / 2);
                        DrawWeaponSprite(g, weaponSprite, wx, topY, mw, mh);
                        DrawWeaponSprite(g, weaponSprite, wx, midTopY, mw, mh);
                        DrawWeaponSprite(g, weaponSprite, wx, midBotY, mw, mh);
                        DrawWeaponSprite(g, weaponSprite, wx, botY, mw, mh);
                    }
                    break;

                case WeaponType.TorpedoBay:
                    // Two torpedoes: above and below body
                    {
                        int tw = hw / 4;
                        int th = hh / 8;
                        int cx = hx + (int)(hw * m.BodyCenterX);
                        DrawWeaponSprite(g, weaponSprite, cx, hy + (int)(hh * m.BodyTopY) - th, tw, th);
                        DrawWeaponSprite(g, weaponSprite, cx, hy + (int)(hh * m.BodyBotY), tw, th);
                    }
                    break;

                case WeaponType.IonBeam:
                    // Cone from nose extending forward
                    {
                        int coneW = hw / 3;
                        int coneH = hh / 3;
                        int nx = hx + (int)(hw * m.NoseX);
                        int ny = hy + (int)(hh * m.NoseY);
                        DrawWeaponSprite(g, weaponSprite, nx, ny, coneW, coneH);
                    }
                    break;
            }
        }

        /// Draws a single weapon sprite centered at the given position.
        private static void DrawWeaponSprite(Graphics g, Image sprite, int cx, int cy, int width, int height)
        {
            var dest = new Rectangle(cx - width / 2, cy - height / 2, width, height);
            g.DrawImage(sprite, dest, 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel);
        }
```

**Step 2: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded. SpriteRenderer is now complete.

---

### Task 6: Swap call sites and delete ShipRenderer

**Files:**
- Modify: `lab01/Prototype/Prototype/Form1.cs` (line 586)
- Modify: `lab01/Prototype/Prototype/UI/ShipCard.cs` (line 158)
- Delete: `lab01/Prototype/Prototype/UI/ShipRenderer.cs`

**Step 1: Update Form1.cs**

In `Form1.cs`, change line 586 from:
```csharp
ShipRenderer.DrawShip(e.Graphics, _currentShip, drawRect);
```
to:
```csharp
SpriteRenderer.DrawShip(e.Graphics, _currentShip, drawRect);
```

**Step 2: Update ShipCard.cs**

In `ShipCard.cs`, change line 158 from:
```csharp
ShipRenderer.DrawShip(e.Graphics, _ship, bounds);
```
to:
```csharp
SpriteRenderer.DrawShip(e.Graphics, _ship, bounds);
```

**Step 3: Delete ShipRenderer.cs**

Delete the file `lab01/Prototype/Prototype/UI/ShipRenderer.cs` — it is no longer used.

**Step 4: Verify build succeeds**

Run: `dotnet build lab01/Prototype/Prototype/Prototype.csproj`
Expected: Build succeeded with 0 errors, 0 warnings about ShipRenderer.

---

### Task 7: Visual test and mount point tuning

**Files:**
- Possibly modify: `lab01/Prototype/Prototype/UI/SpriteRenderer.cs` (mount point values)

**Step 1: Run the application**

Run: `dotnet run --project lab01/Prototype/Prototype/Prototype.csproj`

**Step 2: Visual verification checklist**

Test each of the 3 ship types with each of the 5 weapons (15 combinations):

- [ ] Fighter + each weapon: hull renders tinted in LightSkyBlue, weapon overlays are visible and positioned on the hull (not floating in empty space)
- [ ] Cruiser + each weapon: hull renders tinted in Gold, weapon overlays positioned correctly
- [ ] Bomber + each weapon: hull renders tinted in Salmon, weapon overlays positioned correctly
- [ ] Change ship color: hull tint updates correctly
- [ ] ShipCard (fleet panel): miniature preview renders correctly at small size
- [ ] Battle: ships render correctly during battle animation

**Step 3: Tune mount points if needed**

If weapons appear misaligned, adjust the percentage values in the `Mounts` dictionary in `SpriteRenderer.cs`. For example, if the Fighter's laser appears above the nose:

```csharp
// Before: NoseY: 0.45f
// After:  NoseY: 0.48f  (moved down slightly)
```

This is visual tuning — run the app, check alignment, adjust, repeat.

---

### Task 8: Commit

**Step 1: Stage and commit all changes**

```bash
git add lab01/Prototype/Prototype/Prototype.csproj
git add lab01/Prototype/Prototype/Assets/
git add lab01/Prototype/Prototype/UI/SpriteRenderer.cs
git add docs/plans/
git commit -m "Replace GDI+ ShipRenderer with sprite-based SpriteRenderer

- Add 8 PNG sprites (3 ship hulls + 5 weapon overlays) as embedded resources
- New SpriteRenderer: loads sprites, tints hulls via ColorMatrix, composites weapons at mount points
- Percentage-based mount point system (tunable per ship type)
- Delete old ShipRenderer.cs (all GDI+ polygon drawing removed)
- 2 call sites updated: Form1.cs, ShipCard.cs"
```

Note: also `git rm` the old ShipRenderer.cs if git doesn't pick up the deletion automatically.
