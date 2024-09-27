@tool
extends Node3D

@export var material: ShaderMaterial
@export var update: bool = false:
	set(value):
		generate()
		update = false

func generate():
	if material == null:
		return

	# Removing old stuff
	for child in get_children():
		child.free()

	# Adding new stuff
	for i in range(0, 4):
		# Setting up the material
		var mat: ShaderMaterial = material.duplicate();
		mat.set_shader_parameter("index", i)

		# Setting up the mesh
		var sphere_mesh = SphereMesh.new()
		sphere_mesh.radius -= (i * 0.015)
		sphere_mesh.material = mat

		# Setting up the node
		var node := MeshInstance3D.new()
		node.mesh = sphere_mesh
		node.name = str(i)
		add_child(node)
		node.owner = get_tree().edited_scene_root

func _ready():
	generate()
