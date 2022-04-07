using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private readonly MyIni _ini = new MyIni();
        private readonly ThrustController thrustController;


        public Program()
        {
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
                throw new Exception(result.ToString());
            thrustController = new ThrustController(this, _ini);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }


        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }
        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource){
                case (UpdateType.Update10):
                    thrustController.Tick10();
                    break;
                default:
                    Command(argument);
                    return;
            }
        }

        public void Command(string argument)
        {
            float value = 0;
            string[] args = argument.Split(' ');
            if (args.Length < 1)
                return;
            try
            {
                value = Convert.ToSingle(args[1]);
            }
            catch { }
            switch (args[0].ToLower())
            {
                case "disable":
                    thrustController.Enabled = false; break;
                case "enable":
                    thrustController.Enabled = true; break;
                case "toggle":
                    thrustController.Enabled = !thrustController.Enabled; break;
                case "set":
                    thrustController.Setpoint = value; break;
                case "inc":
                    thrustController.Setpoint += value; break;
                case "dec":
                    thrustController.Setpoint -= value; break;
            }
        }

        public float getShipSpeed(IMyCockpit cockpit, Base6Directions.Direction direction)
        {
            var forward3I = cockpit.Position + Base6Directions.GetIntVector(cockpit.Orientation.TransformDirection(direction));
            var forward = Vector3D.Normalize(Vector3D.Subtract(cockpit.CubeGrid.GridIntegerToWorld(forward3I), cockpit.GetPosition()));

            return Convert.ToSingle(Vector3D.Dot(cockpit.GetShipVelocities().LinearVelocity, forward));
        }
    }
}
