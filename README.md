# Jellyfin Payment Tracker

Internes Jellyfin-Server-Plugin (Jellyfin 12.2 / .NET 10) zur monatsweisen Verwaltung von Zahlungen.
Alle Daten liegen in der Plugin-Konfiguration (`<data>/plugins/configurations/Jellyfin.Plugin.PaymentTracker.xml`) – keine externe Datenbank.

## Installation

Voraussetzung: Jellyfin 12.2 oder neuer.

1. *Dashboard → Plugins → Repositories* öffnen und ein Repository hinzufügen:
   - Name: `Payment Tracker`
   - URL: `https://raw.githubusercontent.com/master4x/jellyfin-payment-tracker/master/manifest.json`
2. Unter *Dashboard → Plugins → Katalog* den Eintrag *Payment Tracker* (Kategorie *Administration*) öffnen und installieren.
3. Jellyfin neu starten.
4. Die Seite *Payment Tracker* erscheint im Dashboard-Menü unter *Server*.

Updates zeigt Jellyfin danach unter *Plugins* an bzw. installiert sie über die geplante Aufgabe *Plugins aktualisieren*.
Erscheint eine neue Version nicht sofort: GitHub cacht das Manifest bis zu 5 Minuten.

### Manuelle Installation

ZIP aus den [Releases](https://github.com/master4x/jellyfin-payment-tracker/releases) laden, die DLL nach
`<data>/plugins/PaymentTracker_<version>/` kopieren und Jellyfin neu starten.

## Status-Logik

| Status    | Bedingung                              | Anzeige          |
|-----------|----------------------------------------|------------------|
| `Grace`   | `IsInGracePeriod == true` (hat Vorrang)| 🎁 Kulanz        |
| `Paid`    | bezahlt bis > aktueller Monat          | 🟢 Vorausbezahlt |
| `Due`     | bezahlt bis == aktueller Monat         | 🟩 Fällig        |
| `Overdue` | bezahlt bis < aktueller Monat          | 🔴 Rückstand     |

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

Lokaler Build: `dotnet build -c Release`, Tests: `dotnet test --solution Jellyfin.Plugin.PaymentTracker.slnx`.
