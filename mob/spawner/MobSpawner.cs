using Godot;
using System;

public partial class MobSpawner : Node3D {

  private Marker3D _marker3d;
  private Timer timer;

  private PackedScene MobPrefab;

  public override void _Ready() {
    if (MobPrefab == null) {
      GD.PrintErr("mob prefab null");
    }
  }
}
