using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace MapZoneToSchedule
{
    [HarmonyPatch(typeof(Pawn), "TickRare")]
    public class Pawn_TickRare_Patch
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

            MapZoneToSchedule.WriteDebug(
                $"Verifying area for {__instance.NameShortColored}");
            verifyArea(__instance);
        }

        private static void verifyArea(Pawn pawn)
        {
            var currentAssignmentLabel = pawn.timetable.CurrentAssignment.label.ToLower();
            var currentAreaRestriction = pawn.playerSettings.AreaRestriction;
            var currentAreaRestrictionLabel = "unrestricted";
            if (currentAreaRestriction != null)
            {
                currentAreaRestrictionLabel = currentAreaRestriction.Label.ToLower();
            }

            if (currentAreaRestrictionLabel == currentAssignmentLabel)
            {
                MapZoneToSchedule.WriteDebug(
                    $"{pawn.NameShortColored} area {currentAreaRestrictionLabel} matches assignment {currentAssignmentLabel}");
                return;
            }

            var possibleAssignmentArea = getAreaByLabel(pawn, currentAssignmentLabel);
            if (possibleAssignmentArea == null)
            {
                MapZoneToSchedule.WriteDebug(
                    $"Found no area matching {currentAssignmentLabel} for {pawn.NameShortColored}");
                lastAutoArea.Remove(pawn);
                if (!lastManualArea.ContainsKey(pawn))
                {
                    return;
                }

                MapZoneToSchedule.WriteDebug(
                    $"Trying to match lastManualArea: {lastManualArea[pawn]} for {pawn.NameShortColored}");
                possibleAssignmentArea = getAreaByLabel(pawn, lastManualArea[pawn]);
                MapZoneToSchedule.WriteDebug(
                    possibleAssignmentArea == null
                        ? $"Setting back unrestricted as area for {pawn.NameShortColored}"
                        : $"Setting back {possibleAssignmentArea.Label.ToLower()} as area for {pawn.NameShortColored}");
                trySetAssignment(pawn, possibleAssignmentArea);
                lastManualArea.Remove(pawn);
            }
            else
            {
                MapZoneToSchedule.WriteDebug(
                    $"Found {possibleAssignmentArea.Label.ToLower()} as possible area for {pawn.NameShortColored}");

                if (lastAutoArea.ContainsKey(pawn) &&
                    currentAssignmentLabel == lastAutoArea[pawn])
                {
                    MapZoneToSchedule.WriteDebug(
                        $"Already set {currentAssignmentLabel} automatically so assuming manual override. Ignoring {pawn.NameShortColored} until new schedule-type.");

                    MapZoneToSchedule.WriteDebug(
                        $"Setting lastManualArea to {currentAreaRestrictionLabel} for {pawn.NameShortColored}");
                    lastManualArea[pawn] = currentAreaRestrictionLabel;
                    return;
                }

                MapZoneToSchedule.WriteDebug(
                    $"Setting {possibleAssignmentArea.Label.ToLower()} as area for {pawn.NameShortColored} based on schedule");
                trySetAssignment(pawn, possibleAssignmentArea);
                lastAutoArea[pawn] = possibleAssignmentArea.Label.ToLower();
            }
        }

        private static Area getAreaByLabel(Pawn pawn, string AreaLabel)
        {
            return pawn.Map.areaManager.AllAreas.FirstOrDefault(area => area.Label.ToLower() == AreaLabel);
        }

        private static void trySetAssignment(Pawn pawn, Area newArea)
        {
            pawn.playerSettings.AreaRestriction = newArea;
        }
    }
}