using Godot;

public partial class PlayerAttackController : Node
{
    [Export] public PackedScene PlayerAttackBulletObj { get; private set; }
    [Export] public Node2D PlayerBulletGroupNode { get; private set; }
    [Export] public Node2D PlayerNode { get; private set; }
    private float _attackTimerThreshold = 0.1f;
    private float _bulletSpeed = 1500f;
    private float _attackTimerCurrent = 0.0f;
    private bool _focused = false;

    public override void _Process(double delta)
    {
        _attackTimerCurrent += (float)delta;

        if (Input.IsActionPressed("player_focus"))
        {
            _focused = true;
        }
        else
        {
            _focused = false;
        }

        if (_attackTimerCurrent > _attackTimerThreshold)
        {
            DoAttack();
        }

        MoveBullets((float)delta);
    }

    public void DoAttack()
    {
        _attackTimerCurrent = 0.0f;
        for (int i = 0; i < 5; i++)
        {
            Node2D playerBullet = PlayerAttackBulletObj.Instantiate<Node2D>();
            playerBullet.GlobalPosition = PlayerNode.GlobalPosition;
            float offsetx = (i - 2) * 20;
            float offsety = Mathf.Abs((i - 2) * 10);
            if (!_focused)
            {
                float rotationOffset = (i - 2) * 10;
                playerBullet.RotationDegrees = rotationOffset;
            }
            playerBullet.Position = new Vector2(playerBullet.Position.X + offsetx, playerBullet.Position.Y + offsety);
            PlayerBulletGroupNode.AddChild(playerBullet);
        }
    }

    private void MoveBullets(float delta)
    {
        foreach (Node2D bullet in PlayerBulletGroupNode.GetChildren())
        {
            Vector2 velocity = new Vector2(0, -1).Rotated(bullet.Rotation) * _bulletSpeed * delta;
            bullet.Position += velocity;
            if (bullet.Position.Y < -100)
            {
                bullet.QueueFree();
            }
        }
    }
}
