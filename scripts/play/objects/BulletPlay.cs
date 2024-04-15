using Editor;
using Godot;
using System;

public partial class BulletPlay : Node2D
{
    public Vector2 Velocity { get; set; }
    public bool IsClearProtected { get; set; } = false;
    public BulletData BulletData { get; set; } = null;
}
