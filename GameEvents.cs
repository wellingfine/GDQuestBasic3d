using Godot;
using System;

// 全局事件总线：注册为 Autoload 后任何节点都能拿到它。
// 它只负责转发信号，自己不含任何游戏逻辑，用来打断「发送者」和「接收者」之间的直接依赖。
// 好处：Mob 不需要知道 Player 是否存在、挂在哪；以后刷出来的新 Mob 也不用额外连线。
public partial class GameEvents : Node {

  // 怪物死亡，参数是这只怪值多少分
  [Signal] public delegate void MobDiedEventHandler(int score);

  // 玩家掉出地图（掉进 KillZone）
  // 这里刻意用纯 C# 委托而不是 [Signal]：订阅方全是 C#，没有 GDScript / 编辑器连线需求，
  // 那就没必要付「装箱成 Variant + 跨 C#/C++ 边界 + 拆箱」这趟往返（对比上面的 MobDied）
  public event Action PlayerFell;

  // 静态入口，省掉每次 GetNode<GameEvents>("/root/GameEvents")
  public static GameEvents Instance { get; private set; }

  public override void _EnterTree() {
    Instance = this;
  }

  // 包一层方法，调用方不用记字符串信号名，也不容易拼错
  public void EmitMobDied(int score) {
    EmitSignalMobDied(score);
  }

  // 同样包一层：event 只能在声明类内部 Invoke，广播权归总线自己
  // ?.Invoke() 保证没人订阅时不报错
  public void EmitPlayerFell() {
    PlayerFell?.Invoke();
  }
}
