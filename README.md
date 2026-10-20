# Jellyfin Payment Tracker

Internes Jellyfin-Server-Plugin (Jellyfin 12.2 / .NET 10) zur monatsweisen Verwaltung von Barzahlungen.
Alle Daten liegen in der Plugin-Konfiguration (`<data>/plugins/configurations/Jellyfin.Plugin.PaymentTracker.xml`) – keine externe Datenbank.

## Status-Logik

| Status    | Bedingung                              | Farbe         |
|-----------|----------------------------------------|---------------|
| `GRACE`   | `IsInGracePeriod == true` (hat Vorrang)| 🎁 Lila       |
| `PAID`    | bezahlt bis > aktueller Monat          | 🟢 Grün       |
| `DUE`     | bezahlt bis == aktueller Monat         | 🟩 Gelbgrün   |
| `OVERDUE` | bezahlt bis < aktueller Monat          | 🔴 Rot        |

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

1. *Actions → Release → Run workflow*, Version (z. B. `1.0.1`) und Changelog eingeben.
2. Der Workflow baut die DLL, veröffentlicht das ZIP als GitHub-Release `v1.0.1`, trägt
   Version, MD5-Checksumme und `targetAbi` (aus der `Jellyfin.Controller`-Version) in
   `manifest.json` ein und committet sie auf den Default-Branch.
3. In Jellyfin unter *Dashboard → Plugins → Repositories* die Raw-URL eintragen:
   `https://raw.githubusercontent.com/master4x/jellyfin-payment-tracker/master/manifest.json`

> **Hinweis:** Solange das Repository privat ist, kann Jellyfin weder `manifest.json` noch das
> Release-ZIP ohne Authentifizierung abrufen. Für die Installation per Repository-URL muss das
> Repo (oder ein separates Manifest-Repo bzw. GitHub Pages) öffentlich erreichbar sein.

**Automatische Releases (optional):** Im Workflow den auskommentierten `push: tags`-Block aktivieren.
Danach löst `git tag v1.0.2 && git push origin v1.0.2` ein Release aus.

Lokaler Build: `dotnet build -c Release`.

Manuelle Installation: DLL nach `<data>/plugins/PaymentTracker_1.0.0.0/` kopieren und Jellyfin neu starten.
