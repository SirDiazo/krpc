using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using KRPC.Continuations;
using KRPC.Service.Attributes;
using KRPC.SpaceCenter.ExtensionMethods;
using KRPC.Utils;
using KSP.UI.Screens;
using System.Reflection;

namespace KRPC.SpaceCenter.Services
{
    /// <summary>
    /// Used to manipulate the controls of a vessel. This includes adjusting the
    /// throttle, enabling/disabling systems such as SAS and RCS, or altering the
    /// direction in which the vessel is pointing.
    /// Obtained by calling <see cref="Vessel.Control"/>.
    /// </summary>
    /// <remarks>
    /// Control inputs (such as pitch, yaw and roll) are zeroed when all clients
    /// that have set one or more of these inputs are no longer connected.
    /// </remarks>
    [KRPCClass (Service = "SpaceCenter")]
    public class Control : Equatable<Control>
    {
        readonly Guid vesselId;

        internal Control (global::Vessel vessel)
        {
            vesselId = vessel.id;
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        public override bool Equals (Control other)
        {
            return !ReferenceEquals (other, null) && vesselId == other.vesselId;
        }

        /// <summary>
        /// Hash code for the object.
        /// </summary>
        public override int GetHashCode ()
        {
            return vesselId.GetHashCode ();
        }

        /// <summary>
        /// The KSP vessel.
        /// </summary>
        public global::Vessel InternalVessel {
            get { return FlightGlobalsExtensions.GetVesselById (vesselId); }
        }

        /// <summary>
        /// The state of SAS.
        /// </summary>
        /// <remarks>Equivalent to <see cref="AutoPilot.SAS"/></remarks>
        [KRPCProperty]
        public bool SAS {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.SAS)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.SAS, value); }
        }

        /// <summary>
        /// The current <see cref="SASMode"/>.
        /// These modes are equivalent to the mode buttons to
        /// the left of the navball that appear when SAS is enabled.
        /// </summary>
        /// <remarks>Equivalent to <see cref="AutoPilot.SASMode"/></remarks>
        [KRPCProperty]
        public SASMode SASMode {
            get { return GetSASMode (InternalVessel); }
            set { SetSASMode (InternalVessel, value); }
        }

        internal static SASMode GetSASMode (global::Vessel vessel)
        {
            return vessel.Autopilot.Mode.ToSASMode ();
        }

        internal static void SetSASMode (global::Vessel vessel, SASMode value)
        {
            var mode = value.FromSASMode ();
            if (!vessel.Autopilot.CanSetMode (mode))
                throw new InvalidOperationException ("Cannot set SAS mode of vessel");
            vessel.Autopilot.SetMode (mode);
            // Update the UI buttons
            var modeIndex = (int)vessel.Autopilot.Mode;
            var modeButtons = UnityEngine.Object.FindObjectOfType<VesselAutopilotUI> ().modeButtons;
            modeButtons [modeIndex].SetState (true);
        }

        /// <summary>
        /// The current <see cref="SpeedMode"/> of the navball.
        /// This is the mode displayed next to the speed at the top of the navball.
        /// </summary>
        [KRPCProperty]
        [SuppressMessage ("Gendarme.Rules.Correctness", "MethodCanBeMadeStaticRule")]
        public SpeedMode SpeedMode {
            get { return GlobalSpeedMode; }
            set { FlightGlobals.SetSpeedMode (value.FromSpeedMode ()); }
        }

        [KRPCProperty]
        internal static SpeedMode GlobalSpeedMode {
            get { return FlightGlobals.speedDisplayMode.ToSpeedMode (); }
        }

        /// <summary>
        /// The state of RCS.
        /// </summary>
        [KRPCProperty]
        public bool RCS {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.RCS)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.RCS, value); }
        }

        /// <summary>
        /// The state of the landing gear/legs.
        /// </summary>
        [KRPCProperty]
        public bool Gear {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.Gear)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.Gear, value); }
        }

        /// <summary>
        /// The state of the lights.
        /// </summary>
        [KRPCProperty]
        public bool Lights {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.Light)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.Light, value); }
        }

        /// <summary>
        /// The state of the wheel brakes.
        /// </summary>
        [KRPCProperty]
        public bool Brakes {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.Brakes)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.Brakes, value); }
        }

        /// <summary>
        /// The state of the abort action group.
        /// </summary>
        [KRPCProperty]
        public bool Abort {
            get { return InternalVessel.ActionGroups.groups [BaseAction.GetGroupIndex (KSPActionGroup.Abort)]; }
            set { InternalVessel.ActionGroups.SetGroup (KSPActionGroup.Abort, value); }
        }

        /// <summary>
        /// The state of the throttle. A value between 0 and 1.
        /// </summary>
        [KRPCProperty]
        public float Throttle {
            get { return PilotAddon.Get (InternalVessel).Throttle; }
            set { PilotAddon.Set (InternalVessel).Throttle = value; }
        }

        /// <summary>
        /// The state of the pitch control.
        /// A value between -1 and 1.
        /// Equivalent to the w and s keys.
        /// </summary>
        [KRPCProperty]
        public float Pitch {
            get { return PilotAddon.Get (InternalVessel).Pitch; }
            set { PilotAddon.Set (InternalVessel).Pitch = value; }
        }

        /// <summary>
        /// The state of the yaw control.
        /// A value between -1 and 1.
        /// Equivalent to the a and d keys.
        /// </summary>
        [KRPCProperty]
        public float Yaw {
            get { return PilotAddon.Get (InternalVessel).Yaw; }
            set { PilotAddon.Set (InternalVessel).Yaw = value; }
        }

        /// <summary>
        /// The state of the roll control.
        /// A value between -1 and 1.
        /// Equivalent to the q and e keys.
        /// </summary>
        [KRPCProperty]
        public float Roll {
            get { return PilotAddon.Get (InternalVessel).Roll; }
            set { PilotAddon.Set (InternalVessel).Roll = value; }
        }

        /// <summary>
        /// The state of the forward translational control.
        /// A value between -1 and 1.
        /// Equivalent to the h and n keys.
        /// </summary>
        [KRPCProperty]
        public float Forward {
            get { return PilotAddon.Get (InternalVessel).Forward; }
            set { PilotAddon.Set (InternalVessel).Forward = value; }
        }

        /// <summary>
        /// The state of the up translational control.
        /// A value between -1 and 1.
        /// Equivalent to the i and k keys.
        /// </summary>
        [KRPCProperty]
        public float Up {
            get { return PilotAddon.Get (InternalVessel).Up; }
            set { PilotAddon.Set (InternalVessel).Up = value; }
        }

        /// <summary>
        /// The state of the right translational control.
        /// A value between -1 and 1.
        /// Equivalent to the j and l keys.
        /// </summary>
        [KRPCProperty]
        public float Right {
            get { return PilotAddon.Get (InternalVessel).Right; }
            set { PilotAddon.Set (InternalVessel).Right = value; }
        }

        /// <summary>
        /// The state of the wheel throttle.
        /// A value between -1 and 1.
        /// A value of 1 rotates the wheels forwards, a value of -1 rotates
        /// the wheels backwards.
        /// </summary>
        [KRPCProperty]
        public float WheelThrottle {
            get { return PilotAddon.Get (InternalVessel).WheelThrottle; }
            set { PilotAddon.Set (InternalVessel).WheelThrottle = value; }
        }

        /// <summary>
        /// The state of the wheel steering.
        /// A value between -1 and 1.
        /// A value of 1 steers to the left, and a value of -1 steers to the right.
        /// </summary>
        [KRPCProperty]
        public float WheelSteering {
            get { return PilotAddon.Get (InternalVessel).WheelSteer; }
            set { PilotAddon.Set (InternalVessel).WheelSteer = value; }
        }

        /// <summary>
        /// The current stage of the vessel. Corresponds to the stage number in
        /// the in-game UI.
        /// </summary>
        [KRPCProperty]
        public int CurrentStage {
            get { return InternalVessel.currentStage; }
        }

        /// <summary>
        /// Activates the next stage. Equivalent to pressing the space bar in-game.
        /// </summary>
        /// <returns>A list of vessel objects that are jettisoned from the active vessel.</returns>
        [KRPCMethod]
        public IList<Vessel> ActivateNextStage ()
        {
            CheckActiveVessel ();
            if (!StageManager.CanSeparate)
                throw new YieldException (new ParameterizedContinuation<IList<Vessel>> (ActivateNextStage));
            var preVessels = FlightGlobals.Vessels.ToArray ();
            StageManager.ActivateNextStage ();
            return PostActivateStage (preVessels);
        }

        IList<Vessel> PostActivateStage (global::Vessel[] preVessels)
        {
            if (!StageManager.CanSeparate)
                throw new YieldException (new ParameterizedContinuation<IList<Vessel>, global::Vessel[]> (PostActivateStage, preVessels));
            var postVessels = FlightGlobals.Vessels;
            return postVessels.Except (preVessels).Select (vessel => new Vessel (vessel)).ToList ();
        }

        /// <summary>
        /// Returns <c>true</c> if the given action group is enabled.
        /// </summary>
        /// <param name="group">A number between 0 and 9 inclusive.</param>
        [KRPCMethod]
        public bool GetActionGroup (uint group)
        {
            if (KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGExtInstalled())
            {
                if (group < 251)
                {
                    return KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGXGroupState(InternalVessel.rootPart.flightID, group);
                }
                else
                {
                    throw new ArgumentException("Action group must be between 1 and 250 inclusive");
                }
            }
            else
            {
                if (group > 9)
                    throw new ArgumentException("Action group must be between 0 and 9 inclusive");
                return InternalVessel.ActionGroups.groups[BaseAction.GetGroupIndex(ActionGroupExtensions.GetActionGroup(group))];
            }
        }

        /// <summary>
        /// Sets the state of the given action group (a value between 0 and 9
        /// inclusive).
        /// </summary>
        /// <param name="group">A number between 0 and 9 inclusive.</param>
        /// <param name="state"></param>
        [KRPCMethod]
        public void SetActionGroup (uint group, bool state)
        {
            if (KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGExtInstalled())
            {
                if (group < 251)
                {
                    KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGXSetGroup(InternalVessel.rootPart.flightID, group, state);
                }
                else
                {
                    throw new ArgumentException("Action group must be between 1 and 250 inclusive");
                }
            }
            else
            {
                if (group > 9)
                    throw new ArgumentException("Action group must be between 0 and 9 inclusive");
                InternalVessel.ActionGroups.SetGroup(ActionGroupExtensions.GetActionGroup(group), state);
            }
        }

        /// <summary>
        /// Toggles the state of the given action group.
        /// </summary>
        /// <param name="group">A number between 0 and 9 inclusive.</param>
        [KRPCMethod]
        public void ToggleActionGroup (uint group)
        {
            if (KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGExtInstalled())
            {
                if (group < 251)
                {
                    KRPC.SpaceCenter.Services.ActionGroupExtendedMod.AGXToggleGroup(InternalVessel.rootPart.flightID, group);
                }
                else
                {
                    throw new ArgumentException("Action group must be between 1 and 250 inclusive");
                }
            }
            else
            {
                if (group > 9)
                    throw new ArgumentException("Action group must be between 0 and 9 inclusive");
                InternalVessel.ActionGroups.ToggleGroup(ActionGroupExtensions.GetActionGroup(group));
            }
        }

        /// <summary>
        /// Creates a maneuver node at the given universal time, and returns a
        /// <see cref="Node"/> object that can be used to modify it.
        /// Optionally sets the magnitude of the delta-v for the maneuver node
        /// in the prograde, normal and radial directions.
        /// </summary>
        /// <param name="ut">Universal time of the maneuver node.</param>
        /// <param name="prograde">Delta-v in the prograde direction.</param>
        /// <param name="normal">Delta-v in the normal direction.</param>
        /// <param name="radial">Delta-v in the radial direction.</param>
        [KRPCMethod]
        public Node AddNode (double ut, float prograde = 0, float normal = 0, float radial = 0)
        {
            CheckManeuverNodes ();
            return new Node (InternalVessel, ut, prograde, normal, radial);
        }

        /// <summary>
        /// Returns a list of all existing maneuver nodes, ordered by time from first to last.
        /// </summary>
        [KRPCProperty]
        public IList<Node> Nodes {
            get {
                CheckManeuverNodes ();
                return FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Select (x => new Node (FlightGlobals.ActiveVessel, x)).OrderBy (x => x.UT).ToList ();
            }
        }

        /// <summary>
        /// Remove all maneuver nodes.
        /// </summary>
        [KRPCMethod]
        public void RemoveNodes ()
        {
            CheckManeuverNodes ();
            var pcs = InternalVessel.patchedConicSolver;
            foreach (var node in pcs.maneuverNodes.ToList())
                pcs.RemoveManeuverNode (node);
            // TODO: delete the Node objects
        }

        void CheckActiveVessel ()
        {
            if (vesselId != FlightGlobals.ActiveVessel.id)
                throw new InvalidOperationException ("Not the active vessel");
        }

        void CheckManeuverNodes ()
        {
            CheckActiveVessel ();
            if (FlightGlobals.ActiveVessel.patchedConicSolver == null)
                throw new InvalidOperationException ("Maneuver node editing is not available. Either the vessel is in a situation where maneuver nodes cannot be used, or the tracking station has not been upgraded to support them.");
        }
    }

    public static class ActionGroupExtendedMod //Code to support interfacing with the Action Groups Extended mod by Diazo  http://forum.kerbalspaceprogram.com/index.php?/topic/67235-12oct3116-action-groups-extended-250-action-groups-in-flight-editing-now-kosremotetech/&page=1
    {
        public static bool AGExtInstalled() //Check to see if AGX is actually installed, uses System.Reflection namespace. Gate all calls to AGX behind this code to prevent errors.
        {
            try
            {
                Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
                return (bool)calledType.InvokeMember("AGXInstalled", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
            }
            catch
            {
                return false;
            }
        }
        public static bool AGXGroupState(uint FlightID, int group) //FlightID is Vessel.RootPart.FlightID, group is action group number.
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            bool GroupState = (bool)calledType.InvokeMember("AGX2VslGroupState", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { FlightID, group });
            return GroupState;
        }
        public static bool AGXToggleGroup(uint FlightID, int group) //FlightID is Vessel.RootPart.FlightID, group is action group number.
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            bool GroupState = (bool)calledType.InvokeMember("AGX2VslToggleGroupDelayCheck", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { FlightID, group });
            //AGX2VslToggleGroupDelayCheck respects RemoteTech comm delay. Change to 'AGX2VslToggleGroup' to bypass delay and command from local control.
            return GroupState;
        }
        public static bool AGXSetGroup(uint FlightID, int group, bool direction) //FlightID is Vessel.RootPart.FlightID, group is action group number, direction is what state we are chaning the group TO, so true will activate the gorup
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            bool GroupState = (bool)calledType.InvokeMember("AGX2VslActivateGroupDelayCheck", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { FlightID, group, direction });
            //AGX2VslActivateGroupDelayCheck respects RemoteTech comm delay. Change to 'AGX2VslActivateGroup' to bypass delay and command from local control.
            return GroupState;
        }
    }
}
