extends CanvasLayer

# HUD displaying game statistics and information
# Shows money, population, happiness, and build menu

var game_manager: Node = null
var building_system: Node = null

# UI Labels
var money_label: Label
var population_label: Label
var happiness_label: Label
var paused_label: Label

func _ready():
	game_manager = get_node("/root/Main/GameManager")
	building_system = get_node("/root/Main/BuildingSystem")
	
	# Create UI elements
	create_hud()
	
	# Connect to signals
	game_manager.money_changed.connect(_on_money_changed)
	game_manager.population_changed.connect(_on_population_changed)
	game_manager.happiness_changed.connect(_on_happiness_changed)
	game_manager.game_paused.connect(_on_game_paused)

func create_hud():
	# Top bar background
	var top_bar = ColorRect.new()
	top_bar.color = Color(0.1, 0.1, 0.1, 0.9)
	top_bar.size = Vector2(1280, 50)
	top_bar.position = Vector2(0, 0)
	add_child(top_bar)
	
	# Money label
	money_label = Label.new()
	money_label.position = Vector2(20, 15)
	money_label.add_theme_font_size_override("font_size", 20)
	money_label.text = "Money: $%d" % game_manager.money
	add_child(money_label)
	
	# Population label
	population_label = Label.new()
	population_label.position = Vector2(250, 15)
	population_label.add_theme_font_size_override("font_size", 20)
	population_label.text = "Population: %d" % game_manager.population
	add_child(population_label)
	
	# Happiness label
	happiness_label = Label.new()
	happiness_label.position = Vector2(500, 15)
	happiness_label.add_theme_font_size_override("font_size", 20)
	happiness_label.text = "Happiness: %.0f%%" % game_manager.happiness
	add_child(happiness_label)
	
	# Paused indicator
	paused_label = Label.new()
	paused_label.position = Vector2(550, 300)
	paused_label.add_theme_font_size_override("font_size", 48)
	paused_label.text = "PAUSED"
	paused_label.visible = false
	add_child(paused_label)
	
	# Create build menu
	create_build_menu()

func create_build_menu():
	# Build menu panel
	var menu_panel = ColorRect.new()
	menu_panel.color = Color(0.15, 0.15, 0.15, 0.9)
	menu_panel.size = Vector2(200, 400)
	menu_panel.position = Vector2(1080, 60)
	add_child(menu_panel)
	
	# Title
	var title = Label.new()
	title.text = "Build Menu"
	title.position = Vector2(1100, 70)
	title.add_theme_font_size_override("font_size", 18)
	add_child(title)
	
	# Building buttons
	var building_types = ["residential", "commercial", "industrial", "road", "park"]
	var y_offset = 100
	
	for building_type in building_types:
		var button = Button.new()
		button.text = building_type.capitalize()
		button.position = Vector2(1090, y_offset)
		button.size = Vector2(180, 40)
		button.pressed.connect(_on_building_button_pressed.bind(building_type))
		add_child(button)
		
		# Cost label
		var cost = building_system.get_building_cost(building_type)
		var cost_label = Label.new()
		cost_label.text = "$%d" % cost
		cost_label.position = Vector2(1200, y_offset + 10)
		cost_label.add_theme_font_size_override("font_size", 14)
		add_child(cost_label)
		
		y_offset += 50

func _on_building_button_pressed(building_type: String):
	building_system.set_current_building(building_type)
	print("Selected building type: ", building_type)

func _on_money_changed(new_money: int):
	money_label.text = "Money: $%d" % new_money

func _on_population_changed(new_population: int):
	population_label.text = "Population: %d" % new_population

func _on_happiness_changed(new_happiness: float):
	happiness_label.text = "Happiness: %.0f%%" % new_happiness

func _on_game_paused(paused: bool):
	paused_label.visible = paused
