using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

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
                    phasesFile.StoreString("{ ");
                    phasesFile.StoreString($@"""time"":{sequence.Time},");
                    //Sequence.Node is created and assigned when loading in PlayController
                    phasesFile.StoreString($@"""components"": [");
                    //Store ComponentInformation
                    foreach (IComponent component in sequence.Components)
                    {
                        phasesFile.StoreString("{ ");
                        phasesFile.StoreString($@"""type"":""{component.Type}"",");
                        phasesFile.StoreString($@"""name"":""{component.Name}"",");
                        phasesFile.StoreString($@"""modifiers"": [");
                        StoreModifiers(phasesFile, component);

                        phasesFile.StoreString("]"); //end of modifiers
                        phasesFile.StoreString(" },");
                    }

                    phasesFile.StoreString("]"); //end of components
                    phasesFile.StoreString(" },");
                }

                phasesFile.StoreString("]"); //end of sequences
                phasesFile.StoreString(" },");
            }
            phasesFile.StoreString("]}");
            phasesFile.Close();
        }

        private void StoreModifiers(FileAccess phasesFile, IComponent component)
        {
            foreach (IModifier modifier in component.Modifiers)
            {
                if (!modifier.Active) continue;
                phasesFile.StoreString("{ ");
                phasesFile.StoreString($@"""id"":""{modifier.ID}"",");
                phasesFile.StoreString($@"""type"":""{modifier.Type}"",");
                switch (modifier.Type)
                {
                    case ModifierType.DOUBLE:
                        ModifierDouble modifierDouble = (ModifierDouble)modifier;
                        phasesFile.StoreString($@"""value"":{modifierDouble.Value},");
                        break;
                    case ModifierType.INTEGER:
                        ModifierInteger modifierInteger = (ModifierInteger)modifier;
                        phasesFile.StoreString($@"""value"":{modifierInteger.Value},");
                        break;
                    case ModifierType.OPTIONS:
                        ModifierOptions modifierOptions = (ModifierOptions)modifier;
                        phasesFile.StoreString($@"""value"":""{modifierOptions.SelectedOption}"",");
                        break;
                    case ModifierType.POSITION:
                        ModifierPosition modifierPosition = (ModifierPosition)modifier;
                        phasesFile.StoreString($@"""value"":""{modifierPosition.Position}"",");
                        break;
                    case ModifierType.RANGE:
                        ModifierRange modifierRange = (ModifierRange)modifier;
                        phasesFile.StoreString($@"""max"":""{modifierRange.Range.Max.Value}"",");
                        phasesFile.StoreString($@"""min"":""{modifierRange.Range.Min.Value}"",");
                        phasesFile.StoreString($@"""points"": [");
                        foreach (Vector2 point in modifierRange.Range.Points)
                        {
                            phasesFile.StoreString($@"""{point}"",");
                        }
                        phasesFile.StoreString("]"); //end of points
                        break;
                    case ModifierType.REFERENCE:
                        ModifierRef modifierRef = (ModifierRef)modifier;
                        if (modifierRef.Ref != null)
                            phasesFile.StoreString($@"""componentRefName"":""{modifierRef.Ref.Name}"",");
                        else
                            phasesFile.StoreString($@"""componentRefName"":""<NULL>"",");
                        break;
                    default:
                        GD.PrintErr($"Modifier type not supported in SavingManager: {modifier.Type}");
                        break;
                }
                phasesFile.StoreString(" },");
            }
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
