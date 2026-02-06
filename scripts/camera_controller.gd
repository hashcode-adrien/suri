extends Camera2D

# Camera controller for panning and zooming
# WASD/Arrow keys for panning, Mouse wheel for zooming

var pan_speed: float = 400.0
var zoom_speed: float = 0.1
var min_zoom: float = 0.5
var max_zoom: float = 2.0

func _ready():
	# Set initial zoom
	zoom = Vector2(1.0, 1.0)

func _process(delta):
	# Handle camera panning
	var direction = Vector2.ZERO
	
	if Input.is_action_pressed("ui_left"):
		direction.x -= 1
	if Input.is_action_pressed("ui_right"):
		direction.x += 1
	if Input.is_action_pressed("ui_up"):
		direction.y -= 1
	if Input.is_action_pressed("ui_down"):
		direction.y += 1
	
	# Apply movement
	if direction.length() > 0:
		position += direction.normalized() * pan_speed * delta / zoom.x

func _input(event):
	# Handle zoom with mouse wheel
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_WHEEL_UP and event.pressed:
			var new_zoom = zoom.x + zoom_speed
			new_zoom = min(new_zoom, max_zoom)
			zoom = Vector2(new_zoom, new_zoom)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN and event.pressed:
			var new_zoom = zoom.x - zoom_speed
			new_zoom = max(new_zoom, min_zoom)
			zoom = Vector2(new_zoom, new_zoom)
