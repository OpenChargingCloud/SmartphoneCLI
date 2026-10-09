/*
 * Copyright (c) 2014-2026 GraphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of SmartphoneCLI <https://github.com/OpenChargingCloud/SmartphoneCLI>
 *
 * Licensed under the Affero GPL license, Version 3.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.gnu.org/licenses/agpl.html
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System.Reflection;

using org.GraphDefined.Vanaheimr.CLI;

using cloud.charging.open.protocols.WWCP.Node.CommandLine;

#endregion

namespace cloud.charging.open.Smartphone.CommandLine
{

    /// <summary>
    /// The command line of a running smartphone.
    /// </summary>
    /// <remarks>
    /// Everything a command needs is reachable from here, which is why every
    /// command takes one of these: the smartphone itself, and through it its
    /// configuration, its log and everything the JSON API can do. A command is
    /// a third way of asking for the same thing, beside the web interface and
    /// the switches at a start - never an implementation of its own.
    ///
    /// Commands are not listed anywhere. The commands every node has - syncNTS
    /// among them - are the node's, and NodeCLI, which this derives from,
    /// finds them in WWCP_Node. The constructor registers this type as well,
    /// so that this assembly is walked for anything that implements
    /// ICLICommand and can be built from a SmartphoneCLI: a command of the
    /// smartphone's own is a new file below CLI/CLICommands and nothing else.
    /// </remarks>
    public class SmartphoneCLI : NodeCLI
    {

        #region Properties

        /// <summary>
        /// The smartphone these commands are about.
        /// </summary>
        public Smartphone Smartphone { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create the command line of the given smartphone.
        /// </summary>
        /// <param name="Smartphone">The running smartphone.</param>
        /// <param name="AssembliesWithCLICommands">Further assemblies to search for commands. This one and WWCP_Node are searched either way.</param>
        public SmartphoneCLI(Smartphone         Smartphone,
                             params Assembly[]  AssembliesWithCLICommands)

            : base(Smartphone, AssembliesWithCLICommands)

        {

            this.Smartphone = Smartphone;

            RegisterCLIType(typeof(SmartphoneCLI));

        }

        /// <summary>
        /// Create the command line of the given smartphone on the given
        /// terminal, for the given caller - a session over SSH.
        /// </summary>
        /// <param name="Smartphone">The running smartphone.</param>
        /// <param name="Terminal">What the command line is typed at and written on.</param>
        /// <param name="Caller">Who is typing at it.</param>
        /// <param name="AssembliesWithCLICommands">Further assemblies to search for commands. This one and the node's are searched either way.</param>
        public SmartphoneCLI(Smartphone         Smartphone,
                             ICLITerminal       Terminal,
                             CLICaller          Caller,
                             params Assembly[]  AssembliesWithCLICommands)

            : base(Smartphone, Terminal, Caller, AssembliesWithCLICommands)

        {

            this.Smartphone = Smartphone;

            RegisterCLIType(typeof(SmartphoneCLI));

        }

        #endregion


        #region (protected override) GetPrompt()

        /// <summary>
        /// Which port this smartphone answers on, because a machine that is one
        /// of several on a bench should say which one it is before it asks for
        /// a command.
        /// </summary>
        protected override String GetPrompt()

            => $"smartphone:{Smartphone.HTTPPort}> ";

        #endregion

    }

}
