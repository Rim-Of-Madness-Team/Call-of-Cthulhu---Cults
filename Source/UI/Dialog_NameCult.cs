using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.IO;
using System.Text.RegularExpressions;

namespace CultOfCthulhu
{
    public class Dialog_NameCult : Window
    {
        private Pawn suggestingPawn;

        private Map map;

        private string curName = NameGenerator.GenerateName(RulePackDef.Named("NamerCults"));

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 200f);
            }
        }

        public Dialog_NameCult(Map map)
        {
            if (map != null)
            {
                if (map.mapPawns.FreeColonistsCount != 0)
                {

                    if (map.mapPawns.FreeColonistsSpawnedCount != 0)
                    {
                        this.suggestingPawn = map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
                    }
                    else
                    {
                        this.suggestingPawn = map.mapPawns.FreeColonists.RandomElement<Pawn>();
                    }
                }
                else
                {
                    this.suggestingPawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.RandomElement<Pawn>();

                }
            }
            this.forcePause = true;
            //this.closeOnEscapeKey = false;
            this.absorbInputAroundWindow = true;
            this.map = map;
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            if (suggestingPawn != null)
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameCultMessage".Translate(new object[]
                {
                this.suggestingPawn.Name.ToStringShort
                }));
            }
            else
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameCultMessageNullHandler".Translate());
            }
            this.curName = Widgets.TextField(new Rect(0f, rect.height - 35f, rect.width / 2f - 20f, 35f), this.curName);
            if (Widgets.ButtonText(new Rect(rect.width / 2f + 20f, rect.height - 35f, rect.width / 2f - 20f, 35f), "OK".Translate(), true, false, true) || flag)
            {
                if (this.IsValidCultName(this.curName))
                {
                    if (map != null)
                    {
                        CultTracker.Get.PlayerCult.name = this.curName;
                        //Faction.OfPlayer.Name = this.curName;
                        Find.WindowStack.TryRemove(this, true);
                        Messages.Message("CultGainsName".Translate(new object[]
                        {
                        this.curName
                        }), MessageTypeDefOf.PositiveEvent);
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
        }

        private bool IsValidCultName(string s)
        {
            return s.Length != 0 && CultUtility.CheckValidCultName(s);
        }


    }
}
