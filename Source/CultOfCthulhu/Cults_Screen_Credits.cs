using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Cults_Screen_Credits : Window
    {
        private const int ColumnWidth = 800;

        private const float InitialAutoScrollDelay = 1f;

        private const float InitialAutoScrollDelayWonGame = 6f;

        private const float AutoScrollDelayAfterManualScroll = 3f;

        private const float SongStartDelay = 5f;

        private readonly List<CreditsEntry> creds;

        private readonly float MessageDelay;

        private float creationRealtime = -1f;

        private bool playedMusic;

        private float scrollPosition;

        private float timeUntilAutoScroll;

        public bool wonGame;

        public Cults_Screen_Credits() : this(string.Empty)
        {
        }

        public Cults_Screen_Credits(string preCreditsMessage, float DelayBooster = 0f)
        {
            doWindowBackground = false;
            doCloseButton = false;
            doCloseX = false;
            forcePause = true;
            creds = CreditsAssembler.AllCredits().ToList();
            creds.Insert(0, new CreditRecord_Space(100f));
            if (!preCreditsMessage.NullOrEmpty())
            {
                creds.Insert(1, new CreditRecord_Space(100f));
                creds.Insert(2, new CreditRecord_Text(preCreditsMessage));
                creds.Insert(3, new CreditRecord_Space(50f));
            }

            //Main team
            creds.Insert(4, new CreditRecord_Space(100f));
            creds.Insert(5, new CreditRecord_Title("Rim of Madness"));
            creds.Insert(6, new CreditRecord_Space(50f));
            creds.Insert(7, new CreditRecord_Text("Team Members (In Alphabetical Order)", TextAnchor.UpperCenter));
            creds.Insert(8, new CreditRecord_Space(50f));
            creds.Insert(9, new CreditRecord_Role("CoercionRole".Translate(), "Coercion"));
            creds.Insert(10, new CreditRecord_Space(50f));
            creds.Insert(11, new CreditRecord_Role("DrynynRole".Translate(), "Drynyn"));
            creds.Insert(12, new CreditRecord_Space(50f));
            creds.Insert(13, new CreditRecord_Role("erdelfRole".Translate(), "erdelf")); // new
            creds.Insert(14, new CreditRecord_Space(50f));
            creds.Insert(15, new CreditRecord_Role("JareixRole".Translate(), "Jareix"));
            creds.Insert(16, new CreditRecord_Space(50f));
            creds.Insert(17, new CreditRecord_Role("JecrellRole".Translate(), "Jecrell"));
            creds.Insert(18, new CreditRecord_Space(50f));
            creds.Insert(19, new CreditRecord_Role("JunkyardJoeRole".Translate(), "Junkyard Joe"));
            creds.Insert(20, new CreditRecord_Space(50f));
            creds.Insert(21, new CreditRecord_Role("spoonshortageRole".Translate(), "spoonshortage")); // new
            creds.Insert(22, new CreditRecord_Space(50f));
            creds.Insert(23, new CreditRecord_Role("SticksNTricksRole".Translate(), "SticksNTricks")); // new
            creds.Insert(24, new CreditRecord_Space(50f));
            creds.Insert(25, new CreditRecord_Role("PlymouthRole".Translate(), "Plymouth")); // new
            creds.Insert(26, new CreditRecord_Space(50f));
            creds.Insert(27, new CreditRecord_Role("SeraRole".Translate(), "Sera")); // new
            creds.Insert(28, new CreditRecord_Space(50f));
            creds.Insert(29, new CreditRecord_Role("NackbladRole".Translate(), "Nackblad"));
            creds.Insert(30, new CreditRecord_Space(50f));

            // Patreon Supporters
            creds.Insert(31,
                new CreditRecord_Text("Patreon Supporters (In No Particular Order)", TextAnchor.UpperCenter));
            creds.Insert(32, new CreditRecord_Space(50f));
            creds.Insert(33, new CreditRecord_Role("PatreonProducer".Translate(), "XboxOneNoob")); //Michael L.
            creds.Insert(34, new CreditRecord_Space(50f));
            creds.Insert(35, new CreditRecord_Role("PatreonProducer".Translate(), "Joseph Bracken")); // slick liuid
            creds.Insert(36, new CreditRecord_Space(50f));
            creds.Insert(37, new CreditRecord_Role("PatreonProducer".Translate(), "Thom Black")); // Thom Black
            creds.Insert(38, new CreditRecord_Space(50f));
            creds.Insert(39, new CreditRecord_Role("PatreonSupporter".Translate(), "Karol Rybak"));
            creds.Insert(40, new CreditRecord_Space(50f));
            creds.Insert(41, new CreditRecord_Role("PatreonSupporter".Translate(), "Matthias Broxvall"));
            creds.Insert(42, new CreditRecord_Space(50f));
            creds.Insert(43, new CreditRecord_Role("PatreonSupporter".Translate(), "Populous25"));
            creds.Insert(44, new CreditRecord_Space(50f));
            creds.Insert(45, new CreditRecord_Role("PatreonSupporter".Translate(), "Steven James"));
            creds.Insert(46, new CreditRecord_Space(50f));
            creds.Insert(47, new CreditRecord_Role("PatreonSupporter".Translate(), "Hannah Foster"));
            creds.Insert(48, new CreditRecord_Space(50f));
            creds.Insert(49, new CreditRecord_Role("PatreonSupporter".Translate(), "Julian Koch"));
            creds.Insert(50, new CreditRecord_Space(50f));
            creds.Insert(51, new CreditRecord_Role("PatreonSupporter".Translate(), "Geth"));
            creds.Add(new CreditRecord_Space(100f));
            creds.Add(new CreditRecord_Text("ThanksForPlaying".Translate(), TextAnchor.UpperCenter));
            if (DelayBooster != 0f)
            {
                MessageDelay = DelayBooster;
            }
        }

        public override Vector2 InitialSize => new Vector2(Screen.width, Screen.height);

        protected override float Margin => 0f;

        private float ViewWidth => 800f;

        private float ViewHeight => creds.Sum(c => c.DrawHeight(ViewWidth)) + 200f;

        private float MaxScrollPosition => ViewHeight - 400f;

        private float AutoScrollRate
        {
            get
            {
                if (!wonGame)
                {
                    return 30f;
                }

                var num = SongDefOf.EndCreditsSong.clip.length + 5f - 6f;
                return MaxScrollPosition / num;
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            creationRealtime = Time.realtimeSinceStartup;
            if (wonGame)
            {
                timeUntilAutoScroll = InitialAutoScrollDelayWonGame + MessageDelay;
            }
            else
            {
                timeUntilAutoScroll = InitialAutoScrollDelay + MessageDelay;
            }
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (timeUntilAutoScroll > 0f)
            {
                timeUntilAutoScroll -= Time.deltaTime;
            }
            else
            {
                scrollPosition += AutoScrollRate * Time.deltaTime;
            }

            if (!wonGame || playedMusic || !(Time.realtimeSinceStartup > creationRealtime + 5f))
            {
                return;
            }

            Find.MusicManagerPlay.ForceStartSong(SongDefOf.EndCreditsSong, true);
            playedMusic = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.DrawTexture(rect, BaseContent.BlackTex);
            var position = new Rect(rect);
            position.yMin += 30f;
            position.yMax -= 30f;
            position.xMin = rect.center.x - 400f;
            position.width = 800f;
            var viewWidth = ViewWidth;
            var viewHeight = ViewHeight;
            scrollPosition = Mathf.Clamp(scrollPosition, 0f, MaxScrollPosition);
            GUI.BeginGroup(position);
            var position2 = new Rect(0f, 0f, viewWidth, viewHeight);
            position2.y -= scrollPosition;
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Medium;
            var num = 0f;
            foreach (var current in creds)
            {
                var num2 = current.DrawHeight(position2.width);
                var rect2 = new Rect(0f, num, position2.width, num2);
                current.Draw(rect2);
                num += num2;
            }

            GUI.EndGroup();
            GUI.EndGroup();
            if (Event.current.type == EventType.ScrollWheel)
            {
                Scroll(Event.current.delta.y * 25f);
                Event.current.Use();
            }

            if (Event.current.type != EventType.KeyDown)
            {
                return;
            }

            if (Event.current.keyCode == KeyCode.DownArrow)
            {
                Scroll(250f);
                Event.current.Use();
            }

            if (Event.current.keyCode != KeyCode.UpArrow)
            {
                return;
            }

            Scroll(-250f);
            Event.current.Use();
        }

        private void Scroll(float offset)
        {
            scrollPosition += offset;
            timeUntilAutoScroll = 3f;
        }
    }
}