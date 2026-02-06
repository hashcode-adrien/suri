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
                AlbedoColor = new Color(0.2f, 0.3f, 0.15f) // Dark green/brown
            };
            
            _groundPlane = new MeshInstance3D
            {
                Mesh = planeMesh,
                MaterialOverride = material,
                Position = new Vector3(
                    _gridManager.GridWidth * CellSize / 2.0f,
                    0,
                    _gridManager.GridHeight * CellSize / 2.0f
                )
            };
            
            AddChild(_groundPlane);
        }

        private void SyncAllBuildings()
        {
            var grid = _gridManager.GetGrid();
            for (int x = 0; x < _gridManager.GridWidth; x++)
            {
                for (int y = 0; y < _gridManager.GridHeight; y++)
                {
                    var buildingType = grid[x, y];
                    if (buildingType != BuildingType.None)
                    {
                        CreateBuildingMesh(new Vector2I(x, y), buildingType);
                    }
                }
            }
        }

        private void OnGridChanged(int x, int y)
        {
            var gridPos = new Vector2I(x, y);
            var buildingType = _gridManager.GetBuildingAt(gridPos);
            
            // Remove existing mesh if present
            if (_buildingMeshes.ContainsKey(gridPos))
            {
                _buildingMeshes[gridPos].QueueFree();
                _buildingMeshes.Remove(gridPos);
            }
            
            // Create new mesh if not empty
            if (buildingType != BuildingType.None)
            {
                CreateBuildingMesh(gridPos, buildingType);
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
