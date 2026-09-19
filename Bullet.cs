using Godot;
using System;

public partial class Bullet : Area3D {

  // 存活时间（秒），可在检查器里调整
  [Export] public float Lifetime = 2.0f;
  [Export] public float Speed = 15f;

  // 已经存活的时间，纯 C# 自己累加，不用引擎的定时器
  private float _elapsed;

  public override void _Ready() {
    _elapsed = 0.0f;
  }

  public override void _PhysicsProcess(double delta) {

    // Godot 3D 里 -Z 才是正前方，Basis.Z 指向背后，所以要取负号
    // 用 GlobalBasis（世界旋转）配合 GlobalPosition（世界坐标），避免父节点旋转叠加影响
    var dir = -GlobalBasis.Z.Normalized() * Speed * (float)delta;

    GlobalPosition += dir;

    // 累计存活时间，超过就销毁自己
    _elapsed += (float)delta;
    if (_elapsed >= Lifetime) {
      QueueFree();
    }
  }
}
