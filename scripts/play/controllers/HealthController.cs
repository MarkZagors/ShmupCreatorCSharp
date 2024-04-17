using Editor;
using Godot;
using System;
using System.Runtime;

public partial class HealthController : Node
{
    [Export] public PlayerController PlayerController { get; private set; }
    [Export] public VBoxContainer PlayerHealthVBox { get; private set; }
    public int PlayerCurrentHealth { get; set; }
    public int PlayerMaxHealth { get; set; }
    public int EnemyCurrentHealth { get; set; }
    public int EnemyMaxHealth { get; set; }

    public override void _Ready()
    {
        PlayerMaxHealth = 4;
        PlayerCurrentHealth = 4;
        EnemyCurrentHealth = 100;
        EnemyMaxHealth = 100;

        PlayerController.OnPlayerHit += OnPlayerHit;
    }

    public void OnPlayerHit()
    {
        PlayerCurrentHealth -= 1;
        UpdatePlayerUI();
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
