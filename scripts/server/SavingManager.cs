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
    ""isMusicAdded"": ""False""
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

        public void SaveLevelIndex(string levelID, string levelName, string levelAuthor, string songName, string songAuthor, bool isMusicAdded)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Write);
            indexFile.StoreString(
$@"{{
    ""id"": ""{levelID}"",
    ""levelName"": ""{levelName}"",
    ""levelAuthor"": ""{levelAuthor}"",
    ""songName"": ""{songName}"",
    ""songAuthor"": ""{songAuthor}"",
    ""isMusicAdded"": ""{isMusicAdded}""
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
                phasesFile.StoreString($@"""loopTime"":{phase.LoopTime},");
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
                        GD.PrintErr($"Modifier type not supported SAVING in SavingManager: {modifier.Type}");
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

        public List<Phase> LoadLevelPhases(string levelID)
        {
            FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Read);
            Dictionary data = (Dictionary)Json.ParseString(phasesFile.GetAsText());
            phasesFile.Close();

            if (((Godot.Collections.Array)data["phases"]).Count == 0)
            {
                //New level handling
                return new List<Phase> {
                    new Phase(
                        name: "New Phase",
                        id: 0,
                        health: 100,
                        loopTime: 1.0,
                        sequenceList: new List<Sequence>()
                    )
                };
            }

            List<Phase> phases = new();
            foreach (Dictionary phasesJson in (Godot.Collections.Array)data["phases"])
            {
                List<Sequence> sequences = new();
                foreach (Dictionary sequencesJson in (Godot.Collections.Array)phasesJson["sequences"])
                {
                    Sequence sequence = new Sequence
                    {
                        Time = (double)sequencesJson["time"],
                        Components = new List<IComponent>()
                    };

                    foreach (Dictionary componentsJson in (Godot.Collections.Array)sequencesJson["components"])
                    {
                        bool _ = Enum.TryParse((string)componentsJson["type"], out ComponentType componentType);
                        IComponent component = ComponentFactory.CreateComponent(
                            componentType: componentType,
                            name: (string)componentsJson["name"],
                            sequence: sequence
                        );
                        LoadModifiers(componentsJson, component);

                        sequence.Components.Add(component);
                    }
                    ConnectComponentRefs(sequence.Components);
                    SpawnerVerifier.Verify(sequence.Components);
                    sequences.Add(sequence);
                }

                phases.Add(new Phase(
                    name: (string)phasesJson["name"],
                    id: (int)phasesJson["id"],
                    loopTime: (double)phasesJson["loopTime"],
                    health: (int)phasesJson["health"],
                    sequenceList: sequences
                ));
            }

            return phases;
        }

        private void LoadModifiers(Dictionary componentsJson, IComponent loadedComponent)
        {
            foreach (Dictionary modifiersJson in (Godot.Collections.Array)componentsJson["modifiers"])
            {
                bool errType = Enum.TryParse((string)modifiersJson["type"], out ModifierType modifierType);
                bool errId = Enum.TryParse((string)modifiersJson["id"], out ModifierID modifierID);
                switch (modifierType)
                {
                    case ModifierType.DOUBLE:
                        ModifierDouble modifierDouble = (ModifierDouble)loadedComponent.GetModifier(modifierID);
                        double valueDouble = (double)modifiersJson["value"];
                        modifierDouble.Value = valueDouble;
                        modifierDouble.Active = true;
                        break;
                    case ModifierType.INTEGER:
                        ModifierInteger modifierInteger = (ModifierInteger)loadedComponent.GetModifier(modifierID);
                        int valueInteger = (int)modifiersJson["value"];
                        modifierInteger.Value = valueInteger;
                        modifierInteger.Active = true;
                        break;
                    case ModifierType.OPTIONS:
                        ModifierOptions modifierOptions = (ModifierOptions)loadedComponent.GetModifier(modifierID);
                        bool errOption = Enum.TryParse((string)modifiersJson["value"], out Option option);
                        modifierOptions.SelectedOption = option;
                        modifierOptions.Active = true;
                        break;
                    case ModifierType.POSITION:
                        ModifierPosition modifierPosition = (ModifierPosition)loadedComponent.GetModifier(modifierID);
                        Vector2 valuePosition = (Vector2)GD.StrToVar("Vector2" + (string)modifiersJson["value"]);
                        modifierPosition.Position = valuePosition;
                        modifierPosition.Active = true;
                        break;
                    case ModifierType.RANGE:
                        ModifierRange modifierRange = (ModifierRange)loadedComponent.GetModifier(modifierID);
                        double valueMaxRange = (double)modifiersJson["max"];
                        double valueMinRange = (double)modifiersJson["min"];
                        List<Vector2> points = new();
                        foreach (var pointVariant in (Godot.Collections.Array)modifiersJson["points"])
                        {
                            Vector2 vectorPoint = (Vector2)GD.StrToVar("Vector2" + (string)pointVariant);
                            points.Add(vectorPoint);
                        }
                        modifierRange.Range = Range.ManualRange(
                            max: valueMaxRange,
                            min: valueMinRange,
                            points: points
                        );
                        modifierRange.Active = true;
                        break;
                    case ModifierType.REFERENCE:
                        ModifierRef modifierRef = (ModifierRef)loadedComponent.GetModifier(modifierID);
                        string componentRefName = (string)modifiersJson["componentRefName"];
                        modifierRef.LoadedRefName = componentRefName;
                        modifierRef.Active = true;
                        break;
                    default:
                        GD.PrintErr($"Modifier type not supported LOADING in SavingManager: {modifierType}");
                        break;
                }
            }
        }

        private void ConnectComponentRefs(List<IComponent> components)
        {
            foreach (IComponent component in components)
            {
                foreach (IModifier modifier in component.Modifiers)
                {
                    if (modifier.Active && modifier.Type == ModifierType.REFERENCE)
                    {
                        ModifierRef modifierRef = (ModifierRef)modifier;
                        if (modifierRef.LoadedRefName != "<NULL>")
                        {
                            modifierRef.Ref = GetComponentByName(components, modifierRef.LoadedRefName);
                        }
                    }
                }
            }
        }

        private IComponent GetComponentByName(List<IComponent> components, string searchName)
        {
            foreach (IComponent component in components)
            {
                if (component.Name == searchName)
                    return component;
            }
            GD.PrintErr("Component Not found in Saving Manger Loading GetComponentByName");
            return null;
        }

        public AudioStream LoadMusic(string levelID)
        {
            string absoluteMusicPath = OS.GetUserDataDir();
            absoluteMusicPath += $"/levels/{levelID}/music.mp3";
            FileAccess musicFile = FileAccess.Open(absoluteMusicPath, FileAccess.ModeFlags.Read);
            AudioStreamMP3 audioStream = new AudioStreamMP3();
            audioStream.Data = musicFile.GetBuffer((long)musicFile.GetLength());
            audioStream.Loop = true;
            musicFile.Close();
            return audioStream;
        }
    }
}
