using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using JetBrains.Annotations;
using NFive.SDK.Client.Commands;
using NFive.SDK.Client.Communications;
using NFive.SDK.Client.Events;
using NFive.SDK.Client.Interface;
using NFive.SDK.Client.Services;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Models.Player;
using IgiCore.Police.Client.Overlays;
using IgiCore.Police.Shared;
using NFive.Notifications.Client;
using NFive.Notifications.Shared;
using NFive.SDK.Client.Extensions;
using NFive.SDK.Client.Input;
using NFive.SDK.Core.Extensions;
using NFive.SDK.Core.Input;

namespace IgiCore.Police.Client
{
	[PublicAPI]
	public class PoliceService : Service
	{
		private readonly Hotkey arrestHotkey = new Hotkey(InputControl.Arrest);
        private readonly Hotkey escortHotkey = new Hotkey(InputControl.Context);
        private bool inAnim;
        private NotificationManager notifications;

        public PoliceService(ILogger logger, ITickManager ticks, ICommunicationManager comms, ICommandManager commands, IOverlayManager overlay, User user) : base(logger, ticks, comms, commands, overlay, user) { }

		public override Task Started()
		{
			this.notifications = new NotificationManager(this.Comms);

            this.Logger.Debug($"arrestHotkey: {this.arrestHotkey.UserKeyboardKeyDisplayName}");
            this.Logger.Debug($"escortHotkey: {this.escortHotkey.UserKeyboardKeyDisplayName}");

			this.Ticks.On(ArrestKeyHandler);
			this.Ticks.On(EscortKeyHandler);


			return base.Started();
		}

		private void EscortKeyHandler()
		{
			if (!this.escortHotkey.IsJustPressed()) return;


		}

		private async Task ArrestKeyHandler()
		{
			if (!this.arrestHotkey.IsJustPressed()) return;
			var playerPed = Game.PlayerPed;
			if (this.inAnim || playerPed.IsInVehicle()) return;

			var playerPos = playerPed.Position;

			var ped = World.GetAllPeds()
				.Select(p => new { ped = p, distance = new Vector3(p.Position.X, p.Position.Y, p.Position.Z).DistanceToSquared(playerPos) })
				.Where(x => x.distance < 4.0f) // Close by
				.OrderBy(x => x.distance)
				.Select(x => x.ped)
				.Where(p => !p.IsInVehicle()) // Not in a vehicle
				.Where(p => !p.IsSwimming) // Not swimming
				.Where(p => API.GetEntitySpeed(p.Handle) < 2.0f) // Speed is less than 2
				.FirstOrDefault(p => Vector3.Dot(playerPed.ForwardVector, Vector3.Normalize(p.Position - playerPos)).IsBetween(0f, 1.0f)); // In front of

			if (ped == null)
			{
                this.notifications.Show(new Notification
                {
                    Text = "Nobody nearby to handcuff.",
                    Type = "warning"
				});
				return;
			}

			this.notifications.Show(new Notification
			{
				Text = $"Handcuffing {ped.Handle}.",
				Type = "success"
			});

			const string animDict = "mp_arrest_paired";
			while (!API.HasAnimDictLoaded(animDict))
			{
                API.RequestAnimDict(animDict);
                await this.Delay(16);
			}

			await playerPed.Task.PlayAnimation(animDict, "cop_p2_back_right", 8.0f, 30.0f, 3500, (AnimationFlags)48, 0);

			//loadAnimDict("mp_arrest_paired")
			//TaskPlayAnim(GetPlayerPed(-1), "mp_arrest_paired", "crook_p2_back_right", 8.0, -8, -1, 32, 0, 0, 0, 0)

			//.Where(a => Vector3.Dot(a.prop.ForwardVector, Vector3.Normalize(a.prop.Position - Game.Player.Character.Position)).IsBetween(0f, 1.0f)) // In front of

		}
	}
}
