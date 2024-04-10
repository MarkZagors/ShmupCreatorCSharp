using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class Phase
    {
        public string Name { get; private set; }
        public int Health { get; private set; }
        public List<Sequence> SequenceList { get; private set; }

        public Phase(string name, int health, List<Sequence> sequenceList)
        {
            Name = name;
            Health = health;
            SequenceList = sequenceList;
        }
    }
}
