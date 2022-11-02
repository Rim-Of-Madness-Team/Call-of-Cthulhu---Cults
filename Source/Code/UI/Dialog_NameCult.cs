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

        private string curName = NameGenerator.GenerateName(rootPack: RulePackDef.Named(defName: "NamerCults"));

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

        public override Vector2 InitialSize => new Vector2(x: 500f, y: 200f);

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
                Widgets.Label(rect: new Rect(x: 0f, y: 0f, width: rect.width, height: rect.height), label: "NameCultMessage".Translate(
                    arg1: suggestingPawn.Name.ToStringShort
                ));
            }
            else
            {
                Widgets.Label(rect: new Rect(x: 0f, y: 0f, width: rect.width, height: rect.height), label: "NameCultMessageNullHandler".Translate());
            }

            curName = Widgets.TextField(rect: new Rect(x: 0f, y: rect.height - 35f, width: (rect.width / 2f) - 20f, height: 35f), text: curName);
            if (!Widgets.ButtonText(rect: new Rect(x: (rect.width / 2f) + 20f, y: rect.height - 35f, width: (rect.width / 2f) - 20f, height: 35f),
                label: "OK".Translate(), drawBackground: true, doMouseoverSound: false) && !flag)
            {
                return;
            }

            if (IsValidCultName(s: curName))
            {
                if (map != null)
                {
                    CultTracker.Get.PlayerCult.name = curName;
                    //Faction.OfPlayer.Name = this.curName;
                    Find.WindowStack.TryRemove(window: this);
                    Messages.Message(text: "CultGainsName".Translate(
                        arg1: curName
                    ), def: MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                Messages.Message(text: "ColonyNameIsInvalid".Translate(), def: MessageTypeDefOf.RejectInput);
            }

            Event.current.Use();
        }

        private bool IsValidCultName(string s)
        {
            return s.Length != 0 && CultUtility.CheckValidCultName(str: s);
        }
    }
}