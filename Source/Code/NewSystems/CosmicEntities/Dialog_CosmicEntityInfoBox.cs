using System;
using CultOfCthulhu;
using UnityEngine;
using Verse;

namespace CallOfCthulhu
{
    public class Dialog_CosmicEntityInfoBox : Window
    {
        private const float InitialWidth = 640f;
        private const float InitialHeight = 800f;

        private readonly float creationRealTime;

        public Action acceptAction;

        public Action buttonAAction;

        public bool buttonADestructive;

        public string buttonAText;

        public Action buttonBAction;

        public string buttonBText;

        public Action buttonCAction;

        public bool buttonCClose = true;

        public string buttonCText;

        public Action cancelAction;

        public Texture2D image;

        public float interactionDelay = 0f;

        private Vector2 scrollPosition = Vector2.zero;
        public string text;

        public string title;

        public Dialog_CosmicEntityInfoBox(CosmicEntity entity)
        {
            text = entity.Info();
            title = entity.LabelCap;
            if (buttonAText.NullOrEmpty())
            {
                buttonAText = "OK".Translate();
            }

            if (entity.Def.portrait != "")
            {
                image = ContentFinder<Texture2D>.Get(itemPath: entity.Def.portrait);
            }

            forcePause = true;
            absorbInputAroundWindow = true;
            creationRealTime = RealTime.LastRealTime;
            onlyOneOfTypeAllowed = false;
            closeOnAccept = true;
            closeOnCancel = true;
        }

        public override Vector2 InitialSize => new Vector2(x: InitialWidth, y: InitialHeight);

        private float get_TimeUntilInteractive()
        {
            return interactionDelay - (Time.realtimeSinceStartup - creationRealTime);
        }

        private bool get_InteractionDelayExpired()
        {
            return get_TimeUntilInteractive() <= 0f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var num = inRect.y;
            if (!title.NullOrEmpty())
            {
                Text.Font = (GameFont) 2;
                var nameSize = Text.CalcSize(text: title);
                var startingX = (inRect.width / 2) - (nameSize.x / 2);
                Widgets.Label(rect: new Rect(x: startingX, y: num, width: inRect.width - startingX, height: 42f), label: title);
                num += 42f;
            }

            Text.Font = GameFont.Small;
            if (image != null)
            {
                var startingX = (inRect.width / 2) - (image.width * 0.5f);
                Widgets.ButtonImage(butRect: new Rect(x: startingX, y: num, width: inRect.width - startingX, height: image.height), tex: image,
                    baseColor: Color.white, mouseoverColor: Color.white);
                num += image.height;
                num += 42f;
            }

            Text.Font = GameFont.Small;
            var outRect = new Rect(x: inRect.x, y: num, width: inRect.width, height: inRect.height - 35f - 5f - num);
            var width = outRect.width - 16f;
            var viewRect = new Rect(x: 0f, y: num, width: width, height: Text.CalcHeight(text: text, width: width));
            Widgets.BeginScrollView(outRect: outRect, scrollPosition: ref scrollPosition, viewRect: viewRect);
            Widgets.Label(rect: new Rect(x: 0f, y: num, width: viewRect.width, height: viewRect.height), label: text);
            Widgets.EndScrollView();
            var num2 = !buttonCText.NullOrEmpty() ? 3 : 2;
            var num3 = inRect.width / num2;
            var width2 = num3 - 20f;
            if (buttonADestructive)
            {
                GUI.color = new Color(r: 1f, g: 0.3f, b: 0.35f);
            }

            var label = !get_InteractionDelayExpired()
                ? buttonAText + "(" + Mathf.Ceil(f: get_TimeUntilInteractive()).ToString(format: "F0") + ")"
                : buttonAText;
            if (Widgets.ButtonText(rect: new Rect(x: (num3 * (num2 - 1)) + 10f, y: inRect.height - 35f, width: width2, height: 35f), label: label, drawBackground: true,
                doMouseoverSound: false))
            {
                if (get_InteractionDelayExpired())
                {
                    Close();
                }
            }

            GUI.color = Color.white;
        }

        public override void OnCancelKeyPressed()
        {
            if (cancelAction != null)
            {
                cancelAction();
                Close();
            }
            else
            {
                base.OnCancelKeyPressed();
            }
        }

        public override void OnAcceptKeyPressed()
        {
            if (acceptAction != null)
            {
                acceptAction();
                Close();
            }
            else
            {
                base.OnAcceptKeyPressed();
            }
        }

        private static void CreateConfirmation()
        {
        }
    }
}