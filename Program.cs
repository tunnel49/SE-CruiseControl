﻿using System;
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
        private readonly List<ConfigOption> defaultConfig = new List<ConfigOption>()
        {
            new ConfigOption("cockpitTag", "[FA]", "Tag to select the main cockpit", true),
            new ConfigOption("kP", "5", "Proportional Gain of PID Controller", true),
            new ConfigOption("kI", ".1", "Integral Gain of PID Controller", true),
            new ConfigOption("kD", "1", "Derivative Gain of PID Controller", true),
            new ConfigOption("decay", "0.25", "Integral Decay", true),
            new ConfigOption("step","0.1","Time Step between control inputs", true)
        };
        private CustomDataConfig configReader;

        Base6Directions.Direction forward = Base6Directions.Direction.Forward;

        PidController myPid;
        IMyCockpit myCockpit;

        bool enabled = false;
        float setpoint = 20;
        float maxTarget = 90;
        public Program()
        {
            configReader = new CustomDataConfig(Me, defaultConfig);
            Initialize();
        }
        private void Initialize()
        {
            String tag = configReader.Get<string>("cockpitTag");
            float __kP = configReader.Get<float>("kP");
            float __kI = configReader.Get<float>("kI");
            float __kD = configReader.Get<float>("kD");
            float __decay = configReader.Get<float>("decay");
            float __step = configReader.Get<float>("step");

            var blocks = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(
                blocks,
                x => x.CubeGrid == Me.CubeGrid &&
                        x.CustomName.Contains(tag)
            );
            try{
                myCockpit = blocks[0];
            }
            catch (Exception){
                Echo("Could not find matching cockpit block");
                return;
            }
            blocks.Clear();

            myPid = new PidController(__kP, __kI, __kD, __decay, __step);
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
            if ((updateSource & UpdateType.Update10) == 0)
            {
                Read(argument);
                return;
            }

            float process = getShipSpeed(myCockpit, forward);
            float error = setpoint - process;
            float correction = myPid.Control(error);
            Echo("Setpoint (SP):");
            Echo(setpoint.ToString());
            Echo("Process (PV):");
            Echo(process.ToString());
            Echo("Error (E):");
            Echo(error.ToString());
            Echo("Correction:");
            Echo(correction.ToString());
        }

        public void Read(string argument)
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
                    enabled = false; break;
                case "enable":
                    enabled = true; break;
                case "toggle":
                    enabled = !enabled; break;
                case "init":
                    Initialize(); break;
                case "set":
                    setpoint = value; break;
                case "inc":
                    setpoint += value; break;
                case "dec":
                    setpoint -= value; break;
            }
            setpoint = Math.Min(setpoint, maxTarget);
            setpoint = Math.Max(setpoint, 0);
        }

        public float getShipSpeed(IMyCockpit cockpit, Base6Directions.Direction direction)
        {
            var forward3I = cockpit.Position + Base6Directions.GetIntVector(cockpit.Orientation.TransformDirection(direction));
            var forward = Vector3D.Normalize(Vector3D.Subtract(cockpit.CubeGrid.GridIntegerToWorld(forward3I), cockpit.GetPosition()));

            return Convert.ToSingle(Vector3D.Dot(cockpit.GetShipVelocities().LinearVelocity, forward));
        }
    }
}
