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

        public Cults_Screen_Credits() : this(preCreditsMessage: string.Empty)
        {
        }

        public Cults_Screen_Credits(string preCreditsMessage, float DelayBooster = 0f)
        {
            doWindowBackground = false;
            doCloseButton = false;
            doCloseX = false;
            forcePause = true;
            creds = CreditsAssembler.AllCredits().ToList();
            creds.Insert(index: 0, item: new CreditRecord_Space(height: 100f));
            if (!preCreditsMessage.NullOrEmpty())
            {
                creds.Insert(index: 1, item: new CreditRecord_Space(height: 100f));
                creds.Insert(index: 2, item: new CreditRecord_Text(text: preCreditsMessage));
                creds.Insert(index: 3, item: new CreditRecord_Space(height: 50f));
            }

            //Main team
            creds.Insert(index: 4, item: new CreditRecord_Space(height: 100f));
            creds.Insert(index: 5, item: new CreditRecord_Title(title: "Rim of Madness"));
            creds.Insert(index: 6, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 7, item: new CreditRecord_Text(text: "Team Members (In Alphabetical Order)", anchor: TextAnchor.UpperCenter));
            creds.Insert(index: 8, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 9, item: new CreditRecord_Role(roleKey: "CoercionRole".Translate(), creditee: "Coercion"));
            creds.Insert(index: 10, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 11, item: new CreditRecord_Role(roleKey: "DrynynRole".Translate(), creditee: "Drynyn"));
            creds.Insert(index: 12, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 13, item: new CreditRecord_Role(roleKey: "erdelfRole".Translate(), creditee: "erdelf")); // new
            creds.Insert(index: 14, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 15, item: new CreditRecord_Role(roleKey: "JareixRole".Translate(), creditee: "Jareix"));
            creds.Insert(index: 16, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 17, item: new CreditRecord_Role(roleKey: "JecrellRole".Translate(), creditee: "Jecrell"));
            creds.Insert(index: 18, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 19, item: new CreditRecord_Role(roleKey: "JunkyardJoeRole".Translate(), creditee: "Junkyard Joe"));
            creds.Insert(index: 20, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 21, item: new CreditRecord_Role(roleKey: "spoonshortageRole".Translate(), creditee: "spoonshortage")); // new
            creds.Insert(index: 22, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 23, item: new CreditRecord_Role(roleKey: "SticksNTricksRole".Translate(), creditee: "SticksNTricks")); // new
            creds.Insert(index: 24, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 25, item: new CreditRecord_Role(roleKey: "PlymouthRole".Translate(), creditee: "Plymouth")); // new
            creds.Insert(index: 26, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 27, item: new CreditRecord_Role(roleKey: "SeraRole".Translate(), creditee: "Sera")); // new
            creds.Insert(index: 28, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 29, item: new CreditRecord_Role(roleKey: "NackbladRole".Translate(), creditee: "Nackblad"));
            creds.Insert(index: 30, item: new CreditRecord_Space(height: 50f));

            // Patreon Supporters
            creds.Insert(index: 31,
                item: new CreditRecord_Text(text: "Patreon Supporters (In No Particular Order)", anchor: TextAnchor.UpperCenter));
            creds.Insert(index: 32, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 33, item: new CreditRecord_Role(roleKey: "PatreonProducer".Translate(), creditee: "XboxOneNoob")); //Michael L.
            creds.Insert(index: 34, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 35, item: new CreditRecord_Role(roleKey: "PatreonProducer".Translate(), creditee: "Joseph Bracken")); // slick liuid
            creds.Insert(index: 36, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 37, item: new CreditRecord_Role(roleKey: "PatreonProducer".Translate(), creditee: "Thom Black")); // Thom Black
            creds.Insert(index: 38, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 39, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Karol Rybak"));
            creds.Insert(index: 40, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 41, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Matthias Broxvall"));
            creds.Insert(index: 42, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 43, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Populous25"));
            creds.Insert(index: 44, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 45, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Steven James"));
            creds.Insert(index: 46, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 47, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Hannah Foster"));
            creds.Insert(index: 48, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 49, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Julian Koch"));
            creds.Insert(index: 50, item: new CreditRecord_Space(height: 50f));
            creds.Insert(index: 51, item: new CreditRecord_Role(roleKey: "PatreonSupporter".Translate(), creditee: "Geth"));
            creds.Add(item: new CreditRecord_Space(height: 100f));
            creds.Add(item: new CreditRecord_Text(text: "ThanksForPlaying".Translate(), anchor: TextAnchor.UpperCenter));
            if (DelayBooster != 0f)
            {
                MessageDelay = DelayBooster;
            }
        }

        public override Vector2 InitialSize => new Vector2(x: Screen.width, y: Screen.height);

        protected override float Margin => 0f;

        private float ViewWidth => 800f;

        private float ViewHeight => creds.Sum(selector: c => c.DrawHeight(width: ViewWidth)) + 200f;

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

            Find.MusicManagerPlay.ForceStartSong(song: SongDefOf.EndCreditsSong, ignorePrefsVolume: true);
            playedMusic = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(x: 0f, y: 0f, width: Screen.width, height: Screen.height);
            GUI.DrawTexture(position: rect, image: BaseContent.BlackTex);
            var position = new Rect(source: rect);
            position.yMin += 30f;
            position.yMax -= 30f;
            position.xMin = rect.center.x - 400f;
            position.width = 800f;
            var viewWidth = ViewWidth;
            var viewHeight = ViewHeight;
            scrollPosition = Mathf.Clamp(value: scrollPosition, min: 0f, max: MaxScrollPosition);
            GUI.BeginGroup(position: position);
            var position2 = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);
            position2.y -= scrollPosition;
            GUI.BeginGroup(position: position2);
            Text.Font = GameFont.Medium;
            var num = 0f;
            foreach (var current in creds)
            {
                var num2 = current.DrawHeight(width: position2.width);
                var rect2 = new Rect(x: 0f, y: num, width: position2.width, height: num2);
                current.Draw(rect: rect2);
                num += num2;
            }

            GUI.EndGroup();
            GUI.EndGroup();
            if (Event.current.type == EventType.ScrollWheel)
            {
                Scroll(offset: Event.current.delta.y * 25f);
                Event.current.Use();
            }

            if (Event.current.type != EventType.KeyDown)
            {
                return;
            }

            if (Event.current.keyCode == KeyCode.DownArrow)
            {
                Scroll(offset: 250f);
                Event.current.Use();
            }

            if (Event.current.keyCode != KeyCode.UpArrow)
            {
                return;
            }

            Scroll(offset: -250f);
            Event.current.Use();
        }

        private void Scroll(float offset)
        {
            scrollPosition += offset;
            timeUntilAutoScroll = 3f;
        }
    }
}