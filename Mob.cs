using Godot;
using System;

public partial class Mob : Node3D {

  private BatModel _batModel;

  public override void _Ready() {
    _batModel = GetNode<BatModel>("BatModel");
  }

  public void takeDamage() {
    // 受击时让模型播一次受击动画，播完由动画树自动回到 Idle
    _batModel?.PlayOneShotAnimation();
  }
}
