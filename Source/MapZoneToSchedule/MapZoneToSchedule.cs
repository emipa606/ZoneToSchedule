using System.Reflection;
using HarmonyLib;
using Verse;

namespace MapZoneToSchedule;

[StaticConstructorOnStartup]
public class MapZoneToSchedule
{
    static MapZoneToSchedule()
    {
        new Harmony("Mlie.MapZoneToSchedule").PatchAll(Assembly.GetExecutingAssembly());
    }
}