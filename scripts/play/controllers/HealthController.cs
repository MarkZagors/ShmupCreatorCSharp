using Editor;
using Godot;
using System;
using System.Runtime;

public partial class HealthController : Node
{
    [Signal] public delegate void OnEnemyDeathEventHandler();
    [Signal] public delegate void OnPlayerDeathEventHandler();
    [Export] public PlayerController PlayerController { get; private set; }
    [Export] public VBoxContainer PlayerHealthVBox { get; private set; }
    [Export] public PlayStateController PlayStateController { get; private set; }
    [Export] public ProgressBar EnemyHealthBarNode { get; private set; }
    [Export] public Node2D PlayerNode { get; private set; }
    public int PlayerCurrentHealth { get; set; }
    public int PlayerMaxHealth { get; set; }
    public int EnemyCurrentHealth { get; set; }
    public int EnemyMaxHealth { get; set; }

    public override void _Ready()
    {
        PlayerMaxHealth = 4;
        PlayerCurrentHealth = 4;
        EnemyCurrentHealth = 200;
        EnemyMaxHealth = 200;

        PlayerController.OnPlayerHit += OnPlayerHit;
        PlayStateController.PhaseChange += OnPhaseChange;
        PlayStateController.OnWin += OnWin;
        UpdatePlayerUI();
        UpdateEnemyUI();
    }

    public void OnPlayerHit()
    {
        PlayerCurrentHealth -= 1;
        UpdatePlayerUI();

        if (PlayerCurrentHealth <= 0)
        {
            OnLose();
        }
    }

    public void OnEnemyHitboxEnter(Area2D area)
    {
        if (PlayStateController.PlayState == PlayState.MAIN)
        {
            HitEnemy(area);
        }
    }

    private void HitEnemy(Area2D area)
    {
        EnemyCurrentHealth -= 1;
        Node2D playerBullet = area.GetParent<Node2D>();
        playerBullet.QueueFree();

        if (EnemyCurrentHealth < 1)
        {
            EmitSignal(SignalName.OnEnemyDeath);
        }

        UpdateEnemyUI();
    }

    private void OnPhaseChange()
    {
        EnemyCurrentHealth = 200;
        EnemyMaxHealth = 200;
        UpdateEnemyUI();
    }

    private void OnWin()
    {
        PlayerNode.GetNode<Area2D>("PlayerHitbox").SetDeferred("monitoring", false);
    }

    private void OnLose()
    {
        EmitSignal(SignalName.OnPlayerDeath);
        PlayerNode.GetNode<Area2D>("PlayerHitbox").SetDeferred("monitoring", false);
        PlayerNode.Visible = false;
    }

    private void UpdateEnemyUI()
    {
        EnemyHealthBarNode.MaxValue = EnemyMaxHealth;
        EnemyHealthBarNode.Value = EnemyCurrentHealth;
    }

    private void UpdatePlayerUI()
    {
        Godot.Collections.Array<Node> list = PlayerHealthVBox.GetChildren();
        for (int i = 0; i < list.Count; i++)
        {
            Control lifePointNode = (Control)list[i];
            if (i >= PlayerCurrentHealth)
            {
                lifePointNode.Visible = false;
            }
            else
            {
                lifePointNode.Visible = true;
            }
        }
    }
}
