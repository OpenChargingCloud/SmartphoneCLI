# SmartphoneCLI

[![CI](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/ci.yml/badge.svg)](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/ci.yml)
[![Nightly](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/nightly.yml/badge.svg)](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/nightly.yml)

The smartphone of an EV driver, with a web interface and a prompt, until
'quit' or Ctrl+C.

It stands in for the apps on Android and iOS: the thing in the driver's pocket
that will hold an account at an e-mobility provider, issue charging tickets,
and show a charging session while it happens. None of that is here yet. What
is here is what an app has to get right before any of it: the time, the name
servers, and - above all - which root CAs it believes, because an app talks to
its provider over TLS and to nobody else.

It is a sibling of [EVCLI](https://github.com/OpenChargingCloud/EVCLI),
[EMSPCLI](https://github.com/OpenChargingCloud/EMSPCLI),
[PKICLI](https://github.com/OpenChargingCloud/PKICLI) and the others, and is
built the same way: a [Hermod](https://github.com/Vanaheimr/Hermod) HTTP server
carrying a JSON API and one Server-Sent Events stream, and a web interface
built by npm and embedded into the assembly. What every program of the family
has - the sign-in, the name servers (DNS), the time servers (NTS), the
certificates they are held to, the SSH server and the log - is
[WWCP_Node](https://github.com/OpenChargingCloud/WWCP_Node); what is the
smartphone's own is in [Smartphone](https://github.com/OpenChargingCloud/Smartphone).


### Getting it

The libraries it is built from are submodules, so they have to come along:

```
git clone --recurse-submodules https://github.com/OpenChargingCloud/SmartphoneCLI.git
```

If you already cloned it without them:

```
git submodule update --init --recursive
```

**On Windows**, turn long paths on first: `git config --global core.longpaths true`.


### Building and running it

.NET 10 and Node 22 or newer. The build runs `npm ci` and `npm run build` for
the web interface itself whenever its inputs changed:

```
dotnet build SmartphoneCLI.slnx
./run.sh
```

It listens on http://127.0.0.1:2346/ - one below the vehicle's 2347 - and
serves its prompt over SSH on 22346. `--any` for every address, `--port` for
another one, `--help` for every switch.

At the first start it makes one account, `root`, and shows its password once,
on the console. `root` is the driver - the person whose phone this is. Give it
your own SSH key with that very start, and you can type at the phone over SSH
from the start:

```
dotnet run --project SmartphoneCLI -- --authorize-ssh-key root=C:\Users\you\.ssh\id_ed25519.pub
```

Beside the solution it writes `configuration.json`, `accounts/`, `logs/`,
`ssh/`, `known-servers.json` and `certificates/`. All of them are in
`.gitignore`.


### The web interface

| | |
|---|---|
| **Configuration** | what the smartphone is running, and below it the **DNS client**, the **NTS client**, the **SSH server**, the **Certificates** it believes and its **Identities** |
| **Logs** | the log as it happens |


### Certificates

The store is `certificates/` beside the configuration file: one file per
certificate and an `index.json` with what a file cannot say about itself. A
smartphone keeps three kinds:

| | |
|---|---|
| `tlsRoot` | a root CA it believes, beside the roots of the machine it runs on - and what for: `nts`, `dns`, or both. Later its e-mobility provider's. |
| `tlsServer` | a server's own certificate, kept to hold that server to by its fingerprint |
| `tlsIdentity` | who the smartphone is to a server that asks, with its private key - for when it signs in to its e-mobility provider with a certificate |

No root of clients, because nobody connects to a smartphone with a
certificate, and nothing of ISO 15118, which is the vehicle's. Roots go in on
the **Certificates** page, or at a start:

```
dotnet run --project SmartphoneCLI -- --import-certificate tlsRoot=my-root.pem --list-certificates
```

Holding a time server or a name server to a root or to a fingerprint is said
on the **NTS client** and **DNS client** pages.


### Typing at it

```
smartphone:2346> syncNTS
```

`help` lists what can be typed: what every node has - `syncNTS`, `log`, `who`,
`sshKeys`, `apiKeys` - and, as the smartphone grows, its own commands, one
file each below `SmartphoneCLI/CLI/CLICommands/`.


### What comes next

An account at an e-mobility provider (EMSPCLI), charging tickets issued there,
and a charging session watched live.


## Your participation

This software is Open Source under the **Affero GPL 3.0 license**.
We appreciate your participation in this ongoing project, and your help to
improve it and the e-mobility ICT in general. If you find bugs, want to
request a feature or send us a pull request, feel free to use the normal
GitHub features to do so. For this please read the Contributor License
Agreement carefully and send us a signed copy or use a similar free and
open license.
