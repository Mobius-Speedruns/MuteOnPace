using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class CPHInline
{
    public bool Execute()
    {
        // Config from global vars
        // Check if currently muted
        bool currentlyMuted = CPH.GetGlobalVar<bool>("paceman_muted", true);
        if (currentlyMuted)
        {
            // Check walling state
            bool runReset = IsWalling();
            // Unmute when run reset
            if (runReset)
                return lostPace();
            IEnumerable<string> eventLog = GetEventLog();
            Pace currentPace = ExtractCurrentPace(eventLog);
            // Unmute if finished
            if (currentPace.credits.HasValue)
                return lostPace();
            // Unmute if coped
            if (IsCheated(eventLog))
                return lostPace();
            return doNothing();
        }
        else
        {
            // Check walling state
            bool runReset = IsWalling();
            // Remain muted
            if (runReset)
                return doNothing();
            // Fetch IGT records
            IEnumerable<string> eventLog = GetEventLog();
            // Leave muted if coped
            if (IsCheated(eventLog))
                return doNothing();
            Pace currentPace = ExtractCurrentPace(eventLog);
            // Remain muted
            if (currentPace.credits.HasValue)
				return doNothing();
            PaceSplits paceSplits = GetPaceSplits();
            List<bool> splitValues = DetermineSplits(currentPace, paceSplits);
            // Mute if on pace
            if (splitValues.Any(b => b))
                return onPace();
            // Remain muted
            return doNothing();
        }
    }

    // Function which runs when on pace
    public bool onPace()
    {
        CPH.SetArgument("paceman_should_mute", "mute");
        return true;
    }

    // Function which runs when no longer on pace
    public bool lostPace()
    {
        CPH.SetArgument("paceman_should_mute", "unmute");
        return true;
    }

    // Function which runs when not on pace (i.e starting run)
    public bool doNothing()
    {
        CPH.SetArgument("paceman_should_mute", "donothing");
        return true;
    }

    // Helper function to determine if on Pace
    private List<bool> DetermineSplits(Pace currentPace, PaceSplits splits)
    {
        // Determine first_structure and second_structure dynamically
        int? first_structure = null;
        int? second_structure = null;
        if (currentPace.enter_bastion.HasValue && currentPace.enter_fortress.HasValue)
        {
            if (currentPace.enter_fortress < currentPace.enter_bastion)
            {
                first_structure = currentPace.enter_fortress;
                second_structure = currentPace.enter_bastion;
            }
            else
            {
                first_structure = currentPace.enter_bastion;
                second_structure = currentPace.enter_fortress;
            }
        }
        else if (currentPace.enter_bastion.HasValue)
        {
            first_structure = currentPace.enter_bastion;
        }
        else if (currentPace.enter_fortress.HasValue)
        {
            first_structure = currentPace.enter_fortress;
        }

        // Build the list of bools
        return new List<bool>
        {
            CompareSplit(currentPace.enter_nether, splits.enter_nether),
            CompareSplit(first_structure, splits.first_structure),
            CompareSplit(second_structure, splits.second_structure),
            CompareSplit(currentPace.first_portal, splits.first_portal),
            CompareSplit(currentPace.enter_stronghold, splits.enter_stronghold),
            CompareSplit(currentPace.enter_end, splits.enter_end),
            CompareSplit(currentPace.credits, splits.credits)
        };
    }

    // Get run details from speedrunigt
    public Pace ExtractCurrentPace(IEnumerable<string> eventlog)
    {
        Pace pace = new();
        try
        {
            var paceType = typeof(Pace);
            foreach (var line in eventlog)
            {
                if (line.StartsWith("rsg."))
                {
                    var parts = line.Split(' ');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int igTime))
                    {
                        string eventKey = parts[0].Replace("rsg.", "");
                        var prop = paceType.GetProperty(eventKey);
                        if (prop != null && prop.PropertyType == typeof(int? ))
                        {
                            prop.SetValue(pace, igTime);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogDebug("Error extracting Pace: " + ex.Message);
        }

        return pace;
    }

    // Helper method to compare nullable ints
    private bool CompareSplit(int? current, int? target)
    {
        if (current.HasValue && target.HasValue)
        {
            return current.Value < target.Value;
        }

        return false;
    }

    // Helper function to read the PaceSplits settings
    private PaceSplits GetPaceSplits()
    {
        int? enter_nether = CPH.GetGlobalVar<int>("paceman_enter_nether", true);
        int? first_structure = CPH.GetGlobalVar<int>("paceman_first_structure", true);
        int? second_structure = CPH.GetGlobalVar<int>("paceman_second_structure", true);
        int? first_portal = CPH.GetGlobalVar<int>("paceman_first_portal", true);
        int? enter_stronghold = CPH.GetGlobalVar<int>("paceman_enter_stronghold", true);
        int? enter_end = CPH.GetGlobalVar<int>("paceman_enter_end", true);
        int? credits = CPH.GetGlobalVar<int>("paceman_credits", true);
        return new PaceSplits
        {
            enter_nether = enter_nether,
            first_structure = first_structure,
            second_structure = second_structure,
            first_portal = first_portal,
            enter_stronghold = enter_stronghold,
            enter_end = enter_end,
            credits = credits
        };
    }

    // Helper function to determine if on wall
    private bool IsWalling()
    {
        /* 
    	Path to wpstateout.txt, within instance folder. Will likely be:
			C:\Users\<YOURNAME>\path\to\minecraft\instance\.minecraft\wpstateout.txt
    	*/
        string wpstateoutPath = CPH.GetGlobalVar<string>("paceman_wpstateoutPath", true);
        if (!File.Exists(wpstateoutPath))
        {
            CPH.LogDebug("wpstateout not found.");
            return true;
        }

        // File is constantly being written to by speedrunigt, use shared read mode.
        using (var stream = new FileStream(wpstateoutPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            string line = reader.ReadLine();
            CPH.LogDebug($"{line}");
            return line != null && line.ToLower().Contains("wall");
        }
    }

    // Helper function to determine if run was cheated
    private bool IsCheated(IEnumerable<string> eventlog)
    {
        foreach (var line in eventlog)
        {
            if (line.Contains("enable_cheats"))
                return true;
        }

        return false;
    }

    // Read event logs
    public IEnumerable<string>? GetEventLog()
    {
        string igtPath = CPH.GetGlobalVar<string>("paceman_igtPath", true);
        try
        {
            string jsonPath = Path.Combine(igtPath, "latest_world.json");
            if (!File.Exists(jsonPath))
            {
                CPH.LogDebug("latest_world.json not found.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            WorldFile worldFile = JsonConvert.DeserializeObject<WorldFile>(json);
            if (worldFile == null || string.IsNullOrWhiteSpace(worldFile.world_path))
            {
                CPH.LogDebug("Invalid world_path in JSON.");
                return null;
            }

            string eventLogPath = Path.Combine(worldFile.world_path, "speedrunigt", "events.log");
            if (!File.Exists(eventLogPath))
            {
                CPH.LogDebug("events.log not found.");
                return null;
            }

            return File.ReadLines(eventLogPath);
        }
        catch (Exception ex)
        {
            CPH.LogDebug($"Exception while reading events.log: {ex.Message}");
            return null;
        }
    }
}

public class WorldFile
{
    public string world_path { get; set; }
}

public class Pace
{
    public int? enter_nether { get; set; }
    public int? enter_bastion { get; set; }
    public int? enter_fortress { get; set; }
    public int? first_portal { get; set; }
    public int? enter_stronghold { get; set; }
    public int? enter_end { get; set; }
    public int? credits { get; set; }
}

public class PaceSplits
{
    public int? enter_nether { get; set; }
    public int? first_structure { get; set; }
    public int? second_structure { get; set; }
    public int? first_portal { get; set; }
    public int? enter_stronghold { get; set; }
    public int? enter_end { get; set; }
    public int? credits { get; set; }
}