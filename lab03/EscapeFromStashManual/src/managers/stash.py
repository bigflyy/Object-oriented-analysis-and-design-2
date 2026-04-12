"""Stash - grid-based inventory system."""
from typing import List, Optional, Tuple
from src.models import Loot


class Stash:
    """Grid-based inventory. Items are placed on a 2D grid."""

    def __init__(self, cols: int = 10, rows: int = 12):
        self._cols = cols
        self._rows = rows
        self._grid: List[List[Optional[Loot]]] = [[None] * cols for _ in range(rows)]
        self._items: List[Tuple[Loot, int, int]] = []  # (item, col, row)

    @property
    def cols(self) -> int:
        return self._cols

    @property
    def rows(self) -> int:
        return self._rows

    @property
    def items(self) -> List[Loot]:
        return [item for item, _, _ in self._items]

    @property
    def placed_items(self) -> List[Tuple[Loot, int, int]]:
        return list(self._items)

    def can_fit(self, item: Loot) -> bool:
        """Check if item can fit anywhere in the stash."""
        for row in range(self._rows):
            for col in range(self._cols):
                if self._can_place_at(item, col, row):
                    return True
        return False

    def add_item(self, item: Loot, col: int = -1, row: int = -1) -> bool:
        """Add item to stash. If col/row not specified, find first fit."""
        if col < 0 or row < 0:
            # Auto-place
            for r in range(self._rows):
                for c in range(self._cols):
                    if self._can_place_at(item, c, r):
                        return self._place_item(item, c, r)
            return False

        if self._can_place_at(item, col, row):
            return self._place_item(item, col, row)
        return False

    def remove_item(self, item: Loot) -> bool:
        """Remove item from stash."""
        for i, (inv_item, col, row) in enumerate(self._items):
            if inv_item is item:
                for dy in range(item.size_y):
                    for dx in range(item.size_x):
                        r, c = row + dy, col + dx
                        if 0 <= r < self._rows and 0 <= c < self._cols:
                            if self._grid[r][c] is item:
                                self._grid[r][c] = None
                self._items.pop(i)
                return True
        return False

    def get_item_at(self, col: int, row: int) -> Optional[Loot]:
        """Get item at a specific grid cell."""
        if 0 <= col < self._cols and 0 <= row < self._rows:
            return self._grid[row][col]
        return None

    def upgrade(self, cols: int = 0, rows: int = 0):
        """Upgrade stash size."""
        self._cols += cols
        self._rows += rows
        # Rebuild grid
        new_grid = [[None] * self._cols for _ in range(self._rows)]
        for r in range(min(len(self._grid), self._rows)):
            for c in range(min(len(self._grid[0]), self._cols)):
                new_grid[r][c] = self._grid[r][c]
        self._grid = new_grid

    def _can_place_at(self, item: Loot, col: int, row: int) -> bool:
        """Check if item can be placed at position, ignoring its own cells.

        Used by:
        - Drag-and-drop: checks if the release spot is valid
        - Rotation: checks if rotated dimensions fit at origin
        """
        # Check every cell the item would occupy
        for dy in range(item.size_y):
            for dx in range(item.size_x):
                r, c = row + dy, col + dx

                # 1. Boundary check: item fits inside the grid?
                if r >= self._rows or c >= self._cols:
                    return False

                # 2. Occupancy check
                cell = self._grid[r][c]
                # Empty cell -> OK
                # Cell contains THIS item -> OK (it's in the air/being dragged)
                # Cell not empty and contains OTHER item -> Blocked
                if cell is not None and cell is not item:
                    return False
        return True

    def _place_item(self, item: Loot, col: int, row: int) -> bool:
        """Place item at position."""
        # mark cells as occupied by this item
        for dy in range(item.size_y):
            for dx in range(item.size_x):
                self._grid[row + dy][col + dx] = item

        self._items.append((item, col, row))
        return True

    def find_item_position(self, item: Loot) -> Optional[Tuple[int, int]]:
        """Find the top-left position of an item in the stash."""
        for inv_item, col, row in self._items:
            if inv_item is item:
                return (col, row)
        return None

    def find_closest_fit(self, item: Loot, target_col: int, target_row: int) -> Optional[Tuple[int, int]]:
        """Find the closest valid position to (target_col, target_row) for an item.

        Checks ALL valid positions and returns the one with minimum Euclidean distance.
        """
        best = None
        best_dist = float('inf')

        for r in range(self._rows):
            for c in range(self._cols):
                if self._can_place_at(item, c, r):
                    # Euclidean distance to target center
                    dist = (c - target_col) ** 2 + (r - target_row) ** 2
                    if dist < best_dist:
                        best_dist = dist
                        best = (c, r)
        return best

    def remove_and_place(self, item: Loot, new_col: int, new_row: int,
                         orig_col: int = -1, orig_row: int = -1) -> bool:
        """Atomically remove item from current position and place at new position."""
        # 1. Clear grid cells wherever this item exists
        for r in range(self._rows):
            for c in range(self._cols):
                if self._grid[r][c] is item:
                    self._grid[r][c] = None

        # 2. Remove from _items list
        self._items = [(inv, ic, ir) for inv, ic, ir in self._items if inv is not item]

        # 3. Try to place at requested position
        if self._can_place_at(item, new_col, new_row):
            return self._place_item(item, new_col, new_row)

        # 4. Find closest valid position
        fallback = self.find_closest_fit(item, new_col, new_row)
        if fallback is not None:
            return self._place_item(item, fallback[0], fallback[1])

        # 5. No valid position — put back at origin (if provided)
        if orig_col >= 0 and orig_row >= 0 and self._can_place_at(item, orig_col, orig_row):
            return self._place_item(item, orig_col, orig_row)

        return False
