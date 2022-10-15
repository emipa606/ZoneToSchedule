using System.Reflection;
using HarmonyLib;
using Verse;

namespace MapZoneToSchedule;

[StaticConstructorOnStartup]
public class MapZoneToSchedule
{
    static MapZoneToSchedule()
    {
        var harmony = new Harmony("Mlie.MapZoneToSchedule");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public static void WriteDebug(string message)
    {
        // Log.Message($"[MapZoneToSchedule]: {message}");
    }
}