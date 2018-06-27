using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class Cults_Screen_Credits : Window
    {
        private const int ColumnWidth = 800;

        private const float InitialAutoScrollDelay = 1f;

        private const float InitialAutoScrollDelayWonGame = 6f;

        private float MessageDelay = 0f;

        private const float AutoScrollDelayAfterManualScroll = 3f;

        private const float SongStartDelay = 5f;

        private List<CreditsEntry> creds;

        public bool wonGame;

        private float timeUntilAutoScroll;

        private float scrollPosition;

        private bool playedMusic;

        public float creationRealtime = -1f;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2((float)Screen.width, (float)Screen.height);
            }
        }

        protected override float Margin
        {
            get
            {
                return 0f;
            }
        }

        private float ViewWidth
        {
            get
            {
                return 800f;
            }
        }

        private float ViewHeight
        {
            get
            {
                return this.creds.Sum((CreditsEntry c) => c.DrawHeight(this.ViewWidth)) + 200f;
            }
        }

        private float MaxScrollPosition
        {
            get
            {
                return this.ViewHeight - 400f;
            }
        }

        private float AutoScrollRate
        {
            get
            {
                if (this.wonGame)
                {
                    float num = SongDefOf.EndCreditsSong.clip.length + 5f - 6f;
                    return this.MaxScrollPosition / num;
                }
                return 30f;
            }
        }

        public Cults_Screen_Credits() : this(string.Empty)
        {
        }

        public Cults_Screen_Credits(string preCreditsMessage, float DelayBooster=0f)
        {
            this.doWindowBackground = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.forcePause = true;
            this.creds = CreditsAssembler.AllCredits().ToList<CreditsEntry>();
            this.creds.Insert(0, new CreditRecord_Space(100f));
            if (!preCreditsMessage.NullOrEmpty())
            {
                this.creds.Insert(1, new CreditRecord_Space(100f));
                this.creds.Insert(2, new CreditRecord_Text(preCreditsMessage, TextAnchor.UpperLeft));
                this.creds.Insert(3, new CreditRecord_Space(50f));
            }

            //Main team
            this.creds.Insert(4, new CreditRecord_Space(100f));
            this.creds.Insert(5, new CreditRecord_Title("Rim of Madness"));
            this.creds.Insert(6, new CreditRecord_Space(50f));
            this.creds.Insert(7, new CreditRecord_Text("Team Members (In Alphabetical Order)", TextAnchor.UpperCenter));
            this.creds.Insert(8, new CreditRecord_Space(50f));
            this.creds.Insert(9, new CreditRecord_Role("CoercionRole".Translate(), "Coercion"));
            this.creds.Insert(10, new CreditRecord_Space(50f));
            this.creds.Insert(11, new CreditRecord_Role("DrynynRole".Translate(), "Drynyn"));
            this.creds.Insert(12, new CreditRecord_Space(50f));
            this.creds.Insert(13, new CreditRecord_Role("erdelfRole".Translate(), "erdelf")); // new
            this.creds.Insert(14, new CreditRecord_Space(50f));
            this.creds.Insert(15, new CreditRecord_Role("JareixRole".Translate(), "Jareix"));
            this.creds.Insert(16, new CreditRecord_Space(50f));
            this.creds.Insert(17, new CreditRecord_Role("JecrellRole".Translate(), "Jecrell"));
            this.creds.Insert(18, new CreditRecord_Space(50f));
            this.creds.Insert(19, new CreditRecord_Role("JunkyardJoeRole".Translate(), "Junkyard Joe"));
            this.creds.Insert(20, new CreditRecord_Space(50f));
            this.creds.Insert(21, new CreditRecord_Role("spoonshortageRole".Translate(), "spoonshortage")); // new
            this.creds.Insert(22, new CreditRecord_Space(50f));
            this.creds.Insert(23, new CreditRecord_Role("SticksNTricksRole".Translate(), "SticksNTricks")); // new
            this.creds.Insert(24, new CreditRecord_Space(50f));
            this.creds.Insert(25, new CreditRecord_Role("PlymouthRole".Translate(), "Plymouth")); // new
            this.creds.Insert(26, new CreditRecord_Space(50f));
            this.creds.Insert(27, new CreditRecord_Role("SeraRole".Translate(), "Sera")); // new
            this.creds.Insert(28, new CreditRecord_Space(50f));
            this.creds.Insert(29, new CreditRecord_Role("NackbladRole".Translate(), "Nackblad"));
            this.creds.Insert(30, new CreditRecord_Space(50f));

            // Patreon Supporters
            this.creds.Insert(31, new CreditRecord_Text("Patreon Supporters (In No Particular Order)", TextAnchor.UpperCenter));
            this.creds.Insert(32, new CreditRecord_Space(50f));
            this.creds.Insert(33, new CreditRecord_Role("PatreonProducer".Translate(), "XboxOneNoob")); //Michael L.
            this.creds.Insert(34, new CreditRecord_Space(50f));
            this.creds.Insert(35, new CreditRecord_Role("PatreonProducer".Translate(), "Joseph Bracken")); // slick liuid
            this.creds.Insert(36, new CreditRecord_Space(50f));
            this.creds.Insert(37, new CreditRecord_Role("PatreonProducer".Translate(), "Thom Black")); // Thom Black
            this.creds.Insert(38, new CreditRecord_Space(50f));
            this.creds.Insert(39, new CreditRecord_Role("PatreonSupporter".Translate(), "Karol Rybak"));
            this.creds.Insert(40, new CreditRecord_Space(50f));
            this.creds.Insert(41, new CreditRecord_Role("PatreonSupporter".Translate(), "Matthias Broxvall"));
            this.creds.Insert(42, new CreditRecord_Space(50f));
            this.creds.Insert(43, new CreditRecord_Role("PatreonSupporter".Translate(), "Populous25"));
            this.creds.Insert(44, new CreditRecord_Space(50f));
            this.creds.Insert(45, new CreditRecord_Role("PatreonSupporter".Translate(), "Steven James"));
            this.creds.Insert(46, new CreditRecord_Space(50f));
            this.creds.Insert(47, new CreditRecord_Role("PatreonSupporter".Translate(), "Hannah Foster"));
            this.creds.Insert(48, new CreditRecord_Space(50f));
            this.creds.Insert(49, new CreditRecord_Role("PatreonSupporter".Translate(), "Julian Koch"));
            this.creds.Insert(50, new CreditRecord_Space(50f));
            this.creds.Insert(51, new CreditRecord_Role("PatreonSupporter".Translate(), "Geth"));
            this.creds.Add(new CreditRecord_Space(100f));
            this.creds.Add(new CreditRecord_Text("ThanksForPlaying".Translate(), TextAnchor.UpperCenter));
            if (DelayBooster != 0f) MessageDelay = DelayBooster;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.creationRealtime = Time.realtimeSinceStartup;
            if (this.wonGame)
            {
                this.timeUntilAutoScroll = InitialAutoScrollDelayWonGame + MessageDelay;
            }
            else
            {
                this.timeUntilAutoScroll = InitialAutoScrollDelay + MessageDelay;
            }
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (this.timeUntilAutoScroll > 0f)
            {
                this.timeUntilAutoScroll -= Time.deltaTime;
            }
            else
            {
                this.scrollPosition += this.AutoScrollRate * Time.deltaTime;
            }
            if (this.wonGame && !this.playedMusic && Time.realtimeSinceStartup > this.creationRealtime + 5f)
            {
                Find.MusicManagerPlay.ForceStartSong(SongDefOf.EndCreditsSong, true);
                this.playedMusic = true;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
            GUI.DrawTexture(rect, BaseContent.BlackTex);
            Rect position = new Rect(rect);
            position.yMin += 30f;
            position.yMax -= 30f;
            position.xMin = rect.center.x - 400f;
            position.width = 800f;
            float viewWidth = this.ViewWidth;
            float viewHeight = this.ViewHeight;
            this.scrollPosition = Mathf.Clamp(this.scrollPosition, 0f, this.MaxScrollPosition);
            GUI.BeginGroup(position);
            Rect position2 = new Rect(0f, 0f, viewWidth, viewHeight);
            position2.y -= this.scrollPosition;
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Medium;
            float num = 0f;
            foreach (CreditsEntry current in this.creds)
            {
                float num2 = current.DrawHeight(position2.width);
                Rect rect2 = new Rect(0f, num, position2.width, num2);
                current.Draw(rect2);
                num += num2;
            }
            GUI.EndGroup();
            GUI.EndGroup();
            if (Event.current.type == EventType.ScrollWheel)
            {
                this.Scroll(Event.current.delta.y * 25f);
                Event.current.Use();
            }
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    this.Scroll(250f);
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    this.Scroll(-250f);
                    Event.current.Use();
                }
            }
        }

        private void Scroll(float offset)
        {
            this.scrollPosition += offset;
            this.timeUntilAutoScroll = 3f;
        }
    }
}
