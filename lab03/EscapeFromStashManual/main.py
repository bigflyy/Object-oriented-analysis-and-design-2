"""Main entry point - Pygame game loop and input handling."""
import os
import sys
import pygame
from src.managers.game_manager import GameManager
from src.ui.renderer import Renderer


class GameClient:
    """Main game client - handles Pygame loop and input."""
    
    SCREEN_WIDTH = 1200
    SCREEN_HEIGHT = 750
    FPS = 30
    
    def __init__(self):
        pygame.init()
        self._screen = pygame.display.set_mode((self.SCREEN_WIDTH, self.SCREEN_HEIGHT))
        pygame.display.set_caption("Escape From Stash")
        self._clock = pygame.time.Clock()
        
        assets_path = os.path.abspath(os.path.join(os.path.dirname(__file__), "assets", "images"))
        print(f"[GameClient] Asset path: {assets_path}")
        print(f"[GameClient] Exists: {os.path.isdir(assets_path)}")
        self._renderer = Renderer(self._screen, assets_path)
        self._game = GameManager()
        
        # Set window icon from logo
        logo_path = os.path.join(os.path.dirname(__file__), "assets", "images", "logo.png")
        if os.path.exists(logo_path):
            try:
                icon = pygame.image.load(logo_path).convert_alpha()
                pygame.display.set_icon(icon)
            except pygame.error:
                pass
        
        # Background music
        music_path = os.path.join(os.path.dirname(__file__), "assets", "audio", "stolensoundtrack.mp3")
        if os.path.exists(music_path):
            pygame.mixer.music.load(music_path)
            pygame.mixer.music.play(-1)  # Loop forever
    
    def run(self):
        """Main game loop."""
        running = True
        while running:
            dt = self._clock.tick(self.FPS) / 1000.0  # Delta time in seconds
            
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                elif event.type == pygame.KEYDOWN:
                    self._handle_key(event.key, pygame.key.get_mods())
                elif event.type == pygame.MOUSEBUTTONDOWN:
                    self._handle_mouse_down(event)
                elif event.type == pygame.MOUSEBUTTONUP:
                    self._handle_mouse_up(event)
                elif event.type == pygame.MOUSEWHEEL:
                    self._renderer.scroll(event.x, event.y)
            
            if not self._game.is_game_over:
                self._game.update(dt)
            else:
                # Stop music when player dies
                pygame.mixer.music.stop()

            self._game.process_removals()
            self._renderer.render(self._game)
        
        pygame.quit()
    
    def _handle_key(self, key, mods):
        """Handle keyboard input."""
        if key == pygame.K_ESCAPE:
            if self._game.is_dragging:
                self._game.drag_cancel()
            else:
                self._game.cancel_interaction()
        elif key == pygame.K_r and self._game.is_game_over:
            # Restart
            self._game = GameManager()
            pygame.mixer.music.play(-1)  # Restart playback
        elif key == pygame.K_u:
            self._game.upgrade_stash()
        elif key == pygame.K_r and not self._game.is_game_over and not self._game.is_dragging:
            # Rotate item under cursor
            mx, my = pygame.mouse.get_pos()
            col, row = self._get_grid_pos((mx, my))
            if 0 <= col < self._game.player.stash.cols and 0 <= row < self._game.player.stash.rows:
                item = self._game.player.stash.get_item_at(col, row)
                if item is not None:
                    self._game.rotate_item(item)
    
    def _get_grid_pos(self, pos):
        """Convert screen coords to grid cell, accounting for camera."""
        cell_size = Renderer.CELL_SIZE
        stash_x = 5
        stash_y = 5
        col = (pos[0] - stash_x - self._renderer._camera_x) // cell_size
        row = (pos[1] - stash_y - self._renderer._camera_y) // cell_size
        return col, row

    def _handle_mouse_down(self, event):
        """Handle mouse click - dispatch by button type."""
        if event.button not in (1, 2, 3):
            return
        if self._game.is_game_over:
            return

        col, row = self._get_grid_pos(event.pos)
        stash = self._game.player.stash
        if not (0 <= col < stash.cols and 0 <= row < stash.rows):
            return

        clicked_item = stash.get_item_at(col, row)
        if clicked_item is None:
            return

        # Left click: targeting, ammo selection, or drag start
        if event.button == 1:
            if self._game.is_targeting:
                self._game.fire_at_stash_item(clicked_item)
            elif self._game.is_selecting_ammo:
                from src.models import Weapon
                if isinstance(clicked_item, Weapon) and clicked_item in self._game.compatible_weapons:
                    self._game.load_ammo_into(clicked_item)
            elif not self._game.is_dragging:
                self._game.drag_start(clicked_item, col, row)

        # Middle click: rotate
        elif event.button == 2:
            self._game.rotate_item(clicked_item)

        # Right click: use or sell
        elif event.button == 3:
            mods = pygame.key.get_mods()
            if mods & pygame.KMOD_SHIFT:
                self._game.sell_item(clicked_item)
            else:
                self._game.use_item(clicked_item)

    def _handle_mouse_up(self, event):
        """Handle mouse button up - place dragged item."""
        if event.button != 1:
            return
        if self._game.is_dragging:
            col, row = self._get_grid_pos(event.pos)
            if 0 <= col < self._game.player.stash.cols and 0 <= row < self._game.player.stash.rows:
                self._game.drag_place(col, row)
            else:
                self._game.drag_cancel()


def main():
    client = GameClient()
    client.run()


if __name__ == "__main__":
    main()