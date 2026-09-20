using Godot;
using System;

// 玩家角色：第一人称视角控制 + 移动
public partial class Player : CharacterBody3D {

  // 移动速度（米/秒），可在检查器中调整
  [Export] public float Speed = 5.0f;

  // 鼠标灵敏度：每移动 1 像素对应的旋转弧度，值越大转得越快
  [Export] public float MouseSensitivity = 0.002f;

  // 俯仰角上下限（度），防止视角翻转
  [Export] public float MaxPitch = 80.0f;
  [Export] public PackedScene BulletPrefab;
  [Export] public Node3D BulletSpawnerNode;

  // 挂在角色身上的相机，只负责上下（俯仰）旋转
  private Camera3D _camera;
  private Timer _timer;

  public override void _Ready() {
    // 拿到子节点 Camera3D
    _camera = GetNode<Camera3D>("Camera3D");
    _timer = GetNode<Timer>("Timer");

    // 捕获鼠标：光标隐藏并锁定在窗口内，这样能持续拿到相对位移
    Input.MouseMode = Input.MouseModeEnum.Captured;
  }

  // 用 _UnhandledInput 处理输入，GUI 已经消费掉的事件不会传到这里
  public override void _UnhandledInput(InputEvent @event) {

    // ui_cancel 是 Godot 内置动作，默认绑定 ESC：释放鼠标方便退出或操作编辑器
    if (@event.IsActionPressed("ui_cancel")) {
      Input.MouseMode = Input.MouseModeEnum.Visible;
      return;
    }

    // 鼠标没被捕获时不做视角旋转，避免误操作
    if (Input.MouseMode != Input.MouseModeEnum.Captured) {
      return;
    }

    // 鼠标相对上一帧的位移
    if (@event is InputEventMouseMotion motion) {

      // 左右移动 -> 角色绕 Y 轴转（水平视角）
      // 取负号是因为鼠标右移时，Godot 中绕 Y 轴正向旋转是向左
      RotateY(-motion.Relative.X * MouseSensitivity);

      // 上下移动 -> 相机绕自身 X 轴转（俯仰）
      // 只转相机不转身体，角色不会跟着上下翻
      _camera.RotateX(-motion.Relative.Y * MouseSensitivity);

      // 夹住俯仰角，防止越过头顶导致画面翻转
      Vector3 rotation = _camera.Rotation;
      rotation.X = Mathf.Clamp(
        rotation.X,
        Mathf.DegToRad(-MaxPitch),
        Mathf.DegToRad(MaxPitch)
      );
      _camera.Rotation = rotation;
    }
  }

  // 物理帧：所有移动相关逻辑都放这里，MoveAndSlide 必须在物理帧调用
  public override void _PhysicsProcess(double delta) {

    // 把四个方向键合成一个 2D 输入向量
    // y 为负表示前进（屏幕坐标系向上为负）
    Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

    // 把输入转换到角色自身朝向的世界方向：
    // Basis.X 是角色右方，Basis.Z 是角色后方（Godot 中 -Z 为前）
    // 所以按 W 时 input.Y = -1，Basis.Z * -1 正好是前方
    Vector3 direction = (Basis.X * input.X + Basis.Z * input.Y).Normalized();

    // 直接赋值而不是累加，松键立刻停下，没有惯性
    direction = direction * Speed;
    direction.Y = Velocity.Y;

    direction.Y = direction.Y - 20f * (float)delta; // 给个恒定的速度不断向下
    if (Input.IsActionJustPressed("jump") && IsOnFloor()) {
      direction.Y = 10;
    }
    if (Input.IsActionJustReleased("jump") && direction.Y > 0) {
      // 空格一放开就马上向下掉
      direction.Y = 0;
    }

    Velocity = direction;

    // 按 Velocity 移动并由引擎处理碰撞滑动
    MoveAndSlide();

    Shoot();
  }

  public void Shoot() {
    if (!(Input.IsActionPressed("shoot") && _timer.IsStopped())) {
      // 不是（按下并且停了计时器）
      return;
    }
    _timer.Start(); // 重新开始计时

    // 这两个导出变量要在编辑器里手动赋值；字段改名后场景里存的旧值会失效，这里兜一下防止空引用
    if (BulletPrefab == null || BulletSpawnerNode == null) {
      GD.PrintErr("BulletPrefab 或 BulletSpawnerNode 未赋值，无法发射子弹");
      return;
    }

    Node3D bullet = BulletPrefab.Instantiate<Node3D>();
    // bullet 现在是 BulletSpawnerNode 的子节点，Transform 是相对父节点的局部坐标，
    // 所以要用 GlobalTransform 赋值，用 Transform 会把偏移叠两遍

    BulletSpawnerNode.AddChild(bullet);
    bullet.TopLevel = true;
    bullet.GlobalTransform = BulletSpawnerNode.GlobalTransform;


    GD.Print($"transform {bullet.Transform}");
  }
}
