using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Dialog_NameCult : Window
    {
        private readonly Map map;
        private readonly Pawn suggestingPawn;

        private string curName = NameGenerator.GenerateName(RulePackDef.Named("NamerCults"));

        public Dialog_NameCult(Map map)
        {
            if (map != null)
            {
                if (map.mapPawns.FreeColonistsCount != 0)
                {
                    suggestingPawn = map.mapPawns.FreeColonistsSpawnedCount != 0
                        ? map.mapPawns.FreeColonistsSpawned.RandomElement()
                        : map.mapPawns.FreeColonists.RandomElement();
                }
                else
                {
                    suggestingPawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
                        .RandomElement();
                }
            }

            forcePause = true;
            //this.closeOnEscapeKey = false;
            absorbInputAroundWindow = true;
            this.map = map;
        }

        public override Vector2 InitialSize => new Vector2(500f, 200f);

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;
            var flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }

            if (suggestingPawn != null)
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameCultMessage".Translate(
                    suggestingPawn.Name.ToStringShort
                ));
            }
            else
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameCultMessageNullHandler".Translate());
            }

            curName = Widgets.TextField(new Rect(0f, rect.height - 35f, (rect.width / 2f) - 20f, 35f), curName);
            if (!Widgets.ButtonText(new Rect((rect.width / 2f) + 20f, rect.height - 35f, (rect.width / 2f) - 20f, 35f),
                "OK".Translate(), true, false) && !flag)
            {
                return;
            }

            if (IsValidCultName(curName))
            {
                if (map != null)
                {
                    CultTracker.Get.PlayerCult.name = curName;
                    //Faction.OfPlayer.Name = this.curName;
                    Find.WindowStack.TryRemove(this);
                    Messages.Message("CultGainsName".Translate(
                        curName
                    ), MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                Messages.Message("ColonyNameIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
            }

            Event.current.Use();
        }

        private bool IsValidCultName(string s)
        {
            return s.Length != 0 && CultUtility.CheckValidCultName(s);
        }
    }
}