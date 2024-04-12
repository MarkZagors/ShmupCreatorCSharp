using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Editor
{
    public partial class SavingManager : Node
    {
        [Signal] public delegate void OnSaveEventHandler();

        public void OnSavingButtonClicked()
        {
            EmitSignal(SignalName.OnSave);
        }

        public void OnSaveAndExitClicked()
        {
            EmitSignal(SignalName.OnSave);
            GetTree().ChangeSceneToFile("res://scenes/pages/LevelPicker.tscn");
        }

        public void CreateNewLevel()
        {
            DirAccess levelsDirectory = DirAccess.Open("user://levels");
            Guid newLevelID = Guid.NewGuid();
            levelsDirectory.MakeDir(newLevelID.ToString());

            FileAccess indexFile = FileAccess.Open($"user://levels/{newLevelID}/index.json", FileAccess.ModeFlags.Write);
            indexFile.StoreString(
$@"{{
    ""id"": ""{newLevelID}"",
    ""levelName"": ""New Level"",
    ""levelAuthor"": ""Unknown"",
    ""songName"": ""Unknown song"",
    ""songAuthor"": ""Unknown Artist"",
}}"
            );
            indexFile.Close();

            FileAccess phasesFile = FileAccess.Open($"user://levels/{newLevelID}/phases.json", FileAccess.ModeFlags.Write);
            phasesFile.StoreString(
$@"{{
    ""phases"": [],
}}"
            );
            phasesFile.Close();
        }

        public void SaveLevelIndex(string levelID, string levelName, string levelAuthor, string songName, string songAuthor)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Write);
            indexFile.StoreString(
$@"{{
    ""id"": ""{levelID}"",
    ""levelName"": ""{levelName}"",
    ""levelAuthor"": ""{levelAuthor}"",
    ""songName"": ""{songName}"",
    ""songAuthor"": ""{songAuthor}"",
}}"
            );
            indexFile.Close();
        }

        public void SaveLevelPhases(List<Phase> phases, string levelID)
        {
            FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Write);
            phasesFile.StoreString(@"{""phases"": [");
            foreach (Phase phase in phases)
            {
                phasesFile.StoreString("{ ");
                // Store Phase information
                phasesFile.StoreString($@"""name"":""{phase.Name}"",");
                phasesFile.StoreString($@"""id"":{phase.ID},");
                phasesFile.StoreString($@"""health"":{phase.Health},");
                phasesFile.StoreString($@"""sequences"": [");
                //Store Sequence information
                foreach (Sequence sequence in phase.SequenceList)
                {
                    phasesFile.StoreString($@"""time"":{sequence.Time}");
                    //Sequence.Node is created and assigned when loading in PlayController
                    phasesFile.StoreString($@"""components"": [");
                    //Store ComponentInformation
                    foreach (IComponent component in sequence.Components)
                    {
                        phasesFile.StoreString($@"""name"":{component.Name}");
                    }

                    phasesFile.StoreString("]"); //end of components
                }

                phasesFile.StoreString("]"); //end of sequences
                phasesFile.StoreString(" }");
            }
            phasesFile.StoreString("]}");
            phasesFile.Close();
        }

        public Dictionary LoadLevelIndex(string levelID)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
            Dictionary data = (Dictionary)Json.ParseString(indexFile.GetAsText());
            indexFile.Close();
            return data;
        }

    }
}
