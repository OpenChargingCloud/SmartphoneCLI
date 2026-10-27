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

using cloud.charging.open.protocols.WWCP.Node.CommandLine;
using cloud.charging.open.protocols.WWCP.Node.Configuration;

using cloud.charging.open.Smartphone.CommandLine;

#endregion

namespace cloud.charging.open.Smartphone
{

    /// <summary>
    /// One smartphone, with its web interface and a prompt, until 'quit',
    /// Ctrl+C or SIGTERM.
    /// </summary>
    /// <remarks>
    /// What every kind of node's program does is the node's: the switches and
    /// the words -h explains them with, why it could not be set up or could not
    /// start, what goes into the certificate store, the banner and the prompt.
    /// A smartphone adds no switch and no line of its own to any of them yet,
    /// so what is left here is which program this is, and the smartphone it
    /// makes.
    /// </remarks>
    public class Program
    {

        #region (private static) Usage

        /// <summary>
        /// What -h shows: every node's switches, in a smartphone's words.
        /// </summary>
        private static readonly NodeUsage Usage = new (

            Program:           "SmartphoneCLI",
            Kind:              Smartphone.SmartphoneKind,
            DefaultPort:       Smartphone.DefaultHTTPPort,
            FrontendSources:   "libs/Smartphone/Smartphone/Frontend",
            CertificateKinds:  Smartphone.CertificateKinds

        );

        #endregion


        public static async Task<Int32> Main(String[] Arguments)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            #region Arguments

            // Every node's switches; a smartphone has none of its own yet.
            var arguments = NodeArguments.Parse(Arguments);

            if (arguments.Refused(Usage) is Int32 refused)
                return refused;

            if (arguments.RefuseTheRest(Usage) is Int32 unknown)
                return unknown;

            var root = NodeProgram.RepositoryRoot("SmartphoneCLI.slnx");

            #endregion

            #region The smartphone

            Smartphone smartphone;

            try
            {
                smartphone = new Smartphone(
                                 HTTPHostname:      arguments.HTTPHostname,
                                 HTTPPort:          arguments.Port,
                                 AccountsPath:      arguments.AccountsPathBelow(root),
                                 ConfigFile:        new WWCPConfigFile(arguments.ConfigFilePathBelow(root)),
                                 Frontend:          arguments.Frontend,
                                 CertificatesPath:  arguments.CertificatesPath,
                                 ConsoleLogLevel:   arguments.ConsoleLogLevel,
                                 LogPath:           arguments.LogPathBelow(root),
                                 BridgeDebugLog:    !arguments.NoTrace,
                                 SSH:               arguments.SSH,
                                 Web:               arguments.Web
                             );
            }
            catch (Exception e)
            {
                return NodeProgram.CouldNotBeSetUp(Smartphone.SmartphoneKind, e, arguments.Verbose);
            }

            await using (smartphone)
            {

                // What somebody signed in over SSH gets: this program's own command
                // line, with its commands beside the node's.
                smartphone.CommandLines = (terminal, caller) => new SmartphoneCLI(smartphone, terminal, caller);

                if (smartphone.ImportCertificates(arguments, out _) is Int32 notImported)
                    return notImported;

                if (arguments.ListCertificates)
                    smartphone.ListCertificates();

                if (await smartphone.Started(arguments.Verbose) is Int32 notStarted)
                    return notStarted;

                #region What somebody who just started this needs to know

                foreach (var line in smartphone.Banner())
                    Console.WriteLine(line);

                #endregion

                #region The command line, until 'quit', Ctrl+C or SIGTERM

                // The node's: a prompt where somebody can type, and waiting
                // where nobody can, with the log sharing the screen.
                await new SmartphoneCLI(smartphone).RunUntilStopped();

                #endregion

            }

            #endregion

            return 0;

        }

    }

}
