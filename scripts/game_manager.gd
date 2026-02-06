extends Node

# Game state management and core systems
# Manages economy, population, time progression, and game state

signal money_changed(new_money)
signal population_changed(new_population)
signal happiness_changed(new_happiness)
signal game_paused(paused)

# Economy variables
var money: int = 10000
var income: int = 0
var expenses: int = 0

# Population variables
var population: int = 0
var max_population: int = 0
var happiness: float = 50.0

# Time progression
var game_tick_timer: float = 0.0
var game_tick_interval: float = 5.0  # Calculate income/expenses every 5 seconds
var is_paused: bool = false

# Building data
var buildings: Dictionary = {}

func _ready():
	emit_signal("money_changed", money)
	emit_signal("population_changed", population)
	emit_signal("happiness_changed", happiness)

func _process(delta):
	if is_paused:
		return
	
	game_tick_timer += delta
	if game_tick_timer >= game_tick_interval:
		game_tick_timer = 0.0
		process_game_tick()

func process_game_tick():
	# Calculate income and expenses
	calculate_income()
	calculate_expenses()
	
	# Apply net income
	var net_income = income - expenses
	change_money(net_income)
	
	# Update population
	update_population()
	
	# Update happiness
	update_happiness()

func calculate_income():
	income = 0
	for building_pos in buildings:
		var building = buildings[building_pos]
		match building.type:
			"residential":
				income += 10
			"commercial":
				income += 20
			"industrial":
				income += 15

func calculate_expenses():
	expenses = 0
	for building_pos in buildings:
		var building = buildings[building_pos]
		match building.type:
			"residential":
				expenses += 2
			"commercial":
				expenses += 3
			"industrial":
				expenses += 4
			"road":
				expenses += 1
			"park":
				expenses += 2

func update_population():
	# Calculate max population based on residential zones
	max_population = 0
	for building_pos in buildings:
		var building = buildings[building_pos]
		if building.type == "residential":
			max_population += 10
	
	# Gradually move population toward max
	if population < max_population:
		population = min(population + 1, max_population)
		emit_signal("population_changed", population)
	elif population > max_population:
		population = max(population - 1, max_population)
		emit_signal("population_changed", population)

func update_happiness():
	# Base happiness
	var new_happiness = 50.0
	
	var park_count = 0
	var industrial_count = 0
	var total_buildings = 0
	
	for building_pos in buildings:
		var building = buildings[building_pos]
		total_buildings += 1
		if building.type == "park":
			park_count += 1
		elif building.type == "industrial":
			industrial_count += 1
	
	if total_buildings > 0:
		# Parks boost happiness
		new_happiness += (park_count * 10.0)
		# Industrial zones reduce happiness
		new_happiness -= (industrial_count * 5.0)
		
		# Clamp between 0 and 100
		new_happiness = clamp(new_happiness, 0.0, 100.0)
	
	if abs(new_happiness - happiness) > 0.5:
		happiness = new_happiness
		emit_signal("happiness_changed", happiness)

func change_money(amount: int):
	money += amount
	emit_signal("money_changed", money)

func can_afford(cost: int) -> bool:
	return money >= cost

func add_building(pos: Vector2i, building_type: String):
	buildings[pos] = {"type": building_type}

func remove_building(pos: Vector2i):
	if buildings.has(pos):
		buildings.erase(pos)

func toggle_pause():
	is_paused = !is_paused
	emit_signal("game_paused", is_paused)

func get_building_at(pos: Vector2i):
	return buildings.get(pos, null)
