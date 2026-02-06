using Godot;
using System.Collections.Generic;

namespace Suri
{
    /// <summary>
    /// Renders the grid in 3D using procedurally-generated meshes.
    /// Reads from GridManager's grid data and updates when buildings change.
    /// </summary>
    public partial class GridManager3D : Node3D
    {
        private GridManager _gridManager;
        private Dictionary<Vector2I, Node3D> _buildingMeshes = new Dictionary<Vector2I, Node3D>();
        private Dictionary<Vector2I, Node3D> _cottages = new Dictionary<Vector2I, Node3D>();
        private HashSet<Vector2I> _cottageCells = new HashSet<Vector2I>();
        private Node3D _buildingsContainer;
        private MeshInstance3D _groundPlane;
        
        private const float CellSize = 1.0f;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/Main/GridManager");
            
            // Create containers
            _buildingsContainer = new Node3D { Name = "BuildingsContainer" };
            AddChild(_buildingsContainer);
            
            // Subscribe to grid changes
            _gridManager.GridChanged += OnGridChanged;
            
            // Create ground plane
            CreateGroundPlane();
            
            // Initial sync - build all existing buildings
            SyncAllBuildings();
        }

        private void CreateGroundPlane()
        {
            var planeMesh = new PlaneMesh
            {
                Size = new Vector2(_gridManager.GridWidth * CellSize, _gridManager.GridHeight * CellSize)
            };
            
            var material = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.3f, 0.5f, 0.2f) // Dark green/brown
            };
            
            _groundPlane = new MeshInstance3D
            {
                Mesh = planeMesh,
                MaterialOverride = material,
                Position = new Vector3(
                    _gridManager.GridWidth * CellSize / 2.0f,
                    -0.01f, // Slightly below ground to avoid z-fighting
                    _gridManager.GridHeight * CellSize / 2.0f
                )
            };
            
            AddChild(_groundPlane);
        }

        private void SyncAllBuildings()
        {
            RebuildAll();
        }

        private void OnGridChanged(int x, int y)
        {
            // When any cell changes, we need to check if it affects cottage formations
            // For simplicity, do a full rebuild when near existing cottages or residential areas
            RebuildAll();
        }

        /// <summary>
        /// Completely rebuilds all 3D meshes from scratch, including cottage detection.
        /// </summary>
        private void RebuildAll()
        {
            // Clear all existing meshes
            foreach (var mesh in _buildingMeshes.Values)
            {
                mesh.QueueFree();
            }
            _buildingMeshes.Clear();

            foreach (var cottage in _cottages.Values)
            {
                cottage.QueueFree();
            }
            _cottages.Clear();
            _cottageCells.Clear();

            // First pass: detect and place all 3x3 residential cottages
            DetectAndPlaceCottages();

            // Second pass: create individual meshes for non-cottage buildings
            var grid = _gridManager.GetGrid();
            for (int x = 0; x < _gridManager.GridWidth; x++)
            {
                for (int y = 0; y < _gridManager.GridHeight; y++)
                {
                    var gridPos = new Vector2I(x, y);
                    
                    // Skip if this cell is part of a cottage
                    if (_cottageCells.Contains(gridPos))
                        continue;

                    var buildingType = grid[x, y];
                    if (buildingType != BuildingType.None)
                    {
                        CreateBuildingMesh(gridPos, buildingType);
                    }
                }
            }
        }

        /// <summary>
        /// Detects all 3x3 residential blocks and places cottage models.
        /// Uses greedy scanning to avoid overlaps.
        /// </summary>
        private void DetectAndPlaceCottages()
        {
            var grid = _gridManager.GetGrid();

            // Scan from top-left to bottom-right
            for (int y = 0; y <= _gridManager.GridHeight - 3; y++)
            {
                for (int x = 0; x <= _gridManager.GridWidth - 3; x++)
                {
                    var topLeft = new Vector2I(x, y);
                    
                    // Skip if this cell is already part of a cottage
                    if (_cottageCells.Contains(topLeft))
                        continue;

                    // Check if all 9 cells are Residential and not already claimed
                    bool allResidential = true;
                    for (int dy = 0; dy < 3 && allResidential; dy++)
                    {
                        for (int dx = 0; dx < 3 && allResidential; dx++)
                        {
                            var checkPos = new Vector2I(x + dx, y + dy);
                            if (_cottageCells.Contains(checkPos) || 
                                grid[x + dx, y + dy] != BuildingType.Residential)
                            {
                                allResidential = false;
                            }
                        }
                    }

                    if (allResidential)
                    {
                        // Mark all 9 cells as part of this cottage
                        for (int dy = 0; dy < 3; dy++)
                        {
                            for (int dx = 0; dx < 3; dx++)
                            {
                                _cottageCells.Add(new Vector2I(x + dx, y + dy));
                            }
                        }

                        // Load and place the cottage
                        PlaceCottage(topLeft);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the cottage GLB model and places it at the center of a 3x3 block.
        /// </summary>
        private void PlaceCottage(Vector2I topLeft)
        {
            try
            {
                // Load the cottage GLB file
                var cottageScene = GD.Load<PackedScene>("res://assets/Meshy_AI_Enchanted_Cottage_0206220057_texture.glb");
                if (cottageScene == null)
                {
                    GD.PrintErr("Failed to load cottage GLB file");
                    return;
                }

                var cottage = cottageScene.Instantiate<Node3D>();
                if (cottage == null)
                {
                    GD.PrintErr("Failed to instantiate cottage scene");
                    return;
                }

                // Position at center of 3x3 block
                // Center offset is 1.5 because: topLeft + 1.5 cells = middle of a 3-cell span
                cottage.Position = new Vector3(
                    (topLeft.X + 1.5f) * CellSize,
                    0f,
                    (topLeft.Y + 1.5f) * CellSize
                );

                // Scale to fit 3x3 area (2.0 = covers approximately 2 units per 3 cells)
                cottage.Scale = new Vector3(2f, 2f, 2f);

                _buildingsContainer.AddChild(cottage);
                _cottages[topLeft] = cottage;

                GD.Print($"Placed cottage at grid position ({topLeft.X}, {topLeft.Y})");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Error placing cottage: {e.Message}");
            }
        }

        private void CreateBuildingMesh(Vector2I gridPos, BuildingType buildingType)
        {
            var data = BuildingRegistry.GetBuildingData(buildingType);
            Node3D buildingNode;

            switch (buildingType)
            {
                case BuildingType.Residential:
                    buildingNode = CreateResidential(data.Color);
                    break;
                case BuildingType.Commercial:
                    buildingNode = CreateCommercial(data.Color);
                    break;
                case BuildingType.Industrial:
                    buildingNode = CreateIndustrial(data.Color);
                    break;
                case BuildingType.Road:
                    buildingNode = CreateRoad(data.Color);
                    break;
                case BuildingType.Park:
                    buildingNode = CreatePark(data.Color);
                    break;
                default:
                    return;
            }

            // Position the building at the grid cell
            buildingNode.Position = new Vector3(
                (gridPos.X + 0.5f) * CellSize,
                0,
                (gridPos.Y + 0.5f) * CellSize
            );

            _buildingsContainer.AddChild(buildingNode);
            _buildingMeshes[gridPos] = buildingNode;
        }

        private Node3D CreateResidential(Color color)
        {
            // Green house - simple box
            var container = new Node3D();
            
            var mesh = new BoxMesh { Size = new Vector3(0.8f, 1.5f, 0.8f) };
            var material = new StandardMaterial3D { AlbedoColor = color };
            var instance = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = material,
                Position = new Vector3(0, 0.75f, 0), // Centered vertically
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            
            container.AddChild(instance);
            return container;
        }

        private Node3D CreateCommercial(Color color)
        {
            // Blue taller building
            var container = new Node3D();
            
            var mesh = new BoxMesh { Size = new Vector3(0.8f, 2.0f, 0.8f) };
            var material = new StandardMaterial3D { AlbedoColor = color };
            var instance = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = material,
                Position = new Vector3(0, 1.0f, 0),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            
            container.AddChild(instance);
            return container;
        }

        private Node3D CreateIndustrial(Color color)
        {
            // Yellow factory with chimney
            var container = new Node3D();
            
            // Base building
            var baseMesh = new BoxMesh { Size = new Vector3(0.9f, 1.0f, 0.9f) };
            var baseMaterial = new StandardMaterial3D { AlbedoColor = color };
            var baseInstance = new MeshInstance3D
            {
                Mesh = baseMesh,
                MaterialOverride = baseMaterial,
                Position = new Vector3(0, 0.5f, 0),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            container.AddChild(baseInstance);
            
            // Chimney
            var chimneyMesh = new CylinderMesh
            {
                TopRadius = 0.1f,
                BottomRadius = 0.1f,
                Height = 0.6f
            };
            var chimneyMaterial = new StandardMaterial3D { AlbedoColor = Colors.DarkGray };
            var chimneyInstance = new MeshInstance3D
            {
                Mesh = chimneyMesh,
                MaterialOverride = chimneyMaterial,
                Position = new Vector3(0.25f, 1.3f, 0.25f),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            container.AddChild(chimneyInstance);
            
            return container;
        }

        private Node3D CreateRoad(Color color)
        {
            // Dark gray flat surface
            var container = new Node3D();
            
            var mesh = new BoxMesh { Size = new Vector3(0.9f, 0.1f, 0.9f) };
            var material = new StandardMaterial3D { AlbedoColor = color };
            var instance = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = material,
                Position = new Vector3(0, 0.05f, 0),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            
            container.AddChild(instance);
            return container;
        }

        private Node3D CreatePark(Color color)
        {
            // Light green flat ground with tree
            var container = new Node3D();
            
            // Ground
            var groundMesh = new BoxMesh { Size = new Vector3(0.9f, 0.2f, 0.9f) };
            var groundMaterial = new StandardMaterial3D { AlbedoColor = color };
            var groundInstance = new MeshInstance3D
            {
                Mesh = groundMesh,
                MaterialOverride = groundMaterial,
                Position = new Vector3(0, 0.1f, 0),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            container.AddChild(groundInstance);
            
            // Tree (sphere on top)
            var treeMesh = new SphereMesh { Radius = 0.2f, Height = 0.4f };
            var treeMaterial = new StandardMaterial3D { AlbedoColor = Colors.DarkGreen };
            var treeInstance = new MeshInstance3D
            {
                Mesh = treeMesh,
                MaterialOverride = treeMaterial,
                Position = new Vector3(0, 0.4f, 0),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            };
            container.AddChild(treeInstance);
            
            return container;
        }
    }
}
