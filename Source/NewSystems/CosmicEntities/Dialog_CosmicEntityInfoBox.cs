using System;
using System.Net.Mime;
using CultOfCthulhu;
using UnityEngine;
using Verse;

namespace CallOfCthulhu
{
	public class Dialog_CosmicEntityInfoBox : Window
	{
		private const float InitialWidth = 640f;
		private const float InitialHeight = 800f;
		public string text;

		public string title;

		public string buttonAText;

		public Texture2D image;

		public Action buttonAAction;

		public bool buttonADestructive;

		public string buttonBText;

		public Action buttonBAction;

		public string buttonCText;

		public Action buttonCAction;

		public bool buttonCClose = true;

		public float interactionDelay = 0f;

		public Action acceptAction;

		public Action cancelAction;

		private Vector2 scrollPosition = Vector2.zero;

		private float creationRealTime = -1f;

		public Dialog_CosmicEntityInfoBox(CosmicEntity entity)
		{
			this.text = entity.Info();
			this.title = entity.LabelCap;
			if (buttonAText.NullOrEmpty())
			{
				this.buttonAText = "OK".Translate();
			}
			if (entity.Def.Portrait != "")
				this.image = ContentFinder<Texture2D>.Get(entity.Def.Portrait);
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.creationRealTime = RealTime.LastRealTime;
			this.onlyOneOfTypeAllowed = false;
			this.closeOnAccept = true;
			this.closeOnCancel = true;
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(InitialWidth, InitialHeight);
			}
		}

		private float get_TimeUntilInteractive()
		{
			return this.interactionDelay - (Time.realtimeSinceStartup - this.creationRealTime);
		}

		private bool get_InteractionDelayExpired()
		{
			return this.get_TimeUntilInteractive() <= 0f;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = inRect.y;
			if (!this.title.NullOrEmpty())
			{
				Text.Font = (GameFont)2;
				var nameSize = Text.CalcSize(this.title);
				var startingX = (inRect.width/2) - (nameSize.x/2);
				Widgets.Label(new Rect(startingX, num, inRect.width - startingX, 42f), this.title);
				num += 42f;
			}
			Text.Font = GameFont.Small;
			if (this.image != null)
			{
				var startingX = (inRect.width/2) - (image.width * 0.5f);
				Widgets.ButtonImage(new Rect(startingX, num, inRect.width - startingX, image.height), image, Color.white, Color.white);
				num += image.height;
				num += 42f;
			}
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 35f - 5f - num);
			float width = outRect.width - 16f;
			Rect viewRect = new Rect(0f, num, width, Text.CalcHeight(this.text, width));
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			Widgets.Label(new Rect(0f, num, viewRect.width, viewRect.height), this.text);
			Widgets.EndScrollView();
			int num2 = (!this.buttonCText.NullOrEmpty()) ? 3 : 2;
			float num3 = inRect.width / (float)num2;
			float width2 = num3 - 20f;
			if (this.buttonADestructive)
			{
				GUI.color = new Color(1f, 0.3f, 0.35f);
			}
			string label = (!this.get_InteractionDelayExpired()) ? (this.buttonAText + "(" + Mathf.Ceil(this.get_TimeUntilInteractive()).ToString("F0") + ")") : this.buttonAText;
			if (Widgets.ButtonText(new Rect(num3 * (float)(num2 - 1) + 10f, inRect.height - 35f, width2, 35f), label, true, false, true))
			{
				if (this.get_InteractionDelayExpired())
				{
					this.Close(true);
				}
			}
			GUI.color = Color.white;
		}

		public override void OnCancelKeyPressed()
		{
			if (this.cancelAction != null)
			{
				this.cancelAction();
				this.Close(true);
			}
			else
			{
				base.OnCancelKeyPressed();
			}
		}

		public override void OnAcceptKeyPressed()
		{
			if (this.acceptAction != null)
			{
				this.acceptAction();
				this.Close(true);
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
