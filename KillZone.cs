using Godot;

// 掉出地图的判定区域：Area3D + 一个无限大的平面碰撞体（WorldBoundaryShape3D）。
// 这里只做「谁掉下去了」的分发，不写具体规则：
//   玩家 -> 广播 PlayerFell，由订阅方（Player）自己决定怎么复活
//   怪物 -> 直接回收
// 这样 KillZone 不依赖 Player / UI / 计分，和 GameEvents 的解耦思路一致。
public partial class KillZone : Area3D {

  public override void _Ready() {
    // 用 C# 事件订阅内置信号：编译期类型检查，不用去编辑器里手动连线
    BodyEntered += OnBodyEntered;
  }

  // Area3D 与物理体重叠时触发，body 可能是 CharacterBody3D / RigidBody3D / StaticBody3D
  private void OnBodyEntered(Node3D body) {
    if (body is Player) {
      GameEvents.Instance.EmitPlayerFell();
      return;
    }

    if (body is Mob mob) {
      // 怪掉下去直接回收，不给分；想给分就把这行换成 mob.TakeDamage()
      mob.QueueFree();
    }
  }
}
