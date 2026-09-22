using Godot;
using System;

public partial class Bullet : Area3D {

  // 存活时间（秒），可在检查器里调整
  [Export] public float Lifetime = 2.0f;
  [Export] public float Speed = 15f;

  // 已经存活的时间，纯 C# 自己累加，不用引擎的定时器
  private float _elapsed;

  // 防止同一帧内多次碰撞重复触发销毁
  private bool _isDestroyed;

  public override void _Ready() {
    _elapsed = 0.0f;
    _isDestroyed = false;

    // 用 C# 事件语法订阅内置节点事件：编译期类型检查、可一键重命名，不需要在编辑器里连线
    BodyEntered += OnBodyEntered;
  }

  // Area3D 与物理体（RigidBody3D / CharacterBody3D / StaticBody3D）重叠时触发
  private void OnBodyEntered(Node3D body) {
    if (_isDestroyed) {
      return;
    }

    // 打到怪物就让它掉血
    if (body is Mob mob) {
      mob.takeDamage();
    }

    // 子弹命中即销毁，不论撞到的是怪物还是墙/地面
    Destroy();
  }

  private void Destroy() {
    _isDestroyed = true;
    QueueFree();
  }

  public override void _PhysicsProcess(double delta) {

    // Godot 3D 里 -Z 才是正前方，Basis.Z 指向背后，所以要取负号
    // 用 GlobalBasis（世界旋转）配合 GlobalPosition（世界坐标），避免父节点旋转叠加影响
    var dir = -GlobalBasis.Z.Normalized() * Speed * (float)delta;

    GlobalPosition += dir;

    // 累计存活时间，超过就销毁自己
    _elapsed += (float)delta;
    if (_elapsed >= Lifetime) {
      Destroy();
    }
  }
}
