using Godot;

public partial class BatModel : Node3D {

  // 动画树里 OneShot 节点的 request 参数路径（对应 bat_model.tscn 里的 parameters/OneShot/request）
  private const string OneShotRequestPath = "parameters/OneShot/request";

  private AnimationTree _animationTree;

  public override void _Ready() {
    _animationTree = GetNode<AnimationTree>("AnimationTree");

    // 编辑器里 AnimationTree 的 Active 默认是关的，不打开它就不会接管 AnimationPlayer
    _animationTree.Active = true;
  }

  // 触发一次性动画（custom/aa）：播完自动回到 Idle
  public void PlayOneShotAnimation() {
    if (_animationTree == null) {
      return;
    }

    _animationTree.Set(OneShotRequestPath, (int)AnimationNodeOneShot.OneShotRequest.Fire);
  }
  public void PlayDead() {

  }
}
