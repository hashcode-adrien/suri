extends Node2D

# Building placement and management system
# Handles building placement, demolition, and grid interaction

signal building_placed(building_type, pos)
signal building_demolished(pos)

@export var tile_size: int = 64

# Building costs
var building_costs: Dictionary = {
	"residential": 100,
	"commercial": 150,
	"industrial": 200,
	"road": 50,
	"park": 75
}

# Building colors (placeholder graphics)
var building_colors: Dictionary = {
	"residential": Color(0.2, 0.8, 0.2),  # Green
	"commercial": Color(0.2, 0.2, 0.8),    # Blue
	"industrial": Color(0.9, 0.9, 0.2),    # Yellow
	"road": Color(0.5, 0.5, 0.5),          # Gray
	"park": Color(0.5, 1.0, 0.5)           # Light green
}

var current_building_type: String = ""
var game_manager: Node = null
var tile_map: TileMap = null

func _ready():
	game_manager = get_node("/root/Main/GameManager")
	tile_map = get_node("/root/Main/TileMap")

func _input(event):
	if event is InputEventMouseButton:
		if event.pressed:
			var mouse_pos = get_global_mouse_position()
			var tile_pos = world_to_tile(mouse_pos)
			
			if event.button_index == MOUSE_BUTTON_LEFT:
				# Place building
				if current_building_type != "":
					try_place_building(tile_pos)
			elif event.button_index == MOUSE_BUTTON_RIGHT:
				# Demolish building
				try_demolish_building(tile_pos)

func try_place_building(tile_pos: Vector2i):
	# Check if there's already a building here
	if game_manager.get_building_at(tile_pos) != null:
		return
	
	# Check if we can afford it
	var cost = building_costs.get(current_building_type, 0)
	if not game_manager.can_afford(cost):
		print("Not enough money!")
		return
	
	# Place the building
	game_manager.change_money(-cost)
	game_manager.add_building(tile_pos, current_building_type)
	
	# Visual representation on tilemap
	tile_map.set_cell(0, tile_pos, 0, Vector2i(0, 0))
	
	# Create colored rectangle for building
	create_building_visual(tile_pos, current_building_type)
	
	emit_signal("building_placed", current_building_type, tile_pos)

func try_demolish_building(tile_pos: Vector2i):
	var building = game_manager.get_building_at(tile_pos)
	if building == null:
		return
	
	# Remove building
	game_manager.remove_building(tile_pos)
	tile_map.erase_cell(0, tile_pos)
	
	# Remove visual
	remove_building_visual(tile_pos)
	
	emit_signal("building_demolished", tile_pos)

func create_building_visual(tile_pos: Vector2i, building_type: String):
	var visual = ColorRect.new()
	visual.size = Vector2(tile_size - 4, tile_size - 4)
	visual.position = tile_to_world(tile_pos) + Vector2(2, 2)
	visual.color = building_colors.get(building_type, Color.WHITE)
	visual.name = "Building_%d_%d" % [tile_pos.x, tile_pos.y]
	add_child(visual)

func remove_building_visual(tile_pos: Vector2i):
	var visual_name = "Building_%d_%d" % [tile_pos.x, tile_pos.y]
	var visual = get_node_or_null(visual_name)
	if visual:
		visual.queue_free()

func set_current_building(building_type: String):
	current_building_type = building_type

func world_to_tile(world_pos: Vector2) -> Vector2i:
	return Vector2i(floor(world_pos.x / tile_size), floor(world_pos.y / tile_size))

func tile_to_world(tile_pos: Vector2i) -> Vector2:
	return Vector2(tile_pos.x * tile_size, tile_pos.y * tile_size)

func get_building_cost(building_type: String) -> int:
	return building_costs.get(building_type, 0)
