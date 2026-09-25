using Godot;
using System;

public partial class Mob : RigidBody3D {

  private BatModel _batModel;

  private Player _player;

  // private speed=Random()

  private int health = 5;

  public override void _Ready() {
    _batModel = GetNode<BatModel>("BatModel");
    _player = GetNode<Player>("/root/Game/Player");
  }

  public override void _PhysicsProcess(double delta) {
    var dir = GlobalPosition.DirectionTo(_player.GlobalPosition);
    dir.Y = 0;

    LinearVelocity = dir * 3;

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

  public void takeDamage() {
    if (health == 0) {
      return;
    }
    // 受击时让模型播一次受击动画，播完由动画树自动回到 Idle
    _batModel?.PlayOneShotAnimation();
    health -= 1;
    if (health == 0) {
      // 播放死亡动画
      // _batModel.
    }

  }
}
