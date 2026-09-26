using Godot;

// 全局事件总线：注册为 Autoload 后任何节点都能拿到它。
// 它只负责转发信号，自己不含任何游戏逻辑，用来打断「发送者」和「接收者」之间的直接依赖。
// 好处：Mob 不需要知道 Player 是否存在、挂在哪；以后刷出来的新 Mob 也不用额外连线。
public partial class GameEvents : Node {

  // 怪物死亡，参数是这只怪值多少分
  [Signal] public delegate void MobDiedEventHandler(int score);

  // 静态入口，省掉每次 GetNode<GameEvents>("/root/GameEvents")
  public static GameEvents Instance { get; private set; }

  public override void _EnterTree() {
    Instance = this;
  }

  // 包一层方法，调用方不用记字符串信号名，也不容易拼错
  public void EmitMobDied(int score) {
    EmitSignalMobDied(score);
  }
}
