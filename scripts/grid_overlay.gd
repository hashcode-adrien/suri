extends Node2D

# Visual grid overlay to help players see where they can build
# Draws a grid pattern over the game world

@export var tile_size: int = 64
@export var grid_size: Vector2i = Vector2i(30, 20)
@export var grid_color: Color = Color(0.3, 0.3, 0.3, 0.3)
@export var grid_line_width: float = 1.0

func _ready():
	queue_redraw()

func _draw():
	# Draw vertical lines
	for x in range(grid_size.x + 1):
		var start = Vector2(x * tile_size, 0)
		var end = Vector2(x * tile_size, grid_size.y * tile_size)
		draw_line(start, end, grid_color, grid_line_width)
	
	# Draw horizontal lines
	for y in range(grid_size.y + 1):
		var start = Vector2(0, y * tile_size)
		var end = Vector2(grid_size.x * tile_size, y * tile_size)
		draw_line(start, end, grid_color, grid_line_width)
