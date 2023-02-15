using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;
using VNPE;
using DubsBadHygiene;

namespace NVPE_ThirstPatch
{
	[StaticConstructorOnStartup]
	static public class HarmonyPatches
	{
		public static Harmony harmonyInstance;


		static HarmonyPatches()
		{
			harmonyInstance = new Harmony("rimworld.rwmods.NVPE_ThirstPatch");
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}
	}


	[HarmonyPatch(typeof(Building_Dripper))]
	[HarmonyPatch("TickRare")]
	internal static class Building_DripperThirst
	{
		public static void Postfix(ref Building_Dripper __instance)
		{
			if (DubsBadHygiene.Settings.ThirstNeed == true)
			{
				// Find if anyone is thirsty
				List<Thing> LinkedStuff = __instance.facilityComp.LinkedBuildings;
				foreach(Thing LinkedThing in LinkedStuff)
				{
					// Find a bed
					if (LinkedThing is Building_Bed FoundBed)
					{
						// Find people in the bed
						foreach(Pawn BedPerson in FoundBed.CurOccupants)
						{
							Need_Thirst BedThirst = BedPerson.needs.TryGetNeed<Need_Thirst>();

							if (BedThirst != null)
							{
								// Are they thirsty?
								if (BedThirst.CurLevelPercentage < 0.5)
								{
									// Is the dripper piped?
									CompPipe PipeProperties = __instance.TryGetComp<CompPipe>();
									if (PipeProperties != null)
									{

										// Can we draw the water?
										ContaminationLevel WaterTaint = ContaminationLevel.Treated;
										if (PipeProperties.pipeNet.PullWater((float)(2.7 * BedThirst.CurLevelPercentage), out WaterTaint) == true)	// People drink 2.7L of water a day
										{
											// Sippy sippy
											//BedPerson.needs.TryGetNeed<Need_Thirst>().Drink();
											BedThirst.Drink(100.0f);
											
											SanitationUtil.ContaminationCheckDrinking(BedPerson, WaterTaint);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

}
