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
    partial class Program
    {
        public class ThrustController
        {
            private readonly Program program;
            private readonly PidController myPid;
            private readonly IMyCockpit myCockpit;
            private readonly Base6Directions.Direction forward = Base6Directions.Direction.Forward;

            private bool enabled = false;
            private float maxTarget = 90, setpoint = 20;

            public bool Enabled
            { 
                get { return enabled; }
                set { enabled = value; }
            }
            public float Setpoint
            { 
                get { return setpoint; }
                set {
                    if (value > maxTarget)
                        setpoint = maxTarget;
                    else if (value < 0)
                        setpoint = 0;
                    else
                        setpoint = value;
                }
            }
            public float MaxTarget
            {
                get { return maxTarget; }
                set { maxTarget = value; }
            }
            public ThrustController(Program program, MyIni ini)
            {
                this.program = program;
                string tag = ini.Get("CruiseControl", "cockpit_tag").ToString();
                float __kP = ini.Get("CruiseControl", "kP").ToSingle();
                float __kI = ini.Get("CruiseControl", "kI").ToSingle();
                float __kD = ini.Get("CruiseControl", "kD").ToSingle();
                float __decay = ini.Get("CruiseControl", "decay").ToSingle();
                float __step = ini.Get("CruiseControl", "step").ToSingle();


                var blocks = new List<IMyCockpit>();
                program.GridTerminalSystem.GetBlocksOfType<IMyCockpit>(
                    blocks,
                    x => x.CubeGrid == program.Me.CubeGrid &&
                            x.CustomName.Contains(tag)
                );
                try
                {
                    myCockpit = blocks[0];
                }
                catch (Exception)
                {
                    program.Echo("Could not find matching cockpit block");
                    return;
                }
                blocks.Clear();

                myPid = new PidController(__kP, __kI, __kD, __decay, __step);
            }

            public void Tick10()
            {
                Tick10(program.Echo);
            }

            public void Tick10(Action<string> echo)
            {
                float process = program.getShipSpeed(myCockpit, forward);
                float error = setpoint - process;
                float correction = myPid.Control(error);
                echo("Setpoint (SP):");
                echo(setpoint.ToString());
                echo("Process (PV):");
                echo(process.ToString());
                echo("Error (E):");
                echo(error.ToString());
                echo("Correction:");
                echo(correction.ToString());
            }
        }
    }
}
