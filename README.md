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
record, whose signed meter values are checked with the key the station's
proof gave for its energy meter - and simulated only where the provider
reports nothing.
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
[WWCP_POI](https://github.com/OpenChargingCloud/WWCP_POI); signed meter
values are read and verified by
[ChargyCore.NET](https://github.com/OpenChargingCloud/ChargyCore.NET).


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

SFTP is there, as on every node of the family, but off: the phone has no
files to move for anyone yet. **Configuration > SSH server** switches it on
and off while the phone runs, without its SSH server starting anew, and keeps
it in `configuration.json` (`"ssh": { "sftp": true }`).

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
| **Map** | the charging stations on an OpenStreetMap map, each pin in the colour of what its charge points are now - available, charging, reserved, out of order, planned - of a roaming network as it says it over its WebSocket. A search above it finds a station by its name, street, city, operator or EVSE ID; **Locate me** shows where this device is. A pin opens the station: its address, operator and price, its charge points with their status, connector and power, and **Charge now!** at the one chosen. Of a roaming network, the pin checks the station first, says whether it is signed, and leads to **What is signed about it**: who attested it and until when, each charge point's tariff and price, its energy meter with the public key it signs with, and the web payment page of its operator; **Pay ad hoc at its operator** leads there |
| **Charging** | what this phone charges right now - several sessions at once, each with what it charged, how fast, for how long, what it costs so far, and **Stop charging**; **History** below it lists every session, running and past, and a row opens one with everything about it - at a station of a roaming network, whether its meter signed the values of its charge detail record, and the price stated for its start |
| **Configuration** | what the smartphone is running, and below it **Map** - the trust anchor of the roaming network the map is of, which of its identity keys are trusted, and a URL kept for later - the **E-mobility provider** - the account this phone charges with, its account key and the provider's ticket issuer - the **DNS client**, the **NTS client**, the **SSH server**, the **Certificates** it believes, its **Identities** and its **Server identities** |
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

What an app pins of a roaming network is its **trust anchor**: what its
operator prints as a QR code, on its website or in its documents - where the
network is served, one URL or more, and the thumbprints of the identity keys
of its operator, with how many of their holders have to agree. It goes in
under Configuration → Map as the text of the QR code (`WWCP/TA-2:` and what
follows), its URI or its JSON.

Setting it trusts none of its keys yet, as in every app of the family: a QR
code is as trustworthy as the place it was found, and one stuck on a charging
station may be anybody's. Configuration → Map asks the network for the key
statement of its operator (`…/RNs/{id}/keyStatement`) and shows each key of
the anchor, per holder, with what the statement says of it, and whether the
statement is signed validly and valid now. An administrator trusts each key on
its own, confirmed first, and can trust it no longer. Setting the same anchor
again, in any of its forms, keeps the keys trusted; another one trusts none.
Both are kept in `configuration.json`, the anchor as its JSON:

```json
"roamingNetwork": {
    "trustAnchor": { "@id": "QA", "urls": [ "https://hub.example/poi/RNs/QA" ], "keys": [ "a1…" ] },
    "trustedKeys": [ { "id": "a1…", "acceptedAt": "2026-11-03T09:00:00.0000000+00:00", "acceptedBy": "admin" } ]
}
```

The map then shows the charging pools the network lists (`…/RNs/{id}/CPs`),
asked at its URLs in their order, the next where one does not answer, and
again after a minute or on **Reload**. That list is not signed: it only says
where to look. A pin, as it opens, has the smartphone fetch the **proof** of
each of its stations (`…/CSs/{id}/proof`). The phone checks it offline, against
the keys trusted here. That is a trust context of them, evaluated of the key
statement the proof gives at the time of the check, as WWCP_POI evaluates one.
The proof holds everything the network says of the station: its EVSEs, their
energy meters with the public keys they sign their measurements with, and the
tariffs they take. They sit in a hash tree whose root the operator of the
network attested for a quarter of an hour, with a key its identity key
certified. The page says which holders attest it and until when, at most, it
holds. The price statements of the station (`…/CSs/{id}/priceStatements`), new
prices its operator signed, are checked against the keys the proof gives, each
on its own and then all together. Of each entity the one in force is taken.
Where two of one sequence contradict each other, a split view, none is taken,
and the price is not known.

What a proof that holds gives is shown, and nothing of one that does not:
nothing is charged at a station nobody trusted here attested, nor at one whose
check has run out. **Charge now!** there takes the price its proof says
- the price statement of the EVSE before that of its station before that of
its pool before the tariff - and the energy meter's key is known before the
first meter value is.

Which charge points are free the network says over the WebSocket of its
history (`…/history/ws`): the phone subscribes to the admin status and the
status of its EVSEs, asks for all of it once, and the pins take the colour of
what they are, the map asking again every ten seconds. Where the network says
a charge point is taken, reserved or out of service, nothing is started there.
Nothing of it is signed - like the list of pools, it is only where to look -
and nothing of it is kept once the network is not reached, at none of the URLs
of its trust anchor: the pins are grey then, of a status not known, and a
charge point is started at as one a driver stands in front of.

Where a driver pays ad hoc, without an e-mobility provider, the proof says
too: the web payment page of the station's operator (`webPaymentURL` of
WWCP_POI) - the station's own, else that of its pool, else the operator's,
signed by a key of the operator that may sign `payments`, as whoever changes
it redirects payments. The popup of a station whose proof holds offers it as
**Pay ad hoc at its operator**, for the station of the charge point chosen;
there the driver pays and starts, and the page leads on to the session. Only
an https URL is linked, another one only said. It is not the URL in the
dynamic QR code of a station, which is of one EVSE and changes every few
minutes.

Paid there, the page started the session and gives its live link - a
[Charge Transparency LiveLink](https://github.com/OpenChargingCloud/ChargyCore.NET/tree/master/documentation/ChargeTransparencyLiveLink)
document, or the https URL of one. The driver hands it over in the popup of
the station, for the charge point paid for (`POST v1/charging/sessions` with
`"liveLink"`), and only where the station's proof holds and the link names no
other charge point. From then on the phone follows it with
[ChargyCore.NET](https://github.com/OpenChargingCloud/ChargyCore.NET) - over
https, Server-Sent Events or a WebSocket, as the link says - and keeps every
signed document that comes, as it came: the session is starting until the
first comes, charging from then on, and finished once the meter signs the end
of the transaction or the station ends the stream. Its energy and cost are
what the documents say, for the price the proof gives. A restart of the
phone follows it on.

The page may give a stop link as well - an https URL of this session alone -
and the driver hands it over with the live link (`"stopLink"`), or with the
stop (`POST v1/charging/sessions/{id}/stop` with `{ "stopLink" }`). The phone
stops the session with a POST to it, which the page takes with a 2xx; the
session charges on until the meter signs its end, and is stopped then. Only
the host of the stop link is shown and logged: whoever has the link stops the
session. Where the page gave none, the driver stops it there.

Handed over as well is the limit the driver chose at the page (`"limit"`), as
OCPP 2.1 says it to the station, which keeps to it - the `transactionLimit`
of `RequestStartTransaction`: `maxEnergy` in Wh, `maxCost` in the currency of
the tariff, `maxTime` in seconds, `maxSoC` in percent. While it charges, the
session shows each part of it against what the signed values say. Once the
meter signs the end, or the station ends the stream, the phone compares them:
it ended at its limit - within a hundredth of it, as the station counts from
its own start and rounds its own cost -, for another reason, as the meter
says the transaction ended ("E", "L", "R", "A", "P"), or stopped by this
phone. A state of charge no meter measures: whether it ended at that limit,
the phone says it can not tell. A wrong end value it compares nothing with.

Every signed value that comes over the live link is checked as it comes,
as a value of a charge detail record is: with the key the proofs kept give
for the meter of that charge point at the time it was measured, by
ChargyCore - the start value first, the first reading the meter signs as
the begin of the transaction ("B"), then every value after it, and the end
value last: the reading that ends the transaction, alone or with the whole
transaction from its begin - all of the OCMF of the session. A value holds
where it is signed by that key, the meter says it is good - status "G", no
error flagged - and it fits the values that held before it and its readings
each other: none measured before the start value, the meter counting
forwards, a begin it says again the start value that held, and nothing
measured after the end. Signed by another key, of another meter, not good
as the meter says, or not fitting, it is a wrong one: before the end, the
phone stops the session, says why, follows its live link no more, and stops
the station through the stop link of the page, telling it why - where the
page gave none, or did not take the stop, the driver is told to stop it
there. Where the phone can not decide - the identity keys trusted here do
not suffice, or no proof kept attests a key of the meter for that time - it
says so and stops nothing, and checks again with what comes next; an end
value with the proof of the station asked for again at the end.

A wrong end value stops nothing - the transaction ended - and neither does
none, where the station ended the stream and its meter signed no end: the
phone disputes the session, as it does one it stopped for a wrong value. The
page may give a dispute link for that - an https URL of this session alone -
and the driver hands it over with the live link (`"disputeLink"`), or later
(`POST v1/charging/sessions/{id}/dispute` with `{ "disputeLink" }`). The
phone sends it a POST with why (`"reason"`) and the signed values it
disputes, as they came (`"signedValues"`: the start value, and the wrong one
or the last one), which the page takes with a 2xx - once the phone knows, and
after a restart where it was not sent yet; one the page did not take is sent
again where the driver asks. Only the host of the dispute link is shown and
logged. Where the page gave none, the driver is told to dispute the session
there. A wrong value counts for nothing: the energy, the cost and the limit
are what the values that count say.

The page may give the reference of the payment it took as well, and the
driver hands it over with the live link (`"paymentReference"`). The station
writes it into the identification of every signed value of the session - the
`"ID"` of its OCMF -, so that the meter signs whose session it is: a value
that names another payment, or none, is of another session, a wrong one -
before the end the phone stops the session for it, at the end it disputes
it. Where the page gave no reference, that the values are of this very
session is not proven - the live link names the charge point alone - and the
session's page says so.

A session started there keeps, in `charging-sessions.json`, the proof its
station was checked by and the price statements that held. An attestation
holds for minutes, and the network serves the proof of its head and of no
time before, so while the session charges the phone asks for the proof again
every five minutes, and once more at its stop, keeping three at most: the one
of the start, the first after it, the latest. When the charge detail record
comes, each of its signed meter values - an OCMF document - is checked with
the key the proofs give for the energy meter of the EVSE at the time the
meter signed it, as WWCP_POI's `HistoricalMeterEvidence` takes it, and its
signature by ChargyCore. The session's page says what the meter signed was
charged and whether the record says the same, and the price the operator
stated for the charge point, in force at the start (`SessionPriceEvidence`) -
with the price the phone counted with, and what the record says it costs,
beside it. All of it is checked under the keys trusted now: a key no longer
trusted checks nothing of a session it attested. That the meter's clock was
right, and that the operator assigned the price to this very session, the
page says is not proven.

Only the keys trusted here are trusted. The servers only pass on what is
signed, and a key a proof names of itself proves nothing - nor one a charge
detail record names.


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
   record's signed meter values are kept as they came and, at a station of a
   roaming network, checked as above. A provider that tells nothing leaves the
   figures simulated.

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

Charging ad hoc, without an e-mobility provider, is there from end to end: a
free charge point is found on the map, its proof and tariff checked, its
operator's web payment page offered - reached as well by the dynamic QR code
of the station - and the live link of the session the page starts followed,
its signed start value, every value after it and its end value checked with
the key the proof gives for the meter, stopped through the stop link of the
page, its end compared with the limit chosen there, and disputed through the
dispute link of the page where a value does not hold. A value is bound to the
payment at the page where the page gives the reference of the payment and the
meter signs it as the identification of the value (`"ID"`): the phone checks
that it does - that the station and its CSMS have it signed is up to them.

Then a start through a charge point operator, end to end: the phone, the EMSP and
an operator that starts and stops at a station on `START_SESSION` and
`STOP_SESSION` and sends its session and its charge detail record, peered
through the [RoamingHub](https://github.com/OpenChargingCloud/RoamingHubCLI) -
once the hub passes on what one peer sends to another, and the
[CSMS](https://github.com/OpenChargingCloud/CSMSCLI) starts and stops on these
commands; neither does yet.


## Your participation

This software is Open Source under the **Affero GPL 3.0 license**.
We appreciate your participation in this ongoing project, and your help to
improve it and the e-mobility ICT in general. If you find bugs, want to
request a feature or send us a pull request, feel free to use the normal
GitHub features to do so. For this please read the Contributor License
Agreement carefully and send us a signed copy or use a similar free and
open license.
