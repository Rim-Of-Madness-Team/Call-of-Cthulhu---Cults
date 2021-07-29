using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{

	public class JobDriver_HaulToTransporter : JobDriver_HaulToContainer
	{
		public int initialCount;

		public CompTransporterByakhee Transporter
		{
			get
			{
				if (base.Container == null)
				{
					return null;
				}
				return base.Container.TryGetComp<CompTransporterByakhee>();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.initialCount, "initialCount", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			ThingCount thingCount;
			if (this.job.targetA.IsValid)
			{
				thingCount = new ThingCount(this.job.targetA.Thing, this.job.targetA.Thing.stackCount);
			}
			else
			{
				thingCount = LoadTransportersJobUtility.FindThingToLoad(this.pawn, base.Container.TryGetComp<CompTransporterByakhee>());
			}
			this.job.targetA = thingCount.Thing;
			this.job.count = thingCount.Count;
			this.initialCount = thingCount.Count;
			this.pawn.Reserve(thingCount.Thing, this.job, 1, -1, null, true);
		}
	}
}
