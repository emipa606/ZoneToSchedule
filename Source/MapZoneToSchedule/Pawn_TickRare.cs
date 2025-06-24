using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace MapZoneToSchedule;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.TickRare))]
public class Pawn_TickRare
{
    private static readonly Dictionary<Pawn, string> lastManualArea = new();
    private static readonly Dictionary<Pawn, string> lastAutoArea = new();

    public static void Postfix(Pawn __instance)
    {
        if (__instance == null)
        {
            return;
        }

        if (!__instance.IsColonistPlayerControlled)
        {
            return;
        }

        if (__instance.Drafted)
        {
            return;
        }

        if (__instance.timetable == null)
        {
            return;
        }

        if (__instance.playerSettings == null)
        {
            return;
        }

        verifyArea(__instance);
    }

    private static void verifyArea(Pawn pawn)
    {
        var currentAssignmentLabel = pawn.timetable.CurrentAssignment.label.ToLower();
        var currentAreaRestriction = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
        var currentAreaRestrictionLabel = "unrestricted";
        if (currentAreaRestriction != null)
        {
            currentAreaRestrictionLabel = currentAreaRestriction.Label.ToLower();
        }

        if (currentAreaRestrictionLabel == currentAssignmentLabel)
        {
            return;
        }

        var possibleAssignmentArea = getAreaByLabel(pawn, currentAssignmentLabel);
        if (possibleAssignmentArea == null)
        {
            lastAutoArea.Remove(pawn);
            if (!lastManualArea.TryGetValue(pawn, out _))
            {
                return;
            }

            possibleAssignmentArea = getAreaByLabel(pawn, lastManualArea[pawn]);
            trySetAssignment(pawn, possibleAssignmentArea);
            lastManualArea.Remove(pawn);
        }
        else
        {
            if (lastAutoArea.ContainsKey(pawn) &&
                currentAssignmentLabel == lastAutoArea[pawn])
            {
                lastManualArea[pawn] = currentAreaRestrictionLabel;
                return;
            }

            trySetAssignment(pawn, possibleAssignmentArea);
            lastAutoArea[pawn] = possibleAssignmentArea.Label.ToLower();
        }
    }

    private static Area getAreaByLabel(Pawn pawn, string areaLabel)
    {
        return pawn.Map.areaManager.AllAreas.FirstOrDefault(area => area.Label.ToLower() == areaLabel);
    }

    private static void trySetAssignment(Pawn pawn, Area newArea)
    {
        pawn.playerSettings.AreaRestrictionInPawnCurrentMap = newArea;
    }
}