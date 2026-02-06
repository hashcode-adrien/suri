extends Node

# Handles pause input and game state
# Toggles pause when P key is pressed

var game_manager: Node = null

func _ready():
	game_manager = get_node("/root/Main/GameManager")

func _input(event):
	if event.is_action_pressed("pause"):
		game_manager.toggle_pause()
