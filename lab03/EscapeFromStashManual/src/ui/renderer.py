"""Renderer - handles all Pygame drawing."""
import os
import pygame
from typing import Dict, Optional
from src.managers.game_manager import GameManager, Config
from src.models import Weapon, Consumable, Armor, Ammo


class Renderer:
    """Handles all rendering for the game UI."""
    
    # Colors
    COLOR_BG = (20, 20, 25)
    COLOR_GRID = (40, 40, 50)
    COLOR_GRID_BORDER = (60, 60, 70)
    COLOR_WEAPON = (139, 69, 19)
    COLOR_CONSUMABLE = (34, 139, 34)
    COLOR_ARMOR = (70, 70, 90)
    COLOR_AMMO = (218, 165, 32)
    COLOR_TEXT = (220, 220, 220)
    COLOR_TEXT_DIM = (120, 120, 130)
    COLOR_HIGHLIGHT = (255, 255, 100, 80)
    COLOR_TARGET = (255, 50, 50, 100)
    COLOR_TARGET_BORDER = (255, 0, 0)
    COLOR_AMMO_HIGHLIGHT = (100, 200, 255, 100)
    COLOR_AMMO_BORDER = (100, 200, 255)
    COLOR_MARKET_SLOT = (50, 50, 60)
    COLOR_MARKET_PROGRESS = (80, 120, 80)
    COLOR_HP_GREEN = (50, 200, 50)
    COLOR_HP_YELLOW = (200, 200, 50)
    COLOR_HP_RED = (200, 50, 50)
    COLOR_BUTTON = (60, 60, 80)
    COLOR_BUTTON_HOVER = (80, 80, 110)
    COLOR_BUTTON_TEXT = (230, 230, 230)
    
    CELL_SIZE = 50
    GRID_PADDING = 5

    def __init__(self, screen: pygame.Surface, assets_path: str):
        self._screen = screen
        self._assets_path = assets_path
        self._font = pygame.font.SysFont("consolas", 16)
        self._font_small = pygame.font.SysFont("consolas", 12)
        self._font_large = pygame.font.SysFont("consolas", 24)
        self._images: Dict[str, Optional[pygame.Surface]] = {}
        self._camera_x = 0
        self._camera_y = 0
        self._load_images()

    def scroll(self, dx: float, dy: float):
        """Scroll the stash grid with mouse wheel."""
        self._camera_y += int(dy * 30)
        self._camera_x += int(dx * 30)
        # Clamp so grid always covers full visible area
        self._camera_y = min(0, max(self._camera_y, -200))
        self._camera_x = min(0, max(self._camera_x, -200))
    
    def _load_images(self):
        """Load all images from assets/images directory."""
        img_dir = self._assets_path
        if not os.path.isdir(img_dir):
            print(f"[Renderer] WARNING: image dir not found: {img_dir}")
            return
        for filename in os.listdir(img_dir):
            if filename.lower().endswith((".png", ".jpg", ".jpeg")):
                key = os.path.splitext(filename)[0]  # keep original case
                fp = os.path.join(img_dir, filename)
                try:
                    img = pygame.image.load(fp).convert_alpha()
                    self._images[key] = img
                    self._images[key.lower()] = img  # also store lowercase
                    print(f"[Renderer] Loaded: {filename} ({img.get_width()}x{img.get_height()})")
                except pygame.error as e:
                    print(f"[Renderer] FAILED to load {filename}: {e}")
        
        if not self._images:
            print(f"[Renderer] WARNING: no images loaded from {img_dir}")
    
    def _get_image(self, item_name: str) -> Optional[pygame.Surface]:
        """Get cached image by name (exact match, then lowercase)."""
        # Try exact match first
        if item_name in self._images:
            return self._images[item_name]
        # Try lowercase
        low = item_name.lower()
        if low in self._images:
            return self._images[low]
        return None
    
    def render(self, game: GameManager):
        """Render the entire game state."""
        self._screen.fill(self.COLOR_BG)

        # Layout: stash on left, market/player info on right
        stash_x = self.GRID_PADDING
        stash_y = self.GRID_PADDING

        self._render_stash_grid(game, stash_x, stash_y)

        stash_right = stash_x + game.player.stash.cols * self.CELL_SIZE + self.GRID_PADDING * 3

        # Split right side into two columns
        col_w = 300
        self._render_player_info(game, stash_right, stash_y)
        self._render_flea_market(game, stash_right + col_w, stash_y)
        self._render_action_buttons(game, stash_right + col_w, stash_y + 250)
        self._render_log(game, stash_y + game.player.stash.rows * self.CELL_SIZE + self.GRID_PADDING * 2)
        
        # Flash feedback overlay
        if game._feedback:
            text, color, timer = game._feedback[0]
            alpha = min(1.0, timer / 0.5)  # Fade in first 0.5s
            if color == "red":
                fg = (255, 60, 60)
            elif color == "yellow":
                fg = (255, 220, 60)
            else:
                fg = self.COLOR_TEXT
            surf = self._font_large.render(text, True, fg)
            surf.set_alpha(int(255 * alpha))
            r = surf.get_rect(center=(self._screen.get_width() // 2, self._screen.get_height() // 2 - 50))
            self._screen.blit(surf, r)

        # Overlay for interaction modes
        if game.is_targeting:
            self._render_targeting_overlay(game)
        elif game.is_selecting_ammo:
            self._render_ammo_selection_overlay(game)

        # Drag overlay
        if game.is_dragging:
            self._render_drag_overlay(game, stash_x, stash_y)

        # Game over
        if game.is_game_over:
            self._render_game_over(game)
        
        pygame.display.flip()
    
    def _render_stash_grid(self, game: GameManager, x: int, y: int):
        """Render the stash grid with items."""
        stash = game.player.stash
        cell = self.CELL_SIZE
        ox = self._camera_x
        oy = self._camera_y

        # Draw grid cells
        for row in range(stash.rows):
            for col in range(stash.cols):
                rect = pygame.Rect(
                    x + col * cell + ox,
                    y + row * cell + oy,
                    cell, cell
                )
                pygame.draw.rect(self._screen, self.COLOR_GRID, rect)
                pygame.draw.rect(self._screen, self.COLOR_GRID_BORDER, rect, 1)
        
        # Draw items
        drawn = set()
        for item, col, row in stash.placed_items:
            if item in drawn:
                continue
            drawn.add(item)
            
            item_rect = pygame.Rect(
                x + col * cell + ox,
                y + row * cell + oy,
                item.size_x * cell,
                item.size_y * cell
            )

            # Determine background color
            if isinstance(item, Weapon):
                color = self.COLOR_WEAPON
            elif isinstance(item, Consumable):
                color = self.COLOR_CONSUMABLE
            elif isinstance(item, Armor):
                color = self.COLOR_ARMOR
            else:
                color = self.COLOR_AMMO

            # Draw item background
            pygame.draw.rect(self._screen, color, item_rect)
            pygame.draw.rect(self._screen, self.COLOR_GRID_BORDER, item_rect, 2)

            # Draw image if available
            img = self._get_image(item.name)
            if img is not None:
                scale_factor = 0.85

                if item.rotation == 0:
                    target_w = int(item_rect.width * scale_factor)
                    target_h = int(item_rect.height * scale_factor)
                    scaled = pygame.transform.scale(img, (target_w, target_h))
                elif item.rotation == 180:
                    target_w = int(item_rect.width * scale_factor)
                    target_h = int(item_rect.height * scale_factor)
                    scaled = pygame.transform.scale(img, (target_w, target_h))
                    scaled = pygame.transform.rotate(scaled, 180)
                elif item.rotation == 90:
                    # Was originally size_x=size_y of a wide item, now tall
                    # Scale to what it was BEFORE rotation (the wide dims), then rotate
                    # size_x and size_y are already swapped, so original was size_y x size_x
                    pre_w = int(item.size_y * cell * scale_factor)
                    pre_h = int(item.size_x * cell * scale_factor)
                    scaled = pygame.transform.scale(img, (pre_w, pre_h))
                    scaled = pygame.transform.rotate(scaled, -90)
                else:  # 270
                    pre_w = int(item.size_y * cell * scale_factor)
                    pre_h = int(item.size_x * cell * scale_factor)
                    scaled = pygame.transform.scale(img, (pre_w, pre_h))
                    scaled = pygame.transform.rotate(scaled, -270)

                img_x = item_rect.x + (item_rect.width - scaled.get_width()) // 2
                img_y = item_rect.y + (item_rect.height - scaled.get_height()) // 2
                self._screen.blit(scaled, (img_x, img_y))

            # Durability / ammo info (name is on the sprite, we only show dynamic data)
            if isinstance(item, Weapon):
                info = f"{item.loaded_ammo}/{item.magazine_size}  |  {item.durability}%"
            elif isinstance(item, Armor):
                info = f"{item.current_durability}/{item.max_durability}"
            elif isinstance(item, Ammo):
                info = f"x{item.stack_size}"
            elif isinstance(item, Consumable):
                info = f"+{item.calories} food"
            else:
                info = ""

            if info:
                info_surf = self._font_small.render(info, True, self.COLOR_TEXT)
                info_shadow = self._font_small.render(info, True, (0, 0, 0))
                bx = item_rect.x + 3
                by = item_rect.y + item_rect.height - 16
                self._screen.blit(info_shadow, (bx + 1, by + 1))
                self._screen.blit(info_surf, (bx, by))

            # Show HP bar for damaged items
            if item.health < item.max_health and item.max_health > 1:
                hp_pct = item.health / item.max_health
                bar_w = item_rect.width - 4
                bar_h = 6
                bar_x = item_rect.x + 2
                bar_y = item_rect.y + 2
                # Background
                pygame.draw.rect(self._screen, (40, 0, 0), (bar_x, bar_y, bar_w, bar_h))
                # Color based on HP
                if hp_pct > 0.6:
                    hp_color = (50, 200, 50)
                elif hp_pct > 0.3:
                    hp_color = (200, 200, 50)
                else:
                    hp_color = (200, 50, 50)
                pygame.draw.rect(self._screen, hp_color, (bar_x, bar_y, int(bar_w * hp_pct), bar_h))

            # OVERLAY: item on market (greyed out)
            if game.market.is_listed(item):
                grey = pygame.Surface(item_rect.size, pygame.SRCALPHA)
                grey.fill((100, 100, 100, 120))
                self._screen.blit(grey, item_rect.topleft)
                pygame.draw.rect(self._screen, (180, 180, 50), item_rect, 2)

            # OVERLAY: targeting mode (semi-transparent red on top of the item)
            if game.is_targeting and game.targeting_weapon is not None:
                target_surf = pygame.Surface(item_rect.size, pygame.SRCALPHA)
                target_surf.fill((255, 40, 40, 80))  # semi-transparent red
                self._screen.blit(target_surf, item_rect.topleft)
                pygame.draw.rect(self._screen, (255, 80, 80), item_rect, 2)
            elif game.is_selecting_ammo and item in game.compatible_weapons:
                target_surf = pygame.Surface(item_rect.size, pygame.SRCALPHA)
                target_surf.fill((80, 180, 255, 80))
                self._screen.blit(target_surf, item_rect.topleft)
                pygame.draw.rect(self._screen, (100, 200, 255), item_rect, 2)
    
    def _render_player_info(self, game: GameManager, x: int, y: int):
        """Render player HP, hunger, money."""
        player = game.player

        # Title
        title = self._font_large.render("PLAYER", True, self.COLOR_TEXT)
        self._screen.blit(title, (x, y))

        # Money & Hunger side by side
        money_surf = self._font.render(f"Money: {player.money:.0f}\u20BD", True, self.COLOR_TEXT)
        hunger_surf = self._font.render(f"Hunger: {player.hunger:.0f}/100", True, self.COLOR_TEXT)
        rents_surf = self._font_small.render(f"Rents paid: {player.rents_paid}", True, (180, 180, 180))
        due = int(game.rents_due_in)
        due_surf = self._font_small.render(f"Rent in: {due}s", True, (180, 180, 180))
        self._screen.blit(money_surf, (x, y + 30))
        self._screen.blit(hunger_surf, (x, y + 52))
        self._screen.blit(rents_surf, (x + 200, y + 54))
        self._screen.blit(due_surf, (x + 200, y + 70))

        # Hunger bar
        bar_rect = pygame.Rect(x, y + 88, 260, 10)
        pygame.draw.rect(self._screen, self.COLOR_GRID, bar_rect)
        fill_w = max(0, min(260, int(260 * player.hunger / 100)))
        hunger_color = self.COLOR_HP_GREEN if player.hunger > 30 else self.COLOR_HP_RED
        pygame.draw.rect(self._screen, hunger_color, (bar_rect.x, bar_rect.y, fill_w, bar_rect.h))

        # HP zones - dynamic color by HP %
        y_offset = y + 108
        for zone in ["head", "thorax", "stomach"]:
            current = player.get_hp(zone)
            max_hp = player.ZONE_HP[zone]
            pct = current / max_hp if max_hp > 0 else 0
            if pct > 0.6:
                color = self.COLOR_HP_GREEN
            elif pct > 0.3:
                color = self.COLOR_HP_YELLOW
            else:
                color = self.COLOR_HP_RED
            zone_text = f"{zone.capitalize()}: {current}/{max_hp}"
            surf = self._font.render(zone_text, True, color)
            self._screen.blit(surf, (x, y_offset))
            y_offset += 20

            bar = pygame.Rect(x, y_offset, 260, 8)
            pygame.draw.rect(self._screen, self.COLOR_GRID, bar)
            pygame.draw.rect(self._screen, color, (bar.x, bar.y, int(260 * pct), bar.h))
            y_offset += 14

        # Equipped armor
        y_offset += 4
        armor_title = self._font.render("Armor:", True, self.COLOR_TEXT)
        self._screen.blit(armor_title, (x, y_offset))
        y_offset += 18

        for zone in ["head", "thorax", "stomach"]:
            armor = player.get_equipped_armor(zone)
            if armor:
                pct = armor.current_durability / armor.max_durability if armor.max_durability > 0 else 0
                if pct > 0.6:
                    dur_color = self.COLOR_HP_GREEN
                elif pct > 0.3:
                    dur_color = self.COLOR_HP_YELLOW
                else:
                    dur_color = self.COLOR_HP_RED
                zone_covered = " + ".join(armor.zones)
                armor_name = f"{armor.name} [{zone_covered}] ({armor.current_durability}/{armor.max_durability})"
            else:
                dur_color = self.COLOR_TEXT_DIM
                armor_name = "---"
            surf = self._font_small.render(f"  {zone}: {armor_name}", True, dur_color)
            self._screen.blit(surf, (x, y_offset))
            y_offset += 14
    
    def _render_flea_market(self, game: GameManager, x: int, y: int):
        """Render flea market slots."""
        title = self._font_large.render("FLEA MARKET (3 slots)", True, self.COLOR_TEXT)
        self._screen.blit(title, (x, y))
        
        for i in range(3):
            slot_y = y + 28 + i * 50
            slot_rect = pygame.Rect(x, slot_y, 250, 45)
            
            slot = game.market.get_slot_info(i)
            if slot is not None:
                pygame.draw.rect(self._screen, self.COLOR_MARKET_SLOT, slot_rect)
                pygame.draw.rect(self._screen, self.COLOR_GRID_BORDER, slot_rect, 1)

                name_surf = self._font.render(f"{slot.item.name} - {slot.price:.0f}\u20BD", True, self.COLOR_TEXT)
                self._screen.blit(name_surf, (x + 5, slot_y + 3))

                # Progress bar
                bar_rect = pygame.Rect(x + 5, slot_y + 28, 200, 8)
                pygame.draw.rect(self._screen, self.COLOR_GRID, bar_rect)
                pygame.draw.rect(self._screen, self.COLOR_MARKET_PROGRESS,
                                 (bar_rect.x, bar_rect.y, int(200 * slot.progress), bar_rect.h))
                
                if slot.progress < 1.0:
                    time_surf = self._font_small.render(f"{slot.time_left:.0f}s left", True, self.COLOR_TEXT_DIM)
                    self._screen.blit(time_surf, (x + 210, slot_y + 25))
            else:
                pygame.draw.rect(self._screen, self.COLOR_MARKET_SLOT, slot_rect, 1)
                empty_surf = self._font.render(f"[Slot {i+1}] Empty", True, self.COLOR_TEXT_DIM)
                self._screen.blit(empty_surf, (x + 10, slot_y + 12))
    
    def _render_action_buttons(self, game: GameManager, x: int, y: int):
        """Render action buttons area."""
        tips = [
            "R-click item: Use",
            "Shift+R-click: Sell",
            "Mid-click item: Rotate",
            "ESC: Cancel",
        ]
        for i, tip in enumerate(tips):
            surf = self._font_small.render(tip, True, self.COLOR_TEXT_DIM)
            self._screen.blit(surf, (x, y + i * 18))
        
        # Stash upgrade button hint
        upgrade_surf = self._font_small.render(
            f"Upgrade Stash: {Config.STASH_UPGRADE_COST:.0f}\u20BD (U)", True, self.COLOR_TEXT_DIM
        )
        self._screen.blit(upgrade_surf, (x, y + 80))
    
    def _render_log(self, game: GameManager, y: int):
        """Render game log at bottom."""
        log_bg = pygame.Rect(5, y, self._screen.get_width() - 10, 120)
        pygame.draw.rect(self._screen, self.COLOR_MARKET_SLOT, log_bg)
        pygame.draw.rect(self._screen, self.COLOR_GRID_BORDER, log_bg, 1)
        
        messages = game.log_messages[-6:]
        for i, msg in enumerate(messages):
            surf = self._font_small.render(msg, True, self.COLOR_TEXT_DIM)
            self._screen.blit(surf, (10, y + 5 + i * 18))
    
    def _render_targeting_overlay(self, game: GameManager):
        """Render targeting mode overlay."""
        overlay = pygame.Surface(self._screen.get_size(), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 30))
        self._screen.blit(overlay, (0, 0))

        text = self._font_large.render("SELECT TARGET - Click to fire", True, (255, 80, 80))
        text_rect = text.get_rect(center=(self._screen.get_width() // 2, 30))
        self._screen.blit(text, text_rect)
    
    def _render_ammo_selection_overlay(self, game: GameManager):
        """Render ammo selection overlay."""
        overlay = pygame.Surface(self._screen.get_size(), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 80))
        self._screen.blit(overlay, (0, 0))
        
        text = self._font_large.render("SELECT WEAPON TO LOAD - Click highlighted weapon", True, (100, 200, 255))
        text_rect = text.get_rect(center=(self._screen.get_width() // 2, 30))
        self._screen.blit(text, text_rect)

    def _render_drag_overlay(self, game: GameManager, x: int, y: int):
        """Render floating item under cursor + highlight valid drop cells."""
        item = game.dragging_item
        if item is None:
            return

        mx, my = pygame.mouse.get_pos()
        cell = self.CELL_SIZE

        # Highlight valid drop cells
        for row in range(game.player.stash.rows):
            for col in range(game.player.stash.cols):
                if game.player.stash._can_place_at(item, col, row):
                    for dy in range(item.size_y):
                        for dx in range(item.size_x):
                            cr = row + dy
                            cc = col + dx
                            if 0 <= cr < game.player.stash.rows and 0 <= cc < game.player.stash.cols:
                                cr2 = x + cc * cell + self._camera_x
                                cy2 = y + cr * cell + self._camera_y
                                hl = pygame.Rect(cr2, cy2, cell, cell)
                                pygame.draw.rect(self._screen, (0, 180, 0, 40), hl)
                                pygame.draw.rect(self._screen, (0, 255, 0), hl, 1)

        # Draw floating item at cursor
        item_rect = pygame.Rect(
            mx - (item.size_x * cell) // 2,
            my - (item.size_y * cell) // 2,
            item.size_x * cell,
            item.size_y * cell
        )
        # Semi-transparent background
        drag_surf = pygame.Surface(item_rect.size, pygame.SRCALPHA)
        drag_surf.fill((200, 200, 100, 80))
        self._screen.blit(drag_surf, item_rect.topleft)
        pygame.draw.rect(self._screen, (255, 255, 100), item_rect, 2)

        # Draw image if available
        img = self._get_image(item.name)
        if img is not None:
            scale_factor = 0.85
            target_w = int(item_rect.width * scale_factor)
            target_h = int(item_rect.height * scale_factor)
            scaled = pygame.transform.scale(img, (target_w, target_h))
            ix = item_rect.x + (item_rect.width - scaled.get_width()) // 2
            iy = item_rect.y + (item_rect.height - scaled.get_height()) // 2
            self._screen.blit(scaled, (ix, iy))

    def _render_game_over(self, game: GameManager):
        """Render game over screen."""
        overlay = pygame.Surface(self._screen.get_size(), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 150))
        self._screen.blit(overlay, (0, 0))

        cx = self._screen.get_width() // 2
        cy = self._screen.get_height() // 2

        go_text = self._font_large.render("GAME OVER", True, (255, 50, 50))
        self._screen.blit(go_text, go_text.get_rect(center=(cx, cy - 40)))

        reason_text = self._font.render(game.game_over_reason, True, self.COLOR_TEXT)
        self._screen.blit(reason_text, reason_text.get_rect(center=(cx, cy)))

        rents = self._font.render(f"Rents survived: {game.player.rents_paid}", True, (200, 200, 200))
        self._screen.blit(rents, rents.get_rect(center=(cx, cy + 40)))
        
        restart_text = self._font.render("Press R to restart", True, self.COLOR_TEXT_DIM)
        self._screen.blit(restart_text, restart_text.get_rect(center=(cx, cy + 80)))
