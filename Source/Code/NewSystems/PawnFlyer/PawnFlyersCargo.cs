using RimWorld;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyersCargo : ActiveDropPod
{
    public Pawn pawnFlyer;
    
    public override void ExposeData()
    {
        Scribe_References.Look(ref pawnFlyer, "pawnFlyer");
        base.ExposeData();
    }
}