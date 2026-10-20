# Jellyfin Payment Tracker

Internes Jellyfin-Server-Plugin (Jellyfin 12.2 / .NET 10) zur monatsweisen Verwaltung von Zahlungen.
Alle Daten liegen in der Plugin-Konfiguration (`<data>/plugins/configurations/Jellyfin.Plugin.PaymentTracker.xml`) – keine externe Datenbank.

## Status-Logik

| Status    | Bedingung                              | Farbe         |
|-----------|----------------------------------------|---------------|
| `Grace`   | `IsInGracePeriod == true` (hat Vorrang)| 🎁 Lila       |
| `Paid`    | bezahlt bis > aktueller Monat          | 🟢 Grün       |
| `Due`     | bezahlt bis == aktueller Monat         | 🟩 Gelbgrün   |
| `Overdue` | bezahlt bis < aktueller Monat          | 🔴 Rot        |

## API (nur Administratoren)

| Methode | Route                        | Body                                   |
|---------|------------------------------|----------------------------------------|
| GET     | `/PaymentTracker/Users`      | –                                      |
| POST    | `/PaymentTracker/AddMonth`   | `{ "UserId": "..." }`                  |
| POST    | `/PaymentTracker/SetMonth`   | `{ "UserId": "...", "Year": 2026, "Month": 11 }` |
| POST    | `/PaymentTracker/ToggleGrace`| `{ "UserId": "..." }`                  |
| POST    | `/PaymentTracker/UpdateNotes`| `{ "UserId": "...", "Notes": "..." }`  |

## Bauen & Veröffentlichen

Releases erstellt der Workflow [`.github/workflows/release.yml`](.github/workflows/release.yml):

1. *Actions → Release → Run workflow*, Version (vierstellig, z. B. `1.0.1.0`) und Changelog eingeben.
2. Der Workflow baut die DLL, veröffentlicht das ZIP als GitHub-Release `v1.0.1.0`, trägt
   Version, MD5-Checksumme und `targetAbi` (aus der `Jellyfin.Controller`-Version) in
   `manifest.json` ein und committet sie auf den Default-Branch.
3. In Jellyfin unter *Dashboard → Plugins → Repositories* die Raw-URL eintragen:
   `https://raw.githubusercontent.com/master4x/jellyfin-payment-tracker/master/manifest.json`

Lokaler Build: `dotnet build -c Release`, Tests: `dotnet test --solution Jellyfin.Plugin.PaymentTracker.slnx`.

Manuelle Installation: DLL nach `<data>/plugins/PaymentTracker_1.0.0.0/` kopieren und Jellyfin neu starten.
