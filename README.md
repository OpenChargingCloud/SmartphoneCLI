# SmartphoneCLI

[![CI](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/ci.yml/badge.svg)](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/ci.yml)
[![Nightly](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/nightly.yml/badge.svg)](https://github.com/OpenChargingCloud/SmartphoneCLI/actions/workflows/nightly.yml)

The smartphone of an EV driver, with a web interface and a prompt, until
'quit' or Ctrl+C.

It stands in for the apps on Android and iOS: the thing in the driver's pocket
with a map of charging stations, a "Charge now!" on each of them, and the
sessions it started, watched while they happen. Given the trust anchor of a
roaming network, the map is that network's, and every station is checked
against the anchor before anything it says is believed: its tariffs, its
prices, and the keys its energy meters sign their measurements with. With an
account at an e-mobility provider, every start has a charging ticket of its
own, which tells a charge point operator who pays and not who charges, and
goes through the provider as a start request signed by the ticket's key and
countersigned by the provider. From then on a session is what the provider
reports of it - the charge point operator's session and its charge detail
record - and simulated only where the provider reports nothing.
Underneath is what an app has to get right before any of it: the time, the
name servers, and - above all - what it trusts: which root CAs, because an
app talks to its provider over TLS, and which roaming network operator.

It is a sibling of [EVCLI](https://github.com/OpenChargingCloud/EVCLI),
[EMSPCLI](https://github.com/OpenChargingCloud/EMSPCLI),
[PKICLI](https://github.com/OpenChargingCloud/PKICLI) and the others, and is
built the same way: a [Hermod](https://github.com/Vanaheimr/Hermod) HTTP server
carrying a JSON API and one Server-Sent Events stream, and a web interface
built by npm and embedded into the assembly. What every program of the family
has - the sign-in, the name servers (DNS), the time servers (NTS), the
certificates they are held to, the SSH server and the log - is
[WWCP_Node](https://github.com/OpenChargingCloud/WWCP_Node); what is the
smartphone's own is in [Smartphone](https://github.com/OpenChargingCloud/Smartphone);
roaming networks, their proofs and how they are checked are
[WWCP_POI](https://github.com/OpenChargingCloud/WWCP_POI).


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
`ssh/`, `known-servers.json`, `certificates/`, `charging-sessions.json` and,
with an account at an e-mobility provider, `provider/`. All of them are in
`.gitignore`.


### The web interface

| | |
|---|---|
| **Map** | the charging stations on an OpenStreetMap map, each pin in the colour of what its charge points are now - available, charging, reserved, out of order, planned. A search above it finds a station by its name, street, city, operator or EVSE ID; **Locate me** shows where this device is. A pin opens the station: its address, operator and price, its charge points with their status, connector and power, and **Charge now!** at the one chosen. Of a roaming network, the pin checks the station first, says whether it is signed, and leads to **What is signed about it**: who attested it and until when, each charge point's tariff and price, and its energy meter with the public key it signs with |
| **Charging** | what this phone charges right now - several sessions at once, each with what it charged, how fast, for how long, what it costs so far, and **Stop charging**; **History** below it lists every session, running and past, and a row opens one with everything about it |
| **Configuration** | what the smartphone is running, and below it **Map** - the trust anchor of the roaming network the map is of, and a URL kept for later - the **E-mobility provider** - the account this phone charges with, its account key and the provider's ticket issuer - the **DNS client**, the **NTS client**, the **SSH server**, the **Certificates** it believes, its **Identities** and its **Server identities** |
| **Logs** | the log as it happens |

"root" may do everything; the role **driver** looks at the map, the
sessions and the account at the e-mobility provider and starts and stops a
session, and the node's **viewer** looks at everything and starts nothing.
Only an administrator makes, takes or forgets the account at the provider:
it is whose money is charged with.


### The map and charging, for now

Without a roaming network, the map shows eleven demo stations in and around
Jena, with made-up operators: most of their charge points come and go between
available and charging every ten minutes, a few stay out of order, reserved or
planned. The stations are kept in the shape of OCPI locations, so that the
pools of a roaming network drop in where they are. The URL under
Configuration → Map beside the trust anchor is kept in `configuration.json`
and shown, and not asked.

**Charge now!** starts a simulated session: a vehicle that takes what the
charge point gives, up to 11 kW on AC and 150 kW on DC, until 30 kWh are in.
Without an account at an e-mobility provider, nothing is asked of one and
nothing is charged for real; with one, the start and the stop go through it,
and the session is what the provider reports of it (below) - simulated only
where it reports nothing. Every page that shows a session says which. The
simulated figures are worked out from when it started and stopped rather than
counted by a timer, so a session charges on while nobody looks, and a restart
of the phone keeps every session, the running ones running.

The map's tiles come from `tile.openstreetmap.org` - the one thing the web
interface loads from elsewhere, allowed by its Content-Security-Policy for
images and for nothing else - and **Locate me** asks the browser, which says
where it is only to a page served over HTTPS or from the machine itself
(`127.0.0.1`, `localhost`). From anywhere else it takes HTTPS, below.


### A roaming network

What an app trusts of a roaming network is its **trust anchor**: what its
operator prints as a QR code, on its website or in its documents - where the
network is served, one URL or more, and the thumbprints of the identity keys
of its operator, with how many of their holders have to agree. It goes in
under Configuration → Map as the text of the QR code (`WWCP/TA-2:` and what
follows), its URI or its JSON, and is kept in `configuration.json` as its
JSON:

```json
"roamingNetwork": { "trustAnchor": { "@id": "QA", "urls": [ "https://hub.example/poi/RNs/QA" ], "keys": [ "a1…" ] } }
```

The map then shows the charging pools the network lists (`…/RNs/{id}/CPs`),
asked at its URLs in their order, the next where one does not answer, and
again after a minute or on **Reload**. That list is not signed: it only says
where to look. A pin, as it opens, has the smartphone fetch the **proof** of
each of its stations (`…/CSs/{id}/proof`) and check it, offline, against the
trust anchor: everything the network says of the station - its EVSEs, their
energy meters with the public keys they sign their measurements with, the
tariffs they take - in a hash tree whose root the operator of the network
attested, with a key its identity key certified, for a quarter of an hour.
The price statements of the station (`…/CSs/{id}/priceStatements`), new prices
its operator signed, are checked against the keys the proof gives.

What a proof that holds gives is shown, and nothing of one that does not:
nothing is charged at a station nobody the anchor names attested, nor at one
whose check has run out. **Charge now!** there takes the price its proof says
- the price statement of the EVSE before that of its station before that of
its pool before the tariff - and the energy meter's key is known before the
first meter value is. A roaming network says the status of its charge points
only over its WebSocket, so their pins are grey, of a status not known.

The trust anchor is the only thing trusted. The servers only pass on what is
signed, and a key a proof names of itself proves nothing. A QR code is as
trustworthy as the place it was found: one stuck on a charging station may be
anybody's.


### HTTPS

```
dotnet run --project SmartphoneCLI -- --any --https-cert C:\certs\fullchain.pem --https-key C:\certs\privkey.pem
```

`--https` shows the store's `tlsServerIdentity` instead of a pair of PEM files,
`--redirect-port` answers plain HTTP on a second port with a redirect to
HTTPS, and `--trusted-proxy` names a reverse proxy in front that is believed
about who connects. The same can be kept in the `"web"` section of
`configuration.json` - `"tls"`, `"redirectPort"`, `"hsts"`, `"trustedProxies"`
- and the switches win over the file. Plain HTTP on any address but the
machine's own is said loudly: in the log, at the start, on the Configuration
page.


### An e-mobility provider

Under Configuration → E-mobility provider the phone gets an account at an
e-mobility provider - an [EMSP](https://github.com/OpenChargingCloud/EMSP) of
this family:

- **Signed up** by the phone (`POST /ext/auth/signup` there), with a password
  the phone makes up and keeps, which nobody has to type; or **signed in** with
  an account the driver made in a browser, its password kept by the phone from
  then on. The phone talks to the provider as its web interface does: with the
  session its sign-up or sign-in (`POST /ext/auth/login`) hands out, taken anew
  where the provider says it is over.
- **An account key**, P-256, made on the phone. The provider gets a PKCS#10
  signing request (`POST /api/v1/account-keys`) and certifies the key it
  carries; the key itself never leaves the phone.
- Kept in `provider/` beside the configuration, a directory nobody but the
  account the phone runs under may read: the password and the key are in it.
  **Forget this account** takes the account key back at the provider and
  deletes the file.

With an account, **every "Charge now!" asks for a charging ticket of its
own**, right before the start:

1. a key made for this start alone, and a ticket - its id, the provider, that
   key, good from now for an hour, no limits - in deterministic CBOR;
2. signed twice with ES256 as a COSE_Sign, with the CBOR and COSE of Styx: by
   the ticket's key (kid `"ticket"`) and by the account key (kid the SHA-256 of
   its certificate), and sent to the provider (`POST /api/v1/tickets`);
3. back from the provider signed by the ticket's key and by its ticket issuer,
   the account's signature gone - checked by the phone: the payload as it was
   written, both signatures, and nothing of the account left in it. A ticket
   that fails is refused, and nothing is started.

Then **the start goes through the provider**, as the draft
[docs/RemoteStartWithChargingTickets.md](https://github.com/OpenChargingCloud/Smartphone/blob/master/docs/RemoteStartWithChargingTickets.md)
has it:

1. a start request - the ticket's digest, the EVSE, now, a nonce, and at a
   station of a roaming network the version of its proof the phone checked
   and the tariff of its price - signed by the ticket's key, and sent with the
   ticket (`POST /api/v1/tickets/{id}/start`);
2. back countersigned by the provider's ticket issuer, with the OCPI token of
   this start alone, the authorization reference and when it signed in its
   protected header - checked by the phone: its own bytes and signature as
   they went, the issuer's signature, the token covered by it, and the time
   near the phone's clock. A start that fails is refused, and nothing is
   started;
3. **Stop** the same way: a stop request naming the ticket and the start,
   countersigned with the start's token (`POST /api/v1/tickets/{id}/stop`). A
   stop that fails leaves the session charging;
4. **what became of it**, asked of the provider while a page shows the
   session, at most every 15 seconds (`GET /api/v1/tickets/{id}`): the
   provider's state of the start, the operator's answer to the command, and
   the session and the charge detail record the operator pushed for it -
   taken only where they name the start's token and reference. From the first
   answer on, the session is what they say: starting until the operator
   reports charging, then its kWh and its cost, finished with the figures of
   its charge detail record, or failed with the operator's answer. The
   record's signed meter values are kept as they came. A provider that tells
   nothing leaves the figures simulated.

The session names its ticket - its id, its validity, the thumbprint of its key
and the fingerprint of the issuer - and its start and stop: the token, the
reference, and the digests of both requests as the provider countersigned
them. The ticket, its key and both requests are kept in `provider/tickets/`,
as the meter values of the session will be proven with that key. A charge
point operator that sees the ticket learns which provider pays, and not who
charges, and no two sessions share a key or a token. The account is not
forgotten while a session started through it charges.

The provider's side of the start is a draft as well. The stand-in EMSP of the
tests countersigns as the draft says, and so does the
[EMSP](https://github.com/OpenChargingCloud/EMSP), which sends the start on to
the charge point operator as OCPI `START_SESSION` and the stop as
`STOP_SESSION` - on OCPI 2.2.1 or later, and only for an EVSE that an operator
peered with it has pushed among its locations. Without one, "Charge now!" with
an account there gets as far as the EVSE: the EMSP takes the ticket and the
start request, answers that it knows no such EVSE, and nothing is started.
What became of a start both tell as the draft says: the stand-in as its test
has the operator report it, and the EMSP as the operator reported it - which,
without an operator, is a ticket with no start, and a session that stays
simulated.


### Certificates

The store is `certificates/` beside the configuration file: one file per
certificate and an `index.json` with what a file cannot say about itself. A
smartphone keeps four kinds:

| | |
|---|---|
| `tlsRoot` | a root CA it believes, beside the roots of the machine it runs on - and what for: `nts`, `dns`, or both. Later its e-mobility provider's. |
| `tlsServer` | a server's own certificate, kept to hold that server to by its fingerprint |
| `tlsIdentity` | who the smartphone is to a server that asks, with its private key - for when it signs in to its e-mobility provider with a certificate |
| `tlsServerIdentity` | who its web interface is over HTTPS, with its private key - what `--https` shows |

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

A start through a charge point operator, end to end: the phone, the EMSP and
an operator that starts and stops at a station on `START_SESSION` and
`STOP_SESSION` and sends its session and its charge detail record, peered
through the [RoamingHub](https://github.com/OpenChargingCloud/RoamingHubCLI) -
once the hub passes on what one peer sends to another, and the
[CSMS](https://github.com/OpenChargingCloud/CSMSCLI) starts and stops on these
commands; neither does yet. Then the signed meter values of a charge detail
record checked against the key the station's proof gave; and the status of the
charge points of a roaming network, followed over its WebSocket.


## Your participation

This software is Open Source under the **Affero GPL 3.0 license**.
We appreciate your participation in this ongoing project, and your help to
improve it and the e-mobility ICT in general. If you find bugs, want to
request a feature or send us a pull request, feel free to use the normal
GitHub features to do so. For this please read the Contributor License
Agreement carefully and send us a signed copy or use a similar free and
open license.
