using Godot;
using System;

// 蝙蝠小怪：每帧朝玩家水平推进，受击掉血，血尽时广播死亡事件。
// 分数只通过 GameEvents 广播，Mob 自身不关心谁在计分。
public partial class Mob : RigidBody3D {

  // 追击速度（单位/秒）。TODO: 想要个体差异化时改成 [Export] 并在 _Ready 里随机取值
  private const float MoveSpeed = 3.0f;

  private BatModel _batModel;

  private Player _player;

  // 场景里挂好的两个 3D 音效播放器：AudioHurt 受击、AudioDie 死亡
  private AudioStreamPlayer3D _audioHurt;
  private AudioStreamPlayer3D _audioDie;

  private int _health = 5;

  // 打死这只怪给多少分，可在检查器里按怪的强度调
  [Export] public int ScoreValue = 1;

  public override void _Ready() {
    _batModel = GetNode<BatModel>("BatModel");
    _player = GetNode<Player>("/root/Game/Player");
    _audioHurt = GetNode<AudioStreamPlayer3D>("AudioHurt");
    _audioDie = GetNode<AudioStreamPlayer3D>("AudioDie");
  }

  public override void _PhysicsProcess(double delta) {
    // 每帧重新算一次朝向玩家的水平方向，玩家移动后能立刻跟上
    var dir = GlobalPosition.DirectionTo(_player.GlobalPosition);
    dir.Y = 0;

    LinearVelocity = dir * MoveSpeed;

    // 贴脸时 dir 接近零向量，归一化会出 NaN，先判长度再转向
    if (dir.LengthSquared() > 0.0001f) {
      FacePlayer(dir.Normalized());
    }
  }

  // 只转模型的 Y 轴（俯仰保持水平），刚体本身的 Transform 交给物理服务器，不去抢
  private void FacePlayer(Vector3 dir) {
    // Forward 是 (0,0,-1)，SignedAngleTo 算出把它转到 dir 需要绕 UP 转多少弧度；
    // 再 + PI 是因为蝙蝠模型自带 180 度偏转，不补就是背对玩家
    float yaw = Vector3.Forward.SignedAngleTo(dir, Vector3.Up) + Mathf.Pi;
    _batModel.GlobalRotation = new Vector3(0.0f, yaw, 0.0f);
  }

  // 被子弹命中时调用：播放受击表现 → 扣血 → 血尽则广播死亡
  public void TakeDamage() {
    if (_health <= 0) {
      // 已死，忽略后续命中，避免同一只怪重复扣分、重复加分
      return;
    }

    // 受击动画播完由动画树自动回到 Idle，不用手动切状态
    _batModel?.PlayOneShotAnimation();
    _audioHurt?.Play();

    _health -= 1;
    if (_health > 0) {
      return;
    }

    // 只广播「我死了，值多少分」，谁关心分数谁自己去订阅，Mob 不依赖 Player / UI
    GameEvents.Instance.EmitMobDied(ScoreValue);
    _audioDie?.Play();

    // TODO: 播死亡动画，等动画播完再 QueueFree()；
    // 现在直接释放会把刚播放的死亡音效一起掐断
    // QueueFree();
  }
}
